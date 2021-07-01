///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.serde.compiletime.resolve;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.expression.core.ExprNodeUtilityCodegen;

namespace com.espertech.esper.common.@internal.view.derived
{
    public class StatViewAdditionalPropsForge
    {  
        private StatViewAdditionalPropsForge(
            string[] additionalProps,
            ExprNode[] additionalEvals,
            Type[] additionalTypes,
            DataInputOutputSerdeForge[] additionalSerdes)
        {
            AdditionalProps = additionalProps;
            AdditionalEvals = additionalEvals;
            AdditionalTypes = additionalTypes;
            AdditionalSerdes = additionalSerdes;
        }

        public string[] AdditionalProps { get; }

        public ExprNode[] AdditionalEvals { get; }

        public Type[] AdditionalTypes { get; }

        public DataInputOutputSerdeForge[] AdditionalSerdes { get; }

        public static StatViewAdditionalPropsForge Make(
            ExprNode[] validated,
            int startIndex,
            EventType parentEventType,
            int streamNumber,
            ViewForgeEnv viewForgeEnv)
        {
            if (validated.Length <= startIndex) {
                return null;
            }

            IList<string> additionalProps = new List<string>();
            IList<ExprNode> lastValueForges = new List<ExprNode>();
            IList<Type> lastValueTypes = new List<Type>();
            IList<DataInputOutputSerdeForge> lastSerdes = new List<DataInputOutputSerdeForge>();

            var copyAllProperties = false;

            for (var i = startIndex; i < validated.Length; i++) {
                if (validated[i] is ExprWildcard) {
                    copyAllProperties = true;
                }
                else {
                    additionalProps.Add(ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(validated[i]));
                    var evalType = validated[i].Forge.EvaluationType;
                    lastValueTypes.Add(evalType);
                    lastValueForges.Add(validated[i]);
                    lastSerdes.Add(viewForgeEnv.SerdeResolver.SerdeForDerivedViewAddProp(evalType, viewForgeEnv.StatementRawInfo));
                }
            }

            if (copyAllProperties) {
                foreach (var propertyDescriptor in parentEventType.PropertyDescriptors) {
                    if (propertyDescriptor.IsFragment) {
                        continue;
                    }

                    additionalProps.Add(propertyDescriptor.PropertyName);
                    var type = propertyDescriptor.PropertyType;
                    lastValueForges.Add(
                        new ExprIdentNodeImpl(parentEventType, propertyDescriptor.PropertyName, streamNumber));
                    lastValueTypes.Add(type);
                    lastSerdes.Add(viewForgeEnv.SerdeResolver.SerdeForDerivedViewAddProp(type, viewForgeEnv.StatementRawInfo));
                }
            }

            var addPropsArr = additionalProps.ToArray();
            var valueExprArr = lastValueForges.ToArray();
            var typeArr = lastValueTypes.ToArray();
            var additionalForges = lastSerdes.ToArray();
            return new StatViewAdditionalPropsForge(addPropsArr, valueExprArr, typeArr, additionalForges);
        }

        public static void AddCheckDupProperties(
            IDictionary<string, object> target,
            StatViewAdditionalPropsForge addProps,
            params ViewFieldEnum[] builtin)
        {
            if (addProps == null) {
                return;
            }

            for (var i = 0; i < addProps.AdditionalProps.Length; i++) {
                var name = addProps.AdditionalProps[i];
                for (var j = 0; j < builtin.Length; j++) {
                    if (name.Equals(builtin[j].GetName(), StringComparison.InvariantCultureIgnoreCase)) {
                        throw new ArgumentException(
                            "The property by name '" + name + "' overlaps the property name that the view provides");
                    }
                }

                target.Put(name, addProps.AdditionalTypes[i]);
            }
        }

        public CodegenExpression Codegen(
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            return NewInstance<StatViewAdditionalPropsEval>(
                Constant(AdditionalProps),
                CodegenEvaluators(AdditionalEvals, method, this.GetType(), classScope),
                Constant(AdditionalTypes),
                DataInputOutputSerdeForgeExtensions.CodegenArray(AdditionalSerdes, method, classScope, null));
        }
    }
} // end of namespace