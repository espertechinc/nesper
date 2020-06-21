///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.statement.multimatch;
using com.espertech.esper.compat.threading.locks;

namespace com.espertech.esper.common.@internal.context.util
{
    public class EPStatementAgentInstanceHandle
    {
        private readonly int hashCode;

        public EPStatementAgentInstanceHandle(
            EPStatementHandle statementHandle,
            int agentInstanceId,
            IReaderWriterLock statementAgentInstanceLock)
        {
            StatementHandle = statementHandle;
            AgentInstanceId = agentInstanceId;
            StatementAgentInstanceLock = statementAgentInstanceLock;
            hashCode = 31 * statementHandle.GetHashCode() + agentInstanceId;
        }

        public IReaderWriterLock StatementAgentInstanceLock { get; }
        //public StatementAgentInstanceLock StatementAgentInstanceLock { get; }

        public int AgentInstanceId { get; }

        public bool IsCanSelfJoin => StatementHandle.IsCanSelfJoin;

        public int Priority => StatementHandle.Priority;

        public bool IsPreemptive => StatementHandle.IsPreemptive;

        public bool HasVariables => StatementHandle.HasVariables;

        public bool HasTableAccess => StatementHandle.HasTableAccess;

        public StatementAgentInstanceFilterVersion StatementFilterVersion { get; } =
            new StatementAgentInstanceFilterVersion();

        public EPStatementDispatch OptionalDispatchable { get; set; }

        public bool IsDestroyed { get; set; }

        public FilterFaultHandler FilterFaultHandler { get; set; }

        public int StatementId => StatementHandle.StatementId;

        public MultiMatchHandler MultiMatchHandler => StatementHandle.MultiMatchHandler;

        public EPStatementHandle StatementHandle { get; }

        /// <summary>
        ///     Tests filter version.
        /// </summary>
        /// <param name="filterVersion">to test</param>
        /// <returns>indicator whether version is up-to-date</returns>
        public bool IsCurrentFilter(long filterVersion)
        {
            return StatementFilterVersion.IsCurrentFilter(filterVersion);
        }

        public override bool Equals(object otherObj)
        {
            if (this == otherObj) {
                return true;
            }

            if (!(otherObj is EPStatementAgentInstanceHandle)) {
                return false;
            }

            var other = (EPStatementAgentInstanceHandle) otherObj;
            return other.StatementHandle.StatementId == StatementHandle.StatementId &&
                   other.AgentInstanceId == AgentInstanceId;
        }

        public override int GetHashCode()
        {
            return hashCode;
        }

        /// <summary>
        ///     Invoked by runtime to indicate that a statements's
        ///     filer and schedule processing is done, and now it's time to process join results.
        /// </summary>
        public void InternalDispatch()
        {
            OptionalDispatchable?.Execute();
        }

        public override string ToString()
        {
            return "EPStatementAgentInstanceHandle{" +
                   "name=" +
                   StatementHandle.StatementName +
                   " id=" +
                   AgentInstanceId +
                   '}';
        }
    }
} // end of namespace