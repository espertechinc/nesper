///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.dataflow.interfaces;
using com.espertech.esper.common.@internal.epl.dataflow.realize;
using com.espertech.esper.common.@internal.epl.dataflow.util;

namespace com.espertech.esper.common.@internal.context.aifactory.createdataflow
{
    public class DataflowDesc
    {
        public string DataflowName { get; set; }

        public IDictionary<int, DataFlowOperatorFactory> OperatorFactories { get; set; }

        public IDictionary<int, OperatorMetadataDescriptor> OperatorMetadata { get; set; }

        public ISet<int> OperatorBuildOrder { get; set; }

        public IDictionary<string, EventType> DeclaredTypes { get; set; }

        public StatementContext StatementContext { get; set; }

        public IList<LogicalChannel> LogicalChannels { get; set; }
    }
} // end of namespace