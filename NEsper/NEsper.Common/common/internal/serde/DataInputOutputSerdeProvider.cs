///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.serde
{
	public interface DataInputOutputSerdeProvider {
	    DataInputOutputSerdeWCollation<E> ValueNullable<E>(Type type);

	    DataInputOutputSerdeWCollation<E> RefCountedSet<E>(Type type);

	    DataInputOutputSerdeWCollation<E> SortedRefCountedSet<E>(Type type);

	    DataInputOutputSerdeWCollation<E> ObjectArrayMayNullNull<E>(Type[] types);

	    DataInputOutputSerdeWCollation<E> ListEvents<E>(EventType eventType);

	    DataInputOutputSerdeWCollation<E> LinkedHashMapEventsAndInt<E>(EventType eventType);

	    DataInputOutputSerdeWCollation<E> EventNullable<E>(EventType eventType);

	    DataInputOutputSerdeWCollation<E> RefCountedSetAtomicInteger<E>(EventType eventType);

	    DIOSerdeTreeMapEventsMayDeque TreeMapEventsMayDeque<E>(Type[] valueTypes, EventType eventType);
	}
} // end of namespace