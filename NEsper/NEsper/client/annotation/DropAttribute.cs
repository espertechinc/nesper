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
    /// An execution directive for use in an EPL statement, that causes processing of an
    /// event to stop after the EPL statement marked with @Drop has processed the event,
    /// applicable only if multiple statements must process the same event.
    /// <para/>
    /// Ensure the engine configuration for prioritized execution is set before using
    /// this annotation.
    /// </summary>
    public class DropAttribute : Attribute
    {
        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </returns>
        public override string ToString()
        {
            return "@Drop()";
        }
    }
}
