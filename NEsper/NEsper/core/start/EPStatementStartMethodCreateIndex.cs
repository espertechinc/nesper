///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

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

	    public override EPStatementStartResult StartInternal(EPServicesContext services, StatementContext statementContext, bool isNewStatement, bool isRecoveringStatement, bool isRecoveringResilient)
        {
	        var spec = _statementSpec.CreateIndexDesc;
	        var namedWindowProcessor = services.NamedWindowMgmtService.GetProcessor(spec.WindowName);
	        var tableMetadata = services.TableService.GetTableMetadata(spec.WindowName);
	        if (namedWindowProcessor == null && tableMetadata == null) {
	            throw new ExprValidationException("A named window or table by name '" + spec.WindowName + "' does not exist");
	        }
	        var indexedEventType = namedWindowProcessor != null ? namedWindowProcessor.NamedWindowType : tableMetadata.InternalEventType;
	        var infraContextName = namedWindowProcessor != null ? namedWindowProcessor.ContextName : tableMetadata.ContextName;
	        EPLValidationUtil.ValidateContextName(namedWindowProcessor == null, spec.WindowName, infraContextName, _statementSpec.OptionalContextName, true);

            // validate index
            var explicitIndexDesc = EventTableIndexUtil.ValidateCompileExplicitIndex(spec.IndexName, spec.IsUnique, spec.Columns, indexedEventType, statementContext);
            var advancedIndexDesc = explicitIndexDesc.AdvancedIndexProvisionDesc == null ? null : explicitIndexDesc.AdvancedIndexProvisionDesc.IndexDesc;
            var imk = new IndexMultiKey(spec.IsUnique, explicitIndexDesc.HashPropsAsList, explicitIndexDesc.BtreePropsAsList, advancedIndexDesc);

            // for tables we add the index to metadata
            if (tableMetadata != null) {
                services.TableService.ValidateAddIndex(statementContext.StatementName, tableMetadata, spec.IndexName, explicitIndexDesc, imk);
	        }
	        else {
                namedWindowProcessor.ValidateAddIndex(statementContext.StatementName, spec.IndexName, explicitIndexDesc, imk);
            }

            // allocate context factory
            Viewable viewable = new ViewableDefaultImpl(indexedEventType);
	        var contextFactory = new StatementAgentInstanceFactoryCreateIndex(
	            services, spec, viewable, namedWindowProcessor, 
	            tableMetadata?.TableName,
	            _statementSpec.OptionalContextName,
	            explicitIndexDesc);
	        statementContext.StatementAgentInstanceFactory = contextFactory;

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
	        if (_statementSpec.OptionalContextName != null) {
	            var mergeView = new ContextMergeView(indexedEventType);
	            var statement = new ContextManagedStatementCreateIndexDesc(_statementSpec, statementContext, mergeView, contextFactory);
	            services.ContextManagementService.AddStatement(_statementSpec.OptionalContextName, statement, isRecoveringResilient);
	            stopMethod = new ProxyEPStatementStopMethod(() => {});

	            var contextManagementService = services.ContextManagementService;
	            destroyMethod.AddCallback(() => contextManagementService.DestroyedStatement(_statementSpec.OptionalContextName, statementContext.StatementName, statementContext.StatementId));
	        }
	        else {
	            var defaultAgentInstanceContext = GetDefaultAgentInstanceContext(statementContext);
	            StatementAgentInstanceFactoryResult result;
	            try {
	                result = contextFactory.NewContext(defaultAgentInstanceContext, isRecoveringResilient);
	            }
	            catch (EPException ex) {
	                if (ex.InnerException is ExprValidationException) {
	                    throw (ExprValidationException) ex.InnerException;
	                }
                    destroyMethod.Destroy();
	                throw;
	            }
                catch (Exception)
                {
                    destroyMethod.Destroy();
                    throw;
                }
	            var stopCallback = services.EpStatementFactory.MakeStopMethod(result);
	            stopMethod = new ProxyEPStatementStopMethod(stopCallback.Stop);

	            if (statementContext.StatementExtensionServicesContext != null && statementContext.StatementExtensionServicesContext.StmtResources != null) {
	                var holder = statementContext.StatementExtensionServicesContext.ExtractStatementResourceHolder(result);
	                statementContext.StatementExtensionServicesContext.StmtResources.Unpartitioned = holder;
	                statementContext.StatementExtensionServicesContext.PostProcessStart(result, isRecoveringResilient);
	            }
	        }

	        if (tableMetadata != null) {
	            services.StatementVariableRefService.AddReferences(statementContext.StatementName, tableMetadata.TableName);
	        }
	        else {
	            services.StatementVariableRefService.AddReferences(statementContext.StatementName, namedWindowProcessor.NamedWindowType.Name);
	        }

	        return new EPStatementStartResult(viewable, stopMethod, destroyMethod);
	    }
	}
} // end of namespace
