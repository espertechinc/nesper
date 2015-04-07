///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.filter;

namespace com.espertech.esper.epl.spec
{
    /// <summary>
    /// Specification for building an event stream out of a filter for events (supplying 
    /// type and basic filter criteria) and views onto these events which are staggered onto 
    /// each other to supply a readonly stream of events.
    /// </summary>
    [Serializable]
    public class FilterStreamSpecCompiled : StreamSpecBase, StreamSpecCompiled
    {
        /// <summary>Ctor. </summary>
        /// <param name="filterSpec">specifies what events we are interested in.</param>
        /// <param name="viewSpecs">specifies what view to use to derive data</param>
        /// <param name="optionalStreamName">stream name, or null if none supplied</param>
        /// <param name="streamSpecOptions">additional options such as unidirectional stream in a join</param>
        public FilterStreamSpecCompiled(FilterSpecCompiled filterSpec, ViewSpec[] viewSpecs, String optionalStreamName, StreamSpecOptions streamSpecOptions)
            : base(optionalStreamName, viewSpecs, streamSpecOptions)
        {
            FilterSpec = filterSpec;
        }

        /// <summary>Returns filter specification for which events the stream will getSelectListEvents. </summary>
        /// <value>filter spec</value>
        public FilterSpecCompiled FilterSpec { get; set; }
    }
}
