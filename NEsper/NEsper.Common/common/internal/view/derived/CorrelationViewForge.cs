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
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.common.@internal.view.util;

using static com.espertech.esper.common.@internal.epl.expression.core.ExprNodeUtilityCodegen;

namespace com.espertech.esper.common.@internal.view.derived
{
    /// <summary>
    ///     Factory for <seealso cref="CorrelationView" /> instances.
    /// </summary>
    public class CorrelationViewForge : ViewFactoryForgeBase
    {
        internal StatViewAdditionalPropsForge additionalProps;
        internal ExprNode expressionX;
        internal ExprNode expressionY;
        private IList<ExprNode> viewParameters;

        public override string ViewName => "Correlation";

        private string ViewParamMessage =>
            ViewName + " view requires two expressions providing x and y values as properties";

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
                ViewName,
                parentEventType,
                viewParameters,
                true,
                viewForgeEnv,
                streamNumber);
            if (validated.Length < 2) {
                throw new ViewParameterException(ViewParamMessage);
            }

            if (!validated[0].Forge.EvaluationType.IsNumeric() || !validated[1].Forge.EvaluationType.IsNumeric()) {
                throw new ViewParameterException(ViewParamMessage);
            }

            expressionX = validated[0];
            expressionY = validated[1];

            additionalProps = StatViewAdditionalPropsForge.Make(validated, 2, parentEventType, streamNumber);
            eventType = CorrelationView.CreateEventType(additionalProps, viewForgeEnv, streamNumber);
        }

        internal override Type TypeOfFactory()
        {
            return typeof(CorrelationViewFactory);
        }

        internal override string FactoryMethod()
        {
            return "correlation";
        }

        internal override void Assign(
            CodegenMethod method,
            CodegenExpressionRef factory,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            if (additionalProps != null) {
                method.Block.SetProperty(factory, "AdditionalProps", additionalProps.Codegen(method, classScope));
            }

            method.Block
                .SetProperty(
                    factory,
                    "ExpressionXEval",
                    CodegenEvaluator(expressionX.Forge, method, GetType(), classScope))
                .SetProperty(
                    factory,
                    "ExpressionYEval",
                    CodegenEvaluator(expressionY.Forge, method, GetType(), classScope));
        }
    }
} // end of namespace