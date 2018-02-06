///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.supportunit.bean
{
    [Serializable]
    public class SupportDateTimeOffset
    {
        /// <summary>
        /// Gets or sets the date and time with offset.
        /// </summary>
        public DateTimeOffset Value { get; set; }

        /// <summary>
        /// Returns the local date and time.
        /// </summary>
        public DateTime LocalDateTime {
            get { return Value.LocalDateTime; }
        }

        /// <summary>
        /// Returns the universal date and time.
        /// </summary>
        public DateTime UniversalDateTime
        {
            get { return Value.UtcDateTime; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SupportDateTimeOffset"/> class.
        /// </summary>
        /// <param name="value">The date time offset.</param>
        public SupportDateTimeOffset(DateTimeOffset value)
        {
            Value = value;
        }
    }
}
