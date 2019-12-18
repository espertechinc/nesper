namespace com.espertech.esper.regressionlib.support.bean
{
    ///////////////////////////////////////////////////////////////////////////////////////
    // Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
    // http://esper.codehaus.org                                                          /
    // ---------------------------------------------------------------------------------- /
    // The software in this package is published under the terms of the GPL license       /
    // a copy of which has been included with this distribution in the license.txt file.  /
    ///////////////////////////////////////////////////////////////////////////////////////


    public class SupportVariableSetEvent
    {
        public SupportVariableSetEvent(
            string variableName,
            string value)
        {
            VariableName = variableName;
            Value = value;
        }

        public string VariableName { get; }

        public string Value { get; }
    }
} // end of namespace