///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace com.espertech.esper.common.@internal.util
{
    /// <summary>
    ///     Exception for resolution of a method failed.
    /// </summary>
    public class MethodResolverNoSuchCtorException : Exception
    {
        [JsonIgnore]
        [NonSerialized]
        private readonly ConstructorInfo nearestMissCtor;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="message">message</param>
        /// <param name="nearestMissCtor">best-match method</param>
        public MethodResolverNoSuchCtorException(
            string message,
            ConstructorInfo nearestMissCtor)
            : base(message)
        {
            this.nearestMissCtor = nearestMissCtor;
        }

        protected MethodResolverNoSuchCtorException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }

        /// <summary>
        ///     Returns the best-match ctor.
        /// </summary>
        /// <returns>ctor</returns>
        public ConstructorInfo NearestMissCtor => nearestMissCtor;
    }
} // end of namespace