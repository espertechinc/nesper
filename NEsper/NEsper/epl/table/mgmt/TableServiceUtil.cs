///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.lookup;
using com.espertech.esper.events;
using com.espertech.esper.events.arr;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.table.mgmt
{
    public class TableServiceUtil
    {
        public static string GetTableNameFromEventType(EventType type)
        {
            if (!(type is EventTypeSPI))
            {
                return null;
            }
            var spi = (EventTypeSPI) type;
            if (spi.Metadata.TypeClass == TypeClass.TABLE)
            {
                return spi.Metadata.PrimaryName;
            }
            return null;
        }
    
        public static StreamTypeServiceImpl StreamTypeFromTableColumn(TableMetadataColumnAggregation column, string engineURI)
        {
            if (column.OptionalEventType == null)
            {
                throw new ArgumentException("Required event type not provided");
            }
            return new StreamTypeServiceImpl(column.OptionalEventType, column.OptionalEventType.Name, false, engineURI);
        }
    
        public static Pair<int[], IndexMultiKey> GetIndexMultikeyForKeys(IDictionary<String, TableMetadataColumn> items, ObjectArrayEventType eventType)
        {
            IList<IndexedPropDesc> indexFields = new List<IndexedPropDesc>();
            IList<int> keyIndexes = new List<int>();
            var count = 0;
            foreach (var entry in items)
            {
                if (entry.Value.IsKey)
                {
                    indexFields.Add(new IndexedPropDesc(entry.Key, eventType.GetPropertyType(entry.Key)));
                    keyIndexes.Add(count+1);
                }
                count++;
            }
            var keyColIndexes = CollectionUtil.IntArray(keyIndexes);
            return new Pair<int[], IndexMultiKey>(keyColIndexes, new IndexMultiKey(true, indexFields, Collections.GetEmptyList<IndexedPropDesc>(), null));
        }
    }
}
