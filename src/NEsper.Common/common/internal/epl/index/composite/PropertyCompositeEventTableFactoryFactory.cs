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
using com.espertech.esper.common.@internal.epl.index.@base;

namespace com.espertech.esper.common.@internal.epl.index.composite
{
    public class PropertyCompositeEventTableFactoryFactory : EventTableFactoryFactory
    {
        private readonly int _indexedStreamNum;
        private readonly bool _isFireAndForget;
        private readonly EventPropertyValueGetter _keyGetter;
        private readonly string[] _keyProps;
        private readonly Type[] _keyTypes;
        private readonly DataInputOutputSerde _keySerde;
        private readonly EventPropertyValueGetter[] _rangeGetters;
        private readonly string[] _rangeProps;
        private readonly Type[] _rangeTypes;
        private readonly DataInputOutputSerde[] _rangeKeySerdes;
        private readonly int? _subqueryNum;

        public PropertyCompositeEventTableFactoryFactory(
            int indexedStreamNum,
            int? subqueryNum,
            bool isFireAndForget,
            string[] keyProps,
            Type[] keyTypes,
            EventPropertyValueGetter keyGetter,
            DataInputOutputSerde keySerde,
            string[] rangeProps,
            Type[] rangeTypes,
            EventPropertyValueGetter[] rangeGetters,
            DataInputOutputSerde[] rangeKeySerdes)
        {
            _indexedStreamNum = indexedStreamNum;
            _subqueryNum = subqueryNum;
            _isFireAndForget = isFireAndForget;
            _keyProps = keyProps;
            _keyTypes = keyTypes;
            _keyGetter = keyGetter;
            _keySerde = keySerde;
            _rangeProps = rangeProps;
            _rangeTypes = rangeTypes;
            _rangeGetters = rangeGetters;
            _rangeKeySerdes = rangeKeySerdes;
        }

        public EventTableFactory Create(
            EventType eventType,
            EventTableFactoryFactoryContext eventTableFactoryContext)
        {
            return eventTableFactoryContext.EventTableIndexService.CreateComposite(
                _indexedStreamNum,
                eventType,
                _keyProps,
                _keyTypes,
                _keyGetter,
                null,
                _keySerde,
                _rangeProps,
                _rangeTypes,
                _rangeGetters,
                _rangeKeySerdes,
                null,
                _isFireAndForget);
        }
    }
} // end of namespace