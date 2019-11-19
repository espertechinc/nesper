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

namespace com.espertech.esper.common.@internal.epl.index.hash
{
    public class PropertyHashedFactoryFactory : EventTableFactoryFactoryBase
    {
        private readonly string[] indexProps;
        private readonly Type[] indexTypes;
        private readonly bool unique;
        private readonly EventPropertyValueGetter valueGetter;

        public PropertyHashedFactoryFactory(
            int indexedStreamNum,
            int? subqueryNum,
            object optionalSerde,
            bool isFireAndForget,
            string[] indexProps,
            Type[] indexTypes,
            bool unique,
            EventPropertyValueGetter valueGetter)
            : base(indexedStreamNum, subqueryNum, optionalSerde, isFireAndForget)
        {
            this.indexProps = indexProps;
            this.indexTypes = indexTypes;
            this.unique = unique;
            this.valueGetter = valueGetter;
        }

        public override EventTableFactory Create(
            EventType eventType,
            StatementContext statementContext)
        {
            return statementContext.EventTableIndexService.CreateHashedOnly(
                indexedStreamNum,
                eventType,
                indexProps,
                indexTypes,
                unique,
                null,
                valueGetter,
                optionalSerde,
                isFireAndForget,
                statementContext);
        }
    }
} // end of namespace