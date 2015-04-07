///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.client.annotation
{
    /// <summary>
    /// Annotation for defining the name of the functions returning external data window key and value
    /// objects for use with queries against external data windows.
    /// </summary>
    public class ExternalDWQueryAttribute : Attribute
    {
        public ExternalDWQueryAttribute()
        {
            FunctionKeys = string.Empty;
            FunctionValues = string.Empty;
        }

        /// <summary>Returns function name that return key objects. </summary>
        /// <returns>function name</returns>
        public String FunctionKeys { get; set; }

        /// <summary>Returns function name that return value objects. </summary>
        /// <returns>function name</returns>
        public String FunctionValues { get; set; }
    }
}