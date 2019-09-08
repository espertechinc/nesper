///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.client.module
{
    /// <summary>
    /// Exception thrown when an EPL text could not be parsed.
    /// </summary>
    [Serializable]
    public class ParseException : Exception
    {
        /// <summary>Ctor. </summary>
        /// <param name="message">error message</param>
        public ParseException(string message)
            : base(message)
        {
        }
    }
}