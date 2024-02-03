///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Runtime.Serialization;

using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.streamtype
{
    /// <summary>Base class for stream and property name resolution errors.</summary>
    public abstract class StreamTypesException : Exception
    {
        private readonly StreamTypesExceptionSuggestionGen _optionalSuggestionGenerator;

        protected StreamTypesException(
            string message,
            StreamTypesExceptionSuggestionGen optionalSuggestionGenerator)
            : base(message)
        {
            _optionalSuggestionGenerator = optionalSuggestionGenerator;
        }

        protected StreamTypesException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }

        /// <summary>
        ///     Returns the optional suggestion for a matching name.
        /// </summary>
        /// <value>suggested match</value>
        public Pair<int, string> OptionalSuggestion => _optionalSuggestionGenerator?.Suggestion;
    }
} // end of namespace