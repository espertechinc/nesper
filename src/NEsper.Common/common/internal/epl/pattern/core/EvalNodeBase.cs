///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.epl.pattern.core
{
    public abstract class EvalNodeBase : EvalNode
    {
        protected EvalNodeBase(PatternAgentInstanceContext context)
        {
            Context = context;
        }

        public PatternAgentInstanceContext Context { get; }

        public abstract EvalStateNode NewState(Evaluator parentNode);
    }
} // end of namespace