///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.table.strategy;
using com.espertech.esper.events;
using com.espertech.esper.events.arr;

namespace com.espertech.esper.epl.table.mgmt
{
    public class TableMetadataInternalEventToPublic
    {
        private readonly ObjectArrayEventType _publicEventType;
        private readonly TableMetadataColumnPairPlainCol[] _plains;
        private readonly TableMetadataColumnPairAggMethod[] _methods;
        private readonly TableMetadataColumnPairAggAccess[] _accessors;
        private readonly EventAdapterService _eventAdapterService;
        private readonly int _numColumns;
    
        public TableMetadataInternalEventToPublic(ObjectArrayEventType publicEventType, TableMetadataColumnPairPlainCol[] plains, TableMetadataColumnPairAggMethod[] methods, TableMetadataColumnPairAggAccess[] accessors, EventAdapterService eventAdapterService)
        {
            _publicEventType = publicEventType;
            _plains = plains;
            _methods = methods;
            _accessors = accessors;
            _eventAdapterService = eventAdapterService;
            _numColumns = publicEventType.PropertyDescriptors.Count;
        }
    
        public EventBean Convert(EventBean @event, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
        {
            var data = ConvertToUnd(@event, eventsPerStream, isNewData, context);
            return _eventAdapterService.AdapterForType(data, _publicEventType);
        }
    
        public object[] ConvertToUnd(EventBean @event, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
        {
            var bean = (ObjectArrayBackedEventBean) @event;
            var row = ExprTableEvalStrategyUtil.GetRow(bean);
            var data = new object[_numColumns];
            foreach (var plain in _plains) {
                data[plain.Dest] = bean.Properties[plain.Source];
            }
            var count = 0;
            foreach (var access in _accessors) {
                data[access.Dest] = access.Accessor.GetValue(row.States[count++], eventsPerStream, isNewData, context);
            }
            count = 0;
            foreach (var method in _methods) {
                data[method.Dest] = row.Methods[count++].Value;
            }
            return data;
        }
    }
}
