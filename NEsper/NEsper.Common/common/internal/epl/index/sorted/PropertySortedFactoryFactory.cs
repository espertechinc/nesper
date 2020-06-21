///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.serde;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.index.@base;
using com.espertech.esper.common.@internal.epl.subselect;

namespace com.espertech.esper.common.@internal.epl.index.sorted
{
    public class PropertySortedFactoryFactory : EventTableFactoryFactoryBase
    {
        private readonly string _indexProp;
        private readonly Type _indexType;
        private readonly EventPropertyValueGetter _valueGetter;
        private readonly DataInputOutputSerde<object> _indexSerde;

        public PropertySortedFactoryFactory(
            int indexedStreamNum,
            int? subqueryNum,
            bool isFireAndForget,
            string indexProp,
            Type indexType,
            EventPropertyValueGetter valueGetter,
            DataInputOutputSerde<object> indexSerde)
            : base(indexedStreamNum, subqueryNum, isFireAndForget)
        {
            _indexProp = indexProp;
            _indexType = indexType;
            _valueGetter = valueGetter;
            _indexSerde = indexSerde;
        }

        public override EventTableFactory Create(
            EventType eventType,
            EventTableFactoryFactoryContext eventTableFactoryContext)
        {
            return eventTableFactoryContext.EventTableIndexService.CreateSorted(
                indexedStreamNum,
                eventType,
                _indexProp,
                _indexType,
                _valueGetter,
                _indexSerde,
                null,
                isFireAndForget,
                eventTableFactoryContext);
        }
    }
} // end of namespace