///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.container;
using com.espertech.esper.compat.threading;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.core.service.multimatch;
using com.espertech.esper.core.thread;
using com.espertech.esper.epl.agg.factory;
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.named;
using com.espertech.esper.epl.spec;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.events;
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
    
        public static AgentInstanceContext MakeAgentInstanceContext(IContainer container, SchedulingService stub)
        {
            return new AgentInstanceContext(
                MakeContext(container, stub),
                null, -1, null, null, null);
        }
    
        public static AgentInstanceContext MakeAgentInstanceContext(IContainer container)
        {
            return new AgentInstanceContext(
                MakeContext(container),
                null, -1, null, null, null);
        }
    
        public static AgentInstanceViewFactoryChainContext MakeAgentInstanceViewFactoryContext(
            IContainer container, SchedulingService stub)
        {
            AgentInstanceContext agentInstanceContext = MakeAgentInstanceContext(container, stub);
            return new AgentInstanceViewFactoryChainContext(agentInstanceContext, false, null, null);
        }
    
        public static AgentInstanceViewFactoryChainContext MakeAgentInstanceViewFactoryContext(
            IContainer container)
        {
            AgentInstanceContext agentInstanceContext = MakeAgentInstanceContext(container);
            return new AgentInstanceViewFactoryChainContext(agentInstanceContext, false, null, null);
        }
    
        public static ViewFactoryContext MakeViewContext(IContainer container)
        {
            StatementContext stmtContext = MakeContext(container);
            return new ViewFactoryContext(stmtContext, 1, "somenamespacetest", "somenametest", false, -1, false);
        }
    
        public static StatementContext MakeContext(IContainer container)
        {
            var sched = new SupportSchedulingServiceImpl();
            return MakeContext(container, sched);
        }
    
        public static StatementContext MakeContext(IContainer container, int statementId)
        {
            var sched = new SupportSchedulingServiceImpl();
            return MakeContext(container, statementId, sched);
        }
    
        public static StatementContext MakeContext(IContainer container, SchedulingService stub)
        {
            return MakeContext(container, 1, stub);
        }
    
        public static StatementContext MakeContext(IContainer container, int statementId, SchedulingService stub)
        {
            var lockManager = container.Resolve<ILockManager>();
            var rwLockManager = container.Resolve<IReaderWriterLockManager>();
            var threadLocalManager = container.Resolve<IThreadLocalManager>();
            var classLoaderProvider = container.Resolve<ClassLoaderProvider>();

            var config = new Configuration(container);
            config.EngineDefaults.ViewResources.IsAllowMultipleExpiryPolicies = true;
    
            var timeSourceService = new TimeSourceServiceImpl();
            var stmtEngineServices = new StatementContextEngineServices(
                container,
                "engURI",
                container.Resolve<EventAdapterService>(),
                new NamedWindowMgmtServiceImpl(false, null),
                null, new TableServiceImpl(rwLockManager, threadLocalManager),
                new EngineSettingsService(new Configuration(container).EngineDefaults, new Uri[0]),
                new ValueAddEventServiceImpl(lockManager),
                config,
                null,
                null,
                null,
                null,
                new StatementEventTypeRefImpl(rwLockManager),
                null, null, null, null, null,
                new ViewServicePreviousFactoryImpl(), null,
                new PatternNodeFactoryImpl(),
                new FilterBooleanExpressionFactoryImpl(),
                timeSourceService,
                SupportEngineImportServiceFactory.Make(classLoaderProvider),
                AggregationFactoryFactoryDefault.INSTANCE,
                new SchedulingServiceImpl(timeSourceService, lockManager),
                null);
    
            return new StatementContext(
                container,
                stmtEngineServices,
                stub,
                new ScheduleBucket(1),
                new EPStatementHandle(statementId, "name1", "epl1", StatementType.SELECT, "epl1", false, null, 0, false, false, new MultiMatchHandlerFactoryImpl().GetDefaultHandler()),
                new ViewResolutionServiceImpl(new PluggableObjectRegistryImpl(new PluggableObjectCollection[]{ViewEnumHelper.BuiltinViews}), null, null),
                new PatternObjectResolutionServiceImpl(container, null),
                null,
                null,
                null,
                null,
                new StatementResultServiceImpl("name", null, null, new ThreadingServiceImpl(new ConfigurationEngineDefaults.ThreadingConfig()), threadLocalManager), // statement result svc
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
