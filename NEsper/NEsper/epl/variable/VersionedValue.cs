///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.epl.variable
{
    /// <summary>
    /// Holds a version of a value and a timestamp when that version is taken.
    /// </summary>
    public class VersionedValue<T>
    {
        private readonly int version;
        private readonly T value;
        private readonly long timestamp;

        /// <summary>Ctor.</summary>
        /// <param name="version">version number</param>
        /// <param name="value">value at that version</param>
        /// <param name="timestamp">time when version was taken</param>
        public VersionedValue(int version, T value, long timestamp)
        {
            this.version = version;
            this.value = value;
            this.timestamp = timestamp;
        }

        /// <summary>Returns the version.</summary>
        /// <returns>version</returns>
        public int Version
        {
            get { return version; }
        }

        /// <summary>Returns the value.</summary>
        /// <returns>value</returns>
        public T Value
        {
            get { return value; }
        }

        /// <summary>Returns the time the version was taken.</summary>
        /// <returns>time of version</returns>
        public long Timestamp
        {
            get { return timestamp; }
        }

        public override String ToString()
        {
            return value + "@" + version + "@" + (new DateTime(timestamp));
        }
    }
} // End of namespace
