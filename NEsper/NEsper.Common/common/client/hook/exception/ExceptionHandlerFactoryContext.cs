///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.client.hook.exception
{
    /// <summary>
    /// Context provided to <see cref="ExceptionHandlerFactory"/> implementations 
    /// providing engine contextual information.
    /// </summary>
    public class ExceptionHandlerFactoryContext
    {
        /// <summary>Ctor. </summary>
        /// <param name="engineURI">engine URI</param>
        public ExceptionHandlerFactoryContext(string engineURI)
        {
            EngineURI = engineURI;
        }

        /// <summary>Returns the engine URI. </summary>
        /// <value>engine URI</value>
        public string EngineURI { get; private set; }
    }
}