///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Runtime.Serialization;

namespace com.espertech.esper.compat.concurrency
{
    public class ValueNotAvailableException : Exception
    {
        public ValueNotAvailableException()
        {
        }

        public ValueNotAvailableException(string message) : base(message)
        {
        }

        public ValueNotAvailableException(
            string message,
            Exception innerException) : base(message, innerException)
        {
        }

        protected ValueNotAvailableException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}