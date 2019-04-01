///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
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

namespace com.espertech.esper.common.@internal.epl.dataflow.util
{
    [DataFlowOpProvideSignal]
    public class DefaultSupportSourceOpForge : DataFlowOperatorForge
    {
        [DataFlowOpParameter] private string name;

        public DataFlowOpForgeInitializeResult InitializeForge(DataFlowOpForgeInitializeContext context)
        {
            return null;
        }

        public CodegenExpression Make(
            CodegenMethodScope parent, SAIFFInitializeSymbol symbols, CodegenClassScope classScope)
        {
            return new SAIFFInitializeBuilder(
                    typeof(DefaultSupportSourceOpFactory), GetType(), "so", parent, symbols, classScope)
                .Constant("name", name)
                .Build();
        }
    }
} // end of namespace