///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.regressionlib.support.bean
{
    public class SupportEventNodeData
    {
        public string nodeId;
        public string value;

        public SupportEventNodeData(
            string nodeId,
            string value)
        {
            this.nodeId = nodeId;
            this.value = value;
        }

        public string NodeId => nodeId;

        public string Value => value;
    }
} // end of namespace