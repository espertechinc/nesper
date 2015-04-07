///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.context.factory;
using com.espertech.esper.core.context.mgr;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.spec;
using com.espertech.esper.epl.variable;
using com.espertech.esper.epl.view;
using com.espertech.esper.view;

namespace com.espertech.esper.core.start
{
    /// <summary>
    /// Starts and provides the stop method for EPL statements.
    /// </summary>
    public class EPStatementStartMethodCreateVariable : EPStatementStartMethodBase
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    
        public EPStatementStartMethodCreateVariable(StatementSpecCompiled statementSpec)
            : base(statementSpec)
        {
        }
    
        public override EPStatementStartResult StartInternal(EPServicesContext services, StatementContext statementContext, bool isNewStatement, bool isRecoveringStatement, bool isRecoveringResilient)
        {
            var createDesc = StatementSpec.CreateVariableDesc;

            VariableServiceUtil.CheckAlreadyDeclaredTable(createDesc.VariableName, services.TableService);
    
            // Get assignment value
            Object value = null;
            if (createDesc.Assignment != null)
            {
                // Evaluate assignment expression
                var typeService = new StreamTypeServiceImpl(new EventType[0], new String[0], new bool[0], services.EngineURI, false);
                var evaluatorContextStmt = new ExprEvaluatorContextStatement(statementContext, false);
                var validationContext = new ExprValidationContext(
                    typeService,
                    statementContext.MethodResolutionService, null,
                    statementContext.SchedulingService,
                    statementContext.VariableService,
                    statementContext.TableService, evaluatorContextStmt,
                    statementContext.EventAdapterService,
                    statementContext.StatementName,
                    statementContext.StatementId,
                    statementContext.Annotations,
                    statementContext.ContextDescriptor,
                    statementContext.ScriptingService,
                    false, false, false, false, null, false);
                var validated = ExprNodeUtility.GetValidatedSubtree(ExprNodeOrigin.VARIABLEASSIGN, createDesc.Assignment, validationContext);
                value = validated.ExprEvaluator.Evaluate(new EvaluateParams(null, true, evaluatorContextStmt));
            }
    
            // Create variable
            try
            {
                services.VariableService.CreateNewVariable(
                    StatementSpec.OptionalContextName, 
                    createDesc.VariableName, 
                    createDesc.VariableType,
                    createDesc.IsConstant, 
                    createDesc.IsArray,
                    createDesc.IsArrayOfPrimitive,
                    value, 
                    services.EngineImportService);
            }
            catch (VariableExistsException ex)
            {
                // for new statement we don't allow creating the same variable
                if (isNewStatement)
                {
                    throw new ExprValidationException("Cannot create variable: " + ex.Message, ex);
                }
            }
            catch (VariableDeclarationException ex)
            {
                throw new ExprValidationException("Cannot create variable: " + ex.Message, ex);
            }

            var destroyMethod = new EPStatementDestroyMethod(
                () =>
                {
                    try
                    {
                        services.StatementVariableRefService.RemoveReferencesStatement(statementContext.StatementName);
                    }
                    catch (Exception ex)
                    {
                        Log.Error("Error removing variable '" + createDesc.VariableName + "': " + ex.Message);
                    }
                });

            var stopMethod = new EPStatementStopMethod(() => { });
    
            VariableMetaData variableMetaData = services.VariableService.GetVariableMetaData(createDesc.VariableName);
            Viewable outputView;
    
            if (StatementSpec.OptionalContextName != null)
            {
                EventType eventType = CreateVariableView.GetEventType(statementContext.StatementId, services.EventAdapterService, variableMetaData);
                var contextFactory = new StatementAgentInstanceFactoryCreateVariable(statementContext, services, variableMetaData, eventType);
                var mergeView = new ContextMergeView(eventType);
                outputView = mergeView;
                var statement = new ContextManagedStatementCreateVariableDesc(StatementSpec, statementContext, mergeView, contextFactory);
                services.ContextManagementService.AddStatement(StatementSpec.OptionalContextName, statement, isRecoveringResilient);
            }
            else
            {
                // allocate
                services.VariableService.AllocateVariableState(createDesc.VariableName, VariableServiceConstants.NOCONTEXT_AGENTINSTANCEID, statementContext.ExtensionServicesContext);
                var createView = new CreateVariableView(statementContext.StatementId, services.EventAdapterService, services.VariableService, createDesc.VariableName, statementContext.StatementResultService);
    
                services.VariableService.RegisterCallback(createDesc.VariableName, 0, createView.Update);
                statementContext.StatementStopService.StatementStopped +=
                    () => services.VariableService.UnregisterCallback(createDesc.VariableName, 0, createView.Update);
    
                // Create result set processor, use wildcard selection
                StatementSpec.SelectClauseSpec.SetSelectExprList(new SelectClauseElementWildcard());
                StatementSpec.SelectStreamDirEnum = SelectClauseStreamSelectorEnum.RSTREAM_ISTREAM_BOTH;
                StreamTypeService typeService = new StreamTypeServiceImpl(new EventType[] {createView.EventType}, new String[] {"create_variable"}, new bool[] {true}, services.EngineURI, false);
                var agentInstanceContext = GetDefaultAgentInstanceContext(statementContext);
                var resultSetProcessorPrototype = ResultSetProcessorFactoryFactory.GetProcessorPrototype(
                        StatementSpec, statementContext, typeService, null, new bool[0], true, ContextPropertyRegistryImpl.EMPTY_REGISTRY, null, services.ConfigSnapshot);
                var resultSetProcessor = EPStatementStartMethodHelperAssignExpr.GetAssignResultSetProcessor(agentInstanceContext, resultSetProcessorPrototype);
    
                // Attach output view
                var outputViewFactory = OutputProcessViewFactoryFactory.Make(StatementSpec, services.InternalEventRouter, agentInstanceContext.StatementContext, resultSetProcessor.ResultEventType, null, services.TableService);
                var outputViewBase = outputViewFactory.MakeView(resultSetProcessor, agentInstanceContext);
                createView.AddView(outputViewBase);
                outputView = outputViewBase;
    
                services.StatementVariableRefService.AddReferences(statementContext.StatementName, Collections.SingletonList(createDesc.VariableName), null);
            }
    
            return new EPStatementStartResult(outputView, stopMethod, destroyMethod);
        }
    }
}
