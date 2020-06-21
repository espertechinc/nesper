///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;


namespace com.espertech.esper.common.@internal.@event.json.core
{
	public class JsonEventBean : EventBeanSPI,
		JsonBackedEventBean
	{
		private EventType eventType;
		private object theEvent;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="theEvent">is the event object</param>
		/// <param name="eventType">is the schema information for the event object.</param>
		public JsonEventBean(
			object theEvent,
			EventType eventType)
		{
			this.eventType = eventType;
			this.theEvent = theEvent;
		}

		public virtual object Underlying {
			get => theEvent;
			set => theEvent = value;
		}

		public object UnderlyingSpi {
			get => theEvent;
			set => theEvent = value;
		}

		public virtual EventType EventType => eventType;

		public object Get(string property)
		{
			EventPropertyGetter getter = eventType.GetGetter(property);
			if (getter == null) {
				throw new PropertyAccessException("Property named '" + property + "' is not a valid property name for this type");
			}

			return getter.Get(this);
		}

		public object this[string property] => Get(property);

		public override string ToString()
		{
			return "JsonEventBean" +
			       " eventType=" +
			       eventType +
			       " bean=" +
			       theEvent;
		}

		public object GetFragment(string propertyExpression)
		{
			EventPropertyGetter getter = eventType.GetGetter(propertyExpression);
			if (getter == null) {
				throw PropertyAccessException.NotAValidProperty(propertyExpression);
			}

			return getter.GetFragment(this);
		}
	}
} // end of namespace
