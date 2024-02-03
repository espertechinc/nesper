///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.regressionlib.support.bean
{
    public class SupportEventNode
    {
        public string id;

        public SupportEventNode(string id)
        {
            this.id = id;
        }

        public string Id => id;

        public string Compute(object data)
        {
            if (data == null) {
                return null;
            }

            var nodeData = (SupportEventNodeData) data;
            return id + nodeData.Value;
        }
    }
} // end of namespace