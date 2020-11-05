///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Runtime.Serialization;

namespace com.espertech.esper.container
{
    [Serializable]
    public class ItemNotFoundException : Exception
    {
        public ItemNotFoundException()
        {
        }

        public ItemNotFoundException(string message) : base(message)
        {
        }

        public ItemNotFoundException(string message,
            Exception innerException) : base(message, innerException)
        {
        }

        protected ItemNotFoundException(SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}