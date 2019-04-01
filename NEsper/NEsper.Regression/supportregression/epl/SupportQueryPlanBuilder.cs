///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.join.plan;

namespace com.espertech.esper.supportregression.epl
{
    public class SupportQueryPlanBuilder
    {
        private readonly QueryPlan _queryPlan;
    
        public static SupportQueryPlanBuilder Start(int numStreams) {
            return new SupportQueryPlanBuilder(numStreams);
        }
    
        public static SupportQueryPlanBuilder Start(QueryPlan existing) {
            return new SupportQueryPlanBuilder(existing);
        }
    
        private SupportQueryPlanBuilder(int numStreams) {
            QueryPlanIndex[] indexes = new QueryPlanIndex[numStreams];
            for (int i = 0; i < indexes.Length; i++) {
                indexes[i] = new QueryPlanIndex(new LinkedHashMap<TableLookupIndexReqKey, QueryPlanIndexItem>());
            }
            _queryPlan = new QueryPlan(indexes, new QueryPlanNode[numStreams]);
        }
    
        public SupportQueryPlanBuilder(QueryPlan queryPlan) {
            this._queryPlan = queryPlan;
        }
    
        public QueryPlan Get() {
            return _queryPlan;
        }
    
        public SupportQueryPlanBuilder SetIndexFullTableScan(int stream, string indexName) {
            QueryPlanIndex index = _queryPlan.IndexSpecs[stream];
            index.Items.Put(new TableLookupIndexReqKey(indexName), new QueryPlanIndexItem(null, null, null, null, false, null));
            return this;
        }
    
        public SupportQueryPlanBuilder AddIndexHashSingleNonUnique(int stream, string indexName, string property) {
            QueryPlanIndex index = _queryPlan.IndexSpecs[stream];
            index.Items.Put(new TableLookupIndexReqKey(indexName), new QueryPlanIndexItem(new string[] {property}, null, new string[0], null, false, null));
            return this;
        }
    
        public SupportQueryPlanBuilder AddIndexBtreeSingle(int stream, string indexName, string property) {
            QueryPlanIndex index = _queryPlan.IndexSpecs[stream];
            index.Items.Put(new TableLookupIndexReqKey(indexName), new QueryPlanIndexItem(new string[0], null, new string[] {property}, null, false, null));
            return this;
        }
    
        public SupportQueryPlanBuilder SetLookupPlanInner(int stream, TableLookupPlan plan) {
            _queryPlan.ExecNodeSpecs[stream] = new TableLookupNode(plan);
            return this;
        }
    
        public SupportQueryPlanBuilder SetLookupPlanOuter(int stream, TableLookupPlan plan) {
            _queryPlan.ExecNodeSpecs[stream] = new TableOuterLookupNode(plan);
            return this;
        }
    
        public SupportQueryPlanBuilder SetLookupPlanInstruction(int stream, string streamName, LookupInstructionPlan[] instructions) {
            _queryPlan.ExecNodeSpecs[stream] = new LookupInstructionQueryPlanNode(stream, streamName, _queryPlan.IndexSpecs.Length,
                    null, instructions, null);
            return this;
        }
    }
}
