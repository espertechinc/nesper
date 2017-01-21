///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.join.assemble;
using com.espertech.esper.epl.join.rep;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.join.exec.@base
{
    /// <summary>
    /// Execution for a set of lookup instructions and for a set of result assemble
    /// instructions to perform joins and construct a complex result.
    /// </summary>
    public class LookupInstructionExecNode : ExecNode
    {
        private readonly int _rootStream;
        private readonly String _rootStreamName;
        private readonly int _numStreams;
        private readonly bool[] _requiredPerStream;
        private readonly LookupInstructionExec[] _lookupInstructions;
        private readonly BaseAssemblyNode[] _assemblyInstructions;
        private readonly MyResultAssembler _myResultAssembler;
        private readonly int _requireResultsInstruction;

        /// <summary>Ctor. </summary>
        /// <param name="rootStream">is the stream supplying the lookup event</param>
        /// <param name="rootStreamName">is the name of the stream supplying the lookup event</param>
        /// <param name="numStreams">is the number of streams</param>
        /// <param name="lookupInstructions">is a list of lookups to perform</param>
        /// <param name="requiredPerStream">indicates which streams are required and which are optional in the lookup</param>
        public LookupInstructionExecNode(
            int rootStream,
            String rootStreamName,
            int numStreams,
            LookupInstructionExec[] lookupInstructions,
            bool[] requiredPerStream,
            IList<BaseAssemblyNodeFactory> assemblyInstructionFactories)
        {
            _rootStream = rootStream;
            _rootStreamName = rootStreamName;
            _numStreams = numStreams;
            _lookupInstructions = lookupInstructions;
            _requiredPerStream = requiredPerStream;

            // We have a list of factories that are pointing to each other in a tree, i.e.:
            // F1 (->F3), F2 (->F3), F3
            var nodes = new IdentityDictionary<BaseAssemblyNodeFactory, BaseAssemblyNode>();
            foreach (BaseAssemblyNodeFactory factory in assemblyInstructionFactories) {
                nodes[factory] = factory.MakeAssemblerUnassociated();
            }

            // re-associate each node after allocation
            foreach (var nodeWithFactory in nodes) {
                var parentFactory = nodeWithFactory.Key.ParentNode;
                if (parentFactory != null) {
                    nodeWithFactory.Value.ParentAssembler = nodes.Get(parentFactory);
                }
                foreach (var childNodeFactory in nodeWithFactory.Key.ChildNodes) {
                    nodeWithFactory.Value.AddChild(nodes.Get(childNodeFactory));
                }
            }

            _assemblyInstructions = assemblyInstructionFactories.Select(nodes.Get).ToArray();

            _myResultAssembler = new MyResultAssembler(rootStream);
            _assemblyInstructions[_assemblyInstructions.Length - 1].ParentAssembler = _myResultAssembler;
    
            // Determine up to which instruction we are dealing with optional results.
            // When dealing with optional results we don't do fast exists if we find no lookup results
            _requireResultsInstruction = 1;  // we always require results from the very first lookup
            for (int i = 1; i < lookupInstructions.Length; i++)
            {
                int fromStream = lookupInstructions[i].FromStream;
                if (requiredPerStream[fromStream])
                {
                    _requireResultsInstruction = i + 1;      // require results as long as the from-stream is a required stream
                }
                else
                {
                    break;
                }
            }
        }
    
        public override void Process(EventBean lookupEvent, EventBean[] prefillPath, ICollection<EventBean[]> resultFinalRows, ExprEvaluatorContext exprEvaluatorContext)
        {
            var repository = new RepositoryImpl(_rootStream, lookupEvent, _numStreams);
            var processOptional = true;
    
            for (int i = 0; i < _requireResultsInstruction; i++)
            {
                var currentInstruction = _lookupInstructions[i];
                var hasResults = currentInstruction.Process(repository,exprEvaluatorContext);
    
                // no results, check what to do
                if (!hasResults)
                {
                    // If there was a required stream, we are done.
                    if (currentInstruction.HasRequiredStream)
                    {
                        return;
                    }
    
                    // If this is the first stream and there are no results, we are done with lookups
                    if (i == 0)
                    {
                        processOptional = false;  // go to result processing
                    }
                }
            }
    
            if (processOptional)
            {
                for (int i = _requireResultsInstruction; i < _lookupInstructions.Length; i++)
                {
                    var currentInstruction = _lookupInstructions[i];
                    currentInstruction.Process(repository, exprEvaluatorContext);
                }
            }
    
            // go over the assembly instruction set
            var results = repository.NodesPerStream;
    
            // no results - need to execute the very last instruction/top node
            if (results == null)
            {
                var lastAssemblyNode = _assemblyInstructions[_assemblyInstructions.Length - 1];
                lastAssemblyNode.Init(null);
                lastAssemblyNode.Process(null, resultFinalRows, lookupEvent);
                return;
            }
    
            // we have results - execute all instructions
            BaseAssemblyNode assemblyNode;
            for (int i = 0; i < _assemblyInstructions.Length; i++)
            {
                assemblyNode = _assemblyInstructions[i];
                assemblyNode.Init(results);
            }
            for (int i = 0; i < _assemblyInstructions.Length; i++)
            {
                assemblyNode = _assemblyInstructions[i];
                assemblyNode.Process(results, resultFinalRows, lookupEvent);
            }
        }
    
        public override void Print(IndentWriter writer)
        {
            writer.WriteLine("LookupInstructionExecNode" +
                    " rootStream=" + _rootStream +
                    " name=" + _rootStreamName +
                    " requiredPerStream=" + _requiredPerStream.Render());
    
            writer.IncrIndent();
            for (int i = 0; i < _lookupInstructions.Length; i++)
            {
                writer.WriteLine("lookup inst node " + i);
                writer.IncrIndent();
                _lookupInstructions[i].Print(writer);
                writer.DecrIndent();
            }
            writer.DecrIndent();
    
            writer.IncrIndent();
            for (int i = 0; i < _assemblyInstructions.Length; i++)
            {
                writer.WriteLine("assembly inst node " + i);
                writer.IncrIndent();
                _assemblyInstructions[i].Print(writer);
                writer.DecrIndent();
            }
            writer.DecrIndent();
        }
    
        /// <summary>Receives result rows posted by result set assembly nodes. </summary>
        public class MyResultAssembler : ResultAssembler
        {
            private readonly int _rootStream;
    
            /// <summary>Ctor. </summary>
            /// <param name="rootStream">is the root stream for which we get results</param>
            public MyResultAssembler(int rootStream)
            {
                _rootStream = rootStream;
            }
    
            public void Result(EventBean[] row, int fromStreamNum, EventBean myEvent, Node myNode, ICollection<EventBean[]> resultFinalRows, EventBean resultRootEvent)
            {
                row[_rootStream] = resultRootEvent;
                resultFinalRows.Add(row);
            }
        }
    }
}
