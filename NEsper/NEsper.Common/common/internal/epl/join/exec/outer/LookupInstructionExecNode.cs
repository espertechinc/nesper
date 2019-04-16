///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.@join.assemble;
using com.espertech.esper.common.@internal.epl.@join.rep;
using com.espertech.esper.common.@internal.epl.@join.strategy;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.join.exec.outer
{
    /// <summary>
    ///     Execution for a set of lookup instructions and for a set of result assemble instructions to perform
    ///     joins and construct a complex result.
    /// </summary>
    public class LookupInstructionExecNode : ExecNode
    {
        private readonly BaseAssemblyNode[] assemblyInstructions;
        private readonly LookupInstructionExec[] lookupInstructions;
        private readonly MyResultAssembler myResultAssembler;
        private readonly int numStreams;
        private readonly bool[] requiredPerStream;
        private readonly int rootStream;
        private readonly string rootStreamName;
        private readonly int requireResultsInstruction;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="rootStream">is the stream supplying the lookup event</param>
        /// <param name="rootStreamName">is the name of the stream supplying the lookup event</param>
        /// <param name="numStreams">is the number of streams</param>
        /// <param name="lookupInstructions">is a list of lookups to perform</param>
        /// <param name="requiredPerStream">indicates which streams are required and which are optional in the lookup</param>
        /// <param name="assemblyInstructionFactories">factories for assembly</param>
        public LookupInstructionExecNode(
            int rootStream,
            string rootStreamName,
            int numStreams,
            LookupInstructionExec[] lookupInstructions,
            bool[] requiredPerStream,
            BaseAssemblyNodeFactory[] assemblyInstructionFactories)
        {
            this.rootStream = rootStream;
            this.rootStreamName = rootStreamName;
            this.numStreams = numStreams;
            this.lookupInstructions = lookupInstructions;
            this.requiredPerStream = requiredPerStream;

            // We have a list of factories that are pointing to each other in a tree, i.e.:
            // F1 (=>F3), F2 (=>F3), F3
            IDictionary<BaseAssemblyNodeFactory, BaseAssemblyNode> nodes =
                new IdentityDictionary<BaseAssemblyNodeFactory, BaseAssemblyNode>();
            foreach (var factory in assemblyInstructionFactories) {
                var node = factory.MakeAssemblerUnassociated();
                nodes.Put(factory, node);
            }

            // re-associate each node after allocation
            foreach (var nodeWithFactory in nodes) {
                var parentFactory = nodeWithFactory.Key.ParentNode;
                if (parentFactory != null) {
                    var parent = nodes.Get(parentFactory);
                    nodeWithFactory.Value.ParentAssembler = parent;
                }

                foreach (var childNodeFactory in nodeWithFactory.Key.ChildNodes) {
                    var child = nodes.Get(childNodeFactory);
                    nodeWithFactory.Value.AddChild(child);
                }
            }

            assemblyInstructions = new BaseAssemblyNode[assemblyInstructionFactories.Length];
            for (var i = 0; i < assemblyInstructionFactories.Length; i++) {
                assemblyInstructions[i] = nodes.Get(assemblyInstructionFactories[i]);
            }

            myResultAssembler = new MyResultAssembler(rootStream);
            assemblyInstructions[assemblyInstructions.Length - 1].ParentAssembler = myResultAssembler;

            // Determine up to which instruction we are dealing with optional results.
            // When dealing with optional results we don't do fast exists if we find no lookup results
            requireResultsInstruction = 1; // we always require results from the very first lookup
            for (var i = 1; i < lookupInstructions.Length; i++) {
                var fromStream = lookupInstructions[i].FromStream;
                if (requiredPerStream[fromStream]) {
                    requireResultsInstruction =
                        i + 1; // require results as long as the from-stream is a required stream
                }
                else {
                    break;
                }
            }
        }

        public override void Process(
            EventBean lookupEvent,
            EventBean[] prefillPath,
            ICollection<EventBean[]> resultFinalRows,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var repository = new RepositoryImpl(rootStream, lookupEvent, numStreams);
            var processOptional = true;

            for (var i = 0; i < requireResultsInstruction; i++) {
                var currentInstruction = lookupInstructions[i];
                var hasResults = currentInstruction.Process(repository, exprEvaluatorContext);

                // no results, check what to do
                if (!hasResults) {
                    // If there was a required stream, we are done.
                    if (currentInstruction.HasRequiredStream) {
                        return;
                    }

                    // If this is the first stream and there are no results, we are done with lookups
                    if (i == 0) {
                        processOptional = false; // go to result processing
                    }
                }
            }

            if (processOptional) {
                for (var i = requireResultsInstruction; i < lookupInstructions.Length; i++) {
                    var currentInstruction = lookupInstructions[i];
                    currentInstruction.Process(repository, exprEvaluatorContext);
                }
            }

            // go over the assembly instruction set
            var results = repository.NodesPerStream;

            // no results - need to execute the very last instruction/top node
            if (results == null) {
                var lastAssemblyNode = assemblyInstructions[assemblyInstructions.Length - 1];
                lastAssemblyNode.Init(null);
                lastAssemblyNode.Process(null, resultFinalRows, lookupEvent);
                return;
            }

            // we have results - execute all instructions
            BaseAssemblyNode assemblyNode;
            for (var i = 0; i < assemblyInstructions.Length; i++) {
                assemblyNode = assemblyInstructions[i];
                assemblyNode.Init(results);
            }

            for (var i = 0; i < assemblyInstructions.Length; i++) {
                assemblyNode = assemblyInstructions[i];
                assemblyNode.Process(results, resultFinalRows, lookupEvent);
            }
        }

        public override void Print(IndentWriter writer)
        {
            writer.WriteLine(
                "LookupInstructionExecNode" +
                " rootStream=" + rootStream +
                " name=" + rootStreamName +
                " requiredPerStream=" + requiredPerStream.RenderAny());

            writer.IncrIndent();
            for (var i = 0; i < lookupInstructions.Length; i++) {
                writer.WriteLine("lookup inst node " + i);
                writer.IncrIndent();
                lookupInstructions[i].Print(writer);
                writer.DecrIndent();
            }

            writer.DecrIndent();

            writer.IncrIndent();
            for (var i = 0; i < assemblyInstructions.Length; i++) {
                writer.WriteLine("assembly inst node " + i);
                writer.IncrIndent();
                assemblyInstructions[i].Print(writer);
                writer.DecrIndent();
            }

            writer.DecrIndent();
        }

        /// <summary>
        ///     Receives result rows posted by result set assembly nodes.
        /// </summary>
        public class MyResultAssembler : ResultAssembler
        {
            private readonly int rootStream;

            /// <summary>
            ///     Ctor.
            /// </summary>
            /// <param name="rootStream">is the root stream for which we get results</param>
            public MyResultAssembler(int rootStream)
            {
                this.rootStream = rootStream;
            }

            public void Result(
                EventBean[] row,
                int fromStreamNum,
                EventBean myEvent,
                Node myNode,
                ICollection<EventBean[]> resultFinalRows,
                EventBean resultRootEvent)
            {
                row[rootStream] = resultRootEvent;
                resultFinalRows.Add(row);
            }
        }
    }
} // end of namespace