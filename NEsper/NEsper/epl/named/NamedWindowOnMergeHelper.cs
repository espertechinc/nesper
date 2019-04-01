///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.client.annotation;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.spec;
using com.espertech.esper.epl.updatehelper;
using com.espertech.esper.events;
using com.espertech.esper.events.map;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.named
{
    /// <summary>Factory for handles for updates/inserts/deletes/select</summary>
    public class NamedWindowOnMergeHelper
    {
        private readonly List<NamedWindowOnMergeMatch> _matched;
        private readonly List<NamedWindowOnMergeMatch> _unmatched;

        public NamedWindowOnMergeHelper(
            StatementContext statementContext,
            OnTriggerMergeDesc onTriggerDesc,
            EventType triggeringEventType,
            string triggeringStreamName,
            InternalEventRouter internalEventRouter,
            string namedWindowName,
            EventTypeSPI namedWindowType)
        {
            _matched = new List<NamedWindowOnMergeMatch>();
            _unmatched = new List<NamedWindowOnMergeMatch>();

            var count = 1;
            foreach (var matchedItem in onTriggerDesc.Items)
            {
                var actions = new List<NamedWindowOnMergeAction>();
                foreach (var item in matchedItem.Actions)
                {
                    try
                    {
                        if (item is OnTriggerMergeActionInsert)
                        {
                            var insertDesc = (OnTriggerMergeActionInsert) item;
                            actions.Add(
                                SetupInsert(
                                    namedWindowName, internalEventRouter, namedWindowType, count, insertDesc,
                                    triggeringEventType, triggeringStreamName, statementContext));
                        }
                        else if (item is OnTriggerMergeActionUpdate)
                        {
                            var updateDesc = (OnTriggerMergeActionUpdate) item;
                            var updateHelper = EventBeanUpdateHelperFactory.Make(
                                namedWindowName, namedWindowType, updateDesc.Assignments, onTriggerDesc.OptionalAsName,
                                triggeringEventType, true, statementContext.StatementName, statementContext.EngineURI,
                                statementContext.EventAdapterService);
                            var filterEval = updateDesc.OptionalWhereClause == null
                                ? null
                                : updateDesc.OptionalWhereClause.ExprEvaluator;
                            actions.Add(new NamedWindowOnMergeActionUpd(filterEval, updateHelper));
                        }
                        else if (item is OnTriggerMergeActionDelete)
                        {
                            var deleteDesc = (OnTriggerMergeActionDelete) item;
                            var filterEval = deleteDesc.OptionalWhereClause == null
                                ? null
                                : deleteDesc.OptionalWhereClause.ExprEvaluator;
                            actions.Add(new NamedWindowOnMergeActionDel(filterEval));
                        }
                        else
                        {
                            throw new ArgumentException("Invalid type of merge item '" + item.GetType() + "'");
                        }
                        count++;
                    }
                    catch (ExprValidationException ex)
                    {
                        var isNot = item is OnTriggerMergeActionInsert;
                        var message = "Validation failed in when-" + (isNot ? "not-" : "") + "matched (clause " + count +
                                      "): " + ex.Message;
                        throw new ExprValidationException(message, ex);
                    }
                }

                if (matchedItem.IsMatchedUnmatched)
                {
                    _matched.Add(new NamedWindowOnMergeMatch(matchedItem.OptionalMatchCond, actions));
                }
                else
                {
                    _unmatched.Add(new NamedWindowOnMergeMatch(matchedItem.OptionalMatchCond, actions));
                }
            }
        }

        public static IList<SelectClauseElementCompiled> CompileSelectNoWildcard(string triggeringStreamName, IList<SelectClauseElementCompiled> selectClause)
        {
            var selectNoWildcard = new List<SelectClauseElementCompiled>();
            foreach (var element in selectClause) {
                if (!(element is SelectClauseElementWildcard)) {
                    selectNoWildcard.Add(element);
                    continue;
                }
                var streamSelect = new SelectClauseStreamCompiledSpec(triggeringStreamName, null);
                streamSelect.StreamNumber = 1;
                selectNoWildcard.Add(streamSelect);
            }
            return selectNoWildcard;
        }
    
        private NamedWindowOnMergeActionIns SetupInsert(string namedWindowName, InternalEventRouter internalEventRouter, EventTypeSPI eventTypeNamedWindow, int selectClauseNumber, OnTriggerMergeActionInsert desc, EventType triggeringEventType, string triggeringStreamName, StatementContext statementContext)
                {
    
            // Compile insert-into INFO
            string streamName = desc.OptionalStreamName ?? eventTypeNamedWindow.Name;
            var insertIntoDesc = InsertIntoDesc.FromColumns(streamName, desc.Columns);
    
            // rewrite any wildcards to use "stream.wildcard"
            if (triggeringStreamName == null) {
                triggeringStreamName = UuidGenerator.Generate();
            }
            var selectNoWildcard = CompileSelectNoWildcard(triggeringStreamName, desc.SelectClauseCompiled);
    
            // Set up event types for select-clause evaluation: The first type does not contain anything as its the named window row which is not present for insert
            var dummyTypeNoProperties = new MapEventType(EventTypeMetadata.CreateAnonymous("merge_named_window_insert", ApplicationType.MAP), "merge_named_window_insert", 0, null, Collections.EmptyDataMap, null, null, null);
            var eventTypes = new EventType[]{dummyTypeNoProperties, triggeringEventType};
            var streamNames = new string[]{UuidGenerator.Generate(), triggeringStreamName};
            var streamTypeService = new StreamTypeServiceImpl(eventTypes, streamNames, new bool[1], statementContext.EngineURI, false);
    
            // Get select expr processor
            var selectExprEventTypeRegistry = new SelectExprEventTypeRegistry(statementContext.StatementName, statementContext.StatementEventTypeRef);
            var exprEvaluatorContext = new ExprEvaluatorContextStatement(statementContext, false);
                    var insertHelper = SelectExprProcessorFactory.GetProcessor(
                        statementContext.Container,
                        Collections.SingletonList(selectClauseNumber),
                        selectNoWildcard.ToArray(), false,
                        insertIntoDesc, null, null, streamTypeService,
                        statementContext.EventAdapterService,
                        statementContext.StatementResultService,
                        statementContext.ValueAddEventService,
                        selectExprEventTypeRegistry,
                        statementContext.EngineImportService,
                        exprEvaluatorContext,
                        statementContext.VariableService,
                        statementContext.ScriptingService,
                        statementContext.TableService,
                        statementContext.TimeProvider,
                        statementContext.EngineURI,
                        statementContext.StatementId,
                        statementContext.StatementName,
                        statementContext.Annotations,
                        statementContext.ContextDescriptor,
                        statementContext.ConfigSnapshot, null,
                        statementContext.NamedWindowMgmtService, null, null,
                        statementContext.StatementExtensionServicesContext);
            var filterEval = desc.OptionalWhereClause == null ? null : desc.OptionalWhereClause.ExprEvaluator;
    
            var routerToUser = streamName.Equals(namedWindowName) ? null : internalEventRouter;
            var audit = AuditEnum.INSERT.GetAudit(statementContext.Annotations) != null;
    
            string insertIntoTableName = null;
            if (statementContext.TableService.GetTableMetadata(insertIntoDesc.EventTypeName) != null) {
                insertIntoTableName = insertIntoDesc.EventTypeName;
            }
    
            return new NamedWindowOnMergeActionIns(filterEval, insertHelper, routerToUser, insertIntoTableName, statementContext.TableService, statementContext.EpStatementHandle, statementContext.InternalEventEngineRouteDest, audit);
        }

        public IList<NamedWindowOnMergeMatch> Matched => _matched;

        public IList<NamedWindowOnMergeMatch> Unmatched => _unmatched;
    }
} // end of namespace
