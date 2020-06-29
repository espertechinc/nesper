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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.serde.runtime.@event
{
	public interface EventSerdeFactory {
	    void VerifyHADeployment(bool targetHa) ;
	    DataInputOutputSerde<EventBean> NullableEvent(EventType eventType);
	    DataInputOutputSerde<EventBean> NullableEventArray(EventType eventType);
	    DataInputOutputSerde NullableEventOrUnderlying(EventType eventType);
	    DataInputOutputSerde NullableEventArrayOrUnderlying(EventType eventType);
	    DIOSerdeTreeMapEventsMayDeque TreeMapEventsMayDeque<TE>(DataInputOutputSerde<TE>[] criteriaSerdes, EventType eventType);
	    DataInputOutputSerde ObjectArrayMayNullNull<TE>(DataInputOutputSerde<TE>[] serdes);
	    DataInputOutputSerde ListEvents(EventType eventType);
	    DataInputOutputSerde<TE> LinkedHashMapEventsAndInt<TE>(EventType eventType);
	    DataInputOutputSerde<TE> RefCountedSetAtomicInteger<TE>(EventType eventType);
	    DataInputOutputSerde<EventBean> NullableEventMayCollate(EventType eventType);
	}
} // end of namespace
