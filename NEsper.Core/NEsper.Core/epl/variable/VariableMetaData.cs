///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;

namespace com.espertech.esper.epl.variable
{
    [Serializable]
    public class VariableMetaData
    {
        public VariableMetaData(
            String variableName,
            String contextPartitionName,
            int variableNumber,
            Type type,
            EventType eventType,
            bool constant,
            VariableStateFactory variableStateFactory)
        {
            VariableName = variableName;
            ContextPartitionName = contextPartitionName;
            VariableNumber = variableNumber;
            VariableType = type;
            EventType = eventType;
            IsConstant = constant;
            VariableStateFactory = variableStateFactory;
        }

        /// <summary>Returns the variable name. </summary>
        /// <value>variable name</value>
        public string VariableName { get; private set; }

        public string ContextPartitionName { get; private set; }

        /// <summary>Returns the variable number. </summary>
        /// <value>variable index number</value>
        public int VariableNumber { get; private set; }

        /// <summary>Returns the type of the variable. </summary>
        /// <value>type</value>
        public Type VariableType { get; private set; }

        /// <summary>
        ///     Returns the event type if the variable hold Event(s).
        /// </summary>
        /// <value>type</value>
        public EventType EventType { get; private set; }

        public bool IsConstant { get; private set; }

        public VariableStateFactory VariableStateFactory { get; private set; }
    }
}