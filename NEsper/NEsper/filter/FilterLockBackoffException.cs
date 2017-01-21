///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Runtime.Serialization;

namespace com.espertech.esper.filter
{
    [Serializable]
    public class FilterLockBackoffException : Exception
    {
        public FilterLockBackoffException()
        {
        }

        public FilterLockBackoffException(string message) : base(message)
        {
        }

        public FilterLockBackoffException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected FilterLockBackoffException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
