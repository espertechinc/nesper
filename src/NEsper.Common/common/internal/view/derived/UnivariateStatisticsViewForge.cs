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
    ///     Factory for <seealso cref="UnivariateStatisticsView" /> instances.
    /// </summary>
    public class UnivariateStatisticsViewForge : ViewFactoryForgeBase
    {
        private const string NAME = "Univariate statistics";
        private StatViewAdditionalPropsForge _additionalProps;
        private ExprNode _fieldExpression;

        private IList<ExprNode> _viewParameters;

        public override string ViewName => NAME;

        private string ViewParamMessage =>
            ViewName + " view require a single expression returning a numeric value as a parameter";

        public override void SetViewParameters(
            IList<ExprNode> parameters,
            ViewForgeEnv viewForgeEnv,
            int streamNumber)
        {
            _viewParameters = parameters;
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
                _viewParameters,
                true,
                viewForgeEnv,
                streamNumber);
            if (validated.Length < 1) {
                throw new ViewParameterException(ViewParamMessage);
            }

            if (!validated[0].Forge.EvaluationType.IsNumeric()) {
                throw new ViewParameterException(ViewParamMessage);
            }

            _fieldExpression = validated[0];

            _additionalProps = StatViewAdditionalPropsForge.Make(validated, 1, parentEventType, streamNumber, viewForgeEnv);
            eventType = UnivariateStatisticsView.CreateEventType(_additionalProps, viewForgeEnv, streamNumber);
        }

        public override IList<StmtClassForgeableFactory> InitAdditionalForgeables(ViewForgeEnv viewForgeEnv)
        {
            return SerdeEventTypeUtility.Plan(
                eventType,
                viewForgeEnv.StatementRawInfo,
                viewForgeEnv.SerdeEventTypeRegistry,
                viewForgeEnv.SerdeResolver);
        }

        public override Type TypeOfFactory()
        {
            return typeof(UnivariateStatisticsViewFactory);
        }

        public override string FactoryMethod()
        {
            return "Uni";
        }

        internal override void Assign(
            CodegenMethod method,
            CodegenExpressionRef factory,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            if (_additionalProps != null) {
                method.Block.SetProperty(factory, "AdditionalProps", _additionalProps.Codegen(method, classScope));
            }

            method.Block.SetProperty(
                factory,
                "FieldEval",
                CodegenEvaluator(_fieldExpression.Forge, method, GetType(), classScope));
        }

        public override AppliesTo AppliesTo()
        {
            return client.annotation.AppliesTo.WINDOW_UNIVARIATESTAT;
        }
    }
} // end of namespace