///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.compile.stage1.spec;

namespace com.espertech.esper.common.@internal.compile.stage2
{
    /// <summary>
    ///     Specification for building an event stream out of a filter for events (supplying type and basic filter criteria)
    ///     and views onto these events which are staggered onto each other to supply a final stream of events.
    /// </summary>
    public class FilterStreamSpecCompiled : StreamSpecBase,
        StreamSpecCompiled
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="filterSpec">specifies what events we are interested in.</param>
        /// <param name="viewSpecs">specifies what view to use to derive data</param>
        /// <param name="optionalStreamName">stream name, or null if none supplied</param>
        /// <param name="streamSpecOptions">additional options such as unidirectional stream in a join</param>
        public FilterStreamSpecCompiled(
            FilterSpecCompiled filterSpec,
            ViewSpec[] viewSpecs,
            string optionalStreamName,
            StreamSpecOptions streamSpecOptions)
            : base(optionalStreamName, viewSpecs, streamSpecOptions)
        {
            FilterSpecCompiled = filterSpec;
        }

        /// <summary>
        ///     Returns filter specification for which events the stream will getSelectListEvents.
        /// </summary>
        /// <returns>filter spec</returns>
        public FilterSpecCompiled FilterSpecCompiled { get; set; }
    }
} // end of namespace