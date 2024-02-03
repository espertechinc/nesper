///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.dataflow.annotations;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.dataflow.interfaces;
using com.espertech.esper.common.@internal.epl.expression.core;

using static com.espertech.esper.common.@internal.epl.dataflow.core.EPDataFlowServiceImpl;

namespace com.espertech.esper.common.@internal.epl.dataflow.ops
{
    [DataFlowOpProvideSignal]
    public class EmitterForge : DataFlowOperatorForge
    {
#pragma warning disable 649
        [DataFlowOpParameter] private ExprNode name;
#pragma warning restore 649

        public DataFlowOpForgeInitializeResult InitializeForge(DataFlowOpForgeInitializeContext context)
        {
            return null;
        }

        public CodegenExpression Make(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            return new SAIFFInitializeBuilder(
                    OP_PACKAGE_NAME + ".emitter.EmitterFactory",
                    GetType(),
                    "emitter",
                    parent,
                    symbols,
                    classScope)
                .Exprnode("name", name)
                .Build();
        }
    }
} // end of namespace