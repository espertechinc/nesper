///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.epl.@join.queryplan;
using com.espertech.esper.common.@internal.epl.@join.queryplanouter;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.regressionlib.support.util
{
    public class SupportQueryPlanBuilder
    {
        private readonly QueryPlanForge queryPlan;

        private SupportQueryPlanBuilder(int numStreams)
        {
            var indexes = new QueryPlanIndexForge[numStreams];
            for (var i = 0; i < indexes.Length; i++) {
                indexes[i] =
                    new QueryPlanIndexForge(new LinkedHashMap<TableLookupIndexReqKey, QueryPlanIndexItemForge>());
            }

            queryPlan = new QueryPlanForge(indexes, new QueryPlanNodeForge[numStreams]);
        }

        public SupportQueryPlanBuilder(QueryPlanForge queryPlan)
        {
            this.queryPlan = queryPlan;
        }

        public static SupportQueryPlanBuilder Start(int numStreams)
        {
            return new SupportQueryPlanBuilder(numStreams);
        }

        public static SupportQueryPlanBuilder Start(QueryPlanForge existing)
        {
            return new SupportQueryPlanBuilder(existing);
        }

        public QueryPlanForge Get()
        {
            return queryPlan;
        }

        public SupportQueryPlanBuilder SetIndexFullTableScan(
            int stream,
            string indexName)
        {
            var index = queryPlan.IndexSpecs[stream];
            index.Items.Put(
                new TableLookupIndexReqKey(indexName, null),
                new QueryPlanIndexItemForge(Array.Empty<string>(), new Type[0], Array.Empty<string>(), new Type[0], false, null, null));
            return this;
        }

        public SupportQueryPlanBuilder AddIndexHashSingleNonUnique(
            int stream,
            string indexName,
            string property)
        {
            var index = queryPlan.IndexSpecs[stream];
            index.Items.Put(
                new TableLookupIndexReqKey(indexName, null),
                new QueryPlanIndexItemForge(
                    new[] {property},
                    new[] {typeof(string)},
                    Array.Empty<string>(),
                    new Type[0],
                    false,
                    null,
                    null));
            return this;
        }

        public SupportQueryPlanBuilder AddIndexBtreeSingle(
            int stream,
            string indexName,
            string property)
        {
            var index = queryPlan.IndexSpecs[stream];
            index.Items.Put(
                new TableLookupIndexReqKey(indexName, null),
                new QueryPlanIndexItemForge(
                    Array.Empty<string>(),
                    new Type[0],
                    new[] {property},
                    new[] {typeof(string)},
                    false,
                    null,
                    null));
            return this;
        }

        public SupportQueryPlanBuilder SetLookupPlanInner(
            int stream,
            TableLookupPlanForge plan)
        {
            queryPlan.ExecNodeSpecs[stream] = new TableLookupNodeForge(plan);
            return this;
        }

        public SupportQueryPlanBuilder SetLookupPlanOuter(
            int stream,
            TableLookupPlanForge plan)
        {
            queryPlan.ExecNodeSpecs[stream] = new TableOuterLookupNodeForge(plan);
            return this;
        }

        public SupportQueryPlanBuilder SetLookupPlanInstruction(
            int stream,
            string streamName,
            LookupInstructionPlanForge[] instructions)
        {
            queryPlan.ExecNodeSpecs[stream] = new LookupInstructionQueryPlanNodeForge(
                stream,
                streamName,
                queryPlan.IndexSpecs.Length,
                null,
                Arrays.AsList(instructions),
                null);
            return this;
        }
    }
} // end of namespace