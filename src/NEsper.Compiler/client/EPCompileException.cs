///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using com.espertech.esper.compat.collections;

namespace com.espertech.esper.compiler.client
{
    /// <summary>
    ///     Indicates an exception compiling a module or fire-and-forget query
    ///     <para />
    ///     May carry information on individual items.
    /// </summary>
    public class EPCompileException : Exception
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="message">message</param>
        public EPCompileException(string message)
            : base(message)
        {
            Items = new EmptyList<EPCompileExceptionItem>();
        }

        /// <summary>
        ///     Ctor
        /// </summary>
        /// <param name="message">message</param>
        /// <param name="cause">cause</param>
        public EPCompileException(
            string message,
            Exception cause)
            : base(message, cause)
        {
            Items = new EmptyList<EPCompileExceptionItem>();
        }

        /// <summary>
        ///     Ctor
        /// </summary>
        /// <param name="message">message</param>
        /// <param name="cause">cause</param>
        /// <param name="items">additional information on items</param>
        public EPCompileException(
            string message,
            Exception cause,
            IList<EPCompileExceptionItem> items)
            : base(message, cause)
        {
            Items = items;
        }

        protected EPCompileException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }

        /// <summary>
        ///     Returns compilation items.
        /// </summary>
        /// <returns>items</returns>
        public IList<EPCompileExceptionItem> Items { get; }
    }
} // end of namespace