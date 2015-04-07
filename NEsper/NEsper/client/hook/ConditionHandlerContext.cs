///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.client.hook
{
    /// <summary>
    /// Context provided to <see cref="ConditionHandler" /> implementations providing 
    /// engine-condition-contextual information. 
    /// <para/>
    /// Statement information pertains to the statement currently being processed when 
    /// the condition occured.
    /// </summary>
    public class ConditionHandlerContext
    {
        /// <summary>Ctor. </summary>
        /// <param name="engineURI">engine URI</param>
        /// <param name="statementName">statement name</param>
        /// <param name="epl">statement EPL expression text</param>
        /// <param name="engineCondition">condition reported</param>
        public ConditionHandlerContext(String engineURI, String statementName, String epl, BaseCondition engineCondition)
        {
            EngineURI = engineURI;
            StatementName = statementName;
            Epl = epl;
            EngineCondition = engineCondition;
        }

        /// <summary>Returns the engine URI. </summary>
        /// <value>engine URI</value>
        public string EngineURI { get; private set; }

        /// <summary>Returns the statement name, if provided, or the statement id assigned to the statement if no name was provided. </summary>
        /// <value>statement name or id</value>
        public string StatementName { get; private set; }

        /// <summary>Returns the expression text of the statement. </summary>
        /// <value>statement.</value>
        public string Epl { get; private set; }

        /// <summary>Returns the condition reported. </summary>
        /// <value>condition reported</value>
        public BaseCondition EngineCondition { get; private set; }
    }
}
