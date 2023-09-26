///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.annotation;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.time.eval;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.common.@internal.view.util;
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.expression.core.ExprNodeUtilityCodegen;

namespace com.espertech.esper.common.@internal.view.exttimedbatch
{
    public class ExternallyTimedBatchViewForge : ViewFactoryForgeBase,
        DataWindowViewForge,
        DataWindowViewForgeWithPrevious,
        DataWindowBatchingViewForge
    {
        private IList<ExprNode> viewParameters;
        private ExprNode timestampExpression;
        private long? optionalReferencePoint;
        private TimePeriodComputeForge timePeriodComputeForge;

        public override void SetViewParameters(
            IList<ExprNode> parameters,
            ViewForgeEnv viewForgeEnv,
            int streamNumber)
        {
            viewParameters = parameters;
        }

        public override void AttachValidate(
            EventType parentEventType,
            ViewForgeEnv viewForgeEnv)
        {
            var windowName = ViewName;
            var validated = ViewForgeSupport.Validate(windowName, parentEventType, viewParameters, true, viewForgeEnv);
            if (viewParameters.Count < 2 || viewParameters.Count > 3) {
                throw new ViewParameterException(ViewParamMessage);
            }

            // validate first parameter: timestamp expression
            if (!validated[0].Forge.EvaluationType.IsTypeNumeric()) {
                throw new ViewParameterException(ViewParamMessage);
            }

            timestampExpression = validated[0];
            ViewForgeSupport.AssertReturnsNonConstant(windowName, validated[0], 0);
            timePeriodComputeForge = ViewFactoryTimePeriodHelper.ValidateAndEvaluateTimeDeltaFactory(
                ViewName,
                viewParameters[1],
                ViewParamMessage,
                1,
                viewForgeEnv);
            // validate optional parameters
            if (validated.Length == 3) {
                var constant = ViewForgeSupport.ValidateAndEvaluate(windowName, validated[2], viewForgeEnv);
                if (!(constant.IsNumber()) || constant.IsFloatingPointNumber()) {
                    throw new ViewParameterException(
                        "Externally-timed batch view requires a Long-typed reference point in msec as a third parameter");
                }

                optionalReferencePoint = constant.AsInt64();
            }

            eventType = parentEventType;
        }

        internal override Type TypeOfFactory => typeof(ExternallyTimedBatchViewFactory);
        internal override string FactoryMethod => "exttimebatch";

        internal override void Assign(
            CodegenMethod method,
            CodegenExpressionRef factory,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            method.Block
                .DeclareVar<TimePeriodCompute>("eval", timePeriodComputeForge.MakeEvaluator(method, classScope))
                .ExprDotMethod(factory, "setTimePeriodCompute", Ref("eval"))
                .ExprDotMethod(factory, "setOptionalReferencePoint", Constant(optionalReferencePoint))
                .ExprDotMethod(
                    factory,
                    "setTimestampEval",
                    CodegenEvaluator(timestampExpression.Forge, method, GetType(), classScope));
        }

        public override AppliesTo AppliesTo()
        {
            return client.annotation.AppliesTo.WINDOW_EXTTIMEDBATCH;
        }

        public override T Accept<T>(ViewFactoryForgeVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ViewName => "Externally-timed-batch";

        public string ViewParamMessage =>
            $"{ViewName} view requires a timestamp expression and a numeric or time period parameter for window size and an optional long-typed reference point in msec, and an optional list of control keywords as a string parameter (please see the documentation)";
    }
} // end of namespace