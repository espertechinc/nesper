///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
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
    ///     Assembly factory node for an event stream that is a root with a two or more child nodes below it.
    /// </summary>
    public class RootCartProdAssemblyNodeFactory : BaseAssemblyNodeFactory
    {
        private readonly int[] childStreamIndex; // maintain mapping of stream number to index in array
        private readonly bool allSubStreamsOptional;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="streamNum">is the stream number</param>
        /// <param name="numStreams">is the number of streams</param>
        /// <param name="allSubStreamsOptional">true if all substreams are optional and none are required</param>
        public RootCartProdAssemblyNodeFactory(
            int streamNum,
            int numStreams,
            bool allSubStreamsOptional)
            : base(
                streamNum,
                numStreams)
        {
            this.allSubStreamsOptional = allSubStreamsOptional;
            childStreamIndex = new int[numStreams];
        }

        public override void AddChild(BaseAssemblyNodeFactory childNode)
        {
            childStreamIndex[childNode.StreamNum] = childNodes.Count;
            base.AddChild(childNode);
        }

        public override void Print(IndentWriter indentWriter)
        {
            indentWriter.WriteLine("RootCartProdAssemblyNode streamNum=" + streamNum);
        }

        public override BaseAssemblyNode MakeAssemblerUnassociated()
        {
            return new RootCartProdAssemblyNode(streamNum, numStreams, allSubStreamsOptional, childStreamIndex);
        }

        public override CodegenExpression Make(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            return NewInstance<RootCartProdAssemblyNodeFactory>(
                Constant(streamNum),
                Constant(numStreams),
                Constant(allSubStreamsOptional));
        }
    }
} // end of namespace