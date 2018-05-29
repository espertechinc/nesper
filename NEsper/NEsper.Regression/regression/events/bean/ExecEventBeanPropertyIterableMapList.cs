///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat.collections;
using com.espertech.esper.supportregression.execution;

namespace com.espertech.esper.regression.events.bean
{
    public class ExecEventBeanPropertyIterableMapList : RegressionExecution
    {
        public override void Run(EPServiceProvider epService)
        {
            RunAssertionIterable(epService);
        }
    
        private void RunAssertionIterable(EPServiceProvider epService)
        {
            var configField = new ConfigurationEventTypeLegacy();
            configField.AccessorStyle = AccessorStyleEnum.PUBLIC;
            epService.EPAdministrator.Configuration.AddEventType(typeof(MyEventWithField).Name, typeof(MyEventWithField).AssemblyQualifiedName, configField);
            var eventField = new MyEventWithField();
            eventField.OtherEventsIterable = Collections.List(new OtherEvent("id1"));
            eventField.OtherEventsMap = Collections.SingletonMap("key", new OtherEvent("id2"));
            eventField.OtherEventsList = Collections.List(new OtherEvent("id3"));
    
            var configCglib = new ConfigurationEventTypeLegacy();
            epService.EPAdministrator.Configuration.AddEventType(typeof(MyEventWithMethodWCGLIB).Name, typeof(MyEventWithMethodWCGLIB).AssemblyQualifiedName, configCglib);
            var eventMethodCglib = new MyEventWithMethodWCGLIB(Collections.List(new OtherEvent("id1")), Collections.SingletonMap("key", new OtherEvent("id2")), Collections.List(new OtherEvent("id3")));
    
            var configNoCglib = new ConfigurationEventTypeLegacy();
            configNoCglib.CodeGeneration = CodeGenerationEnum.DISABLED;
            epService.EPAdministrator.Configuration.AddEventType(typeof(MyEventWithMethodNoCGLIB).Name, typeof(MyEventWithMethodNoCGLIB).AssemblyQualifiedName, configNoCglib);
            var eventMethodNocglib = new MyEventWithMethodNoCGLIB(Collections.List(new OtherEvent("id1")), Collections.SingletonMap("key", new OtherEvent("id2")), Collections.List(new OtherEvent("id3")));
    
            TryAssertionIterable(epService, typeof(MyEventWithField), eventField);
            TryAssertionIterable(epService, typeof(MyEventWithMethodWCGLIB), eventMethodCglib);
            TryAssertionIterable(epService, typeof(MyEventWithMethodNoCGLIB), eventMethodNocglib);
        }
    
        private void TryAssertionIterable(EPServiceProvider epService, Type typeClass, Object @event) {
            var stmt = epService.EPAdministrator.CreateEPL(
                "select OtherEventsIterable[0] as c0, OtherEventsMap('key') as c1, OtherEventsList[0] as c2 " + 
                "from " + typeClass.Name);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(@event);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "c0.id,c1.id,c2.id".Split(','), new object[]{"id1", "id2", "id3"});
    
            stmt.Dispose();
        }
    
        public abstract class MyEventWithMethod {
            protected MyEventWithMethod(
                IEnumerable<OtherEvent> otherEventsIterable,
                IDictionary<string, OtherEvent> otherEventsMap,
                IList<OtherEvent> otherEventsList)
            {
                OtherEventsIterable = otherEventsIterable;
                OtherEventsMap = otherEventsMap;
                OtherEventsList = otherEventsList;
            }

            public IEnumerable<OtherEvent> OtherEventsIterable { get; }

            public IDictionary<string, OtherEvent> OtherEventsMap { get; }

            public IList<OtherEvent> OtherEventsList { get; }
        }

        public class MyEventWithMethodWCGLIB : MyEventWithMethod
        {
            public MyEventWithMethodWCGLIB(
                IEnumerable<OtherEvent> otherEventsIterable,
                IDictionary<string, OtherEvent> otherEventsMap,
                IList<OtherEvent> otherEventsList)
                : base(otherEventsIterable, otherEventsMap, otherEventsList)
            {
            }
        }

        public class MyEventWithMethodNoCGLIB : MyEventWithMethod
        {
            public MyEventWithMethodNoCGLIB(
                IEnumerable<OtherEvent> otherEventsIterable,
                IDictionary<string, OtherEvent> otherEventsMap,
                IList<OtherEvent> otherEventsList)
                : base(otherEventsIterable, otherEventsMap, otherEventsList)
            {
            }
        }

        public class MyEventWithField
        {
            public IEnumerable<OtherEvent> OtherEventsIterable;
            public IDictionary<string, OtherEvent> OtherEventsMap;
            public IList<OtherEvent> OtherEventsList;
        }

        public class OtherEvent
        {
            public string Id { get; }

            public OtherEvent(string id)
            {
                Id = id;
            }
        }
    }
} // end of namespace
