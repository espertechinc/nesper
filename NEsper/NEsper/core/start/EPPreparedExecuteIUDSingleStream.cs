///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Linq;
using System.Reflection;

using com.espertech.esper.client;
using com.espertech.esper.client.context;
using com.espertech.esper.collection;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.join.hint;
using com.espertech.esper.epl.join.plan;
using com.espertech.esper.epl.spec;
using com.espertech.esper.filter;
using com.espertech.esper.util;

namespace com.espertech.esper.core.start
{
    /// <summary>
    /// Starts and provides the stop method for EPL statements.
    /// </summary>
    public abstract class EPPreparedExecuteIUDSingleStream : EPPreparedExecuteMethod
    {
        private static readonly ILog QueryPlanLog = LogManager.GetLogger(AuditPath.QUERYPLAN_LOG);
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        protected readonly StatementSpecCompiled StatementSpec;
        protected readonly FireAndForgetProcessor Processor;
        protected readonly EPServicesContext Services;
        protected readonly EPPreparedExecuteIUDSingleStreamExec Executor;
        protected readonly StatementContext StatementContext;
        protected bool HasTableAccess;

        public abstract EPPreparedExecuteIUDSingleStreamExec GetExecutor(QueryGraph queryGraph, string aliasName);

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="statementSpec">is a container for the definition of all statement constructs thatmay have been used in the statement, i.e. if defines the select clauses, insert into, outer joins etc.
        /// </param>
        /// <param name="services">is the service instances for dependency injection</param>
        /// <param name="statementContext">is statement-level information and statement services</param>
        /// <throws>com.espertech.esper.epl.expression.core.ExprValidationException if the preparation failed</throws>
        public EPPreparedExecuteIUDSingleStream(
            StatementSpecCompiled statementSpec,
            EPServicesContext services,
            StatementContext statementContext)
        {
            var queryPlanLogging = services.ConfigSnapshot.EngineDefaults.Logging.IsEnableQueryPlan;
            if (queryPlanLogging)
            {
                QueryPlanLog.Info("Query plans for Fire-and-forget query '" + statementContext.Expression + "'");
            }

            HasTableAccess = statementSpec.IntoTableSpec != null ||
                    (statementSpec.TableNodes != null && statementSpec.TableNodes.Length > 0);
            if (statementSpec.InsertIntoDesc != null && services.TableService.GetTableMetadata(statementSpec.InsertIntoDesc.EventTypeName) != null)
            {
                HasTableAccess = true;
            }
            if (statementSpec.FireAndForgetSpec is FireAndForgetSpecUpdate ||
                statementSpec.FireAndForgetSpec is FireAndForgetSpecDelete)
            {
                HasTableAccess |= statementSpec.StreamSpecs[0] is TableQueryStreamSpec;
            }

            StatementSpec = statementSpec;
            Services = services;
            StatementContext = statementContext;

            // validate general FAF criteria
            EPPreparedExecuteMethodHelper.ValidateFAFQuery(statementSpec);

            // obtain processor
            var streamSpec = statementSpec.StreamSpecs[0];
            Processor = FireAndForgetProcessorFactory.ValidateResolveProcessor(streamSpec, services);

            // obtain name and type
            var processorName = Processor.NamedWindowOrTableName;
            var eventType = Processor.EventTypeResultSetProcessor;

            // determine alias
            var aliasName = processorName;
            if (streamSpec.OptionalStreamName != null)
            {
                aliasName = streamSpec.OptionalStreamName;
            }

            // compile filter to optimize access to named window
            var typeService = new StreamTypeServiceImpl(new EventType[] { eventType }, new string[] { aliasName }, new bool[] { true }, services.EngineURI, true);
            var excludePlanHint = ExcludePlanHint.GetHint(typeService.StreamNames, statementContext);
            var queryGraph = new QueryGraph(1, excludePlanHint, false);
            if (statementSpec.FilterRootNode != null)
            {
                ExprNodeUtility.ValidateFilterWQueryGraphSafe(
                    queryGraph, statementSpec.FilterRootNode,
                    statementContext, typeService);
            }

            // validate expressions
            EPStatementStartMethodHelperValidate.ValidateNodes(statementSpec, statementContext, typeService, null);

            // get executor
            Executor = GetExecutor(queryGraph, aliasName);
        }

        /// <summary>
        /// Returns the event type of the prepared statement.
        /// </summary>
        /// <value>event type</value>
        public EventType EventType
        {
            get { return Processor.EventTypeResultSetProcessor; }
        }

        /// <summary>
        /// Executes the prepared query.
        /// </summary>
        /// <returns>query results</returns>
        public EPPreparedQueryResult Execute(ContextPartitionSelector[] contextPartitionSelectors)
        {
            try
            {
                if (contextPartitionSelectors != null && contextPartitionSelectors.Length != 1)
                {
                    throw new ArgumentException("Number of context partition selectors must be one");
                }
                var optionalSingleSelector = contextPartitionSelectors != null && contextPartitionSelectors.Length > 0 ? contextPartitionSelectors[0] : null;

                // validate context
                if (Processor.ContextName != null &&
                    StatementSpec.OptionalContextName != null &&
                    !Processor.ContextName.Equals(StatementSpec.OptionalContextName))
                {
                    throw new EPException("Context for named window is '" + Processor.ContextName + "' and query specifies context '" + StatementSpec.OptionalContextName + "'");
                }

                // handle non-specified context
                if (StatementSpec.OptionalContextName == null)
                {
                    FireAndForgetInstance processorInstance = Processor.GetProcessorInstanceNoContext();
                    if (processorInstance != null)
                    {
                        var rows = Executor.Execute(processorInstance);
                        if (rows != null && rows.Length > 0)
                        {
                            Dispatch();
                        }
                        return new EPPreparedQueryResult(Processor.EventTypePublic, rows);
                    }
                }

                // context partition runtime query
                var agentInstanceIds = EPPreparedExecuteMethodHelper.GetAgentInstanceIds(Processor, optionalSingleSelector, Services.ContextManagementService, Processor.ContextName);

                // collect events and agent instances
                if (agentInstanceIds.IsEmpty())
                {
                    return new EPPreparedQueryResult(Processor.EventTypeResultSetProcessor, CollectionUtil.EVENTBEANARRAY_EMPTY);
                }

                if (agentInstanceIds.Count == 1)
                {
                    int agentInstanceId = agentInstanceIds.First();
                    var processorInstance = Processor.GetProcessorInstanceContextById(agentInstanceId);
                    var rows = Executor.Execute(processorInstance);
                    if (rows.Length > 0)
                    {
                        Dispatch();
                    }
                    return new EPPreparedQueryResult(Processor.EventTypeResultSetProcessor, rows);
                }

                var allRows = new ArrayDeque<EventBean>();
                foreach (int agentInstanceId in agentInstanceIds)
                {
                    var processorInstance = Processor.GetProcessorInstanceContextById(agentInstanceId);
                    if (processorInstance != null)
                    {
                        var rows = Executor.Execute(processorInstance);
                        allRows.AddAll(rows);
                    }
                }
                if (allRows.Count > 0)
                {
                    Dispatch();
                }
                return new EPPreparedQueryResult(Processor.EventTypeResultSetProcessor, allRows.ToArray());
            }
            finally
            {
                if (HasTableAccess)
                {
                    Services.TableService.TableExprEvaluatorContext.ReleaseAcquiredLocks();
                }
            }
        }

        protected void Dispatch()
        {
            Services.InternalEventEngineRouteDest.ProcessThreadWorkQueue();
        }
    }
}
