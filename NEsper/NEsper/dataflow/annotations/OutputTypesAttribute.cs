///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.dataflow.annotations
{
#if DO_NOT_USE
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class | AttributeTargets.Struct)]
    public class OutputTypesAttribute : Attribute
    {
        public int PortNumber { get; set; }

        public OutputTypesAttribute(int portNumber)
        {
            PortNumber = portNumber;
        }

        public OutputTypesAttribute()
        {
            PortNumber = 0;
        }
    }
#endif
}
