///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Threading;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.@event.json.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.threading;

namespace com.espertech.esper.compiler.@internal.util
{
	public class CompilableItemPostCompileLatchJson : CompilableItemPostCompileLatch
	{
		private readonly CountDownLatch _latch = new CountDownLatch(1);
		private readonly ICollection<EventType> _eventTypes;
		private readonly ClassLoader _parentClassLoader;
		private IDictionary<string, byte[]> _moduleBytes;

		public CompilableItemPostCompileLatchJson(
			ICollection<EventType> eventTypes,
			ClassLoader parentClassLoader)
		{
			this._eventTypes = eventTypes;
			this._parentClassLoader = parentClassLoader;
		}

		public void AwaitAndRun()
		{
			try {
				_latch.Await();
			}
			catch (ThreadInterruptedException) {
				Thread.CurrentThread.Interrupt();
				return;
			}

			// load underlying class of Json types
			foreach (EventType eventType in _eventTypes) {
				if (!(eventType is JsonEventType)) {
					continue;
				}

				JsonEventType jsonEventType = (JsonEventType) eventType;
				ByteArrayProvidingClassLoader classLoader = new ByteArrayProvidingClassLoader(_moduleBytes, _parentClassLoader);
				jsonEventType.Initialize(classLoader);
			}
		}

		public void Completed(IDictionary<string, byte[]> moduleBytes)
		{
			this._moduleBytes = moduleBytes;
			_latch.CountDown();
		}
	}
} // end of namespace
