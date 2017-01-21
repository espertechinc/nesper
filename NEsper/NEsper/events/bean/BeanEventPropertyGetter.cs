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
    /// Shortcut-evaluator for use with POCO-backed events only.
    /// </summary>
    public interface BeanEventPropertyGetter : EventPropertyGetter
    {
        /// <summary>
        /// Returns the property as an object.
        /// </summary>
        /// <param name="object">to evaluate</param>
        /// <returns>property of object</returns>
        /// <throws>PropertyAccessException if access failed</throws>
        Object GetBeanProp(Object @object);

        /// <summary>
        /// Returns true if the dynamic property exists.
        /// </summary>
        /// <param name="object">to evaluate</param>
        /// <returns>indicator if property exists</returns>
        bool IsBeanExistsProperty(Object @object);
    }
}
