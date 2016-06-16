///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat.threading;
using com.espertech.esper.core.service;
using com.espertech.esper.core.service.multimatch;
using com.espertech.esper.filter;

namespace com.espertech.esper.core.context.util
{
    [Serializable]
    public class EPStatementAgentInstanceHandle
    {
        private readonly int _hashCode;
        private IReaderWriterLock _statementAgentInstanceLock = null;
        private EPStatementDispatch _optionalDispatchable;

        public EPStatementAgentInstanceHandle(EPStatementHandle statementHandle, IReaderWriterLock statementAgentInstanceLock, int agentInstanceId, StatementAgentInstanceFilterVersion statementFilterVersion, FilterFaultHandlerFactory filterFaultHandlerFactory)
        {
            StatementHandle = statementHandle;
            _statementAgentInstanceLock = statementAgentInstanceLock;
            AgentInstanceId = agentInstanceId;
            _hashCode = 31 * statementHandle.GetHashCode() + agentInstanceId;
            StatementFilterVersion = statementFilterVersion;
            if (filterFaultHandlerFactory != null)
            {
                FilterFaultHandler = filterFaultHandlerFactory.MakeFilterFaultHandler();
            }
        }

        public EPStatementHandle StatementHandle { get; private set; }

        public IReaderWriterLock StatementAgentInstanceLock
        {
            get { return _statementAgentInstanceLock; }
            set { _statementAgentInstanceLock = value; }
        }

        public int AgentInstanceId { get; private set; }

        public int Priority
        {
            get { return StatementHandle.Priority; }
        }

        public bool IsPreemptive
        {
            get { return StatementHandle.IsPreemptive; }
        }

        public bool HasVariables
        {
            get { return StatementHandle.HasVariables; }
        }

        public bool HasTableAccess
        {
            get { return StatementHandle.HasTableAccess; }
        }

        public bool CanSelfJoin
        {
            get { return StatementHandle.IsCanSelfJoin; }
        }

        public StatementAgentInstanceFilterVersion StatementFilterVersion { get; private set; }

        /// <summary>Tests filter version. </summary>
        /// <param name="filterVersion">to test</param>
        /// <returns>indicator whether version is up-to-date</returns>
        public bool IsCurrentFilter(long filterVersion) {
            return StatementFilterVersion.IsCurrentFilter(filterVersion);
        }
    
        public override bool Equals(Object otherObj)
        {
            if (this == otherObj) {
                return true;
            }
    
            if (!(otherObj is EPStatementAgentInstanceHandle)) {
                return false;
            }
    
            var other = (EPStatementAgentInstanceHandle) otherObj;
            return (other.StatementHandle.StatementId == StatementHandle.StatementId) && (other.AgentInstanceId == AgentInstanceId);
        }
    
        public override int GetHashCode()
        {
            return _hashCode;
        }

        /// <summary>
        /// Provides a callback for use when statement processing for filters and schedules is done, for use by join 
        /// statements that require an explicit indicator that all joined streams results have been processed.
        /// </summary>
        /// <value>
        /// is the instance for calling onto after statement callback processing
        /// </value>
        public EPStatementDispatch OptionalDispatchable
        {
            get { return _optionalDispatchable; }
            set { _optionalDispatchable = value; }
        }

        /// <summary>
        /// Invoked by <seealso cref="com.espertech.esper.client.EPRuntime" /> to indicate that a statements's filer 
        /// and schedule processing is done, and now it's time to process join results.
        /// </summary>
        public void InternalDispatch()
        {
            if (_optionalDispatchable != null)
            {
                _optionalDispatchable.Execute();
            }
        }

        public bool IsDestroyed { get; set; }

        public override String ToString()
        {
            return "EPStatementAgentInstanceHandle{" +
                    "name=" + StatementHandle.StatementName +
                    "}";
        }

        public FilterFaultHandler FilterFaultHandler { get; set; }

        public int StatementId
        {
            get { return StatementHandle.StatementId; }
        }

        public MultiMatchHandler MultiMatchHandler
        {
            get { return StatementHandle.MultiMatchHandler; }
        }
    }
}
