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
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.serde.serdeset.additional;
using com.espertech.esper.common.@internal.serde.serdeset.builtin;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.serde.runtime.@event
{
	public class EventSerdeFactoryDefault : EventSerdeFactory
	{

		public readonly static EventSerdeFactoryDefault INSTANCE = new EventSerdeFactoryDefault();

		private EventSerdeFactoryDefault()
		{
		}

		public void VerifyHaDeployment(bool targetHA)
		{
			// no verification required
		}

		public DataInputOutputSerde<EventBean> NullableEvent(EventType eventType)
		{
			return DIOUnsupportedSerde<EventBean>.INSTANCE;
		}

		public DataInputOutputSerde<EventBean> NullableEventArray(EventType eventType)
		{
			return DIOUnsupportedSerde<EventBean>.INSTANCE;
		}

		public DataInputOutputSerde<object> ListEvents(EventType eventType)
		{
			return DIOUnsupportedSerde<object>.INSTANCE;
		}

		public DataInputOutputSerde<TE> LinkedHashMapEventsAndInt<TE>(EventType eventType)
		{
			return DIOUnsupportedSerde<TE>.INSTANCE;
		}

		public DataInputOutputSerde<TE> RefCountedSetAtomicInteger<TE>(EventType eventType)
		{
			return DIOUnsupportedSerde<TE>.INSTANCE;
		}

		public DataInputOutputSerde<EventBean> NullableEventMayCollate(EventType eventType)
		{
			return DIOUnsupportedSerde<EventBean>.INSTANCE;
		}

		public DataInputOutputSerde<object> NullableEventOrUnderlying(EventType eventType)
		{
			return DIOUnsupportedSerde<object>.INSTANCE;
		}

		public DIOSerdeTreeMapEventsMayDeque TreeMapEventsMayDeque<E>(
			DataInputOutputSerde<E>[] criteriaSerdes,
			EventType eventType)
		{
			return null;
		}

		public DataInputOutputSerde<object> ObjectArrayMayNullNull<E>(DataInputOutputSerde<E>[] serdes)
		{
			return DIOUnsupportedSerde<object>.INSTANCE;
		}

		public DataInputOutputSerde<object> NullableEventArrayOrUnderlying(EventType eventType)
		{
			return DIOUnsupportedSerde<object>.INSTANCE;
		}
	}
} // end of namespace
