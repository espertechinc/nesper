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
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.epl.index.@base;

namespace com.espertech.esper.common.@internal.epl.index.hash
{
    public class PropertyHashedFactoryFactory : EventTableFactoryFactoryBase
    {
        private readonly string[] _indexProps;
        private readonly bool _unique;
        private readonly EventPropertyValueGetter _valueGetter;
        private readonly MultiKeyFromObjectArray _transformFireAndForget;
        private readonly DataInputOutputSerde _keySerde;
        private readonly StateMgmtSetting _stateMgmtSettings;

        public PropertyHashedFactoryFactory(
            int indexedStreamNum,
            int? subqueryNum,
            bool isFireAndForget,
            string[] indexProps,
            bool unique,
            EventPropertyValueGetter valueGetter,
            MultiKeyFromObjectArray transformFireAndForget,
            DataInputOutputSerde keySerde,
            StateMgmtSetting stateMgmtSettings)
            : base(indexedStreamNum, subqueryNum, isFireAndForget)
        {
            _indexProps = indexProps;
            _unique = unique;
            _valueGetter = valueGetter;
            _transformFireAndForget = transformFireAndForget;
            _keySerde = keySerde;
            _stateMgmtSettings = stateMgmtSettings;
        }

        public override EventTableFactory Create(
            EventType eventType,
            EventTableFactoryFactoryContext eventTableFactoryContext)
        {
            return eventTableFactoryContext.EventTableIndexService.CreateHashedOnly(
                indexedStreamNum,
                eventType,
                _indexProps,
                _transformFireAndForget,
                _keySerde,
                _unique,
                null,
                _valueGetter,
                null,
                isFireAndForget,
                _stateMgmtSettings);
        }
    }
} // end of namespace