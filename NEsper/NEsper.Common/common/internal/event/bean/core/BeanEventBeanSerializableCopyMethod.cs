///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.common.@internal.@event.bean.core
{
	/// <summary>
	/// Copy method for bean events utilizing serializable.
	/// </summary>
	public class BeanEventBeanSerializableCopyMethod : EventBeanCopyMethod
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly BeanEventType _beanEventType;
		private readonly EventBeanTypedEventFactory _eventAdapterService;

		/// <summary>
		/// Ctor.
		/// </summary>
		/// <param name="beanEventType">event type</param>
		/// <param name="eventAdapterService">for creating the event object</param>
		public BeanEventBeanSerializableCopyMethod(
			BeanEventType beanEventType,
			EventBeanTypedEventFactory eventAdapterService)
		{
			this._beanEventType = beanEventType;
			this._eventAdapterService = eventAdapterService;
		}

		public EventBean Copy(EventBean theEvent)
		{
			object underlying = theEvent.Underlying;
			object copied;
			try {
				copied = SerializableObjectCopier.Copy(underlying);
			}
			catch (IOException e) {
				Log.Error("IOException copying event object for update: " + e.Message, e);
				return null;
			}
			catch (TypeLoadException e) {
				Log.Error("Exception copying event object for update: " + e.Message, e);
				return null;
			}

			return _eventAdapterService.AdapterForTypedObject(copied, _beanEventType);
		}
	}
} // end of namespace
