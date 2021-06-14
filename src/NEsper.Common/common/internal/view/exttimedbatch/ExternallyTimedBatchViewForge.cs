///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
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
        private long? optionalReferencePoint;
        private TimePeriodComputeForge timePeriodComputeForge;
        private ExprNode timestampExpression;
        private IList<ExprNode> viewParameters;

        public override string ViewName => "Externally-timed-batch";

        private string ViewParamMessage => ViewName +
                                           " view requires a timestamp expression and a numeric or time period parameter for window size and an optional long-typed reference point in msec, and an optional list of control keywords as a string parameter (please see the documentation)";

        public override void SetViewParameters(
            IList<ExprNode> parameters,
            ViewForgeEnv viewForgeEnv,
            int streamNumber)
        {
            viewParameters = parameters;
        }

        public override void Attach(
            EventType parentEventType,
            int streamNumber,
            ViewForgeEnv viewForgeEnv)
        {
            var windowName = ViewName;
            var validated = ViewForgeSupport.Validate(
                windowName,
                parentEventType,
                viewParameters,
                true,
                viewForgeEnv,
                streamNumber);
            if (viewParameters.Count < 2 || viewParameters.Count > 3) {
                throw new ViewParameterException(ViewParamMessage);
            }

            // validate first parameter: timestamp expression
            if (!validated[0].Forge.EvaluationType.IsNumeric()) {
                throw new ViewParameterException(ViewParamMessage);
            }

            timestampExpression = validated[0];
            ViewForgeSupport.AssertReturnsNonConstant(windowName, validated[0], 0);

            timePeriodComputeForge = ViewFactoryTimePeriodHelper.ValidateAndEvaluateTimeDeltaFactory(
                ViewName,
                viewParameters[1],
                ViewParamMessage,
                1,
                viewForgeEnv,
                streamNumber);

            // validate optional parameters
            if (validated.Length == 3) {
                var constant = ViewForgeSupport.ValidateAndEvaluate(
                    windowName,
                    validated[2],
                    viewForgeEnv,
                    streamNumber);
                if (!constant.IsNumber() || constant.IsFloatingPointNumber()) {
                    throw new ViewParameterException(
                        "Externally-timed batch view requires a Long-typed reference point in msec as a third parameter");
                }

                optionalReferencePoint = constant.AsInt64();
            }

            eventType = parentEventType;
        }

        internal override Type TypeOfFactory()
        {
            return typeof(ExternallyTimedBatchViewFactory);
        }

        internal override string FactoryMethod()
        {
            return "Exttimebatch";
        }

        internal override void Assign(
            CodegenMethod method,
            CodegenExpressionRef factory,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            method.Block
                .DeclareVar<TimePeriodCompute>("eval", timePeriodComputeForge.MakeEvaluator(method, classScope))
                .SetProperty(factory, "TimePeriodCompute", Ref("eval"))
                .SetProperty(factory, "OptionalReferencePoint", Constant(optionalReferencePoint))
                .SetProperty(
                    factory,
                    "TimestampEval",
                    CodegenEvaluator(timestampExpression.Forge, method, GetType(), classScope));
        }
    }
} // end of namespace