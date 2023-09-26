///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.join.lookup;
using com.espertech.esper.common.@internal.epl.join.queryplan;
using com.espertech.esper.common.@internal.epl.lookupplansubord;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.table.compiletime
{
    public class TableMetaData : Copyable<TableMetaData>
    {
        public TableMetaData()
        {
        }

        public TableMetaData(
            string tableName,
            string tableModuleName,
            NameAccessModifier tableVisibility,
            string optionalContextName,
            NameAccessModifier? optionalContextVisibility,
            string optionalContextModule,
            EventType internalEventType,
            EventType publicEventType,
            string[] keyColumns,
            Type[] keyTypes,
            int[] keyColNums,
            IDictionary<string, TableMetadataColumn> columns,
            int numMethodAggs,
            StateMgmtSetting stateMgmtSettingsPrimaryKey,
            StateMgmtSetting stateMgmtSettingsUnkeyed)
        {
            TableName = tableName;
            TableModuleName = tableModuleName;
            TableVisibility = tableVisibility;
            OptionalContextName = optionalContextName;
            OptionalContextVisibility = optionalContextVisibility;
            OptionalContextModule = optionalContextModule;
            InternalEventType = internalEventType;
            PublicEventType = publicEventType;
            KeyColumns = keyColumns;
            KeyTypes = keyTypes;
            Columns = columns;
            KeyColNums = keyColNums;
            NumMethodAggs = numMethodAggs;
            StateMgmtSettingsPrimaryKey = stateMgmtSettingsPrimaryKey;
            StateMgmtSettingsUnkeyed = stateMgmtSettingsUnkeyed;
            Init();
        }

        private TableMetaData(
            string tableName,
            string tableModuleName,
            NameAccessModifier tableVisibility,
            string optionalContextName,
            NameAccessModifier? optionalContextVisibility,
            string optionalContextModule,
            EventType internalEventType,
            EventType publicEventType,
            string[] keyColumns,
            Type[] keyTypes,
            int[] keyColNums,
            IDictionary<string, TableMetadataColumn> columns,
            int numMethodAggs,
            IndexMultiKey keyIndexMultiKey,
            EventTableIndexMetadata indexMetadata,
            StateMgmtSetting stateMgmtSettingsPrimaryKey,
            StateMgmtSetting stateMgmtSettingsUnkeyed)
        {
            TableName = tableName;
            TableModuleName = tableModuleName;
            TableVisibility = tableVisibility;
            OptionalContextName = optionalContextName;
            OptionalContextVisibility = optionalContextVisibility;
            OptionalContextModule = optionalContextModule;
            InternalEventType = internalEventType;
            PublicEventType = publicEventType;
            KeyColumns = keyColumns;
            KeyTypes = keyTypes;
            KeyColNums = keyColNums;
            Columns = columns;
            NumMethodAggs = numMethodAggs;
            KeyIndexMultiKey = keyIndexMultiKey;
            IndexMetadata = indexMetadata;
            StateMgmtSettingsPrimaryKey = stateMgmtSettingsPrimaryKey;
            StateMgmtSettingsUnkeyed = stateMgmtSettingsUnkeyed;
        }

        public EventType InternalEventType { get; set; }

        public EventType PublicEventType { get; set; }

        public string TableName { get; set; }

        public string OptionalContextName { get; set; }

        public NameAccessModifier? OptionalContextVisibility { get; set; }

        public string OptionalContextModule { get; set; }

        public Type[] KeyTypes { get; set; }

        public IDictionary<string, TableMetadataColumn> Columns { get; set; }

        public int NumMethodAggs { set; get; }

        public string[] KeyColumns { set; get; }

        public ISet<string> UniquenessAsSet {
            get {
                if (KeyColumns == null || KeyColumns.Length == 0) {
                    return EmptySet<string>.Instance;
                }

                return new HashSet<string>(KeyColumns);
            }
        }

        public EventTableIndexMetadata IndexMetadata { get; set; } = new EventTableIndexMetadata();

        public bool IsKeyed => KeyTypes != null && KeyTypes.Length > 0;

        public int[] KeyColNums { get; set; }

        public IndexMultiKey KeyIndexMultiKey { get; set; }

        public string TableModuleName { get; set; }

        public NameAccessModifier TableVisibility { get; set; }

        public StateMgmtSetting StateMgmtSettingsPrimaryKey { get; set; }

        public StateMgmtSetting StateMgmtSettingsUnkeyed { get; set; }

        public TableMetaData Copy()
        {
            return new TableMetaData(
                TableName,
                TableModuleName,
                TableVisibility,
                OptionalContextName,
                OptionalContextVisibility,
                OptionalContextModule,
                InternalEventType,
                PublicEventType,
                KeyColumns,
                KeyTypes,
                KeyColNums,
                Columns,
                NumMethodAggs,
                KeyIndexMultiKey,
                IndexMetadata.Copy(),
                StateMgmtSettingsPrimaryKey,
                StateMgmtSettingsUnkeyed);
        }

        public void Init()
        {
            // add index multi-key for implicit primary-key index
            if (KeyColumns == null || KeyColumns.Length == 0) {
                return;
            }

            var props = new IndexedPropDesc[KeyColumns.Length];
            for (var i = 0; i < props.Length; i++) {
                props[i] = new IndexedPropDesc(KeyColumns[i], KeyTypes[i]);
            }

            KeyIndexMultiKey = new IndexMultiKey(true, props, new IndexedPropDesc[0], null);
            try {
                IndexMetadata.AddIndexExplicit(true, KeyIndexMultiKey, TableName, TableModuleName, null, "");
            }
            catch (ExprValidationException e) {
                throw new EPException("Failed to add primary key index: " + e.Message, e);
            }
        }

        public CodegenExpression Make(
            CodegenMethodScope parent,
            ModuleTableInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(TableMetaData), GetType(), classScope);
            method.Block
                .DeclareVarNewInstance(typeof(TableMetaData), "meta")
                .ExprDotMethod(Ref("meta"), "setTableName", Constant(TableName))
                .ExprDotMethod(Ref("meta"), "setTableModuleName", Constant(TableModuleName))
                .ExprDotMethod(Ref("meta"), "setTableVisibility", Constant(TableVisibility))
                .ExprDotMethod(Ref("meta"), "setOptionalContextName", Constant(OptionalContextName))
                .ExprDotMethod(Ref("meta"), "setOptionalContextVisibility", Constant(OptionalContextVisibility))
                .ExprDotMethod(Ref("meta"), "setOptionalContextModule", Constant(OptionalContextModule))
                .ExprDotMethod(
                    Ref("meta"),
                    "setInternalEventType",
                    EventTypeUtility.ResolveTypeCodegen(InternalEventType, symbols.GetAddInitSvc(method)))
                .ExprDotMethod(
                    Ref("meta"),
                    "setPublicEventType",
                    EventTypeUtility.ResolveTypeCodegen(PublicEventType, symbols.GetAddInitSvc(method)))
                .ExprDotMethod(Ref("meta"), "setKeyColumns", Constant(KeyColumns))
                .ExprDotMethod(Ref("meta"), "setKeyTypes", Constant(KeyTypes))
                .ExprDotMethod(Ref("meta"), "setKeyColNums", Constant(KeyColNums))
                .ExprDotMethod(
                    Ref("meta"),
                    "setColumns",
                    TableMetadataColumn.MakeColumns(Columns, method, symbols, classScope))
                .ExprDotMethod(Ref("meta"), "setNumMethodAggs", Constant(NumMethodAggs))
                .ExprDotMethod(
                    Ref("meta"),
                    "setStateMgmtSettingsPrimaryKey",
                    StateMgmtSettingsPrimaryKey.ToExpression())
                .ExprDotMethod(Ref("meta"), "setStateMgmtSettingsUnkeyed", StateMgmtSettingsUnkeyed.ToExpression())
                .ExprDotMethod(Ref("meta"), "init")
                .MethodReturn(Ref("meta"));
            return LocalMethod(method);
        }

        public CodegenExpression Make(CodegenExpressionRef addInitSvc)
        {
            return NewInstance(
                typeof(TableMetaData),
                Constant(TableName),
                Constant(OptionalContextName),
                Constant(OptionalContextVisibility),
                Constant(OptionalContextModule),
                EventTypeUtility.ResolveTypeCodegen(InternalEventType, addInitSvc),
                EventTypeUtility.ResolveTypeCodegen(PublicEventType, addInitSvc));
        }

        public void AddIndex(
            string indexName,
            string indexModuleName,
            IndexMultiKey imk,
            QueryPlanIndexItem indexItem)
        {
            IndexMetadata.AddIndexExplicit(false, imk, indexName, indexModuleName, indexItem, "");
        }
    }
} // end of namespace