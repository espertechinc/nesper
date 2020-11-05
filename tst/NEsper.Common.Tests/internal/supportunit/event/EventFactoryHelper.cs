///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.supportunit.bean;
using com.espertech.esper.compat.collections;

using NUnit.Framework;

namespace com.espertech.esper.common.@internal.supportunit.@event
{
    public class EventFactoryHelper
    {
        public static EventBean MakeEvent(
            string id,
            SupportEventTypeFactory supportEventTypeFactory)
        {
            var bean = new SupportBeanString(id);
            return SupportEventBeanFactory.CreateObject(supportEventTypeFactory, bean);
        }

        public static EventBean[] MakeEvents(
            string[] ids,
            SupportEventTypeFactory supportEventTypeFactory)
        {
            var events = new EventBean[ids.Length];
            for (var i = 0; i < ids.Length; i++)
            {
                events[i] = MakeEvent(ids[i], supportEventTypeFactory);
            }

            return events;
        }

        public static IDictionary<string, EventBean> MakeEventMap(
            string[] ids,
            SupportEventTypeFactory supportEventTypeFactory)
        {
            IDictionary<string, EventBean> events = new Dictionary<string, EventBean>();
            for (var i = 0; i < ids.Length; i++)
            {
                var id = ids[i];
                var eventBean = MakeEvent(id, supportEventTypeFactory);
                events.Put(id, eventBean);
            }

            return events;
        }

        public static IList<EventBean> MakeEventList(
            string[] ids,
            SupportEventTypeFactory supportEventTypeFactory)
        {
            var events = MakeEvents(ids, supportEventTypeFactory);
            return Arrays.AsList(events);
        }

        public static EventBean[] MakeArray(
            IDictionary<string, EventBean> events,
            string[] ids)
        {
            var eventArr = new EventBean[ids.Length];
            for (var i = 0; i < eventArr.Length; i++)
            {
                eventArr[i] = events.Get(ids[i]);
                if (eventArr[i] == null)
                {
                    Assert.Fail();
                }
            }

            return eventArr;
        }

        public static IList<EventBean> MakeList(
            IDictionary<string, EventBean> events,
            string[] ids)
        {
            IList<EventBean> eventList = new List<EventBean>();
            for (var i = 0; i < ids.Length; i++)
            {
                var bean = events.Get(ids[i]);
                if (bean == null)
                {
                    Assert.Fail();
                }

                eventList.Add(bean);
            }

            return eventList;
        }
    }
} // end of namespace
