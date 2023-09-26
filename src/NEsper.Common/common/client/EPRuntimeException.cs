﻿///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Runtime.Serialization;

namespace com.espertech.esper.common.client
{
    public class EPRuntimeException : EPException
    {
        private static readonly Type MyType =
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType;

        /// <summary> Ctor.</summary>
        /// <param name="message">error message
        /// </param>
        public EPRuntimeException(string message)
            : base(message)
        {
        }

        /// <summary> Ctor for an inner exception and message.</summary>
        /// <param name="message">error message
        /// </param>
        /// <param name="cause">inner exception
        /// </param>
        public EPRuntimeException(
            string message,
            Exception cause)
            : base(message, cause)
        {
        }

        /// <summary> Ctor - just an inner exception.</summary>
        /// <param name="cause">inner exception
        /// </param>
        public EPRuntimeException(Exception cause)
            : base(MyType.FullName + ": " + cause.Message, cause)
        {
        }

        protected EPRuntimeException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}