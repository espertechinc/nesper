///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.util;
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
		private ICollection<Assembly> _assemblies;

		public CompilableItemPostCompileLatchJson(
			ICollection<EventType> eventTypes,
			ClassLoader parentClassLoader)
		{
			_eventTypes = eventTypes;
			_parentClassLoader = parentClassLoader;
		}

		public void AwaitAndRun()
		{
			_latch.Await();

			// load underlying class of Json types
			foreach (var jsonEventType in _eventTypes.OfType<JsonEventType>()) {
				var classLoader = new PriorityClassLoader(_parentClassLoader, _assemblies);
				jsonEventType.Initialize(classLoader);
			}
		}

		public void Completed(ICollection<Assembly> assemblies)
		{
			_assemblies = assemblies;
			_latch.CountDown();
		}
	}
} // end of namespace
