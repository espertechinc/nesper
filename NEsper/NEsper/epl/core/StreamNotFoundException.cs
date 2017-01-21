///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;

using com.espertech.esper.collection;

namespace com.espertech.esper.epl.core
{
    /// <summary> Exception to indicate that a stream name could not be resolved.</summary>
    [Serializable]
    public class StreamNotFoundException : StreamTypesException
    {
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="msgGen">The MSG gen.</param>
        public StreamNotFoundException(String message, StreamTypesExceptionSuggestionGen msgGen)
            : base(message, msgGen)
        {
        }
    }
}
