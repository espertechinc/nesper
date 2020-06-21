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
    ///     Factory for <seealso cref="WeightedAverageView" /> instances.
    /// </summary>
    public class WeightedAverageViewForge : ViewFactoryForgeBase
    {
        internal const string NAME = "Weighted-average";

        internal StatViewAdditionalPropsForge additionalProps;
        internal ExprNode fieldNameWeight;
        internal ExprNode fieldNameX;

        private IList<ExprNode> viewParameters;

        public override string ViewName => NAME;

        private string ViewParamMessage =>
            ViewName + " view requires two expressions returning numeric values as parameters";

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

            fieldNameX = validated[0];
            fieldNameWeight = validated[1];
            additionalProps = StatViewAdditionalPropsForge.Make(validated, 2, parentEventType, streamNumber, viewForgeEnv);
            eventType = WeightedAverageView.CreateEventType(additionalProps, viewForgeEnv, streamNumber);
        }

        public override IList<StmtClassForgeableFactory> InitAdditionalForgeables(ViewForgeEnv viewForgeEnv)
        {
            return SerdeEventTypeUtility.Plan(
                eventType,
                viewForgeEnv.StatementRawInfo,
                viewForgeEnv.SerdeEventTypeRegistry,
                viewForgeEnv.SerdeResolver);
        }

        internal override Type TypeOfFactory()
        {
            return typeof(WeightedAverageViewFactory);
        }

        internal override string FactoryMethod()
        {
            return "Weightedavg";
        }

        internal override void Assign(
            CodegenMethod method,
            CodegenExpressionRef factory,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            if (additionalProps != null) {
                method.Block.SetProperty(
                    factory,
                    "AdditionalProps",
                    additionalProps.Codegen(method, classScope));
            }

            method.Block
                .SetProperty(
                    factory,
                    "FieldNameXEvaluator",
                    CodegenEvaluator(fieldNameX.Forge, method, GetType(), classScope))
                .SetProperty(
                    factory,
                    "FieldNameWeightEvaluator",
                    CodegenEvaluator(fieldNameWeight.Forge, method, GetType(), classScope));
        }
    }
} // end of namespace