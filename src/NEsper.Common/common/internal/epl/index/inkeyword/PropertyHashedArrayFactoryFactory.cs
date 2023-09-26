///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
        private readonly int streamNum;
        private readonly string[] propertyNames;
        private readonly Type[] propertyTypes;
        private readonly DataInputOutputSerde[] propertySerdes;
        private readonly bool unique;
        private readonly EventPropertyValueGetter[] propertyGetters;
        private readonly bool isFireAndForget;
        private readonly StateMgmtSetting stateMgmtSettings;

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
            this.streamNum = streamNum;
            this.propertyNames = propertyNames;
            this.propertyTypes = propertyTypes;
            this.propertySerdes = propertySerdes;
            this.unique = unique;
            this.propertyGetters = propertyGetters;
            this.isFireAndForget = isFireAndForget;
            this.stateMgmtSettings = stateMgmtSettings;
        }

        public EventTableFactory Create(
            EventType eventType,
            EventTableFactoryFactoryContext eventTableFactoryContext)
        {
            return eventTableFactoryContext.EventTableIndexService.CreateInArray(
                streamNum,
                eventType,
                propertyNames,
                propertyTypes,
                propertySerdes,
                unique,
                propertyGetters,
                isFireAndForget,
                stateMgmtSettings);
        }
    }
} // end of namespace