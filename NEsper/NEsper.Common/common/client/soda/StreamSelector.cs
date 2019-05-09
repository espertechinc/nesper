///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.client.soda
{
    /// <summary>
    /// Enumeration for representing selection of the remove stream or the insert stream, or both.
    /// </summary>
    public enum StreamSelector
    {
        /// <summary>Indicates selection of the remove stream only.</summary>
        RSTREAM_ONLY,

        /// <summary>Indicates selection of the insert stream only.</summary>
        ISTREAM_ONLY,

        /// <summary>Indicates selection of both the insert and the remove stream.</summary>
        RSTREAM_ISTREAM_BOTH
    }

    public static class StreamSelectorExtensions
    {
        public static string GetEPL(this StreamSelector enumValue)
        {
            switch (enumValue) {
                case StreamSelector.RSTREAM_ONLY:
                    return "rstream";
                case StreamSelector.ISTREAM_ONLY:
                    return "istream";
                case StreamSelector.RSTREAM_ISTREAM_BOTH:
                    return "irstream";
                default:
                    throw new ArgumentException("invalid value for enum value", nameof(enumValue));
            }
        }
    }
} // End of namespace