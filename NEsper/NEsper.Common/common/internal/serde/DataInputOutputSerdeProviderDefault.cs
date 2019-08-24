///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;

namespace com.espertech.esper.common.@internal.serde
{
    public class DataInputOutputSerdeProviderDefault : DataInputOutputSerdeProvider
    {
        public static readonly DataInputOutputSerdeProviderDefault INSTANCE = new DataInputOutputSerdeProviderDefault();

        private DataInputOutputSerdeProviderDefault()
        {
        }

        public DataInputOutputSerdeWCollation<E> ValueNullable<E>(Type type)
        {
            return null;
        }

        public DataInputOutputSerdeWCollation<E> RefCountedSet<E>(Type type)
        {
            return null;
        }

        public DataInputOutputSerdeWCollation<E> SortedRefCountedSet<E>(Type type)
        {
            return null;
        }

        public DataInputOutputSerdeWCollation<E> ListEvents<E>(EventType eventType)
        {
            return null;
        }

        public DataInputOutputSerdeWCollation<E> LinkedMapEventsAndInt<E>(EventType eventType)
        {
            return null;
        }

        public DataInputOutputSerdeWCollation<E> EventNullable<E>(EventType eventType)
        {
            return null;
        }

        public DataInputOutputSerdeWCollation<E> ObjectArrayMayNullNull<E>(Type[] types)
        {
            return null;
        }

        public DIOSerdeTreeMapEventsMayDeque TreeMapEventsMayDeque<E>(
            Type[] valueTypes,
            EventType eventType)
        {
            return null;
        }

        public DataInputOutputSerdeWCollation<E> RefCountedSetAtomicInteger<E>(EventType eventType)
        {
            return null;
        }

        public DataInputOutputSerdeWCollation<E> ListValues<E>(Type type)
        {
            return null;
        }
    }
} // end of namespace