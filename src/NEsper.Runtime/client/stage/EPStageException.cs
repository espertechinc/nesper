using System;

namespace com.espertech.esper.runtime.client.stage
{
    ///////////////////////////////////////////////////////////////////////////////////////
    // Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
    // http://esper.codehaus.org                                                          /
    // ---------------------------------------------------------------------------------- /
    // The software in this package is published under the terms of the GPL license       /
    // a copy of which has been included with this distribution in the license.txt file.  /
    ///////////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    ///     Stage base exception.
    /// </summary>
    public class EPStageException : Exception
    {
	    /// <summary>
	    ///     Ctor.
	    /// </summary>
	    /// <param name="message">message</param>
	    public EPStageException(string message) : base(message)
        {
        }
    }
} // end of namespace