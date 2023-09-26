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
using com.espertech.esper.common.@internal.fabric;

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
            ViewForgeEnv viewForgeEnv);

        EventType EventType { get; }

        string ViewName { get; }

        IList<StmtClassForgeableFactory> InitAdditionalForgeables(ViewForgeEnv viewForgeEnv);

        IList<ViewFactoryForge> InnerForges { get; }

        void AssignStateMgmtSettings(
            FabricCharge fabricCharge,
            ViewForgeEnv viewForgeEnv,
            int[] grouping)
        {
        }

        T Accept<T>(ViewFactoryForgeVisitor<T> visitor);

        void Accept(ViewForgeVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}