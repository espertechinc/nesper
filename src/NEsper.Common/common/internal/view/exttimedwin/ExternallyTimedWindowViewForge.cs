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
using com.espertech.esper.common.client.annotation;
using com.espertech.esper.common.client.util;
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
        private TimePeriodComputeForge timePeriodComputeForge;
        private ExprNode timestampExpression;
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

        public override void AttachValidate(
            EventType parentEventType,
            int streamNumber,
            ViewForgeEnv viewForgeEnv,
            bool grouped)
        {
            var validated = ViewForgeSupport.Validate(
                ViewName,
                parentEventType,
                viewParameters,
                true,
                viewForgeEnv,
                streamNumber);
            if (viewParameters.Count != 2) {
                throw new ViewParameterException(ViewParamMessage);
            }

            if (!validated[0].Forge.EvaluationType.IsNumeric()) {
                throw new ViewParameterException(ViewParamMessage);
            }

            timestampExpression = validated[0];
            ViewForgeSupport.AssertReturnsNonConstant(ViewName, validated[0], 0);

            timePeriodComputeForge = ViewFactoryTimePeriodHelper.ValidateAndEvaluateTimeDeltaFactory(
                ViewName,
                viewParameters[1],
                ViewParamMessage,
                1,
                viewForgeEnv,
                streamNumber);
            eventType = parentEventType;
        }

        public override Type TypeOfFactory()
        {
            return typeof(ExternallyTimedWindowViewFactory);
        }

        public override string FactoryMethod()
        {
            return "Exttime";
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
                .SetProperty(factory, "TimestampEval", ExprNodeUtilityCodegen.CodegenEvaluator(timestampExpression.Forge, method, GetType(), classScope));
        }

        public override AppliesTo AppliesTo()
        {
            return client.annotation.AppliesTo.WINDOW_EXTTIMED;
        }
    }
} // end of namespace