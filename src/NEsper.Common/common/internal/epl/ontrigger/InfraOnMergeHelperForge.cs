///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.annotation;
using com.espertech.esper.common.client.meta;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.resultset.select.core;
using com.espertech.esper.common.@internal.epl.streamtype;
using com.espertech.esper.common.@internal.epl.table.compiletime;
using com.espertech.esper.common.@internal.epl.table.update;
using com.espertech.esper.common.@internal.epl.updatehelper;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.ontrigger
{
    /// <summary>
    ///     Factory for handles for updates/inserts/deletes/select
    /// </summary>
    public class InfraOnMergeHelperForge
    {
        private readonly InfraOnMergeActionInsForge insertUnmatched;
        private readonly IList<InfraOnMergeMatchForge> matched;
        private readonly bool requiresTableWriteLock;
        private readonly IList<InfraOnMergeMatchForge> unmatched;

        public InfraOnMergeHelperForge(
            OnTriggerMergeDesc onTriggerDesc,
            EventType triggeringEventType,
            string triggeringStreamName,
            string infraName,
            EventTypeSPI infraEventType,
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices services,
            TableMetaData table)
        {
            matched = new List<InfraOnMergeMatchForge>();
            unmatched = new List<InfraOnMergeMatchForge>();

            var count = 1;
            var hasDeleteAction = false;
            var hasInsertIntoTableAction = false;
            var hasUpdateAction = false;

            foreach (var matchedItem in onTriggerDesc.Items) {
                IList<InfraOnMergeActionForge> actions = new List<InfraOnMergeActionForge>();
                foreach (OnTriggerMergeAction item in matchedItem.Actions) {
                    try {
                        if (item is OnTriggerMergeActionInsert) {
                            var insertDesc = (OnTriggerMergeActionInsert) item;
                            var forge = SetupInsert(
                                infraName,
                                infraEventType,
                                insertDesc,
                                triggeringEventType,
                                triggeringStreamName,
                                statementRawInfo,
                                services,
                                table != null);
                            actions.Add(forge);
                            hasInsertIntoTableAction = forge.InsertIntoTable != null;
                        }
                        else if (item is OnTriggerMergeActionUpdate) {
                            var updateDesc = (OnTriggerMergeActionUpdate) item;
                            EventBeanUpdateHelperForge updateHelper = EventBeanUpdateHelperForgeFactory.Make(
                                infraName,
                                infraEventType,
                                updateDesc.Assignments,
                                onTriggerDesc.OptionalAsName,
                                triggeringEventType,
                                true,
                                statementRawInfo.StatementName,
                                services.EventTypeAvroHandler);
                            ExprNode filterEval = updateDesc.OptionalWhereClause;
                            if (table != null) {
                                TableUpdateStrategyFactory.ValidateTableUpdateOnMerge(
                                    table,
                                    updateHelper.UpdateItemsPropertyNames);
                            }

                            var forge = new InfraOnMergeActionUpdForge(filterEval, updateHelper, table);
                            actions.Add(forge);
                            hasUpdateAction = true;
                        }
                        else if (item is OnTriggerMergeActionDelete) {
                            var deleteDesc = (OnTriggerMergeActionDelete) item;
                            ExprNode filterEval = deleteDesc.OptionalWhereClause;
                            actions.Add(new InfraOnMergeActionDelForge(filterEval));
                            hasDeleteAction = true;
                        }
                        else {
                            throw new ArgumentException("Invalid type of merge item '" + item.GetType() + "'");
                        }

                        count++;
                    }
                    catch (Exception ex) when (ex is ExprValidationException || ex is EPException) {
                        var isNot = item is OnTriggerMergeActionInsert;
                        var message = "Validation failed in when-" +
                                      (isNot ? "not-" : "") +
                                      "matched (clause " +
                                      count +
                                      "): " +
                                      ex.Message;
                        throw new ExprValidationException(message, ex);
                    }
                }

                if (matchedItem.IsMatchedUnmatched) {
                    matched.Add(new InfraOnMergeMatchForge(matchedItem.OptionalMatchCond, actions));
                }
                else {
                    unmatched.Add(new InfraOnMergeMatchForge(matchedItem.OptionalMatchCond, actions));
                }
            }

            if (onTriggerDesc.OptionalInsertNoMatch != null) {
                insertUnmatched = SetupInsert(
                    infraName,
                    infraEventType,
                    onTriggerDesc.OptionalInsertNoMatch,
                    triggeringEventType,
                    triggeringStreamName,
                    statementRawInfo,
                    services,
                    table != null);
            }

            requiresTableWriteLock = hasDeleteAction || hasInsertIntoTableAction || hasUpdateAction;
        }

        public CodegenExpression Make(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(InfraOnMergeHelper), GetType(), classScope);
            method.Block
                .DeclareVar<InfraOnMergeActionIns>(
                    "insertUnmatched",
                    insertUnmatched == null ? ConstantNull() : insertUnmatched.Make(method, symbols, classScope))
                .DeclareVar<IList<InfraOnMergeMatch>>("matched", MakeList(matched, method, symbols, classScope))
                .DeclareVar<IList<InfraOnMergeMatch>>("unmatched", MakeList(unmatched, method, symbols, classScope))
                .MethodReturn(
                    NewInstance<InfraOnMergeHelper>(
                        Ref("insertUnmatched"),
                        Ref("matched"),
                        Ref("unmatched"),
                        Constant(requiresTableWriteLock)));
            return LocalMethod(method);
        }

        private CodegenExpression MakeList(
            IList<InfraOnMergeMatchForge> items,
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(IList<InfraOnMergeMatch>), GetType(), classScope);
            method.Block.DeclareVar<IList<InfraOnMergeMatch>>(
                "list",
                NewInstance<List<InfraOnMergeMatch>>(Constant(items.Count)));
            foreach (var item in items) {
                method.Block.ExprDotMethod(Ref("list"), "Add", item.Make(method, symbols, classScope));
            }

            method.Block.MethodReturn(Ref("list"));
            return LocalMethod(method);
        }

        private InfraOnMergeActionInsForge SetupInsert(
            string infraName,
            EventType infraType,
            OnTriggerMergeActionInsert desc,
            EventType triggeringEventType,
            string triggeringStreamName,
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices services,
            bool isTable)
        {
            // Compile insert-into info
            var streamName = desc.OptionalStreamName != null ? desc.OptionalStreamName : infraName;
            InsertIntoDesc insertIntoDesc = InsertIntoDesc.FromColumns(streamName, desc.Columns);

            // rewrite any wildcards to use "stream.wildcard"
            if (triggeringStreamName == null) {
                triggeringStreamName = UuidGenerator.Generate();
            }

            var selectNoWildcard = CompileSelectNoWildcard(triggeringStreamName, desc.SelectClauseCompiled);

            // Set up event types for select-clause evaluation: The first type does not contain anything as its the named-window or table row which is not present for insert
            var eventTypeMetadata = new EventTypeMetadata(
                "merge_infra_insert",
                statementRawInfo.ModuleName,
                EventTypeTypeClass.STREAM,
                EventTypeApplicationType.MAP,
                NameAccessModifier.TRANSIENT,
                EventTypeBusModifier.NONBUS,
                false,
                EventTypeIdPair.Unassigned());
            EventType dummyTypeNoProperties = BaseNestableEventUtil.MakeMapTypeCompileTime(
                eventTypeMetadata,
                Collections.GetEmptyMap<string, object>(),
                null,
                null,
                null,
                null,
                services.BeanEventTypeFactoryPrivate,
                services.EventTypeCompileTimeResolver);
            var eventTypes = new EventType[] {dummyTypeNoProperties, triggeringEventType};
            var streamNames = new string[] {UuidGenerator.Generate(), triggeringStreamName};
            StreamTypeService streamTypeService = new StreamTypeServiceImpl(
                eventTypes,
                streamNames,
                new bool[eventTypes.Length],
                false,
                false);

            // Get select expr processor
            var selectClause = selectNoWildcard.ToArray();
            var args = new SelectProcessorArgs(
                selectClause,
                null,
                false,
                null,
                null,
                streamTypeService,
                null,
                false,
                statementRawInfo.Annotations,
                statementRawInfo,
                services);
            if (isTable && streamName.Equals(infraName)) {
                args.OptionalInsertIntoEventType = infraType;
            }

            SelectExprProcessorForge insertHelperForge =
                SelectExprProcessorFactory.GetProcessor(args, insertIntoDesc, false).Forge;
            ExprNode filterEval = desc.OptionalWhereClause;

            var route = !streamName.Equals(infraName);
            bool audit = AuditEnum.INSERT.GetAudit(statementRawInfo.Annotations) != null;

            TableMetaData insertIntoTable = services.TableCompileTimeResolver.Resolve(insertIntoDesc.EventTypeName);
            return new InfraOnMergeActionInsForge(filterEval, insertHelperForge, insertIntoTable, audit, route);
        }

        public static IList<SelectClauseElementCompiled> CompileSelectNoWildcard(
            string triggeringStreamName,
            IList<SelectClauseElementCompiled> selectClause)
        {
            IList<SelectClauseElementCompiled> selectNoWildcard = new List<SelectClauseElementCompiled>();
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
    }
} // end of namespace