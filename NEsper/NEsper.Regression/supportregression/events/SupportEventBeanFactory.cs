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
using com.espertech.esper.core.support;
using com.espertech.esper.events;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.util;

namespace com.espertech.esper.supportregression.events
{
    public class SupportEventBeanFactory
    {
        public static EventBean CreateObject(Object theEvent)
        {
            return SupportContainer.Resolve<EventAdapterService>().AdapterForObject(theEvent);
        }
    
        public static EventBean CreateMapFromValues(IDictionary<String, Object> testValuesMap, EventType eventType)
        {
            return SupportContainer.Resolve<EventAdapterService>().AdapterForTypedMap(testValuesMap, eventType);
        }   
    
        public static EventBean[] MakeEvents(String[] ids)
        {
            EventBean[] events = new EventBean[ids.Length];
            for (int i = 0; i < events.Length; i++)
            {
                SupportBean bean = new SupportBean();
                bean.TheString = ids[i];
                events[i] = CreateObject(bean);
            }
            return events;
        }
    
        public static EventBean[] MakeEvents(bool[] boolPrimitiveValues)
        {
            EventBean[] events = new EventBean[boolPrimitiveValues.Length];
            for (int i = 0; i < events.Length; i++)
            {
                SupportBean bean = new SupportBean();
                bean.BoolPrimitive = boolPrimitiveValues[i];
                events[i] = CreateObject(bean);
            }
            return events;
        }
    
        public static EventBean[] MakeMarketDataEvents(String[] ids)
        {
            EventBean[] events = new EventBean[ids.Length];
            for (int i = 0; i < events.Length; i++)
            {
                SupportMarketDataBean bean = new SupportMarketDataBean(ids[i], 0, 0L, null);
                events[i] = CreateObject(bean);
            }
            return events;
        }
    
        public static EventBean[] MakeEvents_A(String[] ids)
        {
            EventBean[] events = new EventBean[ids.Length];
            for (int i = 0; i < events.Length; i++)
            {
                SupportBean_A bean = new SupportBean_A(ids[i]);
                events[i] = CreateObject(bean);
            }
            return events;
        }
    
        public static EventBean[] MakeEvents_B(String[] ids)
        {
            EventBean[] events = new EventBean[ids.Length];
            for (int i = 0; i < events.Length; i++)
            {
                SupportBean_B bean = new SupportBean_B(ids[i]);
                events[i] = CreateObject(bean);
            }
            return events;
        }
    
        public static EventBean[] MakeEvents_C(String[] ids)
        {
            EventBean[] events = new EventBean[ids.Length];
            for (int i = 0; i < events.Length; i++)
            {
                SupportBean_C bean = new SupportBean_C(ids[i]);
                events[i] = CreateObject(bean);
            }
            return events;
        }
    
        public static EventBean[] MakeEvents_D(String[] ids)
        {
            EventBean[] events = new EventBean[ids.Length];
            for (int i = 0; i < events.Length; i++)
            {
                SupportBean_D bean = new SupportBean_D(ids[i]);
                events[i] = CreateObject(bean);
            }
            return events;
        }
    }
}
