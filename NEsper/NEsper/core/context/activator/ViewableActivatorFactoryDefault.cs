///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.named;
using com.espertech.esper.epl.property;
using com.espertech.esper.epl.spec;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.filter;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.pattern;
using com.espertech.esper.view;

namespace com.espertech.esper.core.context.activator
{
	public class ViewableActivatorFactoryDefault : ViewableActivatorFactory
    {
	    public ViewableActivator CreateActivatorSimple(FilterStreamSpecCompiled filterStreamSpec)
        {
	        throw new UnsupportedOperationException();
	    }

	    public ViewableActivator CreateFilterProxy(EPServicesContext services, FilterSpecCompiled filterSpec, Attribute[] annotations, bool subselect, InstrumentationAgent instrumentationAgentSubquery, bool isCanIterate, int? streamNumFromClause)
        {
	        return new ViewableActivatorFilterProxy(services, filterSpec, annotations, subselect, instrumentationAgentSubquery, isCanIterate);
	    }

	    public ViewableActivator CreateStreamReuseView(EPServicesContext services, StatementContext statementContext, StatementSpecCompiled statementSpec, FilterStreamSpecCompiled filterStreamSpec, bool isJoin, ExprEvaluatorContextStatement evaluatorContextStmt, bool filterSubselectSameStream, int streamNum, bool isCanIterateUnbound)
        {
	        return new ViewableActivatorStreamReuseView(services, statementContext, statementSpec, filterStreamSpec, isJoin, evaluatorContextStmt, filterSubselectSameStream, streamNum, isCanIterateUnbound);
	    }

	    public ViewableActivator CreatePattern(PatternContext patternContext, EvalRootFactoryNode rootFactoryNode, EventType eventType, bool consumingFilters, bool suppressSameEventMatches, bool discardPartialsOnMatch, bool isCanIterateUnbound)
        {
	        return new ViewableActivatorPattern(patternContext, rootFactoryNode, eventType, consumingFilters, suppressSameEventMatches, discardPartialsOnMatch, isCanIterateUnbound);
	    }

        public ViewableActivator CreateNamedWindow(NamedWindowProcessor processor, NamedWindowConsumerStreamSpec streamSpec, StatementContext statementContext)
        {
	        return new ViewableActivatorNamedWindow(processor, streamSpec.FilterExpressions, streamSpec.OptPropertyEvaluator);
	    }

	    public ViewableActivator CreateTable(TableMetadata metadata, ExprEvaluator[] optionalTableFilters)
	    {
            return new ViewableActivatorTable(metadata, optionalTableFilters);
	    }

	    public ViewableActivator MakeHistorical(HistoricalEventViewable historicalEventViewable)
	    {
            return new ViewableActivatorHistorical(historicalEventViewable);
	    }

	    public ViewableActivator MakeSubqueryNWIndexShare()
	    {
            return new ViewableActivatorSubselectNone();
	    }
    }
} // end of namespace
