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
using com.espertech.esper.compat.collections;
using com.espertech.esper.supportunit.bean;

using NUnit.Framework;

namespace com.espertech.esper.supportunit.events
{
    public class EventFactoryHelper
    {
        public static EventBean MakeEvent(String id)
        {
            SupportBeanString bean = new SupportBeanString(id);
            return SupportEventBeanFactory.CreateObject(bean);
        }
    
        public static EventBean[] MakeEvents(String[] ids)
        {
            EventBean[] events = new EventBean[ids.Length];
            for (int i = 0; i < ids.Length; i++)
            {
                events[i] = MakeEvent(ids[i]);
            }
            return events;
        }
    
        public static IDictionary<String, EventBean> MakeEventMap(String[] ids)
        {
            IDictionary<String, EventBean> events = new Dictionary<String, EventBean>();
            for (int i = 0; i < ids.Length; i++)
            {
                String id = ids[i];
                EventBean eventBean = MakeEvent(id);
                events.Put(id, eventBean);
            }
            return events;
        }
    
        public static IList<EventBean> MakeEventList(String[] ids)
        {
            EventBean[] events = MakeEvents(ids);
            return events;
        }
    
        public static EventBean[] MakeArray(IDictionary<String, EventBean> events, String[] ids)
        {
            EventBean[] eventArr = new EventBean[ids.Length];
            for (int i = 0; i < eventArr.Length; i++)
            {
                eventArr[i] = events.Get(ids[i]);
                if (eventArr[i] == null)
                {
                    Assert.Fail();
                }
            }
            return eventArr;
        }
    
        public static IList<EventBean> MakeList(IDictionary<String, EventBean> events, String[] ids)
        {
            IList<EventBean> eventList = new List<EventBean>();
            for (int i = 0; i < ids.Length; i++)
            {
                EventBean bean = events.Get(ids[i]);
                if (bean == null)
                {
                    Assert.Fail();
                }
                eventList.Add(bean);
            }
            return eventList;
        }
    }
}
