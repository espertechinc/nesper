///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.util;
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

        public override EPStatementStartResult StartInternal(
            EPServicesContext services,
            StatementContext statementContext,
            bool isNewStatement,
            bool isRecoveringStatement,
            bool isRecoveringResilient)
        {
            var createDesc = _statementSpec.CreateVariableDesc;

            VariableServiceUtil.CheckAlreadyDeclaredTable(createDesc.VariableName, services.TableService);

            // Get assignment value
            object value = null;
            if (createDesc.Assignment != null)
            {
                // Evaluate assignment expression
                StreamTypeService typeService = new StreamTypeServiceImpl(
                    new EventType[0], new string[0], new bool[0], services.EngineURI, false);
                var evaluatorContextStmt = new ExprEvaluatorContextStatement(statementContext, false);
                var validationContext = new ExprValidationContext(
                    statementContext.Container,
                    typeService,
                    statementContext.EngineImportService,
                    statementContext.StatementExtensionServicesContext,
                    null,
                    statementContext.SchedulingService,
                    statementContext.VariableService,
                    statementContext.TableService, evaluatorContextStmt,
                    statementContext.EventAdapterService,
                    statementContext.StatementName, statementContext.StatementId,
                    statementContext.Annotations,
                    statementContext.ContextDescriptor,
                    statementContext.ScriptingService,
                    false, false, false, false, null, false);
                var validated = ExprNodeUtility.GetValidatedSubtree(
                    ExprNodeOrigin.VARIABLEASSIGN, createDesc.Assignment, validationContext);
                value = validated.ExprEvaluator.Evaluate(new EvaluateParams(null, true, evaluatorContextStmt));
            }

            // Create variable
            try
            {
                services.VariableService.CreateNewVariable(
                    _statementSpec.OptionalContextName, createDesc.VariableName, createDesc.VariableType, createDesc.IsConstant,
                    createDesc.IsArray, createDesc.IsArrayOfPrimitive, value, services.EngineImportService);
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

            var destroyMethod = new EPStatementDestroyCallbackList();
            var stopMethod = new ProxyEPStatementStopMethod(() => { });

            var variableMetaData = services.VariableService.GetVariableMetaData(createDesc.VariableName);
            Viewable outputView;
            var eventType = CreateVariableView.GetEventType(
                statementContext.StatementId, services.EventAdapterService, variableMetaData);
            var contextFactory =
                new StatementAgentInstanceFactoryCreateVariable(
                    createDesc, _statementSpec, statementContext, services, variableMetaData, eventType);
            statementContext.StatementAgentInstanceFactory = contextFactory;

            if (_statementSpec.OptionalContextName != null)
            {
                var mergeView = new ContextMergeView(eventType);
                outputView = mergeView;
                var statement =
                    new ContextManagedStatementCreateVariableDesc(_statementSpec, statementContext, mergeView, contextFactory);
                services.ContextManagementService.AddStatement(
                    _statementSpec.OptionalContextName, statement, isRecoveringResilient);

                var contextManagementService = services.ContextManagementService;
                destroyMethod.AddCallback(new ProxyDestroyCallback(() => contextManagementService.DestroyedStatement(
                    _statementSpec.OptionalContextName, statementContext.StatementName,
                    statementContext.StatementId)));
            }
            else
            {
                var resultOfStart =
                    (StatementAgentInstanceFactoryCreateVariableResult)
                        contextFactory.NewContext(GetDefaultAgentInstanceContext(statementContext), isRecoveringResilient);
                outputView = resultOfStart.FinalView;

                if (statementContext.StatementExtensionServicesContext != null &&
                    statementContext.StatementExtensionServicesContext.StmtResources != null)
                {
                    var holder =
                        statementContext.StatementExtensionServicesContext.ExtractStatementResourceHolder(resultOfStart);
                    statementContext.StatementExtensionServicesContext.StmtResources.Unpartitioned = holder;
                    statementContext.StatementExtensionServicesContext.PostProcessStart(resultOfStart, isRecoveringResilient);
                }
            }

            services.StatementVariableRefService.AddReferences(
                statementContext.StatementName, Collections.SingletonList(createDesc.VariableName), null);
            return new EPStatementStartResult(outputView, stopMethod, destroyMethod);
        }
    }
} // end of namespace
