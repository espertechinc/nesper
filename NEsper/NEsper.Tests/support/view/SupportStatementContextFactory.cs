///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.hook;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.threading;
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
using com.espertech.esper.epl.variable;
using com.espertech.esper.events.vaevent;
using com.espertech.esper.pattern;
using com.espertech.esper.schedule;
using com.espertech.esper.support.core;
using com.espertech.esper.support.events;
using com.espertech.esper.support.schedule;
using com.espertech.esper.view;

namespace com.espertech.esper.support.view
{
	public class SupportStatementContextFactory
	{
	    public static ExprEvaluatorContext MakeEvaluatorContext() {
	        return new ExprEvaluatorContextStatement(null, false);
	    }

	    public static AgentInstanceContext MakeAgentInstanceContext(SchedulingService stub) {
	        return new AgentInstanceContext(MakeContext(stub), null, -1, null, null, null);
	    }

	    public static AgentInstanceContext MakeAgentInstanceContext() {
	        return new AgentInstanceContext(MakeContext(), null, -1, null, null, null);
	    }

	    public static AgentInstanceViewFactoryChainContext MakeAgentInstanceViewFactoryContext(SchedulingService stub) {
	        var agentInstanceContext = MakeAgentInstanceContext(stub);
	        return new AgentInstanceViewFactoryChainContext(agentInstanceContext, false, null, null);
	    }

	    public static AgentInstanceViewFactoryChainContext MakeAgentInstanceViewFactoryContext() {
	        var agentInstanceContext = MakeAgentInstanceContext();
	        return new AgentInstanceViewFactoryChainContext(agentInstanceContext, false, null, null);
	    }

	    public static StatementContext MakeContext()
	    {
	        var sched = new SupportSchedulingServiceImpl();
	        return MakeContext(sched);
	    }

	    public static ViewFactoryContext MakeViewContext()
	    {
	        var stmtContext = MakeContext();
	        return new ViewFactoryContext(stmtContext, 1, 1, "somenamespacetest", "somenametest");
	    }

	    public static StatementContext MakeContext(SchedulingService stub)
	    {
	        var variableService = new VariableServiceImpl(1000, null, SupportEventAdapterService.Service, null);
	        var config = new Configuration();
	        config.EngineDefaults.ViewResourcesConfig.IsAllowMultipleExpiryPolicies = true;

	        var stmtEngineServices = new StatementContextEngineServices(
                "engURI",
	            SupportEventAdapterService.Service,
	            new NamedWindowServiceImpl(null, variableService, new TableServiceImpl(), false, ReaderWriterLockManager.CreateDefaultLock(), new ExceptionHandlingService("engURI", Collections.GetEmptyList<ExceptionHandler>(), Collections.GetEmptyList<ConditionHandler>()), false, null),
	            null, new TableServiceImpl(),
	            new EngineSettingsService(new Configuration().EngineDefaults, new Uri[0]),
	            new ValueAddEventServiceImpl(),
	            config,
	            null,
	            null,
	            null,
	            null,
	            new StatementEventTypeRefImpl(),
                null,
                null,
                null,
                null);

	        return new StatementContext(
                stmtEngineServices,
	                null,
	                stub,
	                new ScheduleBucket(1),
                    new EPStatementHandle("id1", "name1", "epl1", StatementType.SELECT, "epl1", false, null, 0, false, false, MultiMatchHandlerFactory.DefaultHandler),
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
                    null);
	    }
	}
} // end of namespace
