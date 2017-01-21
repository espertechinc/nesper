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
    /// <summary>
    /// Annotation that can be attached to specify which underlying event representation to use for events.
    /// </summary>
    public class EventRepresentationAttribute : Attribute
    {
        /// <summary>True for object-array, false for Map. </summary>
        /// <returns>array indicator</returns>
        public bool Array { get; set; }
    }
}