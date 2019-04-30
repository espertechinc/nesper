///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;

namespace com.espertech.esper.common.@internal.util
{
    /// <summary> Exception to indicate a parse error in parsing placeholders.</summary>
    [Serializable]
    public class PlaceholderParseException : System.Exception
    {
        /// <summary> Ctor.</summary>
        /// <param name="message">is the error message
        /// </param>
        public PlaceholderParseException(string message)
            : base(message)
        {
        }
    }
}