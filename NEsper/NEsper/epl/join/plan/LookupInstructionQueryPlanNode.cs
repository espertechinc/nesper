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
using com.espertech.esper.compat.threading;
using com.espertech.esper.epl.join.assemble;
using com.espertech.esper.epl.join.exec.@base;
using com.espertech.esper.epl.join.table;
using com.espertech.esper.epl.virtualdw;
using com.espertech.esper.util;
using com.espertech.esper.view;

namespace com.espertech.esper.epl.join.plan
{
    /// <summary>
    /// Query plan for executing a set of lookup instructions and assembling an end result
    /// via a set of assembly instructions.
    /// </summary>
    public class LookupInstructionQueryPlanNode : QueryPlanNode
    {
        private readonly IList<BaseAssemblyNodeFactory> _assemblyInstructionFactories;
        private readonly IList<LookupInstructionPlan> _lookupInstructions;
        private readonly int _numStreams;
        private readonly bool[] _requiredPerStream;
        private readonly int _rootStream;
        private readonly String _rootStreamName;

        /// <summary>Ctor. </summary>
        /// <param name="rootStream">is the stream supplying the lookup event</param>
        /// <param name="rootStreamName">is the name of the stream supplying the lookup event</param>
        /// <param name="numStreams">is the number of streams</param>
        /// <param name="lookupInstructions">is a list of lookups to perform</param>
        /// <param name="requiredPerStream">indicates which streams are required and which are optional in the lookup</param>
        /// <param name="assemblyInstructionFactories">is the bottom-up assembly factory nodes to assemble a lookup result nodes</param>
        public LookupInstructionQueryPlanNode(
            int rootStream,
            String rootStreamName,
            int numStreams,
            bool[] requiredPerStream,
            IList<LookupInstructionPlan> lookupInstructions,
            IList<BaseAssemblyNodeFactory> assemblyInstructionFactories)
        {
            _rootStream = rootStream;
            _rootStreamName = rootStreamName;
            _lookupInstructions = lookupInstructions;
            _numStreams = numStreams;
            _requiredPerStream = requiredPerStream;
            _assemblyInstructionFactories = assemblyInstructionFactories;
        }

        public override ExecNode MakeExec(string statementName, int statementId, Attribute[] annotations, IDictionary<TableLookupIndexReqKey, EventTable>[] indexesPerStream, EventType[] streamTypes, Viewable[] streamViews, HistoricalStreamIndexList[] historicalStreamIndexLists, VirtualDWView[] viewExternal, ILockable[] tableSecondaryIndexLocks)
        {
            var execs = new LookupInstructionExec[_lookupInstructions.Count];

            int count = 0;
            foreach (LookupInstructionPlan instruction in _lookupInstructions)
            {
                LookupInstructionExec exec = instruction.MakeExec(statementName, statementId, annotations,
                                                                  indexesPerStream, streamTypes, streamViews,
                                                                  historicalStreamIndexLists, viewExternal);
                execs[count] = exec;
                count++;
            }

            return new LookupInstructionExecNode(
                _rootStream, _rootStreamName,
                _numStreams, execs, _requiredPerStream,
                _assemblyInstructionFactories);
        }

        public override void AddIndexes(HashSet<TableLookupIndexReqKey> usedIndexes)
        {
            foreach (LookupInstructionPlan plan in _lookupInstructions)
            {
                plan.AddIndexes(usedIndexes);
            }
        }

        protected internal override void Print(IndentWriter writer)
        {
            writer.WriteLine("LookupInstructionQueryPlanNode" +
                             " rootStream=" + _rootStream +
                             " requiredPerStream=" + _requiredPerStream.Render());

            writer.IncrIndent();
            for (int i = 0; i < _lookupInstructions.Count; i++)
            {
                writer.WriteLine("lookup step " + i);
                writer.IncrIndent();
                _lookupInstructions[i].Print(writer);
                writer.DecrIndent();
            }
            writer.DecrIndent();

            writer.IncrIndent();
            for (int i = 0; i < _assemblyInstructionFactories.Count; i++)
            {
                writer.WriteLine("assembly step " + i);
                writer.IncrIndent();
                _assemblyInstructionFactories[i].Print(writer);
                writer.DecrIndent();
            }
            writer.DecrIndent();
        }

        public IList<BaseAssemblyNodeFactory> AssemblyInstructionFactories
        {
            get { return _assemblyInstructionFactories; }
        }

        public IList<LookupInstructionPlan> LookupInstructions
        {
            get { return _lookupInstructions; }
        }

        public int NumStreams
        {
            get { return _numStreams; }
        }

        public bool[] RequiredPerStream
        {
            get { return _requiredPerStream; }
        }

        public int RootStream
        {
            get { return _rootStream; }
        }

        public string RootStreamName
        {
            get { return _rootStreamName; }
        }
    }
}