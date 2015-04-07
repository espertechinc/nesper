///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.epl.spec;

namespace com.espertech.esper.client.dataflow
{
    /// <summary>Context for use with <seealso cref="EPDataFlowOperatorProvider" />. </summary>
    public class EPDataFlowOperatorProviderContext
    {
        /// <summary>Ctor. </summary>
        /// <param name="dataFlowName">data flow name</param>
        /// <param name="operatorName">operator name</param>
        /// <param name="spec">specification</param>
        public EPDataFlowOperatorProviderContext(String dataFlowName, String operatorName, GraphOperatorSpec spec)
        {
            DataFlowName = dataFlowName;
            OperatorName = operatorName;
            Spec = spec;
        }

        /// <summary>Operator name. </summary>
        /// <value>name</value>
        public string OperatorName { get; private set; }

        /// <summary>Data flow name </summary>
        /// <value>name</value>
        public string DataFlowName { get; private set; }

        /// <summary>Operator specification </summary>
        /// <value>spec</value>
        public GraphOperatorSpec Spec { get; private set; }
    }
}