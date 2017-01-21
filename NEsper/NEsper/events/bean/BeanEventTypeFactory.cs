///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;

namespace com.espertech.esper.events.bean
{
    /// <summary>
    /// Interface for a factory for obtaining <seealso cref="BeanEventType"/> instances.
    /// </summary>
    public interface BeanEventTypeFactory
    {
        /// <summary>
        /// Returns the bean event type for a given class assigning the given name.
        /// </summary>
        /// <param name="name">is the name</param>
        /// <param name="clazz">is the class for which to generate an event type</param>
        /// <param name="isPreconfiguredStatic">if from static engine config</param>
        /// <param name="isPreconfigured">if configured before use</param>
        /// <param name="isConfigured">if the class is a configuration value, false if discovered</param>
        /// <returns>is the event type for the class</returns>
        BeanEventType CreateBeanType(String name, Type clazz, bool isPreconfiguredStatic, bool isPreconfigured, bool isConfigured);
    
        /// <summary>Returns the bean event type for a given class assigning the given name. </summary>
        /// <param name="clazz">is the class for which to generate an event type</param>
        /// <returns>is the event type for the class</returns>
        BeanEventType CreateBeanTypeDefaultName(Type clazz);

        /// <summary>Returns the default property resolution style. </summary>
        /// <value>property resolution style</value>
        PropertyResolutionStyle DefaultPropertyResolutionStyle { get; }

        BeanEventType[] CachedTypes { get; }
    }
}
