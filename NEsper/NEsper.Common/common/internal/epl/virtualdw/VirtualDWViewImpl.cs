///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;
using com.espertech.esper.collection;
using com.espertech.esper.common.client;
using com.espertech.esper.common.client.hook.vdw;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.index.@base;
using com.espertech.esper.common.@internal.epl.@join.exec.@base;
using com.espertech.esper.common.@internal.epl.join.exec.util;
using com.espertech.esper.common.@internal.epl.join.lookup;
using com.espertech.esper.common.@internal.epl.join.querygraph;
using com.espertech.esper.common.@internal.epl.join.queryplan;
using com.espertech.esper.common.@internal.epl.lookup;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.common.@internal.epl.virtualdw
{
    public class VirtualDWViewImpl : ViewSupport,
        VirtualDWView
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly static EventTableOrganization TABLE_ORGANIZATION =
            new EventTableOrganization(null, false, false, 0, null, EventTableOrganizationType.VDW);

        private readonly VirtualDWViewFactory _factory;
        private readonly AgentInstanceContext _agentInstanceContext;
        private readonly VirtualDataWindow _dataExternal;
        private string _lastAccessedByDeploymentId;
        private string _lastAccessedByStatementName;
        private int _lastAccessedByNum;

        public VirtualDWViewImpl(
            VirtualDWViewFactory factory,
            AgentInstanceContext agentInstanceContext,
            VirtualDataWindow dataExternal)
        {
            this._factory = factory;
            this._agentInstanceContext = agentInstanceContext;
            this._dataExternal = dataExternal;
        }

        public VirtualDataWindow VirtualDataWindow {
            get => _dataExternal;
        }

        public override void Update(EventBean[] newData, EventBean[] oldData)
        {
            _dataExternal.Update(newData, oldData);
        }

        public override EventType EventType {
            get => _factory.EventType;
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            return _dataExternal.GetEnumerator();
        }

        public void Destroy()
        {
            _dataExternal.Dispose();
        }

        public SubordTableLookupStrategy GetSubordinateLookupStrategy(
            SubordTableLookupStrategyFactoryVDW subordTableFactory, AgentInstanceContext agentInstanceContext)
        {
            Pair<IndexMultiKey, VirtualDWEventTable> tableVW = VirtualDWQueryPlanUtil.GetSubordinateQueryDesc(
                false, subordTableFactory.IndexHashedProps, subordTableFactory.IndexBtreeProps);
            VirtualDWEventTable noopTable = tableVW.Second;
            for (int i = 0; i < noopTable.BtreeAccess.Count; i++) {
                string opRange = subordTableFactory.RangeEvals[i].Type.StringOp;
                VirtualDataWindowLookupOp op = VirtualDataWindowLookupOpExtensions.FromOpString(opRange);
                noopTable.BtreeAccess[i].Operator = op;
            }

            // allocate a number within the statement
            if (_lastAccessedByStatementName == null ||
                !_lastAccessedByDeploymentId.Equals(agentInstanceContext.DeploymentId) ||
                !_lastAccessedByStatementName.Equals(agentInstanceContext.StatementName)) {
                _lastAccessedByNum = 0;
            }

            _lastAccessedByNum++;

            VirtualDataWindowLookupContextSPI context = new VirtualDataWindowLookupContextSPI(
                agentInstanceContext.DeploymentId, agentInstanceContext.StatementName,
                agentInstanceContext.StatementId, agentInstanceContext.Annotations, false, _factory.NamedWindowName,
                noopTable.HashAccess, noopTable.BtreeAccess, _lastAccessedByNum);
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
            TableLookupPlan tableLookupPlan, AgentInstanceContext agentInstanceContext, EventTable[] eventTables,
            int lookupStream)
        {
            VirtualDWEventTable noopTable = (VirtualDWEventTable) eventTables[0];
            for (int i = 0; i < noopTable.HashAccess.Count; i++) {
                Type hashKeyType = tableLookupPlan.VirtualDWHashTypes[i];
                noopTable.HashAccess[i].LookupValueType = hashKeyType;
            }

            for (int i = 0; i < noopTable.BtreeAccess.Count; i++) {
                QueryGraphValueEntryRange range = tableLookupPlan.VirtualDWRangeEvals[i];
                VirtualDataWindowLookupOp op = VirtualDataWindowLookupOpExtensions.FromOpString(range.Type.StringOp);
                VirtualDataWindowLookupFieldDesc rangeField = noopTable.BtreeAccess[i];
                rangeField.Operator = op;
                rangeField.LookupValueType = tableLookupPlan.VirtualDWRangeTypes[i];
            }

            VirtualDataWindowLookup index = _dataExternal.GetLookup(
                new VirtualDataWindowLookupContext(
                    agentInstanceContext.DeploymentId, agentInstanceContext.StatementName,
                    agentInstanceContext.StatementId, agentInstanceContext.Annotations,
                    false, _factory.NamedWindowName, noopTable.HashAccess, noopTable.BtreeAccess));
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
            EventTable eventTable, object[] keyValues, RangeIndexLookupValue[] rangeValues, Attribute[] annotations)
        {
            VirtualDWEventTable noopTable = (VirtualDWEventTable) eventTable;
            for (int i = 0; i < noopTable.BtreeAccess.Count; i++) {
                RangeIndexLookupValueRange range = (RangeIndexLookupValueRange) rangeValues[i];
                VirtualDataWindowLookupOp op = range.Operator.StringOp.FromOpString();
                noopTable.BtreeAccess[i].Operator = op;
            }

            object[] keys = new object[keyValues.Length + rangeValues.Length];
            for (int i = 0; i < keyValues.Length; i++) {
                keys[i] = keyValues[i];
                noopTable.HashAccess[i].LookupValueType = keyValues[i] == null ? null : keyValues[i].GetType();
            }

            int offset = keyValues.Length;
            for (int j = 0; j < rangeValues.Length; j++) {
                object rangeValue = rangeValues[j].Value;
                if (rangeValue is Range) {
                    Range range = (Range) rangeValue;
                    keys[j + offset] = new VirtualDataWindowKeyRange(range.LowEndpoint, range.HighEndpoint);
                    noopTable.BtreeAccess[j].LookupValueType =
                        range.LowEndpoint == null ? null : range.LowEndpoint.GetType();
                }
                else {
                    keys[j + offset] = rangeValue;
                    noopTable.BtreeAccess[j].LookupValueType = rangeValue == null ? null : rangeValue.GetType();
                }
            }

            string namedWindowName = _factory.NamedWindowName;
            VirtualDataWindowLookup index = _dataExternal.GetLookup(
                new VirtualDataWindowLookupContext(
                    null, null, -1, annotations,
                    true, namedWindowName, noopTable.HashAccess, noopTable.BtreeAccess));
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
                    "Exception encountered invoking virtual data window external index for window '" + namedWindowName +
                    "': " + ex.Message, ex);
            }

            return events;
        }

        public void HandleStartIndex(string indexName, QueryPlanIndexItem explicitIndexDesc)
        {
            try {
                IList<VirtualDataWindowEventStartIndex.VDWCreateIndexField> fields =
                    new List<VirtualDataWindowEventStartIndex.VDWCreateIndexField>();
                foreach (string hash in explicitIndexDesc.HashProps) {
                    fields.Add(new VirtualDataWindowEventStartIndex.VDWCreateIndexField(hash, "hash"));
                }

                foreach (string range in explicitIndexDesc.RangeProps) {
                    fields.Add(new VirtualDataWindowEventStartIndex.VDWCreateIndexField(range, "btree"));
                }

                VirtualDataWindowEventStartIndex create = new VirtualDataWindowEventStartIndex(
                    _factory.NamedWindowName, indexName, fields, explicitIndexDesc.IsUnique);
                _dataExternal.HandleEvent(create);
            }
            catch (Exception ex) {
                string message =
                    "Exception encountered invoking virtual data window handle start-index event for window '" +
                    _factory.NamedWindowName + "': " + ex.Message;
                Log.Warn(message, ex);
                throw new EPException(message, ex);
            }
        }

        public void HandleStopIndex(string indexName, QueryPlanIndexItem explicitIndexDesc)
        {
            try {
                VirtualDataWindowEventStopIndex theEvent =
                    new VirtualDataWindowEventStopIndex(_factory.NamedWindowName, indexName);
                _dataExternal.HandleEvent(theEvent);
            }
            catch (Exception ex) {
                string message =
                    "Exception encountered invoking virtual data window handle stop-index event for window '" +
                    _factory.NamedWindowName + "': " + ex.Message;
                Log.Warn(message, ex);
            }
        }

        public void HandleDestroy(int agentInstanceId)
        {
            try {
                VirtualDataWindowEventStopWindow theEvent =
                    new VirtualDataWindowEventStopWindow(_factory.NamedWindowName, agentInstanceId);
                _dataExternal.HandleEvent(theEvent);
            }
            catch (Exception ex) {
                string message =
                    "Exception encountered invoking virtual data window handle stop-window event for window '" +
                    _factory.NamedWindowName + "': " + ex.Message;
                Log.Warn(message, ex);
            }
        }
    }
} // end of namespace