///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.util;

namespace com.espertech.esper.epl.spec
{
    /// <summary>
    /// Specification for a stream, consists simply of an optional stream name and a list 
    /// of views on to of the stream. <para/>Implementation classes for views and patterns 
    /// add additional information defining the stream of events.
    /// </summary>
    public interface StreamSpec : MetaDefItem
    {
        /// <summary>Returns the stream name, or null if undefined. </summary>
        /// <value>stream name</value>
        string OptionalStreamName { get; }

        /// <summary>Returns views definitions onto the stream </summary>
        /// <value>view defs</value>
        ViewSpec[] ViewSpecs { get; }

        /// <summary>Returns the options for the stream such as unidirectional, retain-union etc. </summary>
        /// <value>stream options</value>
        StreamSpecOptions Options { get; }
    }
}
