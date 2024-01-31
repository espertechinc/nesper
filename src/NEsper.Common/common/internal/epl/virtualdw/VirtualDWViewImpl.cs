///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.hook.vdw;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.index.@base;
using com.espertech.esper.common.@internal.epl.join.exec.@base;
using com.espertech.esper.common.@internal.epl.join.exec.util;
using com.espertech.esper.common.@internal.epl.join.lookup;
using com.espertech.esper.common.@internal.epl.join.querygraph;
using com.espertech.esper.common.@internal.epl.join.queryplan;
using com.espertech.esper.common.@internal.epl.lookup;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

using Range = com.espertech.esper.common.@internal.filterspec.Range;

namespace com.espertech.esper.common.@internal.epl.virtualdw
{
    public class VirtualDWViewImpl : ViewSupport,
        VirtualDWView
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly EventTableOrganization TABLE_ORGANIZATION =
            new EventTableOrganization(null, false, false, 0, null, EventTableOrganizationType.VDW);

        private readonly VirtualDWViewFactory _factory;
        private readonly AgentInstanceContext _agentInstanceContext;
        private readonly VirtualDataWindow _dataExternal;
#pragma warning disable 649
        private string _lastAccessedByDeploymentId;
        private string _lastAccessedByStatementName;
#pragma warning restore 649
        private int _lastAccessedByNum;

        public VirtualDWViewImpl(
            VirtualDWViewFactory factory,
            AgentInstanceContext agentInstanceContext,
            VirtualDataWindow dataExternal)
        {
            _factory = factory;
            _agentInstanceContext = agentInstanceContext;
            _dataExternal = dataExternal;
        }

        public VirtualDataWindow VirtualDataWindow => _dataExternal;

        public override void Update(
            EventBean[] newData,
            EventBean[] oldData)
        {
            _dataExternal.Update(newData, oldData);
        }

        public override EventType EventType => _factory.EventType;

        public override IEnumerator<EventBean> GetEnumerator()
        {
            return _dataExternal.GetEnumerator();
        }

        public void Destroy()
        {
            _dataExternal.Dispose();
        }

        public SubordTableLookupStrategy GetSubordinateLookupStrategy(
            SubordTableLookupStrategyFactoryVDW subordTableFactory,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var tableVW = VirtualDWQueryPlanUtil.GetSubordinateQueryDesc(
                false,
                subordTableFactory.IndexHashedProps,
                subordTableFactory.IndexBtreeProps);
            var noopTable = tableVW.Second;
            for (var i = 0; i < noopTable.BtreeAccess.Count; i++) {
                var opRange = subordTableFactory.RangeEvals[i].Type.StringOp();
                var op = opRange.FromOpString();
                noopTable.BtreeAccess[i].Operator = op;
            }

            // allocate a number within the statement
            if (_lastAccessedByStatementName == null ||
                !_lastAccessedByDeploymentId.Equals(exprEvaluatorContext.DeploymentId) ||
                !_lastAccessedByStatementName.Equals(exprEvaluatorContext.StatementName)) {
                _lastAccessedByNum = 0;
            }

            _lastAccessedByNum++;

            var context = new VirtualDataWindowLookupContextSPI(
                exprEvaluatorContext.DeploymentId,
                exprEvaluatorContext.StatementName,
                exprEvaluatorContext.StatementId,
                exprEvaluatorContext.Annotations,
                false,
                _factory.NamedWindowName,
                noopTable.HashAccess,
                noopTable.BtreeAccess,
                _lastAccessedByNum);
            VirtualDataWindowLookup index;
            try {
                index = _dataExternal.GetLookup(context);
            }
            catch (Exception t) {
                throw new EPException(
                    "Failed to obtain lookup for virtual data window '" + _factory.NamedWindowName + "': " + t.Message,
                    t);
            }

            return new SubordTableLookupStrategyVDW(_factory, subordTableFactory, index);
        }

        public JoinExecTableLookupStrategy GetJoinLookupStrategy(
            TableLookupPlan tableLookupPlan,
            AgentInstanceContext agentInstanceContext,
            EventTable[] eventTables,
            int lookupStream)
        {
            var noopTable = (VirtualDWEventTable)eventTables[0];
            for (var i = 0; i < noopTable.HashAccess.Count; i++) {
                var hashKeyType = tableLookupPlan.VirtualDWHashTypes[i];
                noopTable.HashAccess[i].LookupValueType = hashKeyType;
            }

            for (var i = 0; i < noopTable.BtreeAccess.Count; i++) {
                var range = tableLookupPlan.VirtualDWRangeEvals[i];
                var op = range.Type.StringOp().FromOpString();
                var rangeField = noopTable.BtreeAccess[i];
                rangeField.Operator = op;
                rangeField.LookupValueType = tableLookupPlan.VirtualDWRangeTypes[i];
            }

            var index = _dataExternal.GetLookup(
                new VirtualDataWindowLookupContext(
                    agentInstanceContext.DeploymentId,
                    agentInstanceContext.StatementName,
                    agentInstanceContext.StatementId,
                    agentInstanceContext.Annotations,
                    false,
                    _factory.NamedWindowName,
                    noopTable.HashAccess,
                    noopTable.BtreeAccess));
            CheckIndex(index);
            return new JoinExecTableLookupStrategyVirtualDW(_factory.NamedWindowName, index, tableLookupPlan);
        }

