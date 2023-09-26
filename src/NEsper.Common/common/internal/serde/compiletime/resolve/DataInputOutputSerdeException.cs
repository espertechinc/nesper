///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Runtime.Serialization;

namespace com.espertech.esper.common.@internal.serde.compiletime.resolve
{
    public class DataInputOutputSerdeException : Exception
    {
        public DataInputOutputSerdeException(string message) : base(message)
        {
        }

        public DataInputOutputSerdeException(
            string message,
            Exception cause) : base(message, cause)
        {
        }

        protected DataInputOutputSerdeException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
} // end of namespace