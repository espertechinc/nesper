///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client.hook.aggmultifunc;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.approx.countminsketch;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.expression.agg.accessagg.ExprAggMultiFunctionCountMinSketchNode; //MSG_NAME

namespace com.espertech.esper.common.@internal.epl.agg.access.countminsketch
{
    public class AggregationPortableValidationCountMinSketch : AggregationPortableValidation
    {
        private Type[] acceptableValueTypes;

        public AggregationPortableValidationCountMinSketch()
        {
        }

        public AggregationPortableValidationCountMinSketch(Type[] acceptableValueTypes)
        {
            this.acceptableValueTypes = acceptableValueTypes;
        }

        public Type[] AcceptableValueTypes {
            get => acceptableValueTypes;
            set => acceptableValueTypes = value;
        }

        public void ValidateIntoTableCompatible(
            string tableExpression,
            AggregationPortableValidation intoTableAgg,
            string intoExpression,
            AggregationForgeFactory factory)
        {
            AggregationValidationUtil.ValidateAggregationType(this, tableExpression, intoTableAgg, intoExpression);

            if (factory is AggregationForgeFactoryAccessCountMinSketchAdd) {
                var add =
                    (AggregationForgeFactoryAccessCountMinSketchAdd) factory;
                var aggType = add.Parent.AggType;
                if (aggType == CountMinSketchAggType.ADD) {
                    var clazz = add.AddOrFrequencyEvaluatorReturnType;
                    var foundMatch = false;
                    foreach (var allowed in acceptableValueTypes) {
                        if (TypeHelper.IsSubclassOrImplementsInterface(clazz, allowed)) {
                            foundMatch = true;
                        }
                    }

                    if (!foundMatch) {
                        throw new ExprValidationException(
                            "Mismatching parameter return type, expected any of " +
                            acceptableValueTypes.RenderAny() +
                            " but received " +
                            clazz.CleanName());
                    }
                }
            }
        }

        public CodegenExpression Make(
            CodegenMethodScope parent,
            ModuleTableInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(
                typeof(AggregationPortableValidationCountMinSketch),
                this.GetType(),
                classScope);
            method.Block
                .DeclareVar<AggregationPortableValidationCountMinSketch>(
                    "v",
                    NewInstance(typeof(AggregationPortableValidationCountMinSketch)))
                .SetProperty(Ref("v"), "AcceptableValueTypes", Constant(acceptableValueTypes))
                .MethodReturn(Ref("v"));
            return LocalMethod(method);
        }


        public bool IsAggregationMethod(
            string name,
            ExprNode[] parameters,
            ExprValidationContext validationContext)
        {
            return CountMinSketchAggMethodExtensions.FromNameMayMatch(name) != null;
        }

        public AggregationMultiFunctionMethodDesc ValidateAggregationMethod(
            ExprValidationContext validationContext,
            string aggMethodName,
            ExprNode[] @params)
        {
            var aggMethod = CountMinSketchAggMethodExtensions.FromNameMayMatch(aggMethodName);
            AggregationMethodForge forge;
            if (aggMethod == CountMinSketchAggMethod.FREQ) {
                if (@params.Length == 0 || @params.Length > 1) {
                    throw new ExprValidationException(GetMessagePrefix(aggMethod.Value) + "requires a single parameter expression");
                }

                ExprNodeUtilityValidate.GetValidatedSubtree(ExprNodeOrigin.AGGPARAM, @params, validationContext);
                var frequencyEval = @params[0];
                forge = new AgregationMethodCountMinSketchFreqForge(frequencyEval);
            }
            else {
                if (@params.Length != 0) {
                    throw new ExprValidationException(GetMessagePrefix(aggMethod.Value) + "requires a no parameter expressions");
                }

                forge = new AgregationMethodCountMinSketchTopKForge();
            }

            return new AggregationMultiFunctionMethodDesc(forge, null, null, null);
        }

        private string GetMessagePrefix(CountMinSketchAggMethod aggType) {
            return MSG_NAME + " aggregation function '" + aggType.GetMethodName() + "' ";
        }
    }
} // end of namespace