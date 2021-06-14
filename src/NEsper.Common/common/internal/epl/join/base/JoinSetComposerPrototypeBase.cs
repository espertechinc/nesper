///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.view.core;

namespace com.espertech.esper.common.@internal.epl.join.@base
{
    public abstract class JoinSetComposerPrototypeBase : JoinSetComposerPrototype
    {
        protected internal bool isOuterJoins;
        protected internal ExprEvaluator postJoinFilterEvaluator;
        protected internal EventType[] streamTypes;

        public bool OuterJoins {
            set => isOuterJoins = value;
        }

        public EventType[] StreamTypes {
            set => streamTypes = value;
        }

        public ExprEvaluator PostJoinFilterEvaluator {
            set => postJoinFilterEvaluator = value;
        }

        public abstract JoinSetComposerDesc Create(
            Viewable[] streamViews,
            bool isFireAndForget,
            AgentInstanceContext agentInstanceContext,
            bool isRecoveringResilient);
    }
} // end of namespace