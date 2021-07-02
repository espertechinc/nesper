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
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.common.@internal.view.util;

namespace com.espertech.esper.common.@internal.view.derived
{
    /// <summary>
    ///     Factory for <seealso cref="SizeView" /> instances.
    /// </summary>
    public class SizeViewForge : ViewFactoryForgeBase
    {
        private const string NAME = "Size";
        private StatViewAdditionalPropsForge _additionalProps;

        private IList<ExprNode> _viewParameters;

        public override string ViewName => NAME;

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
            _additionalProps = StatViewAdditionalPropsForge.Make(validated, 0, parentEventType, streamNumber, viewForgeEnv);
            eventType = SizeView.CreateEventType(viewForgeEnv, _additionalProps, streamNumber);
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
            return typeof(SizeViewFactory);
        }

        public override string FactoryMethod()
        {
            return "Size";
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
        }

        public override AppliesTo AppliesTo()
        {
            return client.annotation.AppliesTo.WINDOW_SIZE;
        }
    }
} // end of namespace