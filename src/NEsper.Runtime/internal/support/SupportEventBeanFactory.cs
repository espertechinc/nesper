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
using com.espertech.esper.compat;
using com.espertech.esper.container;

namespace com.espertech.esper.runtime.@internal.support
{
    public class SupportEventBeanFactory
    {
        /// <summary>Gets the instance.</summary>
        /// <param name="container">The container.</param>
        /// <returns></returns>
        public static SupportEventBeanFactory GetInstance(IContainer container)
        {
            return container.ResolveSingleton(() => new SupportEventBeanFactory(container));
        }

        public static void RegisterSingleton(IContainer container)
        {
            container.Register<SupportEventBeanFactory>(
                xx => new SupportEventBeanFactory(container),
                Lifespan.Singleton);
        }

        private readonly SupportEventTypeFactory _supportEventTypeFactory;

        /// <summary>Initializes a new instance of the <see cref="SupportEventBeanFactory"/> class.</summary>
        /// <param name="supportEventTypeFactory">The support event type factory.</param>
        public SupportEventBeanFactory(SupportEventTypeFactory supportEventTypeFactory)
        {
            _supportEventTypeFactory = supportEventTypeFactory;
        }

        /// <summary>Initializes a new instance of the <see cref="SupportEventBeanFactory"/> class.</summary>
        /// <param name="container">The container.</param>
        private SupportEventBeanFactory(IContainer container)
            : this(SupportEventTypeFactory.GetInstance(container))
        {
        }

        public EventBean CreateObject(object theEvent)
        {
            if (theEvent is SupportBean)
            {
                return new BeanEventBean(theEvent, _supportEventTypeFactory.SUPPORTBEAN_EVENTTTPE);
            }
            else if (theEvent is SupportBean_S0)
            {
                return new BeanEventBean(theEvent, _supportEventTypeFactory.SUPPORTBEAN_S0_EVENTTTPE);
            }
            else if (theEvent is SupportBean_A)
            {
                return new BeanEventBean(theEvent, _supportEventTypeFactory.SUPPORTBEAN_A_EVENTTTPE);
            }
            else if (theEvent is SupportBeanComplexProps)
            {
                return new BeanEventBean(theEvent, _supportEventTypeFactory.SUPPORTBEANCOMPLEXPROPS_EVENTTTPE);
            }
            else if (theEvent is SupportBeanSimple)
            {
                return new BeanEventBean(theEvent, _supportEventTypeFactory.SUPPORTBEANSIMPLE_EVENTTTPE);
            }
            else
            {
                throw new UnsupportedOperationException("Unexpected type " + theEvent.GetType());
            }
        }
    }
} // end of namespace