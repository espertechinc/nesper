///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.@event.bean.core;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.supportunit.bean;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.supportunit.@event
{
    public class SupportEventBeanFactory
    {
        public static EventBean CreateObject(SupportEventTypeFactory supportEventTypeFactory, object theEvent)
        {
            if (theEvent is SupportBean)
            {
                return new BeanEventBean(theEvent, supportEventTypeFactory.SUPPORTBEAN_EVENTTTPE);
            }

            if (theEvent is SupportBean_S0)
            {
                return new BeanEventBean(theEvent, supportEventTypeFactory.SUPPORTBEAN_S0_EVENTTTPE);
            }

            if (theEvent is SupportBeanString)
            {
                return new BeanEventBean(theEvent, supportEventTypeFactory.SUPPORTBEANSTRING_EVENTTTPE);
            }

            if (theEvent is SupportBean_A)
            {
                return new BeanEventBean(theEvent, supportEventTypeFactory.SUPPORTBEAN_A_EVENTTTPE);
            }

            if (theEvent is SupportBeanComplexProps)
            {
                return new BeanEventBean(theEvent, supportEventTypeFactory.SUPPORTBEANCOMPLEXPROPS_EVENTTTPE);
            }

            if (theEvent is SupportLegacyBean)
            {
                return new BeanEventBean(theEvent, supportEventTypeFactory.SUPPORTLEGACYBEAN_EVENTTTPE);
            }

            if (theEvent is SupportBeanCombinedProps)
            {
                return new BeanEventBean(theEvent, supportEventTypeFactory.SUPPORTBEANCOMBINEDPROPS_EVENTTTPE);
            }

            if (theEvent is SupportBeanPropertyNames)
            {
                return new BeanEventBean(theEvent, supportEventTypeFactory.SUPPORTBEANPROPERTYNAMES_EVENTTTPE);
            }

            if (theEvent is SupportBeanIterableProps)
            {
                return new BeanEventBean(theEvent, supportEventTypeFactory.SUPPORTBEANITERABLEPROPS_EVENTTTPE);
            }

            if (theEvent is SupportBeanIterablePropsContainer)
            {
                return new BeanEventBean(theEvent, supportEventTypeFactory.SUPPORTBEANITERABLEPROPSCONTAINER_EVENTTYPE);
            }

            if (theEvent is SupportBeanSimple)
            {
                return new BeanEventBean(theEvent, supportEventTypeFactory.SUPPORTBEANSIMPLE_EVENTTTPE);
            }

            throw new UnsupportedOperationException("Unexpected type " + theEvent.GetType());
        }

        public static EventBean[] MakeEvents(SupportEventTypeFactory supportEventTypeFactory, string[] ids)
        {
            var events = new EventBean[ids.Length];
            for (var i = 0; i < events.Length; i++)
            {
                var bean = new SupportBean();
                bean.TheString = ids[i];
                events[i] = CreateObject(supportEventTypeFactory, bean);
            }

            return events;
        }
    }
} // end of namespace
