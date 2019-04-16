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
            Type[] additionalTypes)
        {
            AdditionalProps = additionalProps;
            AdditionalEvals = additionalEvals;
            AdditionalTypes = additionalTypes;
        }

        public string[] AdditionalProps { get; }

        public ExprNode[] AdditionalEvals { get; }

        public Type[] AdditionalTypes { get; }

        public static StatViewAdditionalPropsForge Make(
            ExprNode[] validated,
            int startIndex,
            EventType parentEventType,
            int streamNumber)
        {
            if (validated.Length <= startIndex) {
                return null;
            }

            IList<string> additionalProps = new List<string>();
            IList<ExprNode> lastValueForges = new List<ExprNode>();
            IList<Type> lastValueTypes = new List<Type>();
            var copyAllProperties = false;

            for (var i = startIndex; i < validated.Length; i++) {
                if (validated[i] is ExprWildcard) {
                    copyAllProperties = true;
                }
                else {
                    additionalProps.Add(ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(validated[i]));
                    lastValueTypes.Add(validated[i].Forge.EvaluationType);
                    lastValueForges.Add(validated[i]);
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
                }
            }

            var addPropsArr = additionalProps.ToArray();
            var valueExprArr = lastValueForges.ToArray();
            var typeArr = lastValueTypes.ToArray();
            return new StatViewAdditionalPropsForge(addPropsArr, valueExprArr, typeArr);
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
            return NewInstance(
                typeof(StatViewAdditionalPropsEval), Constant(AdditionalProps),
                CodegenEvaluators(AdditionalEvals, method, GetType(), classScope), Constant(AdditionalTypes));
        }
    }
} // end of namespace