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
    /// Shortcut-evaluator for use with PONO-backed events only.
    /// </summary>
    /// <seealso cref="com.espertech.esper.events.EventPropertyGetterSPI" />
    public interface BeanEventPropertyGetter : EventPropertyGetterSPI
    {
        /// <summary>
        /// Returns the property as an object.
        /// </summary>
        /// <param name="object">to evaluate</param>
        /// <exception cref="PropertyAccessException">if access failed</exception>
        /// <returns>property of object</returns>
        Object GetBeanProp(Object @object) ;
    
        /// <summary>
        /// Returns true if the dynamic property exists.
        /// </summary>
        /// <param name="object">to evaluate</param>
        /// <returns>indicator if property exists</returns>
        bool IsBeanExistsProperty(Object @object);

        Type BeanPropType { get; }
        Type TargetType { get; }
    }
} // end of namespace
