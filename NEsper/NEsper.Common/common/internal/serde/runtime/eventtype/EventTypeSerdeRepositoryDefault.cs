///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.@event.bean.service;
using com.espertech.esper.common.@internal.@event.path;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.serde.runtime.eventtype
{
	public class EventTypeSerdeRepositoryDefault : EventTypeSerdeRepository
	{

		public readonly static EventTypeSerdeRepositoryDefault INSTANCE = new EventTypeSerdeRepositoryDefault();

		private EventTypeSerdeRepositoryDefault()
		{
		}

		public void AddSerdes(
			string deploymentId,
			IList<EventTypeCollectedSerde> serdes,
			IDictionary<string, EventType> moduleEventTypes,
			BeanEventTypeFactoryPrivate beanEventTypeFactory)
		{
		}

		public void RemoveSerdes(string deploymentId)
		{
		}
	}
} // end of namespace
