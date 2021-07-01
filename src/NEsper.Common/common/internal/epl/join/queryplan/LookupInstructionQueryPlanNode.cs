///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.index.@base;
using com.espertech.esper.common.@internal.epl.join.assemble;
using com.espertech.esper.common.@internal.epl.join.exec.outer;
using com.espertech.esper.common.@internal.epl.join.queryplanouter;
using com.espertech.esper.common.@internal.epl.join.strategy;
using com.espertech.esper.common.@internal.epl.virtualdw;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat.threading.locks;

namespace com.espertech.esper.common.@internal.epl.join.queryplan
{
    /// <summary>
    ///     Query plan for executing a set of lookup instructions and assembling an end result via
    ///     a set of assembly instructions.
    /// </summary>
    public class LookupInstructionQueryPlanNode : QueryPlanNode
    {
        private readonly BaseAssemblyNodeFactory[] assemblyInstructionFactories;
        private readonly bool[] requiredPerStream;
        private readonly int rootStream;
        private readonly string rootStreamName;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="rootStream">is the stream supplying the lookup event</param>
        /// <param name="rootStreamName">is the name of the stream supplying the lookup event</param>
        /// <param name="numStreams">is the number of streams</param>
        /// <param name="lookupInstructions">is a list of lookups to perform</param>
        /// <param name="requiredPerStream">indicates which streams are required and which are optional in the lookup</param>
        /// <param name="assemblyInstructionFactories">is the bottom-up assembly factory nodes to assemble a lookup result nodes</param>
        public LookupInstructionQueryPlanNode(
            int rootStream,
            string rootStreamName,
            int numStreams,
            bool[] requiredPerStream,
            LookupInstructionPlan[] lookupInstructions,
            BaseAssemblyNodeFactory[] assemblyInstructionFactories)
        {
            this.rootStream = rootStream;
            this.rootStreamName = rootStreamName;
            LookupInstructions = lookupInstructions;
            NumStreams = numStreams;
            this.requiredPerStream = requiredPerStream;
            this.assemblyInstructionFactories = assemblyInstructionFactories;
        }

        public int NumStreams { get; }

        public LookupInstructionPlan[] LookupInstructions { get; }

        public override ExecNode MakeExec(
            AgentInstanceContext agentInstanceContext,
            IDictionary<TableLookupIndexReqKey, EventTable>[] indexesPerStream,
            EventType[] streamTypes,
            Viewable[] streamViews,
            VirtualDWView[] viewExternal,
            ILockable[] tableSecondaryIndexLocks)
        {
            var execs = new LookupInstructionExec[LookupInstructions.Length];

            var count = 0;
            foreach (var instruction in LookupInstructions) {
                var exec = instruction.MakeExec(
                    agentInstanceContext,
                    indexesPerStream,
                    streamTypes,
                    streamViews,
                    viewExternal);
                execs[count] = exec;
                count++;
            }

            return new LookupInstructionExecNode(
                rootStream,
                rootStreamName,
                NumStreams,
                execs,
                requiredPerStream,
                assemblyInstructionFactories);
        }
    }
} // end of namespace