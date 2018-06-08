///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.client.hook;
using com.espertech.esper.collection;
using com.espertech.esper.compat.logging;
using com.espertech.esper.epl.@join.exec.@base;
using com.espertech.esper.epl.join.plan;
using com.espertech.esper.epl.join.table;
using com.espertech.esper.epl.lookup;
using com.espertech.esper.epl.named;
using com.espertech.esper.epl.spec;
using com.espertech.esper.filter;
using com.espertech.esper.view;

namespace com.espertech.esper.epl.virtualdw
{
    public class VirtualDWViewImpl
        : ViewSupport
        , VirtualDWView
    {
        private static readonly EventTableOrganization TABLE_ORGANIZATION = new EventTableOrganization(
            null, false, false, 0, null, EventTableOrganizationType.VDW);

        private static readonly ILog Log =
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly VirtualDataWindow _dataExternal;
        private readonly EventType _eventType;
        private readonly String _namedWindowName;
        private String _lastAccessedByStatementName;
        private int _lastAccessedByNum;

        public VirtualDWViewImpl(VirtualDataWindow dataExternal, EventType eventType, String namedWindowName)
        {
            _dataExternal = dataExternal;
            _eventType = eventType;
            _namedWindowName = namedWindowName;
            _lastAccessedByStatementName = null;
        }

        public VirtualDataWindow VirtualDataWindow
        {
            get { return _dataExternal; }
        }

        public Pair<IndexMultiKey, EventTable> GetSubordinateQueryDesc(
            bool unique,
            IList<IndexedPropDesc> hashedProps,
            IList<IndexedPropDesc> btreeProps)
        {
            var hashFields = hashedProps
                .Select(hashprop => new VirtualDataWindowLookupFieldDesc(hashprop.IndexPropName, VirtualDataWindowLookupOp.EQUALS, hashprop.CoercionType))
                .ToList();
            var btreeFields = btreeProps
                .Select(btreeprop => new VirtualDataWindowLookupFieldDesc(btreeprop.IndexPropName, null, btreeprop.CoercionType))
                .ToList();
            var eventTable = new VirtualDWEventTable(
                unique, hashFields, btreeFields, TABLE_ORGANIZATION);
            var imk = new IndexMultiKey(unique, hashedProps, btreeProps, null);
            return new Pair<IndexMultiKey, EventTable>(imk, eventTable);
        }

        public SubordTableLookupStrategy GetSubordinateLookupStrategy(
            String accessedByStatementName,
            int accessedByStatementId,
            Attribute[] accessedByStmtAnnotations,
            EventType[] outerStreamTypes,
            IList<SubordPropHashKey> hashKeys,
            CoercionDesc hashKeyCoercionTypes,
            IList<SubordPropRangeKey> rangeKeys,
            CoercionDesc rangeKeyCoercionTypes,
            bool nwOnTrigger,
            EventTable eventTable,
            SubordPropPlan joinDesc,
            bool forceTableScan)
        {
            var noopTable = (VirtualDWEventTable)eventTable;
            for (var i = 0; i < noopTable.BtreeAccess.Count; i++)
            {
                noopTable.BtreeAccess[i].Operator = rangeKeys[i].RangeInfo.RangeType.GetStringOp().FromOpString();
            }

            // allocate a number within the statement
            if (_lastAccessedByStatementName == null || !_lastAccessedByStatementName.Equals(accessedByStatementName))
            {
                _lastAccessedByNum = 0;
            }
            _lastAccessedByNum++;

            var context = new VirtualDataWindowLookupContextSPI(
                accessedByStatementName, accessedByStatementId, accessedByStmtAnnotations, false, _namedWindowName,
                noopTable.HashAccess, noopTable.BtreeAccess, joinDesc, forceTableScan, outerStreamTypes,
                accessedByStatementName, _lastAccessedByNum);
            var index = _dataExternal.GetLookup(context);
            CheckIndex(index);
            return new SubordTableLookupStrategyVirtualDW(
                _namedWindowName, index, hashKeys, hashKeyCoercionTypes, rangeKeys, rangeKeyCoercionTypes, nwOnTrigger,
                outerStreamTypes.Length);
        }

        public EventTable GetJoinIndexTable(QueryPlanIndexItem queryPlanIndexItem)
        {

            IList<VirtualDataWindowLookupFieldDesc> hashFields = new List<VirtualDataWindowLookupFieldDesc>();
            var count = 0;
            if (queryPlanIndexItem.IndexProps != null)
            {
                foreach (var indexProp in queryPlanIndexItem.IndexProps)
                {
                    var coercionType = queryPlanIndexItem.OptIndexCoercionTypes == null
                        ? null
                        : queryPlanIndexItem.OptIndexCoercionTypes[count];
                    hashFields.Add(
                        new VirtualDataWindowLookupFieldDesc(indexProp, VirtualDataWindowLookupOp.EQUALS, coercionType));
                    count++;
                }
            }

            IList<VirtualDataWindowLookupFieldDesc> btreeFields = new List<VirtualDataWindowLookupFieldDesc>();
            count = 0;
            if (queryPlanIndexItem.RangeProps != null)
            {
                foreach (var btreeprop in queryPlanIndexItem.RangeProps)
                {
                    var coercionType = queryPlanIndexItem.OptRangeCoercionTypes == null
                        ? null
                        : queryPlanIndexItem.OptRangeCoercionTypes[count];
                    btreeFields.Add(new VirtualDataWindowLookupFieldDesc(btreeprop, null, coercionType));
                    count++;
                }
            }

            return new VirtualDWEventTable(false, hashFields, btreeFields, TABLE_ORGANIZATION);
        }

        public JoinExecTableLookupStrategy GetJoinLookupStrategy(
            String accessedByStmtName,
            int accessedByStmtId,
            Attribute[] accessedByStmtAnnotations,
            EventTable[] eventTables,
            TableLookupKeyDesc keyDescriptor,
            int lookupStreamNum)
        {
            var noopTable = (VirtualDWEventTable)eventTables[0];
            for (var i = 0; i < noopTable.HashAccess.Count; i++)
            {
                var hashKey = keyDescriptor.Hashes[i];
                noopTable.HashAccess[i].LookupValueType = hashKey.KeyExpr.ExprEvaluator.ReturnType;
            }
            for (var i = 0; i < noopTable.BtreeAccess.Count; i++)
            {
                var range = keyDescriptor.Ranges[i];
                var op = range.RangeType.GetStringOp().FromOpString();
                var rangeField = noopTable.BtreeAccess[i];
                rangeField.Operator = op;
                if (range is QueryGraphValueEntryRangeRelOp)
                {
                    rangeField.LookupValueType =
                        ((QueryGraphValueEntryRangeRelOp)range).Expression.ExprEvaluator.ReturnType;
                }
                else
                {
                    rangeField.LookupValueType =
                        ((QueryGraphValueEntryRangeIn)range).ExprStart.ExprEvaluator.ReturnType;
                }
            }

            var index =
                _dataExternal.GetLookup(
                    new VirtualDataWindowLookupContext(
                        accessedByStmtName, accessedByStmtId, accessedByStmtAnnotations, false, _namedWindowName,
                        noopTable.HashAccess, noopTable.BtreeAccess));
            CheckIndex(index);
            return new JoinExecTableLookupStrategyVirtualDW(_namedWindowName, index, keyDescriptor, lookupStreamNum);
        }

        public Pair<IndexMultiKey, EventTable> GetFireAndForgetDesc(
            ISet<String> keysAvailable,
            ISet<String> rangesAvailable)
        {
            IList<VirtualDataWindowLookupFieldDesc> hashFields = new List<VirtualDataWindowLookupFieldDesc>();
            IList<IndexedPropDesc> hashIndexedFields = new List<IndexedPropDesc>();
            foreach (var hashprop in keysAvailable)
            {
                hashFields.Add(new VirtualDataWindowLookupFieldDesc(hashprop, VirtualDataWindowLookupOp.EQUALS, null));
                hashIndexedFields.Add(new IndexedPropDesc(hashprop, _eventType.GetPropertyType(hashprop)));
            }

            IList<VirtualDataWindowLookupFieldDesc> btreeFields = new List<VirtualDataWindowLookupFieldDesc>();
            IList<IndexedPropDesc> btreeIndexedFields = new List<IndexedPropDesc>();
            foreach (var btreeprop in rangesAvailable)
            {
                btreeFields.Add(new VirtualDataWindowLookupFieldDesc(btreeprop, null, null));
                btreeIndexedFields.Add(new IndexedPropDesc(btreeprop, _eventType.GetPropertyType(btreeprop)));
            }

            var noopTable = new VirtualDWEventTable(false, hashFields, btreeFields, TABLE_ORGANIZATION);
            var imk = new IndexMultiKey(false, hashIndexedFields, btreeIndexedFields, null);

            return new Pair<IndexMultiKey, EventTable>(imk, noopTable);
        }

        public ICollection<EventBean> GetFireAndForgetData(
            EventTable eventTable,
            Object[] keyValues,
            RangeIndexLookupValue[] rangeValues,
            Attribute[] annotations)
        {
            var noopTable = (VirtualDWEventTable)eventTable;
            for (var i = 0; i < noopTable.BtreeAccess.Count; i++)
            {
                var range = (RangeIndexLookupValueRange)rangeValues[i];
                var op = range.Operator.GetStringOp().FromOpString();
                noopTable.BtreeAccess[i].Operator = op;
            }

            var keys = new Object[keyValues.Length + rangeValues.Length];
            for (var i = 0; i < keyValues.Length; i++)
            {
                keys[i] = keyValues[i];
                noopTable.HashAccess[i].LookupValueType = keyValues[i] == null ? null : keyValues[i].GetType();
            }
            var offset = keyValues.Length;
            for (var j = 0; j < rangeValues.Length; j++)
            {
                var rangeValue = rangeValues[j].Value;
                if (rangeValue is Range)
                {
                    var range = (Range)rangeValue;
                    keys[j + offset] = new VirtualDataWindowKeyRange(range.LowEndpoint, range.HighEndpoint);
                    noopTable.BtreeAccess[j].LookupValueType = range.LowEndpoint == null
                        ? null
                        : range.LowEndpoint.GetType();
                }
                else
                {
                    keys[j + offset] = rangeValue;
                    noopTable.BtreeAccess[j].LookupValueType = rangeValue == null ? null : rangeValue.GetType();
                }
            }

            var index =
                _dataExternal.GetLookup(
                    new VirtualDataWindowLookupContext(
                        null, -1, annotations, true, _namedWindowName, noopTable.HashAccess, noopTable.BtreeAccess));
            CheckIndex(index);
            if (index == null)
            {
                throw new EPException("Exception obtaining index from virtual data window '" + _namedWindowName + "'");
            }

            ISet<EventBean> events = null;
            try
            {
                events = index.Lookup(keys, null);
            }
            catch (Exception ex)
            {
                Log.Warn(
                    "Exception encountered invoking virtual data window external index for window '" + _namedWindowName +
                    "': " + ex.Message, ex);
            }
            return events;
        }

        private void CheckIndex(VirtualDataWindowLookup index)
        {
            if (index == null)
            {
                throw new EPException(
                    "Exception obtaining index lookup from virtual data window, the implementation has returned a null index");
            }
        }

        public override void Update(EventBean[] newData, EventBean[] oldData)
        {
            _dataExternal.Update(newData, oldData);
        }

        public override EventType EventType
        {
            get { return _eventType; }
        }

        public void Dispose()
        {
            _dataExternal.Dispose();
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            return _dataExternal.GetEnumerator();
        }

        public void HandleStartIndex(CreateIndexDesc spec)
        {
            try
            {
                var fields = spec.Columns
                    .Select(col => new VirtualDataWindowEventStartIndex.VDWCreateIndexField(col.Expressions, col.IndexType, col.Parameters))
                    .ToList();
                var create = new VirtualDataWindowEventStartIndex(
                    spec.WindowName, spec.IndexName, fields, spec.IsUnique);
                _dataExternal.HandleEvent(create);
            }
            catch (Exception ex)
            {
                var message =
                    "Exception encountered invoking virtual data window handle start-index event for window '" +
                    _namedWindowName + "': " + ex.Message;
                Log.Warn(message, ex);
                throw new EPException(message, ex);
            }
        }

        public void HandleStopIndex(CreateIndexDesc spec)
        {
            try
            {
                var theEvent = new VirtualDataWindowEventStopIndex(
                    spec.WindowName, spec.IndexName);
                _dataExternal.HandleEvent(theEvent);
            }
            catch (Exception ex)
            {
                var message =
                    "Exception encountered invoking virtual data window handle stop-index event for window '" +
                    _namedWindowName + "': " + ex.Message;
                Log.Warn(message, ex);
            }
        }

        public void HandleStopWindow()
        {
            try
            {
                var theEvent = new VirtualDataWindowEventStopWindow(_namedWindowName);
                _dataExternal.HandleEvent(theEvent);
            }
            catch (Exception ex)
            {
                var message =
                    "Exception encountered invoking virtual data window handle stop-window event for window '" +
                    _namedWindowName + "': " + ex.Message;
                Log.Warn(message, ex);
            }
        }
    }
}
