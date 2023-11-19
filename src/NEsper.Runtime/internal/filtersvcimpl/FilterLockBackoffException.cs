///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Runtime.Serialization;

using com.espertech.esper.common.client;

namespace com.espertech.esper.runtime.@internal.filtersvcimpl
{
    public class FilterLockBackoffException : EPRuntimeException
    {
        public FilterLockBackoffException(string message)
            : base(message)
        {
        }

        public FilterLockBackoffException(
            string message,
            Exception cause)
            : base(message, cause)
        {
        }

        public FilterLockBackoffException(Exception cause)
            : base(cause)
        {
        }

        protected FilterLockBackoffException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
} // end of namespace