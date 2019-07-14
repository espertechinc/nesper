///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
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
                "@Name('s0') select otherEventsIterable[0] as c0, otherEventsMap('key') as c1, otherEventsList[0] as c2 from " +
                typeClass.Name);
            env.AddListener("s0");

            env.SendEventBean(@event);
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                "c0.id,c1.id,c2.id".SplitCsv(),
                new object[] {"id1", "id2", "id3"});

            env.UndeployAll();
        }

        [Serializable]
        public class MyEventWithMethod
        {
            private readonly IDictionary<string, OtherEvent> otherEventsMap;

            public MyEventWithMethod(
                IEnumerable<OtherEvent> otherEventsIterable,
                IDictionary<string, OtherEvent> otherEventsMap,
                IList<OtherEvent> otherEventsList)
            {
                OtherEventsIterable = otherEventsIterable;
                this.otherEventsMap = otherEventsMap;
                OtherEventsList = otherEventsList;
            }

            public IEnumerable<OtherEvent> OtherEventsIterable { get; }

            public IList<OtherEvent> OtherEventsList { get; }

            public IDictionary<string, OtherEvent> GetOtherEventsMap()
            {
                return otherEventsMap;
            }
        }

        [Serializable]
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