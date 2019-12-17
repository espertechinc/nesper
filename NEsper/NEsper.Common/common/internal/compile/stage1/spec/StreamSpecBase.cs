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
    ///     Abstract base specification for a stream, consists simply of an optional stream name and a list of views
    ///     on to of the stream.
    ///     <para />
    ///     Implementation classes for views and patterns add additional information defining the
    ///     stream of events.
    /// </summary>
    [Serializable]
    public abstract class StreamSpecBase
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="optionalStreamName">stream name, or null if none supplied</param>
        /// <param name="viewSpecs">specifies what view to use to derive data</param>
        /// <param name="streamSpecOptions">
        ///     indicates additional options such as unidirectional stream or retain-union or
        ///     retain-intersection
        /// </param>
        public StreamSpecBase(
            string optionalStreamName,
            ViewSpec[] viewSpecs,
            StreamSpecOptions streamSpecOptions)
        {
            OptionalStreamName = optionalStreamName;
            ViewSpecs = viewSpecs;
            Options = streamSpecOptions;
        }

        /// <summary>
        ///     Default ctor.
        /// </summary>
        public StreamSpecBase()
        {
            ViewSpecs = ViewSpec.EMPTY_VIEWSPEC_ARRAY;
        }

        /// <summary>
        ///     Returns the name assigned.
        /// </summary>
        /// <returns>stream name or null if not assigned</returns>
        public string OptionalStreamName { get; }

        /// <summary>
        ///     Returns view definitions to use to construct views to derive data on stream.
        /// </summary>
        /// <returns>view defs</returns>
        public ViewSpec[] ViewSpecs { get; }

        /// <summary>
        ///     Returns the options for the stream such as unidirectional, retain-union etc.
        /// </summary>
        /// <returns>stream options</returns>
        public StreamSpecOptions Options { get; }
    }
} // end of namespace