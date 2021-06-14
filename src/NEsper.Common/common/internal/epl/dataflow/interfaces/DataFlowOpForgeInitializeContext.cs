///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.container;

namespace com.espertech.esper.common.@internal.epl.dataflow.interfaces
{
    public class DataFlowOpForgeInitializeContext
    {
        public DataFlowOpForgeInitializeContext(
            IContainer container,
            string dataflowName,
            int operatorNumber,
            Attribute[] operatorAnnotations,
            GraphOperatorSpec operatorSpec,
            IDictionary<int, DataFlowOpInputPort> inputPorts,
            IDictionary<int, DataFlowOpOutputPort> outputPorts,
            DataFlowOpForgeCodegenEnv codegenEnv,
            StatementBaseInfo @base,
            StatementCompileTimeServices services)
        {
            DataflowName = dataflowName;
            OperatorNumber = operatorNumber;
            OperatorAnnotations = operatorAnnotations;
            OperatorSpec = operatorSpec;
            InputPorts = inputPorts;
            OutputPorts = outputPorts;
            CodegenEnv = codegenEnv;
            Base = @base;
            Services = services;
            Container = container;
        }

        public string DataflowName { get; }

        public GraphOperatorSpec OperatorSpec { get; }

        public StatementBaseInfo Base { get; }

        public StatementCompileTimeServices Services { get; }

        public StatementRawInfo StatementRawInfo => Base.StatementRawInfo;

        public int OperatorNumber { get; }

        public DataFlowOpForgeCodegenEnv CodegenEnv { get; }

        public IDictionary<int, DataFlowOpInputPort> InputPorts { get; }

        public IDictionary<int, DataFlowOpOutputPort> OutputPorts { get; }

        public Attribute[] OperatorAnnotations { get; }

        public IContainer Container { get; }
    }
} // end of namespace