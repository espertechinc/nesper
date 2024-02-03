///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.methodbase;

namespace com.espertech.esper.common.client.hook.datetimemethod
{
    /// <summary>
    ///     Provides footprint information for date-time method extension.
    /// </summary>
    public class DateTimeMethodDescriptor
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="footprints">footprint array, one array item for each distinct footprint</param>
        public DateTimeMethodDescriptor(DotMethodFP[] footprints)
        {
            Footprints = footprints;
        }

        /// <summary>
        ///     Returns the footprints
        /// </summary>
        /// <value>footprints</value>
        public DotMethodFP[] Footprints { get; }
    }
} // end of namespace