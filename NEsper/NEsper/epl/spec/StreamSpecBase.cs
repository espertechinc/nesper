///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.util;

namespace com.espertech.esper.epl.spec
{
    /// <summary>
    /// Abstract base specification for a stream, consists simply of an optional stream name 
    /// and a list of views on to of the stream. <para/>Implementation classes for views and 
    /// patterns add additional information defining the stream of events.
    /// </summary>
    [Serializable]
    public abstract class StreamSpecBase : MetaDefItem
    {
        /// <summary>Ctor. </summary>
        /// <param name="optionalStreamName">stream name, or null if none supplied</param>
        /// <param name="viewSpecs">specifies what view to use to derive data</param>
        /// <param name="streamSpecOptions">indicates additional options such as unidirectional stream or retain-union or retain-intersection</param>
        protected StreamSpecBase(String optionalStreamName, ViewSpec[] viewSpecs, StreamSpecOptions streamSpecOptions)
        {
            OptionalStreamName = optionalStreamName;
            ViewSpecs = viewSpecs;
            Options = streamSpecOptions;
        }
    
        /// <summary>Default ctor. </summary>
        protected StreamSpecBase()
        {
            ViewSpecs = ViewSpec.EMPTY_VIEWSPEC_ARRAY;
        }

        /// <summary>Returns the name assigned. </summary>
        /// <value>stream name or null if not assigned</value>
        public string OptionalStreamName { get; private set; }

        /// <summary>Returns view definitions to use to construct views to derive data on stream. </summary>
        /// <value>view defs</value>
        public ViewSpec[] ViewSpecs { get; private set; }

        /// <summary>Returns the options for the stream such as unidirectional, retain-union etc. </summary>
        /// <value>stream options</value>
        public StreamSpecOptions Options { get; private set; }
    }
}
