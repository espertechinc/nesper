///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.bytecodemodel.util;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.@join.assemble;
using com.espertech.esper.common.@internal.epl.@join.queryplanbuild;
using com.espertech.esper.common.@internal.epl.@join.queryplanouter;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.join.queryplan
{
    /// <summary>
    ///     Query plan for executing a set of lookup instructions and assembling an end result via
    ///     a set of assembly instructions.
    /// </summary>
    public class LookupInstructionQueryPlanNodeForge : QueryPlanNodeForge
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="rootStream">is the stream supplying the lookup event</param>
        /// <param name="rootStreamName">is the name of the stream supplying the lookup event</param>
        /// <param name="numStreams">is the number of streams</param>
        /// <param name="lookupInstructions">is a list of lookups to perform</param>
        /// <param name="requiredPerStream">indicates which streams are required and which are optional in the lookup</param>
        /// <param name="assemblyInstructionFactories">is the bottom-up assembly factory nodes to assemble a lookup result nodes</param>
        public LookupInstructionQueryPlanNodeForge(
            int rootStream,
            string rootStreamName,
            int numStreams,
            bool[] requiredPerStream,
            IList<LookupInstructionPlanForge> lookupInstructions,
            IList<BaseAssemblyNodeFactory> assemblyInstructionFactories)
        {
            RootStream = rootStream;
            RootStreamName = rootStreamName;
            LookupInstructions = lookupInstructions;
            NumStreams = numStreams;
            RequiredPerStream = requiredPerStream;
            AssemblyInstructionFactories = assemblyInstructionFactories;
        }

        public int RootStream { get; }

        public string RootStreamName { get; }

        public int NumStreams { get; }

        public IList<LookupInstructionPlanForge> LookupInstructions { get; }

        public bool[] RequiredPerStream { get; }

        public IList<BaseAssemblyNodeFactory> AssemblyInstructionFactories { get; }

        public override void AddIndexes(HashSet<TableLookupIndexReqKey> usedIndexes)
        {
            foreach (var plan in LookupInstructions) {
                plan.AddIndexes(usedIndexes);
            }
        }

        public override CodegenExpression Make(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = MakeInstructions(AssemblyInstructionFactories, parent, symbols, classScope);
            return NewInstance<LookupInstructionQueryPlanNode>(
                Constant(RootStream),
                Constant(RootStreamName),
                Constant(NumStreams),
                Constant(RequiredPerStream),
                CodegenMakeableUtil.MakeArray(
                    "lookupInstructions",
                    typeof(LookupInstructionPlan),
                    LookupInstructions.ToArray(),
                    GetType(),
                    parent,
                    symbols,
                    classScope),
                LocalMethod(method));
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="factories">factories</param>
        /// <param name="parents">parents indexes</param>
        /// <param name="children">children indexes</param>
        public static void AssembleFactoriesIntoTree(
            BaseAssemblyNodeFactory[] factories,
            int[] parents,
            int[][] children)
        {
            for (var i = 0; i < parents.Length; i++) {
                if (parents[i] != -1) {
                    factories[i].Parent = factories[parents[i]];
                }
            }

            for (var i = 0; i < children.Length; i++) {
                for (var child = 0; child < children[i].Length; child++) {
                    factories[i].AddChild(factories[children[i][child]]);
                }
            }
        }

        private CodegenMethod MakeInstructions(
            IList<BaseAssemblyNodeFactory> factories,
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(BaseAssemblyNodeFactory[]), GetType(), classScope);

            var parents = new int[factories.Count];
            var children = new int[factories.Count][];
            for (var i = 0; i < factories.Count; i++) {
                var factory = factories[i];
                parents[i] = factory.ParentNode == null ? -1 : FindFactoryChecked(factory.ParentNode, factories);
                children[i] = new int[factory.ChildNodes.Count];
                for (var child = 0; child < factory.ChildNodes.Count; child++) {
                    children[i][child] = FindFactoryChecked(factory.ChildNodes[child], factories);
                }
            }

            method.Block
                .DeclareVar<BaseAssemblyNodeFactory[]>(
                    "factories",
                    CodegenMakeableUtil.MakeArray(
                        "assemblyInstructions",
                        typeof(BaseAssemblyNodeFactory),
                        factories.ToArray(),
                        GetType(),
                        parent,
                        symbols,
                        classScope))
                .StaticMethod(
                    typeof(LookupInstructionQueryPlanNodeForge),
                    "AssembleFactoriesIntoTree",
                    Ref("factories"),
                    Constant(parents),
                    Constant(children))
                .MethodReturn(Ref("factories"));

            return method;
        }

        protected internal override void Print(IndentWriter writer)
        {
            writer.WriteLine(
                "LookupInstructionQueryPlanNode" +
                " rootStream=" +
                RootStream +
                " requiredPerStream=" +
                RequiredPerStream.RenderAny());

            writer.IncrIndent();
            for (var i = 0; i < LookupInstructions.Count; i++) {
                writer.WriteLine("lookup step " + i);
                writer.IncrIndent();
                LookupInstructions[i].Print(writer);
                writer.DecrIndent();
            }

            writer.DecrIndent();

            writer.IncrIndent();
            for (var i = 0; i < AssemblyInstructionFactories.Count; i++) {
                writer.WriteLine("assembly step " + i);
                writer.IncrIndent();
                AssemblyInstructionFactories[i].Print(writer);
                writer.DecrIndent();
            }

            writer.DecrIndent();
        }

        public override void Accept(QueryPlanNodeForgeVisitor visitor)
        {
            visitor.Visit(this);
        }

        private int FindFactoryChecked(
            BaseAssemblyNodeFactory node,
            IList<BaseAssemblyNodeFactory> factories)
        {
            var index = factories.IndexOf(node);
            if (index == -1) {
                throw new UnsupportedOperationException("Assembly factory not found among list");
            }

            return index;
        }
    }
} // end of namespace