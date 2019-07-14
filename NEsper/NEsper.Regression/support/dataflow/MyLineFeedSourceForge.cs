///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.regressionlib.support.dataflow
{
    // The OutputTypes annotation can be used to specify the type of events
    // that are output by the operator.
    // If provided, it is not necessary to declare output types in the data flow.
    // The event representation is object-array.
    [OutputTypes]
    [OutputType(Name = "line", TypeName = "string")]

    // Provide the DataFlowOpProvideSignal annotation to indicate that
    // the source operator provides a final marker.
    [DataFlowOpProvideSignal]
    public class MyLineFeedSourceForge : DataFlowOperatorForge
    {
        public DataFlowOpForgeInitializeResult InitializeForge(DataFlowOpForgeInitializeContext context)
        {
            return null;
        }

        public CodegenExpression Make(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            return NewInstance(typeof(MyLineFeedSourceFactory));
        }
    }
} // end of namespace