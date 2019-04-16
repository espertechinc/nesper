///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.common.@internal.epl.dataflow.interfaces;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.client.dataflow.core
{
    /// <summary>
    /// Context for use with <seealso cref="EPDataFlowOperatorProvider" />.
    /// </summary>
    public class EPDataFlowOperatorProviderContext
    {
        private readonly string dataFlowName;
        private readonly string operatorName;
        private readonly DataFlowOperatorFactory factory;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="dataFlowName">data flow name</param>
        /// <param name="operatorName">operator name</param>
        /// <param name="factory">factory</param>
        public EPDataFlowOperatorProviderContext(
            string dataFlowName,
            string operatorName,
            DataFlowOperatorFactory factory)
        {
            this.dataFlowName = dataFlowName;
            this.operatorName = operatorName;
            this.factory = factory;
        }

        /// <summary>
        /// Operator name.
        /// </summary>
        /// <returns>name</returns>
        public string OperatorName {
            get => operatorName;
        }

        /// <summary>
        /// Data flow name
        /// </summary>
        /// <returns>name</returns>
        public string DataFlowName {
            get => dataFlowName;
        }

        /// <summary>
        /// Returns the factory
        /// </summary>
        /// <returns>factory</returns>
        public DataFlowOperatorFactory Factory {
            get => factory;
        }
    }
} // end of namespace