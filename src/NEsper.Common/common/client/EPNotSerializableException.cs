///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Runtime.Serialization;

namespace com.espertech.esper.common.client
{
    public class EPNotSerializableException : EPRuntimeException
    {
        public EPNotSerializableException(Type classType)
            : base("class is not serializable")
        {
            ClassType = classType;
        }

        public EPNotSerializableException(
            Exception cause,
            Type classType)
            : base("class is not serializable", cause)
        {
            ClassType = classType;
        }

        protected EPNotSerializableException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }

        public Type ClassType { get; }
    }
}