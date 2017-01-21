///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.context;
using com.espertech.esper.collection;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.context.stmt;
using com.espertech.esper.epl.spec;
using com.espertech.esper.epl.spec.util;
using com.espertech.esper.filter;

namespace com.espertech.esper.core.context.mgr
{
	public abstract class ContextControllerPartitionedFactoryBase 
        : ContextControllerFactoryBase 
        , ContextControllerFactory
    {
	    private readonly ContextDetailPartitioned _segmentedSpec;
	    private readonly IList<FilterSpecCompiled> _filtersSpecsNestedContexts;

	    private IDictionary<string, object> _contextBuiltinProps;

	    protected ContextControllerPartitionedFactoryBase(ContextControllerFactoryContext factoryContext, ContextDetailPartitioned segmentedSpec, IList<FilterSpecCompiled> filtersSpecsNestedContexts)
            : base(factoryContext)
        {
	        _segmentedSpec = segmentedSpec;
	        _filtersSpecsNestedContexts = filtersSpecsNestedContexts;
	    }

	    public bool HasFiltersSpecsNestedContexts
	    {
	        get { return _filtersSpecsNestedContexts != null && !_filtersSpecsNestedContexts.IsEmpty(); }
	    }

	    public override void ValidateFactory()
        {
	        Type[] propertyTypes = ContextControllerPartitionedUtil.ValidateContextDesc(_factoryContext.ContextName, _segmentedSpec);
	        _contextBuiltinProps = ContextPropertyEventType.GetPartitionType(_segmentedSpec, propertyTypes);
	    }

	    public override ContextControllerStatementCtxCache ValidateStatement(ContextControllerStatementBase statement)
        {
	        StatementSpecCompiledAnalyzerResult streamAnalysis = StatementSpecCompiledAnalyzer.AnalyzeFilters(statement.StatementSpec);
	        ContextControllerPartitionedUtil.ValidateStatementForContext(_factoryContext.ContextName, statement, streamAnalysis, GetItemEventTypes(_segmentedSpec), _factoryContext.ServicesContext.NamedWindowMgmtService);
	        return new ContextControllerStatementCtxCacheFilters(streamAnalysis.Filters);
	    }

	    public override void PopulateFilterAddendums(IDictionary<FilterSpecCompiled, FilterValueSetParam[][]> filterAddendum, ContextControllerStatementDesc statement, object key, int contextId)
        {
	        ContextControllerStatementCtxCacheFilters statementInfo = (ContextControllerStatementCtxCacheFilters) statement.Caches[_factoryContext.NestingLevel - 1];
	        ContextControllerPartitionedUtil.PopulateAddendumFilters(key, statementInfo.FilterSpecs, _segmentedSpec, statement.Statement.StatementSpec, filterAddendum);
	    }

	    public void PopulateContextInternalFilterAddendums(ContextInternalFilterAddendum filterAddendum, object key)
        {
	        if (_filtersSpecsNestedContexts == null || _filtersSpecsNestedContexts.IsEmpty()) {
	            return;
	        }
	        ContextControllerPartitionedUtil.PopulateAddendumFilters(key, _filtersSpecsNestedContexts, _segmentedSpec, null, filterAddendum.FilterAddendum);
	    }

	    public override FilterSpecLookupable GetFilterLookupable(EventType eventType)
        {
	        return null;
	    }

	    public override bool IsSingleInstanceContext
	    {
	        get { return false; }
	    }

	    public override StatementAIResourceRegistryFactory StatementAIResourceRegistryFactory
	    {
	        get
	        {
	            return () => new StatementAIResourceRegistry(new AIRegistryAggregationMultiPerm(), new AIRegistryExprMultiPerm());
	        }
	    }

	    public override IList<ContextDetailPartitionItem> ContextDetailPartitionItems
	    {
	        get { return _segmentedSpec.Items; }
	    }

	    public override ContextDetail ContextDetail
	    {
	        get { return _segmentedSpec; }
	    }

	    public ContextDetailPartitioned SegmentedSpec
	    {
	        get { return _segmentedSpec; }
	    }

	    public override IDictionary<string, object> ContextBuiltinProps
	    {
	        get { return _contextBuiltinProps; }
	    }

	    public override ContextPartitionIdentifier KeyPayloadToIdentifier(object payload)
        {
	        if (payload is object[]) {
	            return new ContextPartitionIdentifierPartitioned((object[]) payload);
	        }
	        if (payload is MultiKeyUntyped) {
	            return new ContextPartitionIdentifierPartitioned(((MultiKeyUntyped) payload).Keys);
	        }
	        return new ContextPartitionIdentifierPartitioned(new object[] {payload});
	    }

	    private ICollection<EventType> GetItemEventTypes(ContextDetailPartitioned segmentedSpec)
        {
	        IList<EventType> itemEventTypes = new List<EventType>();
	        foreach (ContextDetailPartitionItem item in segmentedSpec.Items) {
	            itemEventTypes.Add(item.FilterSpecCompiled.FilterForEventType);
	        }
	        return itemEventTypes;
	    }
	}
} // end of namespace
