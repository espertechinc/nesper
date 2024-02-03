///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Runtime.Serialization;

namespace com.espertech.esper.common.@internal.@event.propertyparser
{
    public class PropertyParseNodepException : Exception
    {
        public PropertyParseNodepException(string message)
            : base(message)
        {
        }

        protected PropertyParseNodepException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
} // end of namespace