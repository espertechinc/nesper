///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.dataflow.util;
using com.espertech.esper.common.@internal.epl.dataflow.interfaces;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.runtime.@internal.dataflow.op.emitter
{
    public class EmitterFactory : DataFlowOperatorFactory
    {
        private ExprEvaluator name;

        public ExprEvaluator Name {
            set => name = value;
        }

        public void InitializeFactory(DataFlowOpFactoryInitializeContext context)
        {
        }

        public DataFlowOperator Operator(DataFlowOpInitializeContext context)
        {
            var nameText = DataFlowParameterResolution.ResolveStringOptional("name", name, context);
            return new EmitterOp(nameText);
        }
    }
} // end of namespace