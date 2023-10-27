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
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.serde.compiletime.eventtype;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.common.@internal.view.util;
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.epl.expression.core.ExprNodeUtilityCodegen;

namespace com.espertech.esper.common.@internal.view.derived
{
    /// <summary>
    /// Factory for <seealso cref = "UnivariateStatisticsView"/> instances.
    /// </summary>
    public class UnivariateStatisticsViewForge : ViewFactoryForgeBaseDerived
    {
        protected ExprNode fieldExpression;

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
                this.ViewName,
                parentEventType,
                ViewParameters,
                true,
                viewForgeEnv);
            if (validated.Length < 1) {
                throw new ViewParameterException(ViewParamMessage);
            }

            if (!validated[0].Forge.EvaluationType.IsTypeNumeric()) {
                throw new ViewParameterException(ViewParamMessage);
            }

            fieldExpression = validated[0];
            AdditionalProps = StatViewAdditionalPropsForge.Make(validated, 1, parentEventType, viewForgeEnv);
            eventType = UnivariateStatisticsView.CreateEventType(AdditionalProps, viewForgeEnv);
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

        internal override Type TypeOfFactory => typeof(UnivariateStatisticsViewFactory);
        internal override string FactoryMethod => "Uni";

        internal override void Assign(
            CodegenMethod method,
            CodegenExpressionRef factory,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            if (AdditionalProps != null) {
                method.Block.SetProperty(factory, "AdditionalProps", AdditionalProps.Codegen(method, classScope));
            }

            method.Block.SetProperty(
                factory,
                "FieldEval",
                CodegenEvaluator(fieldExpression.Forge, method, GetType(), classScope));
        }

        public override string ViewName => ViewEnum.UNIVARIATE_STATISTICS.GetName();

        public override AppliesTo AppliesTo()
        {
            return client.annotation.AppliesTo.WINDOW_UNIVARIATESTAT;
        }

        public override T Accept<T>(ViewFactoryForgeVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public string ViewParamMessage =>
            ViewName + " view require a single expression returning a numeric value as a parameter";
    }
} // end of namespace