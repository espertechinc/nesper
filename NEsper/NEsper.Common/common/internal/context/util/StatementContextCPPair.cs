///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.context.util
{
    public class StatementContextCPPair
    {
        public StatementContextCPPair(
            int statementId,
            int agentInstanceId,
            StatementContext optionalStatementContext)
        {
            StatementId = statementId;
            AgentInstanceId = agentInstanceId;
            OptionalStatementContext = optionalStatementContext;
        }

        public int StatementId { get; private set; }

        public int AgentInstanceId { get; private set; }

        public StatementContext OptionalStatementContext { get; private set; }

        protected bool Equals(StatementContextCPPair other)
        {
            return AgentInstanceId == other.AgentInstanceId && StatementId == other.StatementId;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != this.GetType())
                return false;
            return Equals((StatementContextCPPair) obj);
        }

        public override int GetHashCode()
        {
            unchecked {
                return (AgentInstanceId * 397) ^ StatementId;
            }
        }
    }
} // end of namespace