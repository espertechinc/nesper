///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.@internal.compile.stage1.spec
{
    /// <summary>
    /// Abstract base specification for a stream, consists simply of an optional stream name and a list of views
    /// on to of the stream.
    /// <para>
    /// Implementation classes for views and patterns add additional information defining the
    /// stream of events.
    /// </para>
    /// </summary>
    [Serializable]
    public class StreamSpecOptions
    {
        public static readonly StreamSpecOptions DEFAULT = new StreamSpecOptions();

        /// <summary>Ctor, sets all options off.</summary>
        private StreamSpecOptions() {
            IsUnidirectional = false;
            IsRetainUnion = false;
            IsRetainIntersection = false;
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="isUnidirectional">- true to indicate a unidirectional stream in a join, applicable for joins</param>
        /// <param name="isRetainUnion">- for retaining the union of multiple data windows</param>
        /// <param name="isRetainIntersection">- for retaining the intersection of multiple data windows</param>
        public StreamSpecOptions(bool isUnidirectional, bool isRetainUnion, bool isRetainIntersection) {
            if (isRetainUnion && isRetainIntersection) {
                throw new ArgumentException("Invalid retain flags");
            }
            IsUnidirectional = isUnidirectional;
            IsRetainUnion = isRetainUnion;
            IsRetainIntersection = isRetainIntersection;
        }

        /// <summary>
        /// Indicator for retaining the union of multiple expiry policies.
        /// </summary>
        /// <value>true for retain union</value>
        public bool IsRetainUnion { get; private set; }

        /// <summary>
        /// Indicator for retaining the intersection of multiple expiry policies.
        /// </summary>
        /// <value>true for retain intersection</value>
        public bool IsRetainIntersection { get; private set; }

        /// <summary>
        /// Returns true to indicate a unidirectional stream in a join, applicable for joins.
        /// </summary>
        /// <value>indicator whether the stream is unidirectional in a join</value>
        public bool IsUnidirectional { get; private set; }
    }
} // end of namespace