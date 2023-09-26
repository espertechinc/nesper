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
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.serde.compiletime.eventtype;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.common.@internal.view.util;

using static com.espertech.esper.common.@internal.epl.expression.core.ExprNodeUtilityCodegen;

namespace com.espertech.esper.common.@internal.view.derived
{
    /// <summary>
    ///     Factory for <seealso cref="CorrelationView" /> instances.
    /// </summary>
    public class CorrelationViewForge : ViewFactoryForgeBaseDerived
    {
        internal StatViewAdditionalPropsForge additionalProps;
        internal ExprNode expressionX;
        internal ExprNode expressionY;

        public override string ViewName => "Correlation";

        private string ViewParamMessage =>
            ViewName + " view requires two expressions providing x and y values as properties";

        public override void SetViewParameters(
            IList<ExprNode> parameters,
            ViewForgeEnv viewForgeEnv,
            int streamNumber)
        {
            ViewParameters = parameters;
        }

        public override void AttachValidate(
            EventType parentEventType,
            ViewForgeEnv viewForgeEnv)
        {
            var validated = ViewForgeSupport.Validate(
                ViewName,
                parentEventType,
                ViewParameters,
                true,
                viewForgeEnv);
            if (validated.Length < 2) {
                throw new ViewParameterException(ViewParamMessage);
            }

            if (!validated[0].Forge.EvaluationType.IsTypeNumeric() ||
                !validated[1].Forge.EvaluationType.IsTypeNumeric()) {
                throw new ViewParameterException(ViewParamMessage);
            }

            expressionX = validated[0];
            expressionY = validated[1];

            additionalProps = StatViewAdditionalPropsForge.Make(validated, 2, parentEventType, viewForgeEnv);
            eventType = CorrelationView.CreateEventType(additionalProps, viewForgeEnv);
        }

        public override IList<StmtClassForgeableFactory> InitAdditionalForgeables(ViewForgeEnv viewForgeEnv)
        {
            return SerdeEventTypeUtility.Plan(
                eventType,
                viewForgeEnv.StatementRawInfo,
                viewForgeEnv.SerdeEventTypeRegistry,
                viewForgeEnv.SerdeResolver,
                viewForgeEnv.StateMgmtSettingsProvider);
        }

        internal override Type TypeOfFactory => typeof(CorrelationViewFactory);

        internal override string FactoryMethod => "Correlation";

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

        public override AppliesTo AppliesTo()
        {
            return client.annotation.AppliesTo.WINDOW_CORRELATION;
        }

        public override T Accept<T>(ViewFactoryForgeVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
} // end of namespace