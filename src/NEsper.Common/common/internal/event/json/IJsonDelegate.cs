///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.@internal.@event.json
{
    public interface IJsonDelegate
    {
        /// <summary>
        /// Allocates the underlying value.
        /// </summary>
        public object Allocate();
        
        /// <summary>
        /// Attempts to get a value from the underlying.
        /// </summary>
        /// <param name="name">name of the property</param>
        /// <param name="underlying">input underlying value</param>
        /// <param name="value">output property value</param>
        public bool TryGetProperty(
            string name,
            object underlying,
            out object value);
            
        /// <summary>
        /// Attempts to set a value into the target underlying.
        /// </summary>
        /// <param name="name">name of the property</param>
        /// <param name="underlying">target underlying value</param>
        /// <param name="value">input property value</param>
        public bool TrySetProperty(
            string name,
            object underlying,
            object value);

        /// <summary>
        /// Attempts to copy the underlying value.  
        /// </summary>
        public object TryCopy(object source);
    }
}