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
    public class SizeViewForge : ViewFactoryForgeBaseDerived
    {
        internal const string NAME = "Size";

        public override string ViewName => NAME;

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
            AdditionalProps = StatViewAdditionalPropsForge.Make(validated, 0, parentEventType, viewForgeEnv);
            eventType = SizeView.CreateEventType(viewForgeEnv, AdditionalProps);
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

        internal override Type TypeOfFactory => typeof(SizeViewFactory);

        internal override string FactoryMethod => "Size";

        internal override void Assign(
            CodegenMethod method,
            CodegenExpressionRef factory,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            if (AdditionalProps != null) {
                method.Block.SetProperty(factory, "AdditionalProps", AdditionalProps.Codegen(method, classScope));
            }
        }


        public override AppliesTo AppliesTo()
        {
            return client.annotation.AppliesTo.WINDOW_SIZE;
        }

        public override T Accept<T>(ViewFactoryForgeVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
} // end of namespace