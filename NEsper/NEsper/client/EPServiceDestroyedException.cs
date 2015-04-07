///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.client
{
    /// <summary>
    /// This exception is thrown to indicate that the EPServiceProvider (engine) instance has been destroyed. 
    /// <para/> 
    /// This exception applies to destroyed engine instances when a client attempts to receive the runtime or administrative interfaces from a destroyed engine instance.
    /// </summary>
    public class EPServiceDestroyedException : Exception
    {
        /// <summary>Ctor. </summary>
        /// <param name="engineURI">engine URI</param>
        public EPServiceDestroyedException(String engineURI)
            : base("EPServiceProvider has already been destroyed for engine URI '" + engineURI + "'")
        {
        }
    }
}
