///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.@internal.compile.stage1.spec
{
    /// <summary>
    ///     Unvalided filter-based stream specification.
    /// </summary>
    [Serializable]
    public class FilterStreamSpecRaw : StreamSpecBase,
        StreamSpecRaw
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="rawFilterSpec">is unvalidated filter specification</param>
        /// <param name="viewSpecs">is the view definition</param>
        /// <param name="optionalStreamName">is the stream name if supplied, or null if not supplied</param>
        /// <param name="streamSpecOptions">additional options, such as unidirectional stream in a join</param>
        public FilterStreamSpecRaw(
            FilterSpecRaw rawFilterSpec,
            ViewSpec[] viewSpecs,
            string optionalStreamName,
            StreamSpecOptions streamSpecOptions)
            : base(optionalStreamName, viewSpecs, streamSpecOptions)
        {
            RawFilterSpec = rawFilterSpec;
        }

        /// <summary>
        ///     Default ctor.
        /// </summary>
        public FilterStreamSpecRaw()
        {
        }

        /// <summary>
        ///     Returns the unvalided filter spec.
        /// </summary>
        /// <returns>filter def</returns>
        public FilterSpecRaw RawFilterSpec { get; }
    }
} // end of namespace