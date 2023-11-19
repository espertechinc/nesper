///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.magic;
using com.espertech.esper.regressionlib.framework;

namespace com.espertech.esper.regressionlib.suite.@event.bean
{
    public class EventBeanPropertyIterableMapList : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            RunAssertionIterable(env);
        }

        private void RunAssertionIterable(RegressionEnvironment env)
        {
            var eventField = new MyEventWithField();
            eventField.otherEventsIterable = Arrays.AsList(new OtherEvent("id1"));
            eventField.otherEventsMap = Collections.SingletonMap("key", new OtherEvent("id2"));
            eventField.otherEventsList = Arrays.AsList(new OtherEvent("id3"));

            var eventMethod = new MyEventWithMethod(
                Arrays.AsList(new OtherEvent("id1")),
                Collections.SingletonMap("key", new OtherEvent("id2")),
                Arrays.AsList(new OtherEvent("id3")));

            TryAssertionIterable(env, typeof(MyEventWithField), eventField);
            TryAssertionIterable(env, typeof(MyEventWithMethod), eventMethod);
        }

        private void TryAssertionIterable(
            RegressionEnvironment env,
            Type typeClass,
            object @event)
        {
            env.CompileDeploy(
                "@name('s0') select otherEventsIterable[0] as c0, otherEventsMap('key') as c1, otherEventsList[0] as c2 from " +
                typeClass.Name);
            env.AddListener("s0");

            env.SendEventBean(@event);
            env.AssertPropsNew(
                "s0",
                new[] { "c0.Id", "c1.Id", "c2.Id" },
                new object[] { "id1", "id2", "id3" });

            env.UndeployAll();
        }

        public class MyEventWithMethod
        {
            private readonly IEnumerable<OtherEvent> _otherEventsIterable;
            private readonly IDictionary<string, OtherEvent> _otherEventsMap;
            private readonly IList<OtherEvent> _otherEventsList;

            public MyEventWithMethod(
                IEnumerable<OtherEvent> otherEventsIterable,
                IDictionary<string, OtherEvent> otherEventsMap,
                IList<OtherEvent> otherEventsList)
            {
                _otherEventsIterable = otherEventsIterable;
                _otherEventsMap = otherEventsMap;
                _otherEventsList = otherEventsList;
            }

            [PropertyName("otherEventsIterable")]
            public IEnumerable<OtherEvent> OtherEventsIterable => _otherEventsIterable;

            [PropertyName("otherEventsList")]
            public IList<OtherEvent> OtherEventsList => _otherEventsList;

            [PropertyName("otherEventsMap")]
            public IDictionary<string, OtherEvent> GetOtherEventsMap()
            {
                return _otherEventsMap;
            }
        }

        public class MyEventWithField
        {
            public IEnumerable<OtherEvent> otherEventsIterable;
            public IList<OtherEvent> otherEventsList;
            public IDictionary<string, OtherEvent> otherEventsMap;
        }

        public class OtherEvent
        {
            public OtherEvent(string id)
            {
                Id = id;
            }

            public string Id { get; }
        }
    }
} // end of namespace