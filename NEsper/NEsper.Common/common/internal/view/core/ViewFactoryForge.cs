///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.util;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.view.core
{
    public interface ViewFactoryForge : CodegenMakeable
    {
        void SetViewParameters(
            IList<ExprNode> parameters,
            ViewForgeEnv viewForgeEnv,
            int streamNumber);

        void Attach(
            EventType parentEventType,
            int streamNumber,
            ViewForgeEnv viewForgeEnv);

        EventType EventType { get; }

        string ViewName { get; }

        void Accept(ViewForgeVisitor visitor);

        IList<StmtClassForgeableFactory> InitAdditionalForgeables(ViewForgeEnv viewForgeEnv);

        IList<ViewFactoryForge> InnerForges { get; }

#if FALSE // MIXIN
        default void accept(ViewForgeVisitor visitor)
        {
            visitor.visit(this);
        }
#endif
    }
}