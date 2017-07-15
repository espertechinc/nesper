///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.annotation;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.named;
using com.espertech.esper.epl.spec;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.epl.table.upd;
using com.espertech.esper.epl.updatehelper;
using com.espertech.esper.events;
using com.espertech.esper.events.map;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.table.merge
{
    /// <summary>Factory for handles for updates/inserts/deletes/select</summary>
    public class TableOnMergeHelper {
        private List<TableOnMergeMatch> matched;
        private List<TableOnMergeMatch> unmatched;
        private bool requiresWriteLock;
    
        public TableOnMergeHelper(StatementContext statementContext,
                                  OnTriggerMergeDesc onTriggerDesc,
                                  EventType triggeringEventType,
                                  string triggeringStreamName,
                                  InternalEventRouter internalEventRouter,
                                  TableMetadata tableMetadata)
                {
            matched = new List<TableOnMergeMatch>();
            unmatched = new List<TableOnMergeMatch>();
    
            int count = 1;
            bool hasDeleteAction = false;
            bool hasInsertIntoTableAction = false;
            bool hasUpdateAction = false;
            foreach (OnTriggerMergeMatched matchedItem in onTriggerDesc.Items) {
                var actions = new List<TableOnMergeAction>();
                foreach (OnTriggerMergeAction item in matchedItem.Actions) {
                    try {
                        if (item is OnTriggerMergeActionInsert) {
                            OnTriggerMergeActionInsert insertDesc = (OnTriggerMergeActionInsert) item;
                            TableOnMergeActionIns action = SetupInsert(tableMetadata, internalEventRouter, count, insertDesc, triggeringEventType, triggeringStreamName, statementContext);
                            actions.Add(action);
                            hasInsertIntoTableAction = action.IsInsertIntoBinding;
                        } else if (item is OnTriggerMergeActionUpdate) {
                            OnTriggerMergeActionUpdate updateDesc = (OnTriggerMergeActionUpdate) item;
                            EventBeanUpdateHelper updateHelper = EventBeanUpdateHelperFactory.Make(tableMetadata.TableName, tableMetadata.InternalEventType, updateDesc.Assignments, onTriggerDesc.OptionalAsName, triggeringEventType, false, statementContext.StatementName, statementContext.EngineURI, statementContext.EventAdapterService);
                            ExprEvaluator filterEval = updateDesc.OptionalWhereClause == null ? null : updateDesc.OptionalWhereClause.ExprEvaluator;
                            TableUpdateStrategy tableUpdateStrategy = statementContext.TableService.GetTableUpdateStrategy(tableMetadata, updateHelper, true);
                            var upd = new TableOnMergeActionUpd(filterEval, tableUpdateStrategy);
                            actions.Add(upd);
                            statementContext.TableService.AddTableUpdateStrategyReceiver(tableMetadata, statementContext.StatementName, upd, updateHelper, true);
                            hasUpdateAction = true;
                        } else if (item is OnTriggerMergeActionDelete) {
                            OnTriggerMergeActionDelete deleteDesc = (OnTriggerMergeActionDelete) item;
                            ExprEvaluator filterEval = deleteDesc.OptionalWhereClause == null ? null : deleteDesc.OptionalWhereClause.ExprEvaluator;
                            actions.Add(new TableOnMergeActionDel(filterEval));
                            hasDeleteAction = true;
                        } else {
                            throw new ArgumentException("Invalid type of merge item '" + item.Class + "'");
                        }
                        count++;
                    } catch (ExprValidationException ex) {
                        bool isNot = item is OnTriggerMergeActionInsert;
                        string message = "Validation failed in when-" + (isNot ? "not-" : "") + "matched (clause " + count + "): " + ex.Message;
                        throw new ExprValidationException(message, ex);
                    }
                }
    
                if (matchedItem.IsMatchedUnmatched) {
                    matched.Add(new TableOnMergeMatch(matchedItem.OptionalMatchCond, actions));
                } else {
                    unmatched.Add(new TableOnMergeMatch(matchedItem.OptionalMatchCond, actions));
                }
            }
    
            // since updates may change future secondary keys
            requiresWriteLock = hasDeleteAction || hasInsertIntoTableAction || hasUpdateAction;
        }
    
        private TableOnMergeActionIns SetupInsert(TableMetadata tableMetadata, InternalEventRouter internalEventRouter, int selectClauseNumber, OnTriggerMergeActionInsert desc, EventType triggeringEventType, string triggeringStreamName, StatementContext statementContext)
                {
    
            // Compile insert-into INFO
            string streamName = desc.OptionalStreamName != null ? Desc.OptionalStreamName : tableMetadata.TableName;
            InsertIntoDesc insertIntoDesc = InsertIntoDesc.FromColumns(streamName, desc.Columns);
            EventType insertIntoTargetType = streamName.Equals(tableMetadata.TableName) ? TableMetadata.InternalEventType : null;
    
            // rewrite any wildcards to use "stream.wildcard"
            if (triggeringStreamName == null) {
                triggeringStreamName = UuidGenerator.Generate();
            }
            List<SelectClauseElementCompiled> selectNoWildcard = NamedWindowOnMergeHelper.CompileSelectNoWildcard(triggeringStreamName, desc.SelectClauseCompiled);
    
            // Set up event types for select-clause evaluation: The first type does not contain anything as its the named window row which is not present for insert
            var dummyTypeNoProperties = new MapEventType(EventTypeMetadata.CreateAnonymous("merge_named_window_insert", EventTypeMetadata.ApplicationType.MAP), "merge_named_window_insert", 0, null, Collections.<string, Object>EmptyMap(), null, null, null);
            var eventTypes = new EventType[]{dummyTypeNoProperties, triggeringEventType};
            var streamNames = new string[]{UuidGenerator.Generate(), triggeringStreamName};
            var streamTypeService = new StreamTypeServiceImpl(eventTypes, streamNames, new bool[1], statementContext.EngineURI, false);
    
            // Get select expr processor
            var selectExprEventTypeRegistry = new SelectExprEventTypeRegistry(statementContext.StatementName, statementContext.StatementEventTypeRef);
            var exprEvaluatorContext = new ExprEvaluatorContextStatement(statementContext, false);
            SelectExprProcessor insertHelper = SelectExprProcessorFactory.GetProcessor(Collections.Singleton(selectClauseNumber),
                    selectNoWildcard.ToArray(new SelectClauseElementCompiled[selectNoWildcard.Count]), false, insertIntoDesc, insertIntoTargetType, null, streamTypeService,
                    statementContext.EventAdapterService, statementContext.StatementResultService, statementContext.ValueAddEventService, selectExprEventTypeRegistry,
                    statementContext.EngineImportService, exprEvaluatorContext, statementContext.VariableService, statementContext.TableService, statementContext.TimeProvider, statementContext.EngineURI, statementContext.StatementId, statementContext.StatementName, statementContext.Annotations, statementContext.ContextDescriptor, statementContext.ConfigSnapshot, null, statementContext.NamedWindowMgmtService, null, null,
                    statementContext.StatementExtensionServicesContext);
            ExprEvaluator filterEval = desc.OptionalWhereClause == null ? null : desc.OptionalWhereClause.ExprEvaluator;
    
            InternalEventRouter routerToUser = streamName.Equals(tableMetadata.TableName) ? null : internalEventRouter;
            bool audit = AuditEnum.INSERT.GetAudit(statementContext.Annotations) != null;
            return new TableOnMergeActionIns(filterEval, insertHelper, routerToUser, statementContext.EpStatementHandle, statementContext.InternalEventEngineRouteDest, audit, tableMetadata.RowFactory);
        }
    
        public List<TableOnMergeMatch> GetMatched() {
            return matched;
        }
    
        public List<TableOnMergeMatch> GetUnmatched() {
            return unmatched;
        }
    
        public bool IsRequiresWriteLock() {
            return requiresWriteLock;
        }
    }
} // end of namespace
