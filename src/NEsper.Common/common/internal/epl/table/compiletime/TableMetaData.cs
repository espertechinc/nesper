///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
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
            StateMgmtSetting primaryKeyStateMgmtSettings)
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
            PrimaryKeyStateMgmtSettings = primaryKeyStateMgmtSettings;
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
            StateMgmtSetting primaryKeyStateMgmtSettings)
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
            PrimaryKeyStateMgmtSettings = primaryKeyStateMgmtSettings;
        }

        public EventType InternalEventType { get; set; }

        public EventType PublicEventType { get; set; }

        public string TableName { get; set; }

        public string OptionalContextName { get; set; }

        public NameAccessModifier? OptionalContextVisibility { get; set; }

        public string OptionalContextModule { get; set; }

        public Type[] KeyTypes { get; set; }

        public int NumMethodAggs { get; set; }

        public string[] KeyColumns { get; set; }

        public EventTableIndexMetadata IndexMetadata { get; set; } = new EventTableIndexMetadata();

        public bool IsKeyed => KeyTypes != null && KeyTypes.Length > 0;

        public int[] KeyColNums { get; set; }

        public IndexMultiKey KeyIndexMultiKey { get; set; }

        public string TableModuleName { get; set;  }

        public NameAccessModifier TableVisibility { get; set; }

        public IDictionary<string, TableMetadataColumn> Columns { get; set; }

        public StateMgmtSetting PrimaryKeyStateMgmtSettings { get; set; }
        
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
                PrimaryKeyStateMgmtSettings);
        }
        
        public void Init()
        {
            // add index multi-key for implicit primary-key index
            if (KeyColumns == null || KeyColumns.Length == 0)
            {
                return;
            }

            var props = new IndexedPropDesc[KeyColumns.Length];
            for (var i = 0; i < props.Length; i++)
            {
                props[i] = new IndexedPropDesc(KeyColumns[i], KeyTypes[i]);
            }

            KeyIndexMultiKey = new IndexMultiKey(true, props, new IndexedPropDesc[0], null);
            try
            {
                IndexMetadata.AddIndexExplicit(true, KeyIndexMultiKey, TableName, TableModuleName, null, "");
            }
            catch (ExprValidationException e)
            {
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
                .DeclareVarNewInstance<TableMetaData>("meta")
                .SetProperty(Ref("meta"), "TableName", Constant(TableName))
                .SetProperty(Ref("meta"), "TableModuleName", Constant(TableModuleName))
                .SetProperty(Ref("meta"), "TableVisibility", Constant(TableVisibility))
                .SetProperty(Ref("meta"), "OptionalContextName", Constant(OptionalContextName))
                .SetProperty(Ref("meta"), "OptionalContextVisibility", Constant(OptionalContextVisibility))
                .SetProperty(Ref("meta"), "OptionalContextModule", Constant(OptionalContextModule))
                .SetProperty(
                    Ref("meta"),
                    "InternalEventType",
                    EventTypeUtility.ResolveTypeCodegen(InternalEventType, symbols.GetAddInitSvc(method)))
                .SetProperty(
                    Ref("meta"),
                    "PublicEventType",
                    EventTypeUtility.ResolveTypeCodegen(PublicEventType, symbols.GetAddInitSvc(method)))
                .SetProperty(Ref("meta"), "KeyColumns", Constant(KeyColumns))
                .SetProperty(Ref("meta"), "KeyTypes", Constant(KeyTypes))
                .SetProperty(Ref("meta"), "KeyColNums", Constant(KeyColNums))
                .SetProperty(
                    Ref("meta"),
                    "Columns",
                    TableMetadataColumn.MakeColumns(Columns, method, symbols, classScope))
                .SetProperty(Ref("meta"), "NumMethodAggs", Constant(NumMethodAggs))
                .SetProperty(Ref("meta"), "PrimaryKeyStateMgmtSettings", PrimaryKeyStateMgmtSettings.ToExpression())
                .ExprDotMethod(Ref("meta"), "Init")
                .MethodReturn(Ref("meta"));
            return LocalMethod(method);
        }

        public CodegenExpression Make(CodegenExpressionRef addInitSvc)
        {
            return NewInstance<TableMetaData>(
                Constant(TableName),
                Constant(OptionalContextName),
                Constant(OptionalContextVisibility),
                Constant(OptionalContextModule),
                EventTypeUtility.ResolveTypeCodegen(InternalEventType, addInitSvc),
                EventTypeUtility.ResolveTypeCodegen(PublicEventType, addInitSvc));
        }

        public ISet<string> UniquenessAsSet
        {
            get {
                if (KeyColumns == null || KeyColumns.Length == 0)
                {
                    return Collections.GetEmptySet<string>();
                }

                return new HashSet<string>(KeyColumns);
            }
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