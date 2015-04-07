///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.client.deploy 
{
    /// <summary>
    /// For use with server environments that support dynamic engine initialization 
    /// (enterprise edition server), indicates that this method provide a single-row 
    /// function and should be registered as such so it becomes callable from EPL
    /// statements using the name specified.
    /// </summary>
    //@Retention(RetentionPolicy.RUNTIME)
    //@Target(ElementType.METHOD)
    public class SingleRowFunctionAttribute : Attribute
    {
        /// <summary>
        /// Single-row function name for use in EPL statements.
        /// </summary>
        /// <value>The name.</value>
        /// <returns>function name.</returns>
        public String Name { get; set; }
    }
}
