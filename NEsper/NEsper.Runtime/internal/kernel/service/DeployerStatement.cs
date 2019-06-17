///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.context.aifactory.createcontext;
using com.espertech.esper.common.@internal.context.airegistry;
using com.espertech.esper.common.@internal.context.mgr;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.statement.dispatch;
using com.espertech.esper.common.@internal.statement.resource;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.runtime.@internal.kernel.statement;

namespace com.espertech.esper.runtime.@internal.kernel.service
{
    public class DeployerStatement
    {
        public static EPStatementSPI DeployStatement(bool recovery, StatementLightweight lightweight, EPServicesContext services, EPRuntimeSPI epRuntime)
        {
            // statement-create: safe operation for registering things
            var statementAgentInstanceFactory = lightweight.StatementContext.StatementAIFactoryProvider.Factory;
            statementAgentInstanceFactory.StatementCreate(lightweight.StatementContext);

            // add statement
            var stmt = MakeStatement(lightweight.StatementContext.UpdateDispatchView, lightweight.StatementContext, (StatementResultServiceImpl) lightweight.StatementResultService, services.EpStatementFactory, epRuntime);

            // add statement to globals
            services.StatementLifecycleService.AddStatement(stmt); // it is now available for lookup

            Viewable finalView;
            StatementDestroyCallback statementDestroyCallback;
            ICollection<StatementAgentInstancePreload> preloads = null;
            var contextName = lightweight.StatementInformationals.OptionalContextName;

            if (contextName == null)
            {
                var result = StartStatementNoContext(lightweight, recovery, services);
                finalView = result.FinalView;
                preloads = result.PreloadList;
                var createContextStmt = result is StatementAgentInstanceFactoryCreateContextResult;
                statementDestroyCallback = new ProxyStatementDestroyCallback()
                {
                    ProcDestroy = (destroyServices, statementContext) => {
                        // All statements other that create-context: get the agent-instance-context and stop
                        // Create-context statements already got destroyed when the last statement associated to context was removed.
                        if (!createContextStmt)
                        {
                            var holder = statementContext.StatementCPCacheService.MakeOrGetEntryCanNull(-1, statementContext);
                            holder.AgentInstanceStopCallback.Stop(new AgentInstanceStopServices(holder.AgentInstanceContext));
                        }

                        // Invoke statement-destroy
                        statementAgentInstanceFactory.StatementDestroy(lightweight.StatementContext);
                    },
                };

                // assign
                StatementAIFactoryAssignments assignments = new StatementAIFactoryAssignmentsImpl(result.OptionalAggegationService, result.PriorStrategies,
                        result.PreviousGetterStrategies, result.SubselectStrategies, result.TableAccessStrategies, result.RowRecogPreviousStrategy);
                lightweight.StatementContext.StatementAIFactoryProvider.Assign(assignments);
            }
            else
            {
                var contextModuleName = lightweight.StatementInformationals.OptionalContextModuleName;
                var statementAIResourceRegistry = lightweight.StatementContext.StatementAIResourceRegistry;

                var contextVisibility = lightweight.StatementInformationals.OptionalContextVisibility;
                var contextDeploymentId = ContextDeployTimeResolver.ResolveContextDeploymentId(contextModuleName, contextVisibility, contextName, lightweight.StatementContext.DeploymentId, lightweight.StatementContext.PathContextRegistry);

                var contextMergeView = lightweight.StatementInformationals.StatementType.IsOnTriggerInfra() 
                    ? new ContextMergeViewForwarding(null) 
                    : new ContextMergeView(null);
                finalView = contextMergeView;
                var statement = new ContextControllerStatementDesc(lightweight, contextMergeView);

                // assignments before add-statement, since add-statement creates context partitions which may preload
                lightweight.StatementContext.StatementAIFactoryProvider.Assign(new StatementAIFactoryAssignmentContext(statementAIResourceRegistry));

                // add statement
                services.ContextManagementService.AddStatement(contextDeploymentId, contextName, statement, recovery);
                statementDestroyCallback = new ProxyStatementDestroyCallback()
                {
                    ProcDestroy = (destroyServices, statementContext) => {
                        services.ContextManagementService.StoppedStatement(contextDeploymentId, contextName, statement);
                        statementAgentInstanceFactory.StatementDestroy(lightweight.StatementContext);
                    },
                };
            }

            // make dispatch view
            finalView.Child = lightweight.StatementContext.UpdateDispatchView;

            // assign parent view
            stmt.StatementContext.DestroyCallback = statementDestroyCallback;
            stmt.ParentView = finalView;

            // execute preloads
            if (preloads != null)
            {
                foreach (var preload in preloads)
                {
                    preload.ExecutePreload();
                }
            }

            return stmt;
        }

        private static EPStatementSPI MakeStatement(UpdateDispatchView dispatchChildView, StatementContext statementContext, StatementResultServiceImpl statementResultService, EPStatementFactory epStatementFactory, EPRuntimeSPI epRuntime)
        {
            var epStatement = epStatementFactory.Statement(new EPStatementFactoryArgs(statementContext, dispatchChildView, statementResultService));
            var info = statementContext.StatementInformationals;
            statementResultService.SetSelectClause(info.SelectClauseTypes, info.SelectClauseColumnNames, info.IsForClauseDelivery, info.GroupDeliveryEval);
            statementResultService.SetContext(epStatement, epRuntime);
            return epStatement;
        }

        private static StatementAgentInstanceFactoryResult StartStatementNoContext(StatementLightweight lightweight, bool recovery, EPServicesContext services)
        {
            var statementContext = lightweight.StatementContext;
            var agentInstanceContext = statementContext.MakeAgentInstanceContextUnpartitioned();

            // start
            var result = lightweight.StatementProvider.StatementAIFactoryProvider.Factory.NewContext(agentInstanceContext, recovery);

            // keep
            var holder = services.StatementResourceHolderBuilder.Build(agentInstanceContext, result);
            statementContext.StatementCPCacheService.StatementResourceService.Unpartitioned = holder;

            return result;
        }
    }
} // end of namespace