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
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.common.@internal.view.util;

namespace com.espertech.esper.common.@internal.view.lastevent
{
    /// <summary>
    /// Factory for <seealso cref="LastEventView" /> instances.
    /// </summary>
    public class LastEventViewForge : ViewFactoryForgeBase,
        DataWindowViewForge
    {
        public const string NAME = "Last-Event";

        public override void SetViewParameters(
            IList<ExprNode> parameters,
            ViewForgeEnv viewForgeEnv,
            int streamNumber)
        {
            ViewForgeSupport.ValidateNoParameters(ViewName, parameters);
        }

        public override void AttachValidate(
            EventType parentEventType,
            ViewForgeEnv viewForgeEnv)
        {
            eventType = parentEventType;
        }

        internal override Type TypeOfFactory => typeof(LastEventViewFactory);

        internal override string FactoryMethod => "Lastevent";

        internal override void Assign(
            CodegenMethod method,
            CodegenExpressionRef factory,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
        }

        public override string ViewName => NAME;

        public override AppliesTo AppliesTo()
        {
            return client.annotation.AppliesTo.WINDOW_LASTEVENT;
        }

        public override T Accept<T>(ViewFactoryForgeVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
} // end of namespace