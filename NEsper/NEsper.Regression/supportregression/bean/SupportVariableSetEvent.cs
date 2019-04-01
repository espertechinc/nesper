///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.supportregression.bean
{
    public class SupportVariableSetEvent
    {
        private readonly string variableName;
        private readonly string value;
    
        public SupportVariableSetEvent(string variableName, string value)
        {
            this.variableName = variableName;
            this.value = value;
        }

        public string VariableName
        {
            get { return variableName; }
        }

        public string Value
        {
            get { return value; }
        }
    }
}
