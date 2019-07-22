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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.epl.expression.core.ExprNodeUtilityCodegen;

namespace com.espertech.esper.common.@internal.view.derived
{
    /// <summary>
    /// Factory for <seealso cref="RegressionLinestView" /> instances.
    /// </summary>
    public class RegressionLinestViewForge : ViewFactoryForgeBase
    {
        private IList<ExprNode> viewParameters;

        private ExprNode expressionX;
        private ExprNode expressionY;
        internal StatViewAdditionalPropsForge additionalProps;

        public override void SetViewParameters(
            IList<ExprNode> parameters,
            ViewForgeEnv viewForgeEnv,
            int streamNumber)
        {
            this.viewParameters = parameters;
        }

        public override void Attach(
            EventType parentEventType,
            int streamNumber,
            ViewForgeEnv viewForgeEnv)
        {
            ExprNode[] validated = ViewForgeSupport.Validate(
                ViewName,
                parentEventType,
                viewParameters,
                true,
                viewForgeEnv,
                streamNumber);

            if (validated.Length < 2) {
                throw new ViewParameterException(ViewParamMessage);
            }

            if ((!TypeHelper.IsNumeric(validated[0].Forge.EvaluationType)) ||
                (!TypeHelper.IsNumeric(validated[1].Forge.EvaluationType))) {
                throw new ViewParameterException(ViewParamMessage);
            }

            expressionX = validated[0];
            expressionY = validated[1];

            additionalProps = StatViewAdditionalPropsForge.Make(validated, 2, parentEventType, streamNumber);
            eventType = RegressionLinestView.CreateEventType(additionalProps, viewForgeEnv, streamNumber);
        }

        internal override Type TypeOfFactory()
        {
            return typeof(RegressionLinestViewFactory);
        }

        internal override string FactoryMethod()
        {
            return "regression";
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
                    CodegenEvaluator(expressionX.Forge, method, this.GetType(), classScope))
                .SetProperty(
                    factory,
                    "ExpressionYEval",
                    CodegenEvaluator(expressionY.Forge, method, this.GetType(), classScope));
        }

        public override string ViewName {
            get => "Regression";
        }

        private string ViewParamMessage {
            get { return ViewName + " view requires two expressions providing x and y values as properties"; }
        }
    }
} // end of namespace