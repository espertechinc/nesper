///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.core.context.factory;
using com.espertech.esper.core.context.mgr;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.lookup;
using com.espertech.esper.epl.spec;
using com.espertech.esper.epl.util;
using com.espertech.esper.view;

namespace com.espertech.esper.core.start
{
    /// <summary>
    /// Starts and provides the stop method for EPL statements.
    /// </summary>
    public class EPStatementStartMethodCreateIndex : EPStatementStartMethodBase
    {
        public EPStatementStartMethodCreateIndex(StatementSpecCompiled statementSpec)
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
            var spec = StatementSpec.CreateIndexDesc;
            var namedWindowProcessor = services.NamedWindowService.GetProcessor(spec.WindowName);
            var tableMetadata = services.TableService.GetTableMetadata(spec.WindowName);
            if (namedWindowProcessor == null && tableMetadata == null) {
                throw new ExprValidationException("A named window or table by name '" + spec.WindowName + "' does not exist");
            }
            var indexedEventType = namedWindowProcessor != null ? namedWindowProcessor.NamedWindowType : tableMetadata.InternalEventType;
            var infraContextName = namedWindowProcessor != null ? namedWindowProcessor.ContextName : tableMetadata.ContextName;
            EPLValidationUtil.ValidateContextName(namedWindowProcessor == null, spec.WindowName, infraContextName, StatementSpec.OptionalContextName, true);
    
            // validate index
            var validated = EventTableIndexUtil.ValidateCompileExplicitIndex(spec.IsUnique, spec.Columns, indexedEventType);
            var imk = new IndexMultiKey(spec.IsUnique, validated.HashProps, validated.BtreeProps);
    
            // for tables we add the index to metadata
            if (tableMetadata != null) {
                services.TableService.ValidateAddIndex(statementContext.StatementName, tableMetadata, spec.IndexName, imk);
            }
            else {
                namedWindowProcessor.ValidateAddIndex(statementContext.StatementName, spec.IndexName, imk);
            }
    
            // allocate context factory
            Viewable viewable = new ViewableDefaultImpl(indexedEventType);
            var contextFactory = new StatementAgentInstanceFactoryCreateIndex(services, spec, viewable, namedWindowProcessor, tableMetadata == null ? null : tableMetadata.TableName);
    
            // provide destroy method which de-registers interest in this index
            var finalTableService = services.TableService;
            var finalStatementName = statementContext.StatementName;
            var destroyMethod = new EPStatementDestroyCallbackList();
            if (tableMetadata != null) {
                destroyMethod.AddCallback(() => finalTableService.RemoveIndexReferencesStmtMayRemoveIndex(finalStatementName, tableMetadata));
            }
            else {
                destroyMethod.AddCallback(() => namedWindowProcessor.RemoveIndexReferencesStmtMayRemoveIndex(imk, finalStatementName));
            }
    
            EPStatementStopMethod stopMethod;
            if (StatementSpec.OptionalContextName != null) {
                var mergeView = new ContextMergeView(indexedEventType);
                var statement = new ContextManagedStatementCreateIndexDesc(StatementSpec, statementContext, mergeView, contextFactory);
                services.ContextManagementService.AddStatement(StatementSpec.OptionalContextName, statement, isRecoveringResilient);
                stopMethod = () => {};
    
                var contextManagementService = services.ContextManagementService;
                destroyMethod.AddCallback(() => contextManagementService.DestroyedStatement(StatementSpec.OptionalContextName, statementContext.StatementName, statementContext.StatementId));
            }
            else {
                var defaultAgentInstanceContext = GetDefaultAgentInstanceContext(statementContext);
                StatementAgentInstanceFactoryCreateIndexResult result;
                try {
                    result = (StatementAgentInstanceFactoryCreateIndexResult) contextFactory.NewContext(defaultAgentInstanceContext, false);
                }
                catch (EPException ex) {
                    if (ex.InnerException is ExprValidationException) {
                        throw (ExprValidationException) ex.InnerException;
                    }
                    throw;
                }
                var stopCallback = result.StopCallback;
                stopMethod = stopCallback.Invoke;
            }
    
            return new EPStatementStartResult(viewable, stopMethod, destroyMethod.Destroy);
        }
    }
}
