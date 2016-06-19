///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.core.service.multimatch;
using com.espertech.esper.core.thread;
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.named;
using com.espertech.esper.epl.spec;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.events.vaevent;
using com.espertech.esper.filter;
using com.espertech.esper.pattern;
using com.espertech.esper.schedule;
using com.espertech.esper.timer;
using com.espertech.esper.view;

namespace com.espertech.esper.core.support
{
	public class SupportStatementContextFactory
	{
	    public static ExprEvaluatorContext MakeEvaluatorContext()
        {
	        return new ExprEvaluatorContextStatement(null, false);
	    }

	    public static AgentInstanceContext MakeAgentInstanceContext(SchedulingService stub)
        {
	        return new AgentInstanceContext(MakeContext(stub), null, -1, null, null, null);
	    }

	    public static AgentInstanceContext MakeAgentInstanceContext()
        {
	        return new AgentInstanceContext(MakeContext(), null, -1, null, null, null);
	    }

	    public static AgentInstanceViewFactoryChainContext MakeAgentInstanceViewFactoryContext(SchedulingService stub)
        {
	        AgentInstanceContext agentInstanceContext = MakeAgentInstanceContext(stub);
	        return new AgentInstanceViewFactoryChainContext(agentInstanceContext, false, null, null);
	    }

	    public static AgentInstanceViewFactoryChainContext MakeAgentInstanceViewFactoryContext()
        {
	        AgentInstanceContext agentInstanceContext = MakeAgentInstanceContext();
	        return new AgentInstanceViewFactoryChainContext(agentInstanceContext, false, null, null);
	    }

	    public static ViewFactoryContext MakeViewContext()
	    {
	        StatementContext stmtContext = MakeContext();
	        return new ViewFactoryContext(stmtContext, 1, "somenamespacetest", "somenametest", false, -1, false);
	    }

	    public static StatementContext MakeContext()
	    {
	        SupportSchedulingServiceImpl sched = new SupportSchedulingServiceImpl();
	        return MakeContext(sched);
	    }

	    public static StatementContext MakeContext(int statementId)
	    {
	        SupportSchedulingServiceImpl sched = new SupportSchedulingServiceImpl();
	        return MakeContext(statementId, sched);
	    }

	    public static StatementContext MakeContext(SchedulingService stub)
        {
	        return MakeContext(1, stub);
	    }

	    public static StatementContext MakeContext(int statementId, SchedulingService stub)
	    {
	        Configuration config = new Configuration();
	        config.EngineDefaults.ViewResourcesConfig.IsAllowMultipleExpiryPolicies = true;

	        StatementContextEngineServices stmtEngineServices = new StatementContextEngineServices(
	            "engURI",
	            SupportEventAdapterService.GetService(),
	            new NamedWindowMgmtServiceImpl(false, null),
	            null, new TableServiceImpl(),
	            new EngineSettingsService(new Configuration().EngineDefaults, new Uri[0]),
	            new ValueAddEventServiceImpl(),
	            config,
	            null,
	            null,
	            null,
	            null,
	            new StatementEventTypeRefImpl(), null, null, null, null, null, new ViewServicePreviousFactoryImpl(), null,
	            new PatternNodeFactoryImpl(), new FilterBooleanExpressionFactoryImpl(), new TimeSourceServiceImpl());

	        return new StatementContext(
                stmtEngineServices,
	            stub,
	            new ScheduleBucket(1),
	            new EPStatementHandle(statementId, "name1", "epl1", StatementType.SELECT, "epl1", false, null, 0, false, false, new MultiMatchHandlerFactoryImpl().GetDefaultHandler()),
	            new ViewResolutionServiceImpl(new PluggableObjectRegistryImpl(new PluggableObjectCollection[] {ViewEnumHelper.BuiltinViews}), null, null),
	            new PatternObjectResolutionServiceImpl(null),
	            null,
	            null,
	            new MethodResolutionServiceImpl(SupportEngineImportServiceFactory.Make(), null),
	            null,
	            null,
	            new StatementResultServiceImpl("name", null, null, new ThreadingServiceImpl(new ConfigurationEngineDefaults.Threading())), // statement result svc
	            null,
	            null,
	            null,
	            null,
	            null,
	            null,
	            null,
	            false,
	            null,
	            null,
	            AggregationServiceFactoryServiceImpl.DEFAULT_FACTORY,
                null,
	            false,
	            null, new StatementSemiAnonymousTypeRegistryImpl(), 0);
	    }
	}
} // end of namespace
