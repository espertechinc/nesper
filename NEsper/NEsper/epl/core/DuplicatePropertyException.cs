///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;

namespace com.espertech.esper.epl.core
{
	/// <summary>
	/// Indicates a property exists in multiple streams.
	/// </summary>
    [Serializable]
    public class DuplicatePropertyException : StreamTypesException
    {
        /// <summary> Ctor.</summary>
        /// <param name="msg">exception message
        /// </param>
        public DuplicatePropertyException(String msg)
            : base(msg, null)
        {
        }
    }
}
