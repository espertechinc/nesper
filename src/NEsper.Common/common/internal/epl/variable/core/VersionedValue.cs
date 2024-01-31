///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.@internal.epl.variable.core
{
    /// <summary>
    ///     Holds a version of a value and a timestamp when that version is taken.
    /// </summary>
    public class VersionedValue<T>
    {
        /// <summary>Ctor.</summary>
        /// <param name="version">version number</param>
        /// <param name="value">value at that version</param>
        /// <param name="timestamp">time when version was taken</param>
        public VersionedValue(
            int version,
            T value,
            long timestamp)
        {
            Version = version;
            Value = value;
            Timestamp = timestamp;
        }

        /// <summary>Returns the version.</summary>
        /// <returns>version</returns>
        public int Version { get; }

        /// <summary>Returns the value.</summary>
        /// <returns>value</returns>
        public T Value { get; }

        /// <summary>Returns the time the version was taken.</summary>
        /// <returns>time of version</returns>
        public long Timestamp { get; }

        public override string ToString()
        {
            return Value + "@" + Version + "@" + new DateTime(Timestamp, DateTimeKind.Utc);
        }
    }
} // End of namespace