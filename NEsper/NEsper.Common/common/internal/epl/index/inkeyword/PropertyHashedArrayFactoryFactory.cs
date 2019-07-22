///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.index.@base;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.index.inkeyword
{
    public class PropertyHashedArrayFactoryFactory : EventTableFactoryFactory
    {
        internal readonly int streamNum;
        internal readonly string[] propertyNames;
        internal readonly Type[] propertyTypes;
        internal readonly bool unique;
        internal readonly EventPropertyValueGetter[] propertyGetters;
        internal readonly bool isFireAndForget;

        public PropertyHashedArrayFactoryFactory(
            int streamNum,
            string[] propertyNames,
            Type[] propertyTypes,
            bool unique,
            EventPropertyValueGetter[] propertyGetters,
            bool isFireAndForget)
        {
            this.streamNum = streamNum;
            this.propertyNames = propertyNames;
            this.propertyTypes = propertyTypes;
            this.unique = unique;
            this.propertyGetters = propertyGetters;
            this.isFireAndForget = isFireAndForget;
        }

        public EventTableFactory Create(
            EventType eventType,
            StatementContext statementContext)
        {
            return statementContext.EventTableIndexService.CreateInArray(
                streamNum,
                eventType,
                propertyNames,
                propertyTypes,
                unique,
                propertyGetters,
                isFireAndForget,
                statementContext);
        }
    }
} // end of namespace