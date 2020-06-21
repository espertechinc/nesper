///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.@event.core;

namespace com.espertech.esper.common.@internal.@event.json.core
{
	public class JsonEventBean : EventBeanSPI,
		JsonBackedEventBean
	{
		private readonly EventType _eventType;
		private object _theEvent;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="theEvent">is the event object</param>
		/// <param name="eventType">is the schema information for the event object.</param>
		public JsonEventBean(
			object theEvent,
			EventType eventType)
		{
			_eventType = eventType;
			_theEvent = theEvent;
		}

		public virtual object Underlying {
			get => _theEvent;
			set => _theEvent = value;
		}

		public object UnderlyingSpi {
			get => _theEvent;
			set => _theEvent = value;
		}

		public virtual EventType EventType => _eventType;

		public object Get(string property)
		{
			var getter = _eventType.GetGetter(property);
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
			       _eventType +
			       " bean=" +
			       _theEvent;
		}

		public object GetFragment(string propertyExpression)
		{
			EventPropertyGetter getter = _eventType.GetGetter(propertyExpression);
			if (getter == null) {
				throw PropertyAccessException.NotAValidProperty(propertyExpression);
			}

			return getter.GetFragment(this);
		}
	}
} // end of namespace
