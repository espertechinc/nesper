///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.client.hook.condition
{
    /// <summary>
    /// Context provided to <see cref="ConditionHandlerFactory" /> implementations 
    /// providing engine contextual information. </summary>
    public class ConditionHandlerFactoryContext
    {
        /// <summary>Ctor. </summary>
        /// <param name="engineURI">engine URI</param>
        public ConditionHandlerFactoryContext(string engineURI)
        {
            EngineURI = engineURI;
        }

        /// <summary>Returns the engine URI. </summary>
        /// <value>engine URI</value>
        public string EngineURI { get; private set; }
    }
}