        private void CheckIndex(VirtualDataWindowLookup index)
        {
            if (index == null) {
                throw new EPException(
                    "Exception obtaining index lookup from virtual data window, the implementation has returned a null index");
            }
        }

        public ICollection<EventBean> GetFireAndForgetData(
            EventTable eventTable,
            object[] keyValues,
            RangeIndexLookupValue[] rangeValues,
            Attribute[] annotations)
        {
            var noopTable = (VirtualDWEventTable)eventTable;
            for (var i = 0; i < noopTable.BtreeAccess.Count; i++) {
                var range = (RangeIndexLookupValueRange)rangeValues[i];
                var op = range.Operator.StringOp().FromOpString();
                noopTable.BtreeAccess[i].Operator = op;
            }

            var keys = new object[keyValues.Length + rangeValues.Length];
            for (var i = 0; i < keyValues.Length; i++) {
                keys[i] = keyValues[i];
                noopTable.HashAccess[i].LookupValueType = keyValues[i] == null ? null : keyValues[i].GetType();
            }

            var offset = keyValues.Length;
            for (var j = 0; j < rangeValues.Length; j++) {
                var rangeValue = rangeValues[j].Value;
                if (rangeValue is Range range) {
                    keys[j + offset] = new VirtualDataWindowKeyRange(range.LowEndpoint, range.HighEndpoint);
                    noopTable.BtreeAccess[j].LookupValueType =
                        range.LowEndpoint?.GetType();
                }
                else {
                    keys[j + offset] = rangeValue;
                    noopTable.BtreeAccess[j].LookupValueType = rangeValue?.GetType();
                }
            }

            var namedWindowName = _factory.NamedWindowName;
            var index = _dataExternal.GetLookup(
                new VirtualDataWindowLookupContext(
                    null,
                    null,
                    -1,
                    annotations,
                    true,
                    namedWindowName,
                    noopTable.HashAccess,
                    noopTable.BtreeAccess));
            CheckIndex(index);
            if (index == null) {
                throw new EPException("Exception obtaining index from virtual data window '" + namedWindowName + "'");
            }

            ISet<EventBean> events = null;
            try {
                events = index.Lookup(keys, null);
            }
            catch (EPException) {
                throw;
            }
            catch (Exception ex) {
                Log.Warn(
                    "Exception encountered invoking virtual data window external index for window '" +
                    namedWindowName +
                    "': " +
                    ex.Message,
                    ex);
            }

            return events;
        }

        public void HandleStartIndex(
            string indexName,
            QueryPlanIndexItem explicitIndexDesc)
        {
            try {
                IList<VirtualDataWindowEventStartIndex.VDWCreateIndexField> fields =
                    new List<VirtualDataWindowEventStartIndex.VDWCreateIndexField>();
                foreach (var hash in explicitIndexDesc.HashProps) {
                    fields.Add(new VirtualDataWindowEventStartIndex.VDWCreateIndexField(hash, "hash"));
                }

                foreach (var range in explicitIndexDesc.RangeProps) {
                    fields.Add(new VirtualDataWindowEventStartIndex.VDWCreateIndexField(range, "btree"));
                }

                var create = new VirtualDataWindowEventStartIndex(
                    _factory.NamedWindowName,
                    indexName,
                    fields,
                    explicitIndexDesc.IsUnique);
                _dataExternal.HandleEvent(create);
            }
            catch (Exception ex) {
                var message =
                    "Exception encountered invoking virtual data window handle start-index event for window '" +
                    _factory.NamedWindowName +
                    "': " +
                    ex.Message;
                Log.Warn(message, ex);
                throw new EPException(message, ex);
            }
        }

        public void HandleStopIndex(
            string indexName,
            QueryPlanIndexItem explicitIndexDesc)
        {
            try {
                var theEvent =
                    new VirtualDataWindowEventStopIndex(_factory.NamedWindowName, indexName);
                _dataExternal.HandleEvent(theEvent);
            }
            catch (Exception ex) {
                var message =
                    "Exception encountered invoking virtual data window handle stop-index event for window '" +
                    _factory.NamedWindowName +
                    "': " +
                    ex.Message;
                Log.Warn(message, ex);
            }
        }

        public void HandleDestroy(int agentInstanceId)
        {
            try {
                var theEvent =
                    new VirtualDataWindowEventStopWindow(_factory.NamedWindowName, agentInstanceId);
                _dataExternal.HandleEvent(theEvent);
            }
            catch (Exception ex) {
                var message =
                    "Exception encountered invoking virtual data window handle stop-window event for window '" +
                    _factory.NamedWindowName +
                    "': " +
                    ex.Message;
                Log.Warn(message, ex);
            }
        }
    }
} // end of namespace