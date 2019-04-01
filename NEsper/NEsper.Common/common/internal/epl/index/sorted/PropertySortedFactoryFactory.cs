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

namespace com.espertech.esper.common.@internal.epl.index.sorted
{
    public class PropertySortedFactoryFactory : EventTableFactoryFactoryBase
    {
        private readonly string indexProp;
        private readonly Type indexType;
        private readonly EventPropertyValueGetter valueGetter;

        public PropertySortedFactoryFactory(
            int indexedStreamNum,
            int? subqueryNum, 
            object optionalSerde, 
            bool isFireAndForget, 
            string indexProp,
            Type indexType, 
            EventPropertyValueGetter valueGetter)
            : base(indexedStreamNum, subqueryNum, optionalSerde, isFireAndForget)
        {
            this.indexProp = indexProp;
            this.indexType = indexType;
            this.valueGetter = valueGetter;
        }

        public override EventTableFactory Create(EventType eventType, StatementContext statementContext)
        {
            return statementContext.EventTableIndexService.CreateSorted(
                indexedStreamNum, eventType, indexProp, indexType,
                valueGetter, optionalSerde, isFireAndForget, statementContext);
        }
    }
} // end of namespace