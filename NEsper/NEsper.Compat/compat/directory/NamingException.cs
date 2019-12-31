///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.compat.directory
{
    public class NamingException : Exception
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="NamingException" /> class.
        /// </summary>
        public NamingException()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="NamingException" /> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public NamingException(string message)
            : base(message)
        {
        }
    }
}