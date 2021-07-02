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
using com.espertech.esper.common.@internal.epl.index.@base;

namespace com.espertech.esper.common.@internal.epl.index.inkeyword
{
    public class PropertyHashedArrayFactoryFactory : EventTableFactoryFactory
    {
        private readonly int _streamNum;
        private readonly string[] _propertyNames;
        private readonly Type[] _propertyTypes;
        private readonly DataInputOutputSerde[] _propertySerdes;
        private readonly bool _unique;
        private readonly EventPropertyValueGetter[] _propertyGetters;
        private readonly bool _isFireAndForget;
        private readonly StateMgmtSetting _stateMgmtSettings;

        public PropertyHashedArrayFactoryFactory(
            int streamNum,
            string[] propertyNames,
            Type[] propertyTypes,
            DataInputOutputSerde[] propertySerdes,
            bool unique,
            EventPropertyValueGetter[] propertyGetters,
            bool isFireAndForget,
            StateMgmtSetting stateMgmtSettings)
        {
            _streamNum = streamNum;
            _propertyNames = propertyNames;
            _propertyTypes = propertyTypes;
            _propertySerdes = propertySerdes;
            _unique = unique;
            _propertyGetters = propertyGetters;
            _isFireAndForget = isFireAndForget;
            _stateMgmtSettings = stateMgmtSettings;
        }

        public EventTableFactory Create(EventType eventType, EventTableFactoryFactoryContext eventTableFactoryContext)
        {
            return eventTableFactoryContext.EventTableIndexService.CreateInArray(
                _streamNum,
                eventType,
                _propertyNames,
                _propertyTypes,
                _propertySerdes,
                _unique,
                _propertyGetters,
                _isFireAndForget,
                eventTableFactoryContext,
                _stateMgmtSettings);
        }
    }
} // end of namespace