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
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.view.exttimedwin
{
    /// <summary>
    ///     Factory for <seealso cref="ExternallyTimedWindowView" />.
    /// </summary>
    public class ExternallyTimedWindowViewForge : ViewFactoryForgeBase,
        DataWindowViewForge,
        DataWindowViewForgeWithPrevious
    {
        internal TimePeriodComputeForge timePeriodComputeForge;

        internal ExprNode timestampExpression;
        private IList<ExprNode> viewParameters;

        public override string ViewName => "Externally-timed";

        private string ViewParamMessage => ViewName +
                                           " view requires a timestamp expression and a numeric or time period parameter for window size";

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
            var validated = ViewForgeSupport.Validate(
                ViewName, parentEventType, viewParameters, true, viewForgeEnv, streamNumber);
            if (viewParameters.Count != 2) {
                throw new ViewParameterException(ViewParamMessage);
            }

            if (!validated[0].Forge.EvaluationType.IsNumeric()) {
                throw new ViewParameterException(ViewParamMessage);
            }

            timestampExpression = validated[0];
            ViewForgeSupport.AssertReturnsNonConstant(ViewName, validated[0], 0);

            timePeriodComputeForge = ViewFactoryTimePeriodHelper.ValidateAndEvaluateTimeDeltaFactory(
                ViewName, viewParameters[1], ViewParamMessage, 1, viewForgeEnv, streamNumber);
            eventType = parentEventType;
        }

        internal override Type TypeOfFactory()
        {
            return typeof(ExternallyTimedWindowViewFactory);
        }

        internal override string FactoryMethod()
        {
            return "exttime";
        }

        internal override void Assign(
            CodegenMethod method,
            CodegenExpressionRef factory,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            method.Block
                .DeclareVar(typeof(TimePeriodCompute), "eval", timePeriodComputeForge.MakeEvaluator(method, classScope))
                .ExprDotMethod(factory, "setTimePeriodCompute", Ref("eval"))
                .ExprDotMethod(
                    factory, "setTimestampEval", ExprNodeUtilityCodegen
                        .CodegenEvaluator(timestampExpression.Forge, method, GetType(), classScope));
        }
    }
} // end of namespace