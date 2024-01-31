///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.context.aifactory.createdataflow;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.dataflow.core
{
    public class DataflowDeployment
    {
        private readonly IDictionary<string, DataflowDesc> dataflows = new Dictionary<string, DataflowDesc>(4);

        public void Add(
            string dataflowName,
            DataflowDesc metadata)
        {
            var existing = dataflows.Get(dataflowName);
            if (existing != null) {
                throw new IllegalStateException("Dataflow already found for name '" + dataflowName + "'");
            }

            dataflows.Put(dataflowName, metadata);
        }

        public DataflowDesc GetDataflow(string dataflowName)
        {
            return dataflows.Get(dataflowName);
        }

        public void Remove(string dataflowName)
        {
            dataflows.Remove(dataflowName);
        }

        public bool IsEmpty()
        {
            return dataflows.IsEmpty();
        }

        public IDictionary<string, DataflowDesc> Dataflows => dataflows;
    }
} // end of namespace