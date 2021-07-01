///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.client.hook.exception
{
    /// <summary>
    /// Context provided to <see cref="ExceptionHandlerFactory"/> implementations 
    /// providing runtime contextual information.
    /// </summary>
    public class ExceptionHandlerFactoryContext
    {
        /// <summary>Ctor. </summary>
        /// <param name="runtimeUri">runtime URI</param>
        public ExceptionHandlerFactoryContext(string runtimeUri)
        {
            RuntimeURI = runtimeUri;
        }

        /// <summary>Returns the runtime URI. </summary>
        /// <value>runtime URI</value>
        public string RuntimeURI { get; private set; }
    }
}