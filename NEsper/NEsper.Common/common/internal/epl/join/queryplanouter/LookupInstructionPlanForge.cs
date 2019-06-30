///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.bytecodemodel.util;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.@join.queryplan;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.join.queryplanouter
{
    /// <summary>
    ///     Plan for lookup using a from-stream event looking up one or more to-streams using a specified lookup plan for each
    ///     to-stream.
    /// </summary>
    public class LookupInstructionPlanForge : CodegenMakeable
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="fromStream">the stream supplying the lookup event</param>
        /// <param name="fromStreamName">the stream name supplying the lookup event</param>
        /// <param name="toStreams">the set of streams to look up in</param>
        /// <param name="lookupPlans">the plan to use for each stream to look up in</param>
        /// <param name="requiredPerStream">indicates which of the lookup streams are required to build a result and which are not</param>
        /// <param name="historicalPlans">plans for use with historical streams</param>
        public LookupInstructionPlanForge(
            int fromStream,
            string fromStreamName,
            int[] toStreams,
            TableLookupPlanForge[] lookupPlans,
            HistoricalDataPlanNodeForge[] historicalPlans,
            bool[] requiredPerStream)
        {
            if (toStreams.Length != lookupPlans.Length)
            {
                throw new ArgumentException("Invalid number of lookup plans for each stream");
            }

            if (requiredPerStream.Length < lookupPlans.Length)
            {
                throw new ArgumentException("Invalid required per stream array");
            }

            if (fromStream < 0 || fromStream >= requiredPerStream.Length)
            {
                throw new ArgumentException("Invalid from stream");
            }

            FromStream = fromStream;
            FromStreamName = fromStreamName;
            ToStreams = toStreams;
            LookupPlans = lookupPlans;
            HistoricalPlans = historicalPlans;
            RequiredPerStream = requiredPerStream;
        }

        public int FromStream { get; }

        public string FromStreamName { get; }

        public int[] ToStreams { get; }

        public TableLookupPlanForge[] LookupPlans { get; }

        public bool[] RequiredPerStream { get; }

        public HistoricalDataPlanNodeForge[] HistoricalPlans { get; }

        public CodegenExpression Make(
            CodegenMethodScope parent,
            CodegenSymbolProvider symbols,
            CodegenClassScope classScope)
        {
            return Make(parent, (SAIFFInitializeSymbol) symbols, classScope);
        }

        public CodegenExpression Make(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            return NewInstance<LookupInstructionPlan>(
                Constant(FromStream),
                Constant(FromStreamName),
                Constant(ToStreams),
                CodegenMakeableUtil.MakeArray("lookupPlans", typeof(TableLookupPlan), LookupPlans, GetType(), parent, symbols, classScope),
                CodegenMakeableUtil.MakeArray(
                    "historicalPlans", typeof(HistoricalDataPlanNode), HistoricalPlans, GetType(), parent, symbols, classScope),
                Constant(RequiredPerStream));
        }

        /// <summary>
        ///     Output the planned instruction.
        /// </summary>
        /// <param name="writer">to output to</param>
        public void Print(IndentWriter writer)
        {
            writer.WriteLine(
                "LookupInstructionPlan" +
                " fromStream=" + FromStream +
                " fromStreamName=" + FromStreamName +
                " toStreams=" + ToStreams.RenderAny()
            );

            writer.IncrIndent();
            for (var i = 0; i < LookupPlans.Length; i++)
            {
                if (LookupPlans[i] != null)
                {
                    writer.WriteLine("plan " + i + " :" + LookupPlans[i].ToString());
                }
                else
                {
                    writer.WriteLine("plan " + i + " : no lookup plan");
                }
            }

            writer.DecrIndent();
        }

        public void AddIndexes(HashSet<TableLookupIndexReqKey> usedIndexes)
        {
            for (var i = 0; i < LookupPlans.Length; i++)
            {
                if (LookupPlans[i] != null)
                {
                    usedIndexes.AddAll(Arrays.AsList(LookupPlans[i].IndexNum));
                }
            }
        }
    }
} // end of namespace