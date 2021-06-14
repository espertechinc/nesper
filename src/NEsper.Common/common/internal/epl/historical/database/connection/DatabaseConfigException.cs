///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;

namespace com.espertech.esper.common.@internal.epl.historical.database.connection
{
    /// <summary>
    /// Exception to indicate that a stream name could not be resolved.
    /// </summary>
    [Serializable]
    public class DatabaseConfigException : System.Exception
    {
        /// <summary> Ctor.</summary>
        /// <param name="msg">message
        /// </param>
        public DatabaseConfigException(string msg)
            : base(msg)
        {
        }

        /// <summary> Ctor.</summary>
        /// <param name="message">error message
        /// </param>
        /// <param name="cause">cause is the inner exception
        /// </param>
        public DatabaseConfigException(
            string message,
            System.Exception cause)
            : base(message, cause)
        {
        }
    }
}