///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.dataflow.interfaces;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.regressionlib.support.dataflow
{
    public class SupportGenericOutputOpWPort : DataFlowOperatorForge,
        DataFlowOperatorFactory,
        DataFlowOperator
    {
        private IList<SupportBean> received = new List<SupportBean>();
        private IList<int?> receivedPorts = new List<int?>();

        public void InitializeFactory(DataFlowOpFactoryInitializeContext context)
        {
        }

        public DataFlowOperator Operator(DataFlowOpInitializeContext context)
        {
            return new SupportGenericOutputOpWPort();
        }

        public DataFlowOpForgeInitializeResult InitializeForge(DataFlowOpForgeInitializeContext context)
        {
            return null;
        }

        public CodegenExpression Make(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            return NewInstance(typeof(SupportGenericOutputOpWPort));
        }

        public void OnInput(
            int port,
            SupportBean @event)
        {
            lock (this) {
                received.Add(@event);
                receivedPorts.Add(port);
            }
        }

        public Pair<IList<SupportBean>, IList<int?>> GetAndReset()
        {
            lock (this) {
                var resultEvents = received;
                var resultPorts = receivedPorts;
                received = new List<SupportBean>();
                receivedPorts = new List<int?>();
                return new Pair<IList<SupportBean>, IList<int?>>(resultEvents, resultPorts);
            }
        }
    }
} // end of namespace