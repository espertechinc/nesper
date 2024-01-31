///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
    public class MethodResolverNoSuchMethodException : Exception
    {
        [JsonIgnore]
        [NonSerialized]
        private readonly MethodInfo nearestMissMethod;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="message">message</param>
        /// <param name="nearestMissMethod">best-match method</param>
        public MethodResolverNoSuchMethodException(
            string message,
            MethodInfo nearestMissMethod)
            : base(message)
        {
            this.nearestMissMethod = nearestMissMethod;
        }

        protected MethodResolverNoSuchMethodException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }

        /// <summary>
        ///     Returns the best-match method.
        /// </summary>
        /// <returns>method</returns>
        public MethodInfo NearestMissMethod => nearestMissMethod;
    }
} // end of namespace