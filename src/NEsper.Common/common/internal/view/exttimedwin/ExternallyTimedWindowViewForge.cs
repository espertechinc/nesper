///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.expression.core.ExprNodeUtilityCodegen;

namespace com.espertech.esper.common.@internal.view.exttimedwin
{
    /// <summary>
    /// Factory for <seealso cref = "ExternallyTimedWindowView"/>.
    /// </summary>
    public class ExternallyTimedWindowViewForge : ViewFactoryForgeBase,
        DataWindowViewForge,
        DataWindowViewForgeWithPrevious
    {
        private IList<ExprNode> viewParameters;
        protected ExprNode timestampExpression;
        protected TimePeriodComputeForge timePeriodComputeForge;

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
            var validated = ViewForgeSupport.Validate(ViewName, parentEventType, viewParameters, true, viewForgeEnv);
            if (viewParameters.Count != 2) {
                throw new ViewParameterException(ViewParamMessage);
            }

            if (!validated[0].Forge.EvaluationType.IsTypeNumeric()) {
                throw new ViewParameterException(ViewParamMessage);
            }

            timestampExpression = validated[0];
            ViewForgeSupport.AssertReturnsNonConstant(ViewName, validated[0], 0);
            timePeriodComputeForge = ViewFactoryTimePeriodHelper.ValidateAndEvaluateTimeDeltaFactory(
                ViewName,
                viewParameters[1],
                ViewParamMessage,
                1,
                viewForgeEnv);
            eventType = parentEventType;
        }

        internal override Type TypeOfFactory => typeof(ExternallyTimedWindowViewFactory);
        internal override string FactoryMethod => "Exttime";

        internal override void Assign(
            CodegenMethod method,
            CodegenExpressionRef factory,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            method.Block.DeclareVar<TimePeriodCompute>("eval", timePeriodComputeForge.MakeEvaluator(method, classScope))
                .SetProperty(factory, "TimePeriodCompute", Ref("eval"))
                .SetProperty(
                    factory,
                    "TimestampEval",
                    CodegenEvaluator(timestampExpression.Forge, method, GetType(), classScope));
        }

        public override AppliesTo AppliesTo()
        {
            return client.annotation.AppliesTo.WINDOW_EXTTIMED;
        }

        public override T Accept<T>(ViewFactoryForgeVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ViewName => "Externally-timed";

        public string ViewParamMessage => ViewName +
                                          " view requires a timestamp expression and a numeric or time period parameter for window size";
    }
} // end of namespace