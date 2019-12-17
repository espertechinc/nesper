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
    ///     Assembly node for an event stream that is a branch with a two or more child nodes (required and optional) below it.
    /// </summary>
    public class CartesianProdAssemblyNodeFactory : BaseAssemblyNodeFactory
    {
        private readonly bool allSubStreamsOptional;
        private readonly int[] childStreamIndex; // maintain mapping of stream number to index in array

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="streamNum">is the stream number</param>
        /// <param name="numStreams">is the number of streams</param>
        /// <param name="allSubStreamsOptional">
        ///     true if all child nodes to this node are optional, or false ifone or more child nodes are required for a result.
        /// </param>
        public CartesianProdAssemblyNodeFactory(
            int streamNum,
            int numStreams,
            bool allSubStreamsOptional)
            : base(
                streamNum,
                numStreams)
        {
            childStreamIndex = new int[numStreams];
            this.allSubStreamsOptional = allSubStreamsOptional;
        }

        public override void AddChild(BaseAssemblyNodeFactory childNode)
        {
            childStreamIndex[childNode.StreamNum] = childNodes.Count;
            base.AddChild(childNode);
        }

        public override void Print(IndentWriter indentWriter)
        {
            indentWriter.WriteLine("CartesianProdAssemblyNode streamNum=" + streamNum);
        }

        public override BaseAssemblyNode MakeAssemblerUnassociated()
        {
            return new CartesianProdAssemblyNode(streamNum, numStreams, allSubStreamsOptional, childStreamIndex);
        }

        public override CodegenExpression Make(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            return NewInstance<CartesianProdAssemblyNodeFactory>(
                Constant(streamNum),
                Constant(numStreams),
                Constant(allSubStreamsOptional));
        }
    }
} // end of namespace