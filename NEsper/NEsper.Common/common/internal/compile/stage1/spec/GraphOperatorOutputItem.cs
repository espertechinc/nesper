///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace com.espertech.esper.common.@internal.compile.stage1.spec
{
    [Serializable]
    public class GraphOperatorOutputItem
    {
        public GraphOperatorOutputItem(
            String streamName,
            IList<GraphOperatorOutputItemType> typeInfo)
        {
            StreamName = streamName;
            TypeInfo = typeInfo;
        }

        public string StreamName { get; private set; }

        public IList<GraphOperatorOutputItemType> TypeInfo { get; private set; }
    }
}