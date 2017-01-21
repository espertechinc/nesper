///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.client.annotation
{
    /// <summary>Annotation for mapping of event-to-value and value-to-event for external data windows. </summary>
    public class ExternalDWValueAttribute : Attribute
    {
        /// <summary>Returns the function name of the function that maps event beans to value objects. </summary>
        /// <returns>event to value mapping function name</returns>
        public String FunctionBeanToValue { get; set; }

        /// <summary>Returns the function name of the function that maps values to event objects. </summary>
        /// <returns>value to event mapping function name</returns>
        public String FunctionValueToBean { get; set; }
    }
}