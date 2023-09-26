///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Runtime.Serialization;

namespace com.espertech.esper.common.client.module
{
    /// <summary>
    /// Exception indicates a problem when determining delpoyment order and uses-dependency checking.
    /// </summary>
    [Serializable]
    public class ModuleOrderException : Exception
    {
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="message">error message</param>
        public ModuleOrderException(string message)
            : base(message)
        {
        }

        protected ModuleOrderException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
} // end of namespace