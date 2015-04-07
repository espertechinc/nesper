///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.context.factory;
using com.espertech.esper.core.context.mgr;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.agg.access;
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.annotation;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.baseagg;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.rettype;
using com.espertech.esper.epl.spec;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.epl.variable;
using com.espertech.esper.events;
using com.espertech.esper.events.arr;
using com.espertech.esper.util;
using com.espertech.esper.view;

namespace com.espertech.esper.core.start
{
    /// <summary>
    /// Starts and provides the stop method for EPL statements.
    /// </summary>
    public class EPStatementStartMethodCreateTable : EPStatementStartMethodBase
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    
        public EPStatementStartMethodCreateTable(StatementSpecCompiled statementSpec)
            : base(statementSpec)
        {
        }
    
        public override EPStatementStartResult StartInternal(EPServicesContext services, StatementContext statementContext, bool isNewStatement, bool isRecoveringStatement, bool isRecoveringResilient)
        {
            CreateTableDesc createDesc = StatementSpec.CreateTableDesc;
    
            // determine whether already declared
            VariableServiceUtil.CheckAlreadyDeclaredVariable(createDesc.TableName, services.VariableService);
            if (isNewStatement) {
                VariableServiceUtil.CheckAlreadyDeclaredTable(createDesc.TableName, services.TableService);
            }
            if (services.EventAdapterService.GetEventTypeByName(createDesc.TableName) != null) {
                throw new ExprValidationException("An event type or schema by name '" + createDesc.TableName + "' already exists");
            }
    
            EPStatementDestroyMethod destroyMethod = new EPStatementDestroyMethod(() => 
            {
                try
                {
                    services.StatementVariableRefService.RemoveReferencesStatement(statementContext.StatementName);
                }
                catch (Exception ex) {
                    log.Error("Error removing table '" + createDesc.TableName + "': " + ex.Message);
                }
            });

            EPStatementStopMethod stopMethod = new EPStatementStopMethod(() => {});
    
            // Determine event type names
            var internalTypeName = "table_" + createDesc.TableName + "__internal";
            var publicTypeName = "table_" + createDesc.TableName + "__public";
    
            TableMetadata metadata;
            try {
                // determine key types
                var keyTypes = GetKeyTypes(createDesc.Columns, services.EngineImportService);
    
                // check column naming, interpret annotations
                var columnDescs = ValidateExpressions(createDesc.Columns, services, statementContext);
    
                // analyze and plan the state holders
                var plan = AnalyzePlanAggregations(createDesc.TableName, statementContext, columnDescs, services, internalTypeName, publicTypeName);
                var tableStateRowFactory = plan.StateRowFactory;
    
                // register new table
                var queryPlanLogging = services.ConfigSnapshot.EngineDefaults.LoggingConfig.IsEnableQueryPlan;
                metadata = services.TableService.AddTable(createDesc.TableName, statementContext.Expression, statementContext.StatementName, keyTypes, plan.TableColumns, tableStateRowFactory, plan.NumberMethodAggregations, statementContext, plan.InternalEventType,
                        plan.PublicEventType, plan.EventToPublic, queryPlanLogging);
            }
            catch (ExprValidationException) {
                services.EventAdapterService.RemoveType(internalTypeName);
                services.EventAdapterService.RemoveType(publicTypeName);
                throw;
            }
    
            // allocate context factory
            var contextFactory = new StatementAgentInstanceFactoryCreateTable(metadata);
            Viewable outputView;
    
            if (StatementSpec.OptionalContextName != null) {
                var mergeView = new ContextMergeView(metadata.PublicEventType);
                outputView = mergeView;
                var statement = new ContextManagedStatementCreateAggregationVariableDesc(StatementSpec, statementContext, mergeView, contextFactory);
                services.ContextManagementService.AddStatement(StatementSpec.OptionalContextName, statement, isRecoveringResilient);
            }
            else {
                var defaultAgentInstanceContext = GetDefaultAgentInstanceContext(statementContext);
                var result = contextFactory.NewContext(defaultAgentInstanceContext, false);
                if (statementContext.ExtensionServicesContext != null && statementContext.ExtensionServicesContext.StmtResources != null) {
                    statementContext.ExtensionServicesContext.StmtResources.StartContextPartition(result, 0);
                }
                outputView = result.FinalView;
            }
    
            services.StatementVariableRefService.AddReferences(statementContext.StatementName, createDesc.TableName);
    
            return new EPStatementStartResult(outputView, stopMethod, destroyMethod);
        }
    
        private Type[] GetKeyTypes(IList<CreateTableColumn> columns, EngineImportService engineImportService)
                
        {
            IList<Type> keys = new List<Type>();
            foreach (var col in columns) {
                if (col.PrimaryKey == null || !col.PrimaryKey.Value) {
                    continue;
                }
                var msg = "Column '" + col.ColumnName + "' may not be tagged as primary key";
                if (col.OptExpression != null) {
                    throw new ExprValidationException(msg + ", an expression cannot become a primary key column");
                }
                if (col.OptTypeIsArray != null && col.OptTypeIsArray.Value) {
                    throw new ExprValidationException(msg + ", an array-typed column cannot become a primary key column");
                }
                object type = EventTypeUtility.BuildType(new ColumnDesc(col.ColumnName, col.OptTypeName, false, false), engineImportService);
                if (!(type is Type)) {
                    throw new ExprValidationException(msg + ", received unexpected event type '" + type + "'");
                }
                keys.Add((Type) type);
            }
            return keys.ToArray();
        }
    
        private ExprAggregateNode ValidateAggregationExpr(ExprNode columnExpressionType, EventType optionalProvidedType, EPServicesContext services, StatementContext statementContext)
                
        {
            // determine validation context types and istream/irstream
            EventType[] types;
            string[] streamNames;
            bool[] istreamOnly;
            if (optionalProvidedType != null) {
                types = new EventType[] {optionalProvidedType};
                streamNames = new string[] {types[0].Name};
                istreamOnly = new bool[] {false}; // always false (expected to be bound by data window), use "ever"-aggregation functions otherwise
            }
            else {
                types = new EventType[0];
                streamNames = new string[0];
                istreamOnly = new bool[0];
            }
    
            var streamTypeService = new StreamTypeServiceImpl(types, streamNames, istreamOnly, services.EngineURI, false);
            var validationContext = new ExprValidationContext(
                streamTypeService, statementContext.MethodResolutionService, null, statementContext.SchedulingService,
                statementContext.VariableService,
                statementContext.TableService,
                new ExprEvaluatorContextStatement(statementContext, false),
                statementContext.EventAdapterService,
                statementContext.StatementName,
                statementContext.StatementId, 
                statementContext.Annotations,
                statementContext.ContextDescriptor, 
                statementContext.ScriptingService,
                false, false, false, false, null, false);
    
            // substitute parameter nodes
            foreach (var childNode in columnExpressionType.ChildNodes) {
                if (childNode is ExprIdentNode) {
                    var identNode = (ExprIdentNode) childNode;
                    var propname = identNode.FullUnresolvedName.Trim();
                    Type clazz = TypeHelper.GetTypeForSimpleName(propname);
                    if (propname.ToLower().Trim().Equals("object")) {
                        clazz = typeof(object);
                    }
                    EngineImportException ex = null;
                    if (clazz == null) {
                        try {
                            clazz = services.EngineImportService.ResolveType(propname);
                        }
                        catch (EngineImportException e) {
                            ex = e;
                        }
                    }
                    if (clazz != null) {
                        var typeNode = new ExprTypedNoEvalNode(propname, clazz);
                        ExprNodeUtility.ReplaceChildNode(columnExpressionType, identNode, typeNode);
                    }
                    else {
                        if (optionalProvidedType == null) {
                            if (ex != null) {
                                throw new ExprValidationException("Failed to resolve type '" + propname + "': " + ex.Message, ex);
                            }
                            throw new ExprValidationException("Failed to resolve type '" + propname + "'");
                        }
                    }
                }
            }
    
            // validate
            var validated = ExprNodeUtility.GetValidatedSubtree(ExprNodeOrigin.CREATETABLECOLUMN, columnExpressionType, validationContext);
            if (!(validated is ExprAggregateNode)) {
                throw new ExprValidationException("Expression '" + ExprNodeUtility.ToExpressionStringMinPrecedenceSafe(validated) + "' is not an aggregation");
            }
    
            return (ExprAggregateNode) validated;
        }
    
        private IList<TableColumnDesc> ValidateExpressions(IList<CreateTableColumn> columns, EPServicesContext services, StatementContext statementContext)
                
        {
            ISet<string> columnNames = new HashSet<string>();
            IList<TableColumnDesc> descriptors = new List<TableColumnDesc>();
    
            var positionInDeclaration = 0;
            foreach (var column in columns) {
                var msgprefix = "For column '" + column.ColumnName + "'";
    
                // check duplicate name
                if (columnNames.Contains(column.ColumnName)) {
                    throw new ExprValidationException("Column '" + column.ColumnName + "' is listed more than once");
                }
                columnNames.Add(column.ColumnName);
    
                // determine presence of type annotation
                var optionalEventType = ValidateExpressionGetEventType(msgprefix, column.Annotations, services.EventAdapterService);
    
                // aggregation node
                TableColumnDesc descriptor;
                if (column.OptExpression != null) {
                    var validated = ValidateAggregationExpr(column.OptExpression, optionalEventType, services, statementContext);
                    descriptor = new TableColumnDescAgg(positionInDeclaration, column.ColumnName, validated, optionalEventType);
                }
                else
                {
                    object unresolvedType = EventTypeUtility.BuildType(
                            new ColumnDesc(
                                column.ColumnName,
                                column.OptTypeName, 
                                column.OptTypeIsArray ?? false,
                                column.OptTypeIsPrimitiveArray ?? false),
                            services.EngineImportService);
                    descriptor = new TableColumnDescTyped(positionInDeclaration, column.ColumnName, unresolvedType, column.PrimaryKey ?? false);
                }
                descriptors.Add(descriptor);
                positionInDeclaration++;
            }
    
            return descriptors;
        }
    
        private static EventType ValidateExpressionGetEventType(string msgprefix, IList<AnnotationDesc> annotations, EventAdapterService eventAdapterService)
                
        {
            IDictionary<String, IList<AnnotationDesc>> annos = AnnotationUtil.MapByNameLowerCase(annotations);
    
            // check annotations used
            IList<AnnotationDesc> typeAnnos = annos.Pluck("type");
            if (!annos.IsEmpty()) {
                throw new ExprValidationException(msgprefix + " unrecognized annotation '" + annos.Keys.First() + "'");
            }
    
            // type determination
            EventType optionalType = null;
            if (typeAnnos != null) {
                string typeName = AnnotationUtil.GetExpectSingleStringValue(msgprefix, typeAnnos);
                optionalType = eventAdapterService.GetEventTypeByName(typeName);
                if (optionalType == null) {
                    throw new ExprValidationException(msgprefix + " failed to find event type '" + typeName + "'");
                }
            }
    
            return optionalType;
        }
    
        private TableAccessAnalysisResult AnalyzePlanAggregations(string tableName, StatementContext statementContext, IList<TableColumnDesc> columns, EPServicesContext services, string internalTypeName, string publicTypeName)
        {
            // once upfront: obtains aggregation factories for each aggregation
            // we do this once as a factory may be a heavier object
            IDictionary<TableColumnDesc, AggregationMethodFactory> aggregationFactories = new Dictionary<TableColumnDesc, AggregationMethodFactory>();
            foreach (var column in columns) {
                if (column is TableColumnDescAgg) {
                    var agg = (TableColumnDescAgg) column;
                    AggregationMethodFactory factory = agg.Aggregation.Factory;
                    aggregationFactories.Put(column, factory);
                }
            }
    
            // sort into these categories:
            // plain / method-agg / access-agg
            // compile all-column public types
            IList<TableColumnDescTyped> plainColumns = new List<TableColumnDescTyped>();
            IList<TableColumnDescAgg> methodAggColumns = new List<TableColumnDescAgg>();
            IList<TableColumnDescAgg> accessAggColumns = new List<TableColumnDescAgg>();
            IDictionary<String, object> allColumnsPublicTypes = new LinkedHashMap<string, object>();
            foreach (var column in columns) {
    
                // handle plain types
                if (column is TableColumnDescTyped) {
                    var typed = (TableColumnDescTyped) column;
                    plainColumns.Add(typed);
                    allColumnsPublicTypes.Put(column.ColumnName, typed.UnresolvedType);
                    continue;
                }
    
                // handle aggs
                var agg = (TableColumnDescAgg) column;
                var aggFactory = aggregationFactories.Get(agg);
                if (aggFactory.IsAccessAggregation) {
                    accessAggColumns.Add(agg);
                }
                else {
                    methodAggColumns.Add(agg);
                }
                allColumnsPublicTypes.Put(column.ColumnName, agg.Aggregation.ReturnType);
            }
    
            // determine column metadata
            //
            IDictionary<String, TableMetadataColumn> columnMetadata = new LinkedHashMap<string, TableMetadataColumn>();
    
            // handle typed columns
            IDictionary<String, object> allColumnsInternalTypes = new LinkedHashMap<string, object>();
            allColumnsInternalTypes.Put(TableServiceConstants.INTERNAL_RESERVED_PROPERTY, typeof(object));
            var indexPlain = 1;
            var groupKeyIndexes = new List<int>();
            var assignPairsPlain = new TableMetadataColumnPairPlainCol[plainColumns.Count];
            foreach (var typedColumn in plainColumns) {
                allColumnsInternalTypes.Put(typedColumn.ColumnName, typedColumn.UnresolvedType);
                columnMetadata.Put(typedColumn.ColumnName, new TableMetadataColumnPlain(typedColumn.ColumnName, typedColumn.IsKey, indexPlain));
                if (typedColumn.IsKey) {
                    groupKeyIndexes.Add(indexPlain);
                }
                assignPairsPlain[indexPlain-1] = new TableMetadataColumnPairPlainCol(typedColumn.PositionInDeclaration, indexPlain);
                indexPlain++;
            }
    
            // determine internally-used event type
            // for use by indexes and lookups
            ObjectArrayEventType internalEventType;
            ObjectArrayEventType publicEventType;
            try {
                internalEventType = (ObjectArrayEventType) services.EventAdapterService.AddNestableObjectArrayType(
                    internalTypeName, allColumnsInternalTypes, null, false, false, false, false, false, true, tableName);
                publicEventType = (ObjectArrayEventType) services.EventAdapterService.AddNestableObjectArrayType(
                    publicTypeName, allColumnsPublicTypes, null, false, false, false, false, false, false, null);
            }
            catch (EPException ex) {
                throw new ExprValidationException("Invalid type information: " + ex.Message, ex);
            }
            services.StatementEventTypeRefService.AddReferences(statementContext.StatementName, new string[] {internalTypeName, publicTypeName});
    
            // handle aggregation-methods single-func first.
            var methodFactories = new AggregationMethodFactory[methodAggColumns.Count];
            var index = 0;
            var assignPairsMethod = new TableMetadataColumnPairAggMethod[methodAggColumns.Count];
            foreach (var column in methodAggColumns) {
                var factory = aggregationFactories.Get(column);
                EPType optionalEnumerationType = EPTypeHelper.OptionalFromEnumerationExpr(statementContext.StatementId, statementContext.EventAdapterService, column.Aggregation);
                methodFactories[index] = factory;
                columnMetadata.Put(column.ColumnName, new TableMetadataColumnAggregation(column.ColumnName, factory, index, null, optionalEnumerationType, column.OptionalAssociatedType));
                assignPairsMethod[index] = new TableMetadataColumnPairAggMethod(column.PositionInDeclaration);
                index++;
            }
    
            // handle access-aggregation (sharable, multi-value) aggregations
            var stateFactories = new AggregationStateFactory[accessAggColumns.Count];
            var assignPairsAccess = new TableMetadataColumnPairAggAccess[accessAggColumns.Count];
            index = 0;
            foreach (var column in accessAggColumns) {
                var factory = aggregationFactories.Get(column);
                stateFactories[index] = factory.GetAggregationStateFactory(false);
                var pair = new AggregationAccessorSlotPair(index, factory.Accessor);
                EPType optionalEnumerationType = EPTypeHelper.OptionalFromEnumerationExpr(statementContext.StatementId, statementContext.EventAdapterService, column.Aggregation);
                columnMetadata.Put(column.ColumnName, new TableMetadataColumnAggregation(column.ColumnName, factory, -1, pair, optionalEnumerationType, column.OptionalAssociatedType));
                assignPairsAccess[index] = new TableMetadataColumnPairAggAccess(column.PositionInDeclaration, factory.Accessor);
                index++;
            }
    
            // create state factory
            var groupKeyIndexesArr = CollectionUtil.IntArray(groupKeyIndexes);
            var stateRowFactory = new TableStateRowFactory(internalEventType, statementContext.MethodResolutionService, methodFactories, stateFactories, groupKeyIndexesArr, services.EventAdapterService);
    
            // create public event provision
            var eventToPublic = new TableMetadataInternalEventToPublic(publicEventType,
                    assignPairsPlain, assignPairsMethod, assignPairsAccess, services.EventAdapterService);
    
            return new TableAccessAnalysisResult(stateRowFactory, columnMetadata, methodAggColumns.Count, internalEventType, publicEventType, eventToPublic);
        }
    
        internal class TableAccessAnalysisResult
        {
            internal TableAccessAnalysisResult(TableStateRowFactory stateRowFactory, IDictionary<String, TableMetadataColumn> tableColumns, int numberMethodAggregations, ObjectArrayEventType internalEventType, ObjectArrayEventType publicEventType, TableMetadataInternalEventToPublic eventToPublic)
            {
                StateRowFactory = stateRowFactory;
                TableColumns = tableColumns;
                NumberMethodAggregations = numberMethodAggregations;
                InternalEventType = internalEventType;
                PublicEventType = publicEventType;
                EventToPublic = eventToPublic;
            }

            public TableStateRowFactory StateRowFactory { get; private set; }

            public IDictionary<string, TableMetadataColumn> TableColumns { get; private set; }

            public int NumberMethodAggregations { get; private set; }

            public ObjectArrayEventType InternalEventType { get; private set; }

            public ObjectArrayEventType PublicEventType { get; private set; }

            public TableMetadataInternalEventToPublic EventToPublic { get; private set; }
        }
    }
}
