///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.regressionlib.support.extend.view
{
    public class MyFlushedSimpleViewForge : ViewFactoryForge
    {
        public void SetViewParameters(
            IList<ExprNode> parameters,
            ViewForgeEnv viewForgeEnv,
            int streamNumber)
        {
        }

        public IList<ViewFactoryForge> InnerForges =>
            EmptyList<ViewFactoryForge>.Instance;

        public IList<StmtClassForgeableFactory> InitAdditionalForgeables(ViewForgeEnv viewForgeEnv) =>
            EmptyList<StmtClassForgeableFactory>.Instance;

        public void Attach(
            EventType parentEventType,
            ViewForgeEnv viewForgeEnv)
        {
            EventType = parentEventType;
        }

        public EventType EventType { get; private set; }

        public string ViewName => "flushed";

        public CodegenExpression Make(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            return new SAIFFInitializeBuilder(
                    typeof(MyFlushedSimpleViewFactory),
                    GetType(),
                    "factory",
                    parent,
                    symbols,
                    classScope)
                .Eventtype("eventType", EventType)
                .Build();
        }

        public T Accept<T>(ViewFactoryForgeVisitor<T> visitor)
        {
            return visitor.VisitExtension(this);
        }
    }
} // end of namespace