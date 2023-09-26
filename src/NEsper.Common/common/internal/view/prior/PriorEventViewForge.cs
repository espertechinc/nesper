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
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.view.prior
{
    public class PriorEventViewForge : ViewFactoryForgeBase
    {
        private readonly bool unbound;
        private readonly StateMgmtSetting stateMgmtSettings;

        public PriorEventViewForge(
            bool unbound,
            EventType eventType,
            StateMgmtSetting stateMgmtSettings)
        {
            this.unbound = unbound;
            this.eventType = eventType;
            this.stateMgmtSettings = stateMgmtSettings;
        }

        public override void SetViewParameters(
            IList<ExprNode> parameters,
            ViewForgeEnv viewForgeEnv,
            int streamNumber)
        {
        }

        public override void AttachValidate(
            EventType parentEventType,
            ViewForgeEnv viewForgeEnv)
        {
            throw new IllegalStateException("Should not be called for 'prior'");
        }

        internal override Type TypeOfFactory => typeof(PriorEventViewFactory);

        internal override string FactoryMethod => "prior";

        internal override void Assign(
            CodegenMethod method,
            CodegenExpressionRef factory,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            method.Block.SetProperty(factory, "Unbound", Constant(unbound));
        }

        public override string ViewName => "prior";

        public override AppliesTo AppliesTo()
        {
            return client.annotation.AppliesTo.PRIOR;
        }

        public override T Accept<T>(ViewFactoryForgeVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
} // end of namespace