///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

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

        public void SetAcceptableValueTypes(Type[] acceptableValueTypes)
        {
            this.acceptableValueTypes = acceptableValueTypes;
        }

        public void ValidateIntoTableCompatible(
            string tableExpression,
            AggregationPortableValidation intoTableAgg,
            string intoExpression,
            AggregationForgeFactory factory)
        {
            AggregationValidationUtil.ValidateAggregationType(this, tableExpression, intoTableAgg, intoExpression);

            if (factory is AggregationForgeFactoryAccessCountMinSketchAdd) {
                AggregationForgeFactoryAccessCountMinSketchAdd add =
                    (AggregationForgeFactoryAccessCountMinSketchAdd) factory;
                CountMinSketchAggType aggType = add.Parent.AggType;
                if (aggType == CountMinSketchAggType.FREQ || aggType == CountMinSketchAggType.ADD) {
                    Type clazz = add.AddOrFrequencyEvaluatorReturnType;
                    bool foundMatch = false;
                    foreach (Type allowed in acceptableValueTypes) {
                        if (TypeHelper.IsSubclassOrImplementsInterface(clazz, allowed)) {
                            foundMatch = true;
                        }
                    }

                    if (!foundMatch) {
                        throw new ExprValidationException(
                            "Mismatching parameter return type, expected any of " +
                            CompatExtensions.RenderAny(acceptableValueTypes) +
                            " but received " +
                            TypeHelper.CleanName(clazz));
                    }
                }
            }
        }

        public CodegenExpression Make(
            CodegenMethodScope parent,
            ModuleTableInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            CodegenMethod method = parent.MakeChild(
                typeof(AggregationPortableValidationCountMinSketch),
                this.GetType(),
                classScope);
            method.Block
                .DeclareVar<AggregationPortableValidationCountMinSketch>(
                    "v",
                    NewInstance(typeof(AggregationPortableValidationCountMinSketch)))
                .SetProperty(Ref("v"), "AcceptableValueTypes", Constant(acceptableValueTypes))
                .MethodReturn(@Ref("v"));
            return LocalMethod(method);
        }
    }
} // end of namespace