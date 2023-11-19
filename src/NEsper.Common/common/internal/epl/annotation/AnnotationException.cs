///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Runtime.Serialization;

using com.espertech.esper.common.client;

namespace com.espertech.esper.common.@internal.epl.annotation
{
    /// <summary>
    ///     Thrown to indicate a problem processing an EPL statement annotation.
    /// </summary>
    public class AnnotationException : EPException
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="message">error message</param>
        public AnnotationException(string message) : base(message)
        {
        }

        protected AnnotationException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
} // end of namespace