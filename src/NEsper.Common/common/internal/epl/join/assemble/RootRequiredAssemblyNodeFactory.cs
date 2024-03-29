///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.util;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.join.assemble
{
    /// <summary>
    ///     Assembly node factory for an event stream that is a root with a one required child node below it.
    /// </summary>
    public class RootRequiredAssemblyNodeFactory : BaseAssemblyNodeFactory
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="streamNum">is the stream number</param>
        /// <param name="numStreams">is the number of streams</param>
        public RootRequiredAssemblyNodeFactory(
            int streamNum,
            int numStreams)
            : base(streamNum, numStreams)
        {
        }

        public override void Print(IndentWriter indentWriter)
        {
            indentWriter.WriteLine("RootRequiredAssemblyNode streamNum=" + streamNum);
        }

        public override BaseAssemblyNode MakeAssemblerUnassociated()
        {
            return new RootRequiredAssemblyNode(streamNum, numStreams);
        }

        public override CodegenExpression Make(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            return NewInstance<RootRequiredAssemblyNodeFactory>(Constant(streamNum), Constant(numStreams));
        }
    }
} // end of namespace