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

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.soda;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.context.compile;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.agg.access.linear;
using com.espertech.esper.common.@internal.epl.enummethod.dot;
using com.espertech.esper.common.@internal.epl.expression.agg.accessagg;
using com.espertech.esper.common.@internal.epl.expression.agg.method;
using com.espertech.esper.common.@internal.epl.expression.chain;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.declared.compiletime;
using com.espertech.esper.common.@internal.epl.expression.dot.core;
using com.espertech.esper.common.@internal.epl.expression.dot.walk;
using com.espertech.esper.common.@internal.epl.expression.funcs;
using com.espertech.esper.common.@internal.epl.expression.ops;
using com.espertech.esper.common.@internal.epl.expression.prev;
using com.espertech.esper.common.@internal.epl.expression.prior;
using com.espertech.esper.common.@internal.epl.expression.subquery;
using com.espertech.esper.common.@internal.epl.expression.table;
using com.espertech.esper.common.@internal.epl.expression.time.node;
using com.espertech.esper.common.@internal.epl.expression.variable;
using com.espertech.esper.common.@internal.epl.pattern.and;
using com.espertech.esper.common.@internal.epl.pattern.core;
using com.espertech.esper.common.@internal.epl.pattern.every;
using com.espertech.esper.common.@internal.epl.pattern.everydistinct;
using com.espertech.esper.common.@internal.epl.pattern.filter;
using com.espertech.esper.common.@internal.epl.pattern.followedby;
using com.espertech.esper.common.@internal.epl.pattern.guard;
using com.espertech.esper.common.@internal.epl.pattern.matchuntil;
using com.espertech.esper.common.@internal.epl.pattern.not;
using com.espertech.esper.common.@internal.epl.pattern.observer;
using com.espertech.esper.common.@internal.epl.pattern.or;
using com.espertech.esper.common.@internal.epl.rowrecog.core;
using com.espertech.esper.common.@internal.epl.rowrecog.expr;
using com.espertech.esper.common.@internal.epl.script.core;
using com.espertech.esper.common.@internal.epl.table.compiletime;
using com.espertech.esper.common.@internal.epl.variable.core;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.common.@internal.type;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.epl.historical.database.core.
    HistoricalEventViewableDatabaseForgeFactory;

using CreateTableColumn = com.espertech.esper.common.client.soda.CreateTableColumn;

namespace com.espertech.esper.common.@internal.compile.stage1.specmapper
{
    /// <summary>
    /// Helper for mapping internal representations of a statement to the SODA object model for statements.
    /// </summary>
    public class StatementSpecMapper
    {
        /// <summary>
        /// Maps the SODA-selector to the internal representation
        /// </summary>
        /// <param name="selector">is the SODA-selector to map</param>
        /// <returns>internal stream selector</returns>
        public static SelectClauseStreamSelectorEnum MapFromSODA(StreamSelector selector)
        {
            if (selector == StreamSelector.ISTREAM_ONLY) {
                return SelectClauseStreamSelectorEnum.ISTREAM_ONLY;
            }
            else if (selector == StreamSelector.RSTREAM_ONLY) {
                return SelectClauseStreamSelectorEnum.RSTREAM_ONLY;
            }
            else if (selector == StreamSelector.RSTREAM_ISTREAM_BOTH) {
                return SelectClauseStreamSelectorEnum.RSTREAM_ISTREAM_BOTH;
            }
            else {
                throw new ArgumentException("Invalid selector '" + selector + "' encountered");
            }
        }

        /// <summary>
        /// Maps the internal stream selector to the SODA-representation
        /// </summary>
        /// <param name="selector">is the internal selector to map</param>
        /// <returns>SODA stream selector</returns>
        public static StreamSelector MapFromSODA(SelectClauseStreamSelectorEnum selector)
        {
            if (selector == SelectClauseStreamSelectorEnum.ISTREAM_ONLY) {
                return StreamSelector.ISTREAM_ONLY;
            }
            else if (selector == SelectClauseStreamSelectorEnum.RSTREAM_ONLY) {
                return StreamSelector.RSTREAM_ONLY;
            }
            else if (selector == SelectClauseStreamSelectorEnum.RSTREAM_ISTREAM_BOTH) {
                return StreamSelector.RSTREAM_ISTREAM_BOTH;
            }
            else {
                throw new ArgumentException("Invalid selector '" + selector + "' encountered");
            }
        }

        /// <summary>
        /// Unmap expresission.
        /// </summary>
        /// <param name="expression">to unmap</param>
        /// <returns>expression</returns>
        public static Expression Unmap(ExprNode expression)
        {
            return UnmapExpressionDeep(expression, new StatementSpecUnMapContext());
        }

        public static StatementSpecRaw Map(
            EPStatementObjectModel sodaStatement,
            StatementSpecMapEnv mapEnv)
        {
            ContextCompileTimeDescriptor contextCompileTimeDescriptor = null;
            if (sodaStatement.ContextName != null) {
                var contextDetail =
                    mapEnv.ContextCompileTimeResolver.GetContextInfo(sodaStatement.ContextName);
                if (contextDetail != null) {
                    contextCompileTimeDescriptor = new ContextCompileTimeDescriptor(
                        sodaStatement.ContextName,
                        contextDetail.ContextModuleName,
                        contextDetail.ContextVisibility,
                        new ContextPropertyRegistry(contextDetail),
                        contextDetail.ValidationInfos);
                }
            }

            var mapContext = new StatementSpecMapContext(contextCompileTimeDescriptor, mapEnv);

            var raw = Map(sodaStatement, mapContext);
            raw.HasPriorExpressions = mapContext.HasPriorExpression;
            raw.ReferencedVariables = mapContext.VariableNames;
            raw.TableExpressions = mapContext.TableExpressions;
            raw.SubstitutionParameters = mapContext.SubstitutionNodes;
            return raw;
        }

        private static StatementSpecRaw Map(
            EPStatementObjectModel sodaStatement,
            StatementSpecMapContext mapContext)
        {
            var defaultStreamSelector =
                MapFromSODA(
                    mapContext.Configuration.Compiler.StreamSelection.DefaultStreamSelector);
            var raw = new StatementSpecRaw(defaultStreamSelector);

            var annotations = MapAnnotations(sodaStatement.Annotations);
            raw.Annotations = annotations;
            MapFireAndForget(sodaStatement.FireAndForgetClause, raw, mapContext);
            MapExpressionDeclaration(sodaStatement.ExpressionDeclarations, raw, mapContext);
            MapScriptExpressions(sodaStatement.ScriptExpressions, raw, mapContext);
            MapClassProvidedExpressions(sodaStatement.ClassProvidedExpressions, raw, mapContext);
            MapContextName(sodaStatement.ContextName, raw, mapContext);
            MapUpdateClause(sodaStatement.UpdateClause, raw, mapContext);
            MapCreateContext(sodaStatement.CreateContext, raw, mapContext);
            MapCreateWindow(sodaStatement.CreateWindow, raw, mapContext);
            MapCreateIndex(sodaStatement.CreateIndex, raw, mapContext);
            MapCreateVariable(sodaStatement.CreateVariable, raw, mapContext);
            MapCreateTable(sodaStatement.CreateTable, raw, mapContext);
            MapCreateSchema(sodaStatement.CreateSchema, raw, mapContext);
            MapCreateExpression(sodaStatement.CreateExpression, raw, mapContext);
            MapCreateClass(sodaStatement.CreateClass, raw, mapContext);
            MapCreateGraph(sodaStatement.CreateDataFlow, raw, mapContext);
            MapOnTrigger(sodaStatement.OnExpr, raw, mapContext);
            var desc = MapInsertInto(sodaStatement.InsertInto, mapContext);
            raw.InsertIntoDesc = desc;
            MapSelect(sodaStatement.SelectClause, raw, mapContext);
            MapFrom(sodaStatement.FromClause, raw, mapContext);
            MapWhere(sodaStatement.WhereClause, raw, mapContext);
            MapGroupBy(sodaStatement.GroupByClause, raw, mapContext);
            MapHaving(sodaStatement.HavingClause, raw, mapContext);
            MapOutputLimit(sodaStatement.OutputLimitClause, raw, mapContext);
            MapOrderBy(sodaStatement.OrderByClause, raw, mapContext);
            MapRowLimit(sodaStatement.RowLimitClause, raw, mapContext);
            MapMatchRecognize(sodaStatement.MatchRecognizeClause, raw, mapContext);
            MapForClause(sodaStatement.ForClause, raw, mapContext);
            MapSQLParameters(sodaStatement.FromClause, raw, mapContext);
            MapIntoVariableClause(sodaStatement.IntoTableClause, raw);
            return raw;
        }

        private static void MapIntoVariableClause(
            IntoTableClause intoClause,
            StatementSpecRaw raw)
        {
            if (intoClause != null) {
                raw.IntoTableSpec = new IntoTableSpec(intoClause.TableName);
            }
        }

        private static void MapFireAndForget(
            FireAndForgetClause fireAndForgetClause,
            StatementSpecRaw raw,
            StatementSpecMapContext mapContext)
        {
            if (fireAndForgetClause is FireAndForgetDelete) {
                raw.FireAndForgetSpec = new FireAndForgetSpecDelete();
            }
            else if (fireAndForgetClause is FireAndForgetInsert insert) {
                IList<IList<ExprNode>> rows = new List<IList<ExprNode>>(insert.Rows.Count);
                foreach (var row in insert.Rows) {
                    IList<ExprNode> nodes = new List<ExprNode>(row.Count);
                    foreach (var expr in row) {
                        nodes.Add(MapExpressionDeep(expr, mapContext));
                    }

                    rows.Add(nodes);
                }

                raw.FireAndForgetSpec = new FireAndForgetSpecInsert(insert.IsUseValuesKeyword, rows);
            }
            else if (fireAndForgetClause is FireAndForgetUpdate upd) {
                IList<OnTriggerSetAssignment> assignments = new List<OnTriggerSetAssignment>();
                foreach (var pair in upd.Assignments) {
                    var expr = MapExpressionDeep(pair.Value, mapContext);
                    assignments.Add(new OnTriggerSetAssignment(expr));
                }

                var updspec = new FireAndForgetSpecUpdate(assignments);
                raw.FireAndForgetSpec = updspec;
            }
            else {
                if (fireAndForgetClause == null) {
                    return;
                }

                throw new IllegalStateException("Unrecognized fire-and-forget clause " + fireAndForgetClause);
            }
        }

        /// <summary>
        /// Maps the internal representation of a statement to the SODA object model.
        /// </summary>
        /// <param name="statementSpec">is the internal representation</param>
        /// <returns>object model of statement</returns>
        public static StatementSpecUnMapResult Unmap(StatementSpecRaw statementSpec)
        {
            var unmapContext = new StatementSpecUnMapContext();
            var model = UnmapInternal(statementSpec, unmapContext);
            return new StatementSpecUnMapResult(model);
        }

        private static EPStatementObjectModel UnmapInternal(
            StatementSpecRaw statementSpec,
            StatementSpecUnMapContext unmapContext)
        {
            var model = new EPStatementObjectModel();
            model.Annotations = UnmapAnnotations(statementSpec.Annotations);
            UnmapFireAndForget(statementSpec.FireAndForgetSpec, model, unmapContext);
            model.ExpressionDeclarations = UnmapExpressionDeclarations(statementSpec.ExpressionDeclDesc, unmapContext);
            model.ScriptExpressions = UnmapScriptExpressions(statementSpec.ScriptExpressions, unmapContext);
            model.ClassProvidedExpressions = UnmapClassProvideds(statementSpec.ClassProvidedList, unmapContext);
            UnmapContextName(statementSpec.OptionalContextName, model);
            UnmapCreateContext(statementSpec.CreateContextDesc, model, unmapContext);
            UnmapCreateWindow(statementSpec.CreateWindowDesc, model, unmapContext);
            UnmapCreateIndex(statementSpec.CreateIndexDesc, model, unmapContext);
            UnmapCreateVariable(statementSpec.CreateVariableDesc, model, unmapContext);
            UnmapCreateTable(statementSpec.CreateTableDesc, model, unmapContext);
            UnmapCreateSchema(statementSpec.CreateSchemaDesc, model);
            UnmapCreateExpression(statementSpec.CreateExpressionDesc, model, unmapContext);
            UnmapCreateClass(statementSpec.CreateClassProvided, model);
            UnmapCreateGraph(statementSpec.CreateDataFlowDesc, model, unmapContext);
            UnmapUpdateClause(statementSpec.StreamSpecs, statementSpec.UpdateDesc, model, unmapContext);
            UnmapOnClause(statementSpec.OnTriggerDesc, model, unmapContext);
            var insertIntoClause = UnmapInsertInto(statementSpec.InsertIntoDesc, unmapContext);
            model.InsertInto = insertIntoClause;
            var selectClause = UnmapSelect(
                statementSpec.SelectClauseSpec,
                statementSpec.SelectStreamSelectorEnum,
                unmapContext);
            model.SelectClause = selectClause;
            UnmapFrom(statementSpec.StreamSpecs, statementSpec.OuterJoinDescList, model, unmapContext);
            UnmapWhere(statementSpec.WhereClause, model, unmapContext);
            UnmapGroupBy(statementSpec.GroupByExpressions, model, unmapContext);
            UnmapHaving(statementSpec.HavingClause, model, unmapContext);
            UnmapOutputLimit(statementSpec.OutputLimitSpec, model, unmapContext);
            UnmapOrderBy(statementSpec.OrderByList, model, unmapContext);
            UnmapRowLimit(statementSpec.RowLimitSpec, model);
            UnmapMatchRecognize(statementSpec.MatchRecognizeSpec, model, unmapContext);
            UnmapForClause(statementSpec.ForClauseSpec, model, unmapContext);
            UnmapSQLParameters(statementSpec.SqlParameters, unmapContext);
            UnmapIntoVariableClause(statementSpec.IntoTableSpec, model);
            return model;
        }

        private static void UnmapIntoVariableClause(
            IntoTableSpec intoTableSpec,
            EPStatementObjectModel model)
        {
            if (intoTableSpec == null) {
                return;
            }

            model.IntoTableClause = new IntoTableClause(intoTableSpec.Name);
        }

        private static void UnmapCreateTable(
            CreateTableDesc desc,
            EPStatementObjectModel model,
            StatementSpecUnMapContext unmapContext)
        {
            if (desc == null) {
                return;
            }

            var clause = new CreateTableClause(desc.TableName);
            IList<CreateTableColumn> cols = new List<CreateTableColumn>();
            foreach (var col in desc.Columns) {
                var optExpr = col.OptExpression != null
                    ? UnmapExpressionDeep(col.OptExpression, unmapContext)
                    : null;
                var annots = UnmapAnnotations(col.Annotations);
                var optType = col.OptType == null ? null : col.OptType.ToEPL();
                var coldesc =
                    new CreateTableColumn(
                        col.ColumnName,
                        optExpr,
                        optType,
                        annots,
                        col.PrimaryKey);
                cols.Add(coldesc);
            }

            clause.Columns = cols;
            model.CreateTable = clause;
        }

        private static void UnmapFireAndForget(
            FireAndForgetSpec fireAndForgetSpec,
            EPStatementObjectModel model,
            StatementSpecUnMapContext unmapContext)
        {
            if (fireAndForgetSpec is FireAndForgetSpecDelete) {
                model.FireAndForgetClause = new FireAndForgetDelete();
            }
            else if (fireAndForgetSpec is FireAndForgetSpecInsert insert) {
                IList<IList<Expression>> rows = new List<IList<Expression>>(insert.Multirow.Count);
                foreach (var exprRow in insert.Multirow) {
                    IList<Expression> row = new List<Expression>(exprRow.Count);
                    foreach (var expr in exprRow) {
                        row.Add(UnmapExpressionDeep(expr, unmapContext));
                    }

                    rows.Add(row);
                }

                var spec = new FireAndForgetInsert(insert.IsUseValuesKeyword);
                spec.Rows = rows;
                model.FireAndForgetClause = spec;
            }
            else if (fireAndForgetSpec is FireAndForgetSpecUpdate upd) {
                var faf = new FireAndForgetUpdate();
                foreach (var assignment in upd.Assignments) {
                    var expr = UnmapExpressionDeep(assignment.Expression, unmapContext);
                    faf.AddAssignment(new Assignment(expr));
                }

                model.FireAndForgetClause = faf;
            }
            else {
                if (fireAndForgetSpec != null) {
                    throw new IllegalStateException("Unrecognized type of fire-and-forget: " + fireAndForgetSpec);
                }
            }
        }

        // Collect substitution parameters
        private static void UnmapSQLParameters(
            IDictionary<int, IList<ExprNode>> sqlParameters,
            StatementSpecUnMapContext unmapContext)
        {
            if (sqlParameters == null) {
                return;
            }

            foreach (var pair in sqlParameters) {
                foreach (var node in pair.Value) {
                    UnmapExpressionDeep(node, unmapContext);
                }
            }
        }

        private static void UnmapOnClause(
            OnTriggerDesc onTriggerDesc,
            EPStatementObjectModel model,
            StatementSpecUnMapContext unmapContext)
        {
            if (onTriggerDesc == null) {
                return;
            }

            if (onTriggerDesc.OnTriggerType == OnTriggerType.ON_DELETE) {
                var window = (OnTriggerWindowDesc)onTriggerDesc;
                model.OnExpr = new OnDeleteClause(window.WindowName, window.OptionalAsName);
            }
            else if (onTriggerDesc.OnTriggerType == OnTriggerType.ON_UPDATE) {
                var window = (OnTriggerWindowUpdateDesc)onTriggerDesc;
                var clause = new OnUpdateClause(window.WindowName, window.OptionalAsName);
                foreach (var assignment in window.Assignments) {
                    var expr = UnmapExpressionDeep(assignment.Expression, unmapContext);
                    clause.AddAssignment(expr);
                }

                model.OnExpr = clause;
            }
            else if (onTriggerDesc.OnTriggerType == OnTriggerType.ON_SELECT) {
                var window = (OnTriggerWindowDesc)onTriggerDesc;
                var onSelect = new OnSelectClause(window.WindowName, window.OptionalAsName);
                onSelect.IsDeleteAndSelect = window.IsDeleteAndSelect;
                model.OnExpr = onSelect;
            }
            else if (onTriggerDesc.OnTriggerType == OnTriggerType.ON_SET) {
                var trigger = (OnTriggerSetDesc)onTriggerDesc;
                var clause = new OnSetClause();
                foreach (var assignment in trigger.Assignments) {
                    var expr = UnmapExpressionDeep(assignment.Expression, unmapContext);
                    clause.AddAssignment(expr);
                }

                model.OnExpr = clause;
            }
            else if (onTriggerDesc.OnTriggerType == OnTriggerType.ON_SPLITSTREAM) {
                var trigger = (OnTriggerSplitStreamDesc)onTriggerDesc;
                var clause = OnInsertSplitStreamClause.Create();
                foreach (var stream in trigger.SplitStreams) {
                    Expression whereClause = null;
                    if (stream.WhereClause != null) {
                        whereClause = UnmapExpressionDeep(stream.WhereClause, unmapContext);
                    }

                    IList<ContainedEventSelect> propertySelects = null;
                    string propertySelectStreamName = null;
                    if (stream.FromClause != null) {
                        propertySelects = UnmapPropertySelects(stream.FromClause.PropertyEvalSpec, unmapContext);
                        propertySelectStreamName = stream.FromClause.OptionalStreamName;
                    }

                    var insertIntoClause = UnmapInsertInto(stream.InsertInto, unmapContext);
                    var selectClause = UnmapSelect(
                        stream.SelectClause,
                        SelectClauseStreamSelectorEnum.ISTREAM_ONLY,
                        unmapContext);
                    clause.AddItem(
                        OnInsertSplitStreamItem.Create(
                            insertIntoClause,
                            selectClause,
                            propertySelects,
                            propertySelectStreamName,
                            whereClause));
                }

                model.OnExpr = clause;
                clause.IsFirst = trigger.IsFirst;
            }
            else if (onTriggerDesc.OnTriggerType == OnTriggerType.ON_MERGE) {
                var trigger = (OnTriggerMergeDesc)onTriggerDesc;
                IList<OnMergeMatchItem> matchItems = new List<OnMergeMatchItem>();
                foreach (var matched in trigger.Items) {
                    IList<OnMergeMatchedAction> actions = new List<OnMergeMatchedAction>();
                    var matchCond = matched.OptionalMatchCond != null
                        ? UnmapExpressionDeep(matched.OptionalMatchCond, unmapContext)
                        : null;
                    var matchItem = new OnMergeMatchItem(matched.IsMatchedUnmatched, matchCond, actions);
                    foreach (var actionitem in matched.Actions) {
                        OnMergeMatchedAction action;
                        if (actionitem is OnTriggerMergeActionDelete delete) {
                            var optionalCondition = delete.OptionalWhereClause == null
                                ? null
                                : UnmapExpressionDeep(delete.OptionalWhereClause, unmapContext);
                            action = new OnMergeMatchedDeleteAction(optionalCondition);
                        }
                        else if (actionitem is OnTriggerMergeActionUpdate merge) {
                            IList<Assignment> assignments = new List<Assignment>();
                            foreach (var pair in merge.Assignments) {
                                var expr = UnmapExpressionDeep(pair.Expression, unmapContext);
                                assignments.Add(new Assignment(expr));
                            }

                            var optionalCondition = merge.OptionalWhereClause == null
                                ? null
                                : UnmapExpressionDeep(merge.OptionalWhereClause, unmapContext);
                            action = new OnMergeMatchedUpdateAction(assignments, optionalCondition);
                        }
                        else if (actionitem is OnTriggerMergeActionInsert insert) {
                            action = UnmapMergeInsert(insert, unmapContext);
                        }
                        else {
                            throw new ArgumentException(
                                "Unrecognized merged action type '" + actionitem.GetType() + "'");
                        }

                        actions.Add(action);
                    }

                    matchItems.Add(matchItem);
                }

                var onMerge = OnMergeClause.Create(trigger.WindowName, trigger.OptionalAsName, matchItems);
                if (trigger.OptionalInsertNoMatch != null) {
                    onMerge.InsertNoMatch = UnmapMergeInsert(trigger.OptionalInsertNoMatch, unmapContext);
                }

                model.OnExpr = onMerge;
            }
            else {
                throw new ArgumentException("Type of on-clause not handled: " + onTriggerDesc.OnTriggerType);
            }
        }

        private static OnMergeMatchedInsertAction UnmapMergeInsert(
            OnTriggerMergeActionInsert insert,
            StatementSpecUnMapContext unmapContext)
        {
            IList<string> columnNames = new List<string>(insert.Columns);
            var select = UnmapSelectClauseElements(insert.SelectClause, unmapContext);
            var optionalCondition = insert.OptionalWhereClause == null
                ? null
                : UnmapExpressionDeep(insert.OptionalWhereClause, unmapContext);
            var optionalPrecedence = insert.EventPrecedence == null
                ? null
                : UnmapExpressionDeep(insert.EventPrecedence, unmapContext);
            return new OnMergeMatchedInsertAction(
                columnNames,
                optionalPrecedence,
                select,
                optionalCondition,
                insert.OptionalStreamName);
        }

        private static void UnmapUpdateClause(
            IList<StreamSpecRaw> desc,
            UpdateDesc updateDesc,
            EPStatementObjectModel model,
            StatementSpecUnMapContext unmapContext)
        {
            if (updateDesc == null) {
                return;
            }

            var type = ((FilterStreamSpecRaw)desc[0]).RawFilterSpec.EventTypeName;
            var clause = new UpdateClause(type, updateDesc.OptionalStreamName);
            foreach (var assignment in updateDesc.Assignments) {
                var expr = UnmapExpressionDeep(assignment.Expression, unmapContext);
                clause.AddAssignment(expr);
            }

            model.UpdateClause = clause;

            if (updateDesc.OptionalWhereClause != null) {
                var expr = UnmapExpressionDeep(updateDesc.OptionalWhereClause, unmapContext);
                model.UpdateClause.OptionalWhereClause = expr;
            }
        }

        private static void UnmapContextName(
            string contextName,
            EPStatementObjectModel model)
        {
            model.ContextName = contextName;
        }

        private static void UnmapCreateContext(
            CreateContextDesc createContextDesc,
            EPStatementObjectModel model,
            StatementSpecUnMapContext unmapContext)
        {
            if (createContextDesc == null) {
                return;
            }

            var desc = UnmapCreateContextDetail(createContextDesc.ContextDetail, unmapContext);
            var clause = new CreateContextClause(createContextDesc.ContextName, desc);
            model.CreateContext = clause;
        }

        private static ContextDescriptor UnmapCreateContextDetail(
            ContextSpec contextDetail,
            StatementSpecUnMapContext unmapContext)
        {
            ContextDescriptor desc;
            if (contextDetail is ContextSpecInitiatedTerminated spec) {
                var startCondition =
                    UnmapCreateContextRangeCondition(spec.StartCondition, unmapContext);
                var endCondition =
                    UnmapCreateContextRangeCondition(spec.EndCondition, unmapContext);
                IList<Expression> distinctExpressions = null;
                if (spec.DistinctExpressions != null && spec.DistinctExpressions.Length > 0) {
                    distinctExpressions = UnmapExpressionDeep(Arrays.AsList(spec.DistinctExpressions), unmapContext);
                }

                desc = new ContextDescriptorInitiatedTerminated(
                    startCondition,
                    endCondition,
                    spec.IsOverlapping,
                    distinctExpressions);
            }
            else if (contextDetail is ContextSpecKeyed seg) {
                IList<ContextDescriptorKeyedSegmentedItem> segmentedItems =
                    new List<ContextDescriptorKeyedSegmentedItem>();
                foreach (var item in seg.Items) {
                    var filter = UnmapFilter(item.FilterSpecRaw, unmapContext);
                    segmentedItems.Add(
                        new ContextDescriptorKeyedSegmentedItem(item.PropertyNames, filter, item.AliasName));
                }

                IList<ContextDescriptorConditionFilter> initCondition = null;
                if (seg.OptionalInit != null) {
                    initCondition = new List<ContextDescriptorConditionFilter>();
                    foreach (ContextSpecCondition filter in seg.OptionalInit) {
                        initCondition.Add(
                            (ContextDescriptorConditionFilter)UnmapCreateContextRangeCondition(filter, unmapContext));
                    }
                }

                ContextDescriptorCondition terminationCondition = null;
                if (seg.OptionalTermination != null) {
                    terminationCondition = UnmapCreateContextRangeCondition(seg.OptionalTermination, unmapContext);
                }

                desc = new ContextDescriptorKeyedSegmented(segmentedItems, initCondition, terminationCondition);
            }
            else if (contextDetail is ContextSpecCategory category) {
                IList<ContextDescriptorCategoryItem> categoryItems = new List<ContextDescriptorCategoryItem>();
                var filter = UnmapFilter(category.FilterSpecRaw, unmapContext);
                foreach (var item in category.Items) {
                    var expr = UnmapExpressionDeep(item.Expression, unmapContext);
                    categoryItems.Add(new ContextDescriptorCategoryItem(expr, item.Name));
                }

                desc = new ContextDescriptorCategory(categoryItems, filter);
            }
            else if (contextDetail is ContextSpecHash init) {
                IList<ContextDescriptorHashSegmentedItem> hashes = new List<ContextDescriptorHashSegmentedItem>();
                foreach (var item in init.Items) {
                    var dot = UnmapChains(
                            new List<Chainable>(Collections.SingletonList(item.Function)),
                            unmapContext)
                        [0];
                    var dotExpression =
                        new SingleRowMethodExpression(new List<DotExpressionItem>(Collections.SingletonList(dot)));
                    var filter = UnmapFilter(item.FilterSpecRaw, unmapContext);
                    hashes.Add(new ContextDescriptorHashSegmentedItem(dotExpression, filter));
                }

                desc = new ContextDescriptorHashSegmented(hashes, init.Granularity, init.IsPreallocate);
            }
            else {
                var nested = (ContextNested)contextDetail;
                IList<CreateContextClause> contexts = new List<CreateContextClause>();
                foreach (var item in nested.Contexts) {
                    var detail = UnmapCreateContextDetail(item.ContextDetail, unmapContext);
                    contexts.Add(new CreateContextClause(item.ContextName, detail));
                }

                desc = new ContextDescriptorNested(contexts);
            }

            return desc;
        }

        private static ContextDescriptorCondition UnmapCreateContextRangeCondition(
            ContextSpecCondition endpoint,
            StatementSpecUnMapContext unmapContext)
        {
            if (endpoint is ContextSpecConditionCrontab crontab) {
                IList<IList<Expression>> crontabs = new List<IList<Expression>>();
                foreach (var crontabItem in crontab.Crontabs) {
                    crontabs.Add(UnmapExpressionDeep(crontabItem, unmapContext));
                }

                return new ContextDescriptorConditionCrontab(crontabs, crontab.IsImmediate);
            }
            else if (endpoint is ContextSpecConditionPattern pattern) {
                var patternExpr = UnmapPatternEvalDeep(pattern.PatternRaw, unmapContext);
                return new ContextDescriptorConditionPattern(
                    patternExpr,
                    pattern.IsInclusive,
                    pattern.IsImmediate,
                    pattern.AsName);
            }
            else if (endpoint is ContextSpecConditionFilter filter) {
                var filterExpr = UnmapFilter(filter.FilterSpecRaw, unmapContext);
                return new ContextDescriptorConditionFilter(filterExpr, filter.OptionalFilterAsName);
            }
            else if (endpoint is ContextSpecConditionTimePeriod period) {
                var expression =
                    (TimePeriodExpression)UnmapExpressionDeep(period.TimePeriod, unmapContext);
                return new ContextDescriptorConditionTimePeriod(expression, period.IsImmediate);
            }
            else if (endpoint is ContextSpecConditionImmediate) {
                return new ContextDescriptorConditionImmediate();
            }
            else if (endpoint is ContextSpecConditionNever) {
                return new ContextDescriptorConditionNever();
            }

            throw new IllegalStateException("Unrecognized endpoint " + endpoint);
        }

        private static void UnmapCreateWindow(
            CreateWindowDesc createWindowDesc,
            EPStatementObjectModel model,
            StatementSpecUnMapContext unmapContext)
        {
            if (createWindowDesc == null) {
                return;
            }

            Expression filter = null;
            if (createWindowDesc.InsertFilter != null) {
                filter = UnmapExpressionDeep(createWindowDesc.InsertFilter, unmapContext);
            }

            var clause = new CreateWindowClause(
                createWindowDesc.WindowName,
                UnmapViews(createWindowDesc.ViewSpecs, unmapContext));
            clause.Insert = createWindowDesc.IsInsert;
            clause.InsertWhereClause = filter;
            clause.Columns = UnmapColumns(createWindowDesc.Columns);
            clause.AsEventTypeName = createWindowDesc.AsEventTypeName;
            clause.IsRetainUnion = createWindowDesc.StreamSpecOptions.IsRetainUnion;
            model.CreateWindow = clause;
        }

        private static void UnmapCreateIndex(
            CreateIndexDesc createIndexDesc,
            EPStatementObjectModel model,
            StatementSpecUnMapContext unmapContext)
        {
            if (createIndexDesc == null) {
                return;
            }

            IList<CreateIndexColumn> cols = new List<CreateIndexColumn>();
            foreach (var item in createIndexDesc.Columns) {
                var col = UnmapCreateIndexColumn(item, unmapContext);
                cols.Add(col);
            }

            model.CreateIndex = new CreateIndexClause(
                createIndexDesc.IndexName,
                createIndexDesc.WindowName,
                cols,
                createIndexDesc.IsUnique);
        }

        private static CreateIndexColumn UnmapCreateIndexColumn(
            CreateIndexItem item,
            StatementSpecUnMapContext unmapContext)
        {
            var columns = UnmapExpressionDeep(item.Expressions, unmapContext);
            var parameters = UnmapExpressionDeep(item.Parameters, unmapContext);
            return new CreateIndexColumn(columns, item.IndexType, parameters);
        }

        private static void UnmapCreateVariable(
            CreateVariableDesc createVariableDesc,
            EPStatementObjectModel model,
            StatementSpecUnMapContext unmapContext)
        {
            if (createVariableDesc == null) {
                return;
            }

            Expression assignment = null;
            if (createVariableDesc.Assignment != null) {
                assignment = UnmapExpressionDeep(createVariableDesc.Assignment, unmapContext);
            }

            var clause = new CreateVariableClause(
                createVariableDesc.VariableType.ToEPL(),
                createVariableDesc.VariableName,
                assignment,
                createVariableDesc.IsConstant);
            model.CreateVariable = clause;
        }

        private static void UnmapCreateSchema(
            CreateSchemaDesc desc,
            EPStatementObjectModel model)
        {
            if (desc == null) {
                return;
            }

            model.CreateSchema = UnmapCreateSchemaInternal(desc);
        }

        private static void UnmapCreateExpression(
            CreateExpressionDesc desc,
            EPStatementObjectModel model,
            StatementSpecUnMapContext unmapContext)
        {
            if (desc == null) {
                return;
            }

            CreateExpressionClause clause;
            if (desc.Expression != null) {
                clause = new CreateExpressionClause(UnmapExpressionDeclItem(desc.Expression));
            }
            else {
                clause = new CreateExpressionClause(UnmapScriptExpression(desc.Script));
            }

            model.CreateExpression = clause;
        }

        private static void UnmapCreateClass(
            string createClassClassText,
            EPStatementObjectModel model)
        {
            if (createClassClassText == null) {
                return;
            }

            model.CreateClass = new CreateClassClause(createClassClassText);
        }

        private static CreateSchemaClause UnmapCreateSchemaInternal(CreateSchemaDesc desc)
        {
            var columns = UnmapColumns(desc.Columns);
            var clause = new CreateSchemaClause(
                desc.SchemaName,
                desc.Types,
                columns,
                desc.Inherits,
                desc.AssignedType.MapToSoda());
            clause.StartTimestampPropertyName = desc.StartTimestampProperty;
            clause.EndTimestampPropertyName = desc.EndTimestampProperty;
            clause.CopyFrom = desc.CopyFrom;
            return clause;
        }

        private static void UnmapCreateGraph(
            CreateDataFlowDesc desc,
            EPStatementObjectModel model,
            StatementSpecUnMapContext unmapContext)
        {
            if (desc == null) {
                return;
            }

            IList<CreateSchemaClause> schemas = new List<CreateSchemaClause>();
            foreach (var schema in desc.Schemas) {
                schemas.Add(UnmapCreateSchemaInternal(schema));
            }

            IList<DataFlowOperator> operators = new List<DataFlowOperator>();
            foreach (var spec in desc.Operators) {
                operators.Add(UnmapGraphOperator(spec, unmapContext));
            }

            var clause = new CreateDataFlowClause(desc.GraphName, schemas, operators);
            model.CreateDataFlow = clause;
        }

        private static DataFlowOperator UnmapGraphOperator(
            GraphOperatorSpec spec,
            StatementSpecUnMapContext unmapContext)
        {
            var op = new DataFlowOperator();
            op.OperatorName = spec.OperatorName;
            op.Annotations = UnmapAnnotations(spec.Annotations);

            IList<DataFlowOperatorInput> inputs = new List<DataFlowOperatorInput>();
            foreach (var @in in spec.Input.StreamNamesAndAliases) {
                inputs.Add(new DataFlowOperatorInput(Arrays.AsList(@in.InputStreamNames), @in.OptionalAsName));
            }

            op.Input = inputs;

            IList<DataFlowOperatorOutput> outputs = new List<DataFlowOperatorOutput>();
            foreach (var @out in spec.Output.Items) {
                IList<DataFlowOperatorOutputType> types = @out.TypeInfo.IsEmpty()
                    ? null
                    : new List<DataFlowOperatorOutputType>(
                        Collections.SingletonList(UnmapTypeInfo(@out.TypeInfo[0])));
                outputs.Add(new DataFlowOperatorOutput(@out.StreamName, types));
            }

            op.Output = outputs;

            if (spec.Detail != null) {
                IList<DataFlowOperatorParameter> parameters = new List<DataFlowOperatorParameter>();
                foreach (var param in spec.Detail.Configs) {
                    var value = param.Value;
                    if (value is StatementSpecRaw raw) {
                        value = UnmapInternal(raw, unmapContext);
                    }

                    if (value is ExprNode node) {
                        value = UnmapExpressionDeep(node, unmapContext);
                    }

                    parameters.Add(new DataFlowOperatorParameter(param.Key, value));
                }

                op.Parameters = parameters;
            }
            else {
                op.Parameters = EmptyList<DataFlowOperatorParameter>.Instance;
            }

            return op;
        }

        private static DataFlowOperatorOutputType UnmapTypeInfo(GraphOperatorOutputItemType typeInfo)
        {
            IList<DataFlowOperatorOutputType> types = EmptyList<DataFlowOperatorOutputType>.Instance;
            if (typeInfo.TypeParameters != null && !typeInfo.TypeParameters.IsEmpty()) {
                types = new List<DataFlowOperatorOutputType>();
                foreach (var type in typeInfo.TypeParameters) {
                    types.Add(UnmapTypeInfo(type));
                }
            }

            return new DataFlowOperatorOutputType(typeInfo.IsWildcard, typeInfo.TypeOrClassname, types);
        }

        private static void UnmapOrderBy(
            IList<OrderByItem> orderByList,
            EPStatementObjectModel model,
            StatementSpecUnMapContext unmapContext)
        {
            if (orderByList == null || orderByList.Count == 0) {
                return;
            }

            var clause = new OrderByClause();
            foreach (var item in orderByList) {
                var expr = UnmapExpressionDeep(item.ExprNode, unmapContext);
                clause.Add(expr, item.IsDescending);
            }

            model.OrderByClause = clause;
        }

        private static void UnmapOutputLimit(
            OutputLimitSpec outputLimitSpec,
            EPStatementObjectModel model,
            StatementSpecUnMapContext unmapContext)
        {
            if (outputLimitSpec == null) {
                return;
            }

            var selector = OutputLimitSelector.DEFAULT;
            if (outputLimitSpec.DisplayLimit == OutputLimitLimitType.FIRST) {
                selector = OutputLimitSelector.FIRST;
            }

            if (outputLimitSpec.DisplayLimit == OutputLimitLimitType.LAST) {
                selector = OutputLimitSelector.LAST;
            }

            if (outputLimitSpec.DisplayLimit == OutputLimitLimitType.SNAPSHOT) {
                selector = OutputLimitSelector.SNAPSHOT;
            }

            if (outputLimitSpec.DisplayLimit == OutputLimitLimitType.ALL) {
                selector = OutputLimitSelector.ALL;
            }

            OutputLimitClause clause;
            if (outputLimitSpec.RateType == OutputLimitRateType.TIME_PERIOD) {
                var timePeriod = (TimePeriodExpression)UnmapExpressionDeep(
                    outputLimitSpec.TimePeriodExpr,
                    unmapContext);
                clause = new OutputLimitClause(selector, timePeriod);
            }
            else if (outputLimitSpec.RateType == OutputLimitRateType.AFTER) {
                if (outputLimitSpec.AfterTimePeriodExpr != null) {
                    var after = (TimePeriodExpression)UnmapExpressionDeep(
                        outputLimitSpec.AfterTimePeriodExpr,
                        unmapContext);
                    clause = new OutputLimitClause(OutputLimitSelector.DEFAULT, OutputLimitUnit.AFTER, after, null);
                }
                else {
                    clause = new OutputLimitClause(
                        OutputLimitSelector.DEFAULT,
                        OutputLimitUnit.AFTER,
                        null,
                        outputLimitSpec.AfterNumberOfEvents);
                }
            }
            else if (outputLimitSpec.RateType == OutputLimitRateType.WHEN_EXPRESSION) {
                var whenExpression = UnmapExpressionDeep(outputLimitSpec.WhenExpressionNode, unmapContext);
                IList<Assignment> thenAssignments = new List<Assignment>();
                clause = new OutputLimitClause(selector, whenExpression, thenAssignments);
                if (outputLimitSpec.ThenExpressions != null) {
                    foreach (var assignment in outputLimitSpec.ThenExpressions) {
                        var expr = UnmapExpressionDeep(assignment.Expression, unmapContext);
                        clause.WithAddThenAssignment(expr);
                    }
                }
            }
            else if (outputLimitSpec.RateType == OutputLimitRateType.CRONTAB) {
                var timerAtExpressions = outputLimitSpec.CrontabAtSchedule;
                var mappedExpr = UnmapExpressionDeep(timerAtExpressions, unmapContext);
                clause = new OutputLimitClause(selector, mappedExpr.ToArray());
            }
            else if (outputLimitSpec.RateType == OutputLimitRateType.TERM) {
                clause = new OutputLimitClause(selector, OutputLimitUnit.CONTEXT_PARTITION_TERM);
            }
            else {
                clause = new OutputLimitClause(
                    selector,
                    outputLimitSpec.Rate,
                    outputLimitSpec.VariableName,
                    OutputLimitUnit.EVENTS);
            }

            clause.AfterNumberOfEvents = outputLimitSpec.AfterNumberOfEvents;
            if (outputLimitSpec.AfterTimePeriodExpr != null) {
                clause.AfterTimePeriodExpression = UnmapExpressionDeep(
                    outputLimitSpec.AfterTimePeriodExpr,
                    unmapContext);
            }

            clause.IsAndAfterTerminate = outputLimitSpec.IsAndAfterTerminate;
            if (outputLimitSpec.AndAfterTerminateExpr != null) {
                clause.AndAfterTerminateAndExpr = UnmapExpressionDeep(
                    outputLimitSpec.AndAfterTerminateExpr,
                    unmapContext);
            }

            if (outputLimitSpec.AndAfterTerminateThenExpressions != null) {
                IList<Assignment> thenAssignments = new List<Assignment>();
                foreach (var assignment in outputLimitSpec.AndAfterTerminateThenExpressions) {
                    var expr = UnmapExpressionDeep(assignment.Expression, unmapContext);
                    thenAssignments.Add(new Assignment(expr));
                }

                clause.AndAfterTerminateThenAssignments = thenAssignments;
            }

            model.OutputLimitClause = clause;
        }

        private static void UnmapRowLimit(
            RowLimitSpec rowLimitSpec,
            EPStatementObjectModel model)
        {
            if (rowLimitSpec == null) {
                return;
            }

            var spec = new RowLimitClause(
                rowLimitSpec.NumRows,
                rowLimitSpec.OptionalOffset,
                rowLimitSpec.NumRowsVariable,
                rowLimitSpec.OptionalOffsetVariable);
            model.RowLimitClause = spec;
        }

        private static void UnmapForClause(
            ForClauseSpec spec,
            EPStatementObjectModel model,
            StatementSpecUnMapContext unmapContext)
        {
            if (spec == null || spec.Clauses == null || spec.Clauses.Count == 0) {
                return;
            }

            var clause = new ForClause();
            foreach (var itemSpec in spec.Clauses) {
                var item = new ForClauseItem(itemSpec.Keyword.Xlate<ForClauseKeyword>());
                item.Expressions = UnmapExpressionDeep(itemSpec.Expressions, unmapContext);
                clause.Items.Add(item);
            }

            model.ForClause = clause;
        }

        private static void UnmapMatchRecognize(
            MatchRecognizeSpec spec,
            EPStatementObjectModel model,
            StatementSpecUnMapContext unmapContext)
        {
            if (spec == null) {
                return;
            }

            var clause = new MatchRecognizeClause();
            clause.PartitionExpressions = UnmapExpressionDeep(spec.PartitionByExpressions, unmapContext);

            IList<SelectClauseExpression> measures = new List<SelectClauseExpression>();
            foreach (var item in spec.Measures) {
                measures.Add(new SelectClauseExpression(UnmapExpressionDeep(item.Expr, unmapContext), item.Name));
            }

            clause.Measures = measures;
            clause.IsAll = spec.IsAllMatches;
            clause.SkipClause = spec.Skip.Skip.GetName().Xlate<MatchRecognizeSkipClause>();
            IList<MatchRecognizeDefine> defines = new List<MatchRecognizeDefine>();
            foreach (var define in spec.Defines) {
                defines.Add(
                    new MatchRecognizeDefine(define.Identifier, UnmapExpressionDeep(define.Expression, unmapContext)));
            }

            clause.Defines = defines;

            if (spec.Interval != null) {
                clause.IntervalClause = new MatchRecognizeIntervalClause(
                    (TimePeriodExpression)UnmapExpressionDeep(spec.Interval.TimePeriodExpr, unmapContext),
                    spec.Interval.IsOrTerminated);
            }

            clause.Pattern = UnmapExpressionDeepRowRecog(spec.Pattern, unmapContext);
            model.MatchRecognizeClause = clause;
        }

        private static void MapOrderBy(
            OrderByClause orderByClause,
            StatementSpecRaw raw,
            StatementSpecMapContext mapContext)
        {
            if (orderByClause == null) {
                return;
            }

            foreach (var element in orderByClause.OrderByExpressions) {
                var orderExpr = MapExpressionDeep(element.Expression, mapContext);
                var item = new OrderByItem(orderExpr, element.IsDescending);
                raw.OrderByList.Add(item);
            }
        }

        private static void MapOutputLimit(
            OutputLimitClause outputLimitClause,
            StatementSpecRaw raw,
            StatementSpecMapContext mapContext)
        {
            if (outputLimitClause == null) {
                return;
            }

            var displayLimit = outputLimitClause.Selector.GetName().Xlate<OutputLimitLimitType>();
            OutputLimitRateType rateType;
            if (outputLimitClause.Unit == OutputLimitUnit.EVENTS) {
                rateType = OutputLimitRateType.EVENTS;
            }
            else if (outputLimitClause.Unit == OutputLimitUnit.TIME_PERIOD) {
                rateType = OutputLimitRateType.TIME_PERIOD;
            }
            else if (outputLimitClause.Unit == OutputLimitUnit.CRONTAB_EXPRESSION) {
                rateType = OutputLimitRateType.CRONTAB;
            }
            else if (outputLimitClause.Unit == OutputLimitUnit.WHEN_EXPRESSION) {
                rateType = OutputLimitRateType.WHEN_EXPRESSION;
            }
            else if (outputLimitClause.Unit == OutputLimitUnit.AFTER) {
                rateType = OutputLimitRateType.AFTER;
            }
            else if (outputLimitClause.Unit == OutputLimitUnit.CONTEXT_PARTITION_TERM) {
                rateType = OutputLimitRateType.TERM;
            }
            else {
                throw new ArgumentException("Unknown output limit unit " + outputLimitClause.Unit);
            }

            var frequency = outputLimitClause.Frequency;
            var frequencyVariable = outputLimitClause.FrequencyVariable;

            if (frequencyVariable != null) {
                mapContext.VariableNames.Add(frequencyVariable);
            }

            ExprNode whenExpression = null;
            IList<OnTriggerSetAssignment> assignments = null;
            if (outputLimitClause.WhenExpression != null) {
                whenExpression = MapExpressionDeep(outputLimitClause.WhenExpression, mapContext);

                assignments = new List<OnTriggerSetAssignment>();
                foreach (var pair in outputLimitClause.ThenAssignments) {
                    var expr = MapExpressionDeep(pair.Value, mapContext);
                    assignments.Add(new OnTriggerSetAssignment(expr));
                }
            }

            IList<ExprNode> timerAtExprList = null;
            if (outputLimitClause.CrontabAtParameters != null) {
                timerAtExprList = MapExpressionDeep(Arrays.AsList(outputLimitClause.CrontabAtParameters), mapContext);
            }

            ExprTimePeriod timePeriod = null;
            if (outputLimitClause.TimePeriodExpression != null) {
                timePeriod = (ExprTimePeriod)MapExpressionDeep(outputLimitClause.TimePeriodExpression, mapContext);
            }

            ExprTimePeriod afterTimePeriod = null;
            if (outputLimitClause.AfterTimePeriodExpression != null) {
                afterTimePeriod = (ExprTimePeriod)MapExpressionDeep(
                    outputLimitClause.AfterTimePeriodExpression,
                    mapContext);
            }

            ExprNode andAfterTerminateAndExpr = null;
            if (outputLimitClause.AndAfterTerminateAndExpr != null) {
                andAfterTerminateAndExpr = MapExpressionDeep(outputLimitClause.AndAfterTerminateAndExpr, mapContext);
            }

            IList<OnTriggerSetAssignment> afterTerminateAssignments = null;
            if (outputLimitClause.AndAfterTerminateThenAssignments != null) {
                afterTerminateAssignments = new List<OnTriggerSetAssignment>();
                foreach (var pair in outputLimitClause.AndAfterTerminateThenAssignments) {
                    var expr = MapExpressionDeep(pair.Value, mapContext);
                    afterTerminateAssignments.Add(new OnTriggerSetAssignment(expr));
                }
            }

            var spec = new OutputLimitSpec(
                frequency,
                frequencyVariable,
                rateType,
                displayLimit,
                whenExpression,
                assignments,
                timerAtExprList,
                timePeriod,
                afterTimePeriod,
                outputLimitClause.AfterNumberOfEvents,
                outputLimitClause.IsAndAfterTerminate,
                andAfterTerminateAndExpr,
                afterTerminateAssignments);
            raw.OutputLimitSpec = spec;
        }

        private static void MapOnTrigger(
            OnClause onExpr,
            StatementSpecRaw raw,
            StatementSpecMapContext mapContext)
        {
            if (onExpr == null) {
                return;
            }

            if (onExpr is OnDeleteClause onDeleteClause) {
                raw.OnTriggerDesc = new OnTriggerWindowDesc(
                    onDeleteClause.WindowName,
                    onDeleteClause.OptionalAsName,
                    OnTriggerType.ON_DELETE,
                    false);
            }
            else if (onExpr is OnSelectClause onSelectClause) {
                raw.OnTriggerDesc = new OnTriggerWindowDesc(
                    onSelectClause.WindowName,
                    onSelectClause.OptionalAsName,
                    OnTriggerType.ON_SELECT,
                    onSelectClause.IsDeleteAndSelect);
            }
            else if (onExpr is OnSetClause setClause) {
                IList<OnTriggerSetAssignment> assignments = new List<OnTriggerSetAssignment>();
                foreach (var pair in setClause.Assignments) {
                    var expr = MapExpressionDeep(pair.Value, mapContext);
                    assignments.Add(new OnTriggerSetAssignment(expr));
                }

                var desc = new OnTriggerSetDesc(assignments);
                raw.OnTriggerDesc = desc;
            }
            else if (onExpr is OnUpdateClause updateClause) {
                IList<OnTriggerSetAssignment> assignments = new List<OnTriggerSetAssignment>();
                foreach (var pair in updateClause.Assignments) {
                    var expr = MapExpressionDeep(pair.Value, mapContext);
                    assignments.Add(new OnTriggerSetAssignment(expr));
                }

                var desc = new OnTriggerWindowUpdateDesc(
                    updateClause.WindowName,
                    updateClause.OptionalAsName,
                    assignments);
                raw.OnTriggerDesc = desc;
            }
            else if (onExpr is OnInsertSplitStreamClause splitClause) {
                IList<OnTriggerSplitStream> streams = new List<OnTriggerSplitStream>();
                foreach (var item in splitClause.Items) {
                    OnTriggerSplitStreamFromClause fromClause = null;
                    if (item.PropertySelects != null) {
                        var propertyEvalSpec = MapPropertySelects(item.PropertySelects, mapContext);
                        fromClause = new OnTriggerSplitStreamFromClause(
                            propertyEvalSpec,
                            item.PropertySelectsStreamName);
                    }

                    ExprNode whereClause = null;
                    if (item.WhereClause != null) {
                        whereClause = MapExpressionDeep(item.WhereClause, mapContext);
                    }

                    var insertDesc = MapInsertInto(item.InsertInto, mapContext);
                    var selectDesc = MapSelectRaw(item.SelectClause, mapContext);

                    streams.Add(new OnTriggerSplitStream(insertDesc, selectDesc, fromClause, whereClause));
                }

                var desc = new OnTriggerSplitStreamDesc(
                    OnTriggerType.ON_SPLITSTREAM,
                    splitClause.IsFirst,
                    streams);
                raw.OnTriggerDesc = desc;
            }
            else if (onExpr is OnMergeClause merge) {
                IList<OnTriggerMergeMatched> matcheds = new List<OnTriggerMergeMatched>();
                foreach (var matchItem in merge.MatchItems) {
                    IList<OnTriggerMergeAction> actions = new List<OnTriggerMergeAction>();
                    foreach (var action in matchItem.Actions) {
                        OnTriggerMergeAction actionItem;
                        if (action is OnMergeMatchedDeleteAction delete) {
                            var optionalConditionX = delete.WhereClause == null
                                ? null
                                : MapExpressionDeep(delete.WhereClause, mapContext);
                            actionItem = new OnTriggerMergeActionDelete(optionalConditionX);
                        }
                        else if (action is OnMergeMatchedUpdateAction update) {
                            IList<OnTriggerSetAssignment> assignments = new List<OnTriggerSetAssignment>();
                            foreach (var pair in update.Assignments) {
                                var expr = MapExpressionDeep(pair.Value, mapContext);
                                assignments.Add(new OnTriggerSetAssignment(expr));
                            }

                            var optionalConditionX = update.WhereClause == null
                                ? null
                                : MapExpressionDeep(update.WhereClause, mapContext);
                            actionItem = new OnTriggerMergeActionUpdate(optionalConditionX, assignments);
                        }
                        else if (action is OnMergeMatchedInsertAction insertAction) {
                            actionItem = MapOnTriggerMergeActionInsert(insertAction, mapContext);
                        }
                        else {
                            throw new ArgumentException("Unrecognized merged action type '" + action.GetType() + "'");
                        }

                        actions.Add(actionItem);
                    }

                    var optionalCondition = matchItem.OptionalCondition == null
                        ? null
                        : MapExpressionDeep(matchItem.OptionalCondition, mapContext);
                    matcheds.Add(new OnTriggerMergeMatched(matchItem.IsMatched, optionalCondition, actions));
                }

                var optionalInsertNoMatch = merge.InsertNoMatch == null
                    ? null
                    : MapOnTriggerMergeActionInsert(merge.InsertNoMatch, mapContext);
                var mergeDesc = new OnTriggerMergeDesc(
                    merge.WindowName,
                    merge.OptionalAsName,
                    optionalInsertNoMatch,
                    matcheds);
                raw.OnTriggerDesc = mergeDesc;
            }
            else {
                throw new ArgumentException("Cannot map on-clause expression type : " + onExpr);
            }
        }

        private static OnTriggerMergeActionInsert MapOnTriggerMergeActionInsert(
            OnMergeMatchedInsertAction insert,
            StatementSpecMapContext mapContext)
        {
            IList<string> columnNames = new List<string>(insert.ColumnNames);
            var select = MapSelectClauseElements(insert.SelectList, mapContext);
            var optionalCondition =
                insert.WhereClause == null ? null : MapExpressionDeep(insert.WhereClause, mapContext);
            var optionalPrecedence = insert.EventPrecedence == null
                ? null
                : MapExpressionDeep(insert.EventPrecedence, mapContext);
            return new OnTriggerMergeActionInsert(
                optionalCondition,
                insert.OptionalStreamName,
                columnNames,
                select,
                optionalPrecedence);
        }

        private static void MapRowLimit(
            RowLimitClause rowLimitClause,
            StatementSpecRaw raw,
            StatementSpecMapContext mapContext)
        {
            if (rowLimitClause == null) {
                return;
            }

            if (rowLimitClause.NumRowsVariable != null) {
                mapContext.VariableNames.Add(rowLimitClause.NumRowsVariable);
            }

            if (rowLimitClause.OptionalOffsetRowsVariable != null) {
                mapContext.VariableNames.Add(rowLimitClause.OptionalOffsetRowsVariable);
            }

            raw.RowLimitSpec = new RowLimitSpec(
                rowLimitClause.NumRows,
                rowLimitClause.OptionalOffsetRows,
                rowLimitClause.NumRowsVariable,
                rowLimitClause.OptionalOffsetRowsVariable);
        }

        private static void MapForClause(
            ForClause clause,
            StatementSpecRaw raw,
            StatementSpecMapContext mapContext)
        {
            if (clause == null || clause.Items.Count == 0) {
                return;
            }

            raw.ForClauseSpec = new ForClauseSpec();
            foreach (var item in clause.Items) {
                var specItem = new ForClauseItemSpec(
                    item.Keyword?.GetName(),
                    MapExpressionDeep(item.Expressions, mapContext));
                raw.ForClauseSpec.Clauses.Add(specItem);
            }
        }

        private static void MapMatchRecognize(
            MatchRecognizeClause clause,
            StatementSpecRaw raw,
            StatementSpecMapContext mapContext)
        {
            if (clause == null) {
                return;
            }

            var spec = new MatchRecognizeSpec();
            spec.PartitionByExpressions = MapExpressionDeep(clause.PartitionExpressions, mapContext);

            IList<MatchRecognizeMeasureItem> measures = new List<MatchRecognizeMeasureItem>();
            foreach (var item in clause.Measures) {
                measures.Add(
                    new MatchRecognizeMeasureItem(MapExpressionDeep(item.Expression, mapContext), item.AsName));
            }

            spec.Measures = measures;
            spec.IsAllMatches = clause.IsAll;
            spec.Skip = new MatchRecognizeSkip(clause.SkipClause.Xlate<MatchRecognizeSkipEnum>());

            IList<MatchRecognizeDefineItem> defines = new List<MatchRecognizeDefineItem>();
            foreach (var define in clause.Defines) {
                defines.Add(
                    new MatchRecognizeDefineItem(define.Name, MapExpressionDeep(define.Expression, mapContext)));
            }

            spec.Defines = defines;

            if (clause.IntervalClause != null) {
                var timePeriod = (ExprTimePeriod)MapExpressionDeep(
                    clause.IntervalClause.Expression,
                    mapContext);
                spec.Interval = new MatchRecognizeInterval(timePeriod, clause.IntervalClause.IsOrTerminated);
            }

            spec.Pattern = MapExpressionDeepRowRecog(clause.Pattern, mapContext);
            raw.MatchRecognizeSpec = spec;
        }

        private static void MapHaving(
            Expression havingClause,
            StatementSpecRaw raw,
            StatementSpecMapContext mapContext)
        {
            if (havingClause == null) {
                return;
            }

            var node = MapExpressionDeep(havingClause, mapContext);
            raw.HavingClause = node;
        }

        private static void UnmapHaving(
            ExprNode havingExprRootNode,
            EPStatementObjectModel model,
            StatementSpecUnMapContext unmapContext)
        {
            if (havingExprRootNode == null) {
                return;
            }

            var expr = UnmapExpressionDeep(havingExprRootNode, unmapContext);
            model.HavingClause = expr;
        }

        private static void MapGroupBy(
            GroupByClause groupByClause,
            StatementSpecRaw raw,
            StatementSpecMapContext mapContext)
        {
            if (groupByClause == null) {
                return;
            }

            foreach (var expr in groupByClause.GroupByExpressions) {
                var element = MapGroupByExpr(expr, mapContext);
                raw.GroupByExpressions.Add(element);
            }
        }

        private static GroupByClauseElement MapGroupByExpr(
            GroupByClauseExpression expr,
            StatementSpecMapContext mapContext)
        {
            if (expr is GroupByClauseExpressionSingle single) {
                var node = MapExpressionDeep(single.Expression, mapContext);
                return new GroupByClauseElementExpr(node);
            }

            if (expr is GroupByClauseExpressionCombination combination) {
                var nodes = MapExpressionDeep(combination.Expressions, mapContext);
                return new GroupByClauseElementCombinedExpr(nodes);
            }

            if (expr is GroupByClauseExpressionGroupingSet set) {
                return new GroupByClauseElementGroupingSet(MapGroupByElements(set.Expressions, mapContext));
            }

            if (expr is GroupByClauseExpressionRollupOrCube rollup) {
                return new GroupByClauseElementRollupOrCube(
                    rollup.IsCube,
                    MapGroupByElements(rollup.Expressions, mapContext));
            }

            throw new IllegalStateException("Group by expression not recognized: " + expr);
        }

        private static IList<GroupByClauseElement> MapGroupByElements(
            IList<GroupByClauseExpression> elements,
            StatementSpecMapContext mapContext)
        {
            IList<GroupByClauseElement> @out = new List<GroupByClauseElement>();
            foreach (var element in elements) {
                @out.Add(MapGroupByExpr(element, mapContext));
            }

            return @out;
        }

        private static void UnmapGroupBy(
            IList<GroupByClauseElement> groupByExpressions,
            EPStatementObjectModel model,
            StatementSpecUnMapContext unmapContext)
        {
            if (groupByExpressions.Count == 0) {
                return;
            }

            IList<GroupByClauseExpression> expressions = new List<GroupByClauseExpression>();
            foreach (var element in groupByExpressions) {
                expressions.Add(UnmapGroupByExpression(element, unmapContext));
            }

            model.GroupByClause = new GroupByClause(expressions);
        }

        private static GroupByClauseExpression UnmapGroupByExpression(
            GroupByClauseElement element,
            StatementSpecUnMapContext unmapContext)
        {
            if (element is GroupByClauseElementExpr elementExpr) {
                var unmapped = UnmapExpressionDeep(elementExpr.Expr, unmapContext);
                return new GroupByClauseExpressionSingle(unmapped);
            }

            if (element is GroupByClauseElementCombinedExpr expr) {
                var unmapped = UnmapExpressionDeep(expr.Expressions, unmapContext);
                return new GroupByClauseExpressionCombination(unmapped);
            }
            else if (element is GroupByClauseElementRollupOrCube rollup) {
                var elements = UnmapGroupByExpressions(
                    rollup.RollupExpressions,
                    unmapContext);
                return new GroupByClauseExpressionRollupOrCube(rollup.IsCube, elements);
            }
            else if (element is GroupByClauseElementGroupingSet set) {
                var elements = UnmapGroupByExpressions(set.Elements, unmapContext);
                return new GroupByClauseExpressionGroupingSet(elements);
            }
            else {
                throw new IllegalStateException("Unrecognized group-by element " + element);
            }
        }

        private static IList<GroupByClauseExpression> UnmapGroupByExpressions(
            IList<GroupByClauseElement> rollupExpressions,
            StatementSpecUnMapContext unmapContext)
        {
            IList<GroupByClauseExpression> @out = new List<GroupByClauseExpression>();
            foreach (var e in rollupExpressions) {
                @out.Add(UnmapGroupByExpression(e, unmapContext));
            }

            return @out;
        }

        private static void MapWhere(
            Expression whereClause,
            StatementSpecRaw raw,
            StatementSpecMapContext mapContext)
        {
            if (whereClause == null) {
                return;
            }

            var node = MapExpressionDeep(whereClause, mapContext);
            raw.WhereClause = node;
        }

        private static void UnmapWhere(
            ExprNode filterRootNode,
            EPStatementObjectModel model,
            StatementSpecUnMapContext unmapContext)
        {
            if (filterRootNode == null) {
                return;
            }

            var expr = UnmapExpressionDeep(filterRootNode, unmapContext);
            model.WhereClause = expr;
        }

        private static void UnmapFrom(
            IList<StreamSpecRaw> streamSpecs,
            IList<OuterJoinDesc> outerJoinDescList,
            EPStatementObjectModel model,
            StatementSpecUnMapContext unmapContext)
        {
            var from = new FromClause();
            model.FromClause = from;

            foreach (var stream in streamSpecs) {
                Stream targetStream;
                if (stream is FilterStreamSpecRaw filterStreamSpec) {
                    var filter = UnmapFilter(filterStreamSpec.RawFilterSpec, unmapContext);
                    var filterStream = new FilterStream(filter, filterStreamSpec.OptionalStreamName);
                    UnmapStreamOpts(stream.Options, filterStream);
                    targetStream = filterStream;
                }
                else if (stream is DBStatementStreamSpec db) {
                    targetStream = new SQLStream(
                        db.DatabaseName,
                        db.SqlWithSubsParams,
                        db.OptionalStreamName,
                        db.MetadataSQL);
                }
                else if (stream is PatternStreamSpecRaw pattern) {
                    var patternExpr = UnmapPatternEvalDeep(pattern.EvalForgeNode, unmapContext);
                    var annotationParts = PatternLevelAnnotationUtil.AnnotationsFromSpec(pattern);
                    var patternStream = new PatternStream(
                        patternExpr,
                        pattern.OptionalStreamName,
                        annotationParts);
                    UnmapStreamOpts(stream.Options, patternStream);
                    targetStream = patternStream;
                }
                else if (stream is MethodStreamSpec method) {
                    var methodStream = new MethodInvocationStream(
                        method.ClassName,
                        method.MethodName,
                        method.OptionalStreamName);
                    foreach (var exprNode in method.Expressions) {
                        var expr = UnmapExpressionDeep(exprNode, unmapContext);
                        methodStream.AddParameter(expr);
                    }

                    methodStream.OptionalEventTypeName = method.EventTypeName;
                    targetStream = methodStream;
                }
                else {
                    throw new ArgumentException("Stream modelled by " + stream.GetType() + " cannot be unmapped");
                }

                if (targetStream is ProjectedStream projStream) {
                    foreach (var viewSpec in stream.ViewSpecs) {
                        var viewExpressions = UnmapExpressionDeep(
                            viewSpec.ObjectParameters,
                            unmapContext);
                        projStream.AddView(View.Create(viewSpec.ObjectNamespace, viewSpec.ObjectName, viewExpressions));
                    }
                }

                from.Add(targetStream);
            }

            foreach (var desc in outerJoinDescList) {
                PropertyValueExpression left = null;
                PropertyValueExpression right = null;
                var additionalProperties = new List<PropertyValueExpressionPair>();

                if (desc.OptLeftNode != null) {
                    left = (PropertyValueExpression)UnmapExpressionFlat(desc.OptLeftNode, unmapContext);
                    right = (PropertyValueExpression)UnmapExpressionFlat(desc.OptRightNode, unmapContext);

                    if (desc.AdditionalLeftNodes != null) {
                        for (var i = 0; i < desc.AdditionalLeftNodes.Length; i++) {
                            var leftNode = desc.AdditionalLeftNodes[i];
                            var rightNode = desc.AdditionalRightNodes[i];
                            var propLeft =
                                (PropertyValueExpression)UnmapExpressionFlat(leftNode, unmapContext);
                            var propRight =
                                (PropertyValueExpression)UnmapExpressionFlat(rightNode, unmapContext);
                            additionalProperties.Add(new PropertyValueExpressionPair(propLeft, propRight));
                        }
                    }
                }

                from.Add(new OuterJoinQualifier(desc.OuterJoinType, left, right, additionalProperties));
            }
        }

        private static void UnmapStreamOpts(
            StreamSpecOptions options,
            ProjectedStream stream)
        {
            stream.IsUnidirectional = options.IsUnidirectional;
            stream.IsRetainUnion = options.IsRetainUnion;
            stream.IsRetainIntersection = options.IsRetainIntersection;
        }

        private static StreamSpecOptions MapStreamOpts(ProjectedStream stream)
        {
            return new StreamSpecOptions(stream.IsUnidirectional, stream.IsRetainUnion, stream.IsRetainIntersection);
        }

        private static SelectClause UnmapSelect(
            SelectClauseSpecRaw selectClauseSpec,
            SelectClauseStreamSelectorEnum selectStreamSelectorEnum,
            StatementSpecUnMapContext unmapContext)
        {
            var clause = SelectClause.Create();
            clause.StreamSelector = MapFromSODA(selectStreamSelectorEnum);
            clause.AddElements(UnmapSelectClauseElements(selectClauseSpec.SelectExprList, unmapContext));
            clause.IsDistinct = selectClauseSpec.IsDistinct;
            return clause;
        }

        private static IList<SelectClauseElement> UnmapSelectClauseElements(
            IList<SelectClauseElementRaw> selectExprList,
            StatementSpecUnMapContext unmapContext)
        {
            IList<SelectClauseElement> elements = new List<SelectClauseElement>();
            foreach (var raw in selectExprList) {
                if (raw is SelectClauseStreamRawSpec streamSpec) {
                    elements.Add(new SelectClauseStreamWildcard(streamSpec.StreamName, streamSpec.OptionalAsName));
                }
                else if (raw is SelectClauseElementWildcard) {
                    elements.Add(new SelectClauseWildcard());
                }
                else if (raw is SelectClauseExprRawSpec rawSpec) {
                    var expression = UnmapExpressionDeep(rawSpec.SelectExpression, unmapContext);
                    var selectExpr = new SelectClauseExpression(expression, rawSpec.OptionalAsName);
                    selectExpr.IsAnnotatedByEventFlag = rawSpec.IsEvents;
                    elements.Add(selectExpr);
                }
                else {
                    throw new IllegalStateException("Unexpected select clause element typed " + raw.GetType().Name);
                }
            }

            return elements;
        }

        private static InsertIntoClause UnmapInsertInto(
            InsertIntoDesc insertIntoDesc,
            StatementSpecUnMapContext unmapContext)
        {
            if (insertIntoDesc == null) {
                return null;
            }

            var selector = MapFromSODA(insertIntoDesc.StreamSelector);
            var precedence = UnmapExpressionDeep(insertIntoDesc.EventPrecedence, unmapContext);
            return InsertIntoClause.Create(
                insertIntoDesc.EventTypeName,
                insertIntoDesc.ColumnNames.ToArray(),
                selector,
                precedence);
        }

        private static void MapCreateContext(
            CreateContextClause createContext,
            StatementSpecRaw raw,
            StatementSpecMapContext mapContext)
        {
            if (createContext == null) {
                return;
            }

            var detail = MapCreateContextDetail(createContext.Descriptor, mapContext);

            var desc = new CreateContextDesc(createContext.ContextName, detail);
            raw.CreateContextDesc = desc;
        }

        private static ContextSpec MapCreateContextDetail(
            ContextDescriptor descriptor,
            StatementSpecMapContext mapContext)
        {
            ContextSpec detail;
            if (descriptor is ContextDescriptorInitiatedTerminated desc) {
                var init = MapCreateContextRangeCondition(desc.InitCondition, mapContext);
                var term = MapCreateContextRangeCondition(desc.TermCondition, mapContext);
                ExprNode[] distinctExpressions = null;
                if (desc.OptionalDistinctExpressions != null && desc.OptionalDistinctExpressions.Count > 0) {
                    distinctExpressions =
                        MapExpressionDeep(desc.OptionalDistinctExpressions, mapContext).ToArrayOrNull();
                }

                detail = new ContextSpecInitiatedTerminated(init, term, desc.IsOverlapping, distinctExpressions);
            }
            else if (descriptor is ContextDescriptorKeyedSegmented seg) {
                IList<ContextSpecKeyedItem> itemsdesc = new List<ContextSpecKeyedItem>();
                foreach (var item in seg.Items) {
                    var rawSpec = MapFilter(item.Filter, mapContext);
                    itemsdesc.Add(new ContextSpecKeyedItem(rawSpec, item.PropertyNames, item.StreamName));
                }

                IList<ContextSpecConditionFilter> optionalInit = null;
                if (seg.InitiationConditions != null && !seg.InitiationConditions.IsEmpty()) {
                    optionalInit = new List<ContextSpecConditionFilter>();
                    foreach (var filter in seg.InitiationConditions) {
                        optionalInit.Add(
                            (ContextSpecConditionFilter)MapCreateContextRangeCondition(filter, mapContext));
                    }
                }

                ContextSpecCondition optionalTermination = null;
                if (seg.TerminationCondition != null) {
                    optionalTermination = MapCreateContextRangeCondition(seg.TerminationCondition, mapContext);
                }

                detail = new ContextSpecKeyed(itemsdesc, optionalInit, optionalTermination);
            }
            else if (descriptor is ContextDescriptorCategory cat) {
                var rawSpec = MapFilter(cat.Filter, mapContext);
                IList<ContextSpecCategoryItem> itemsdesc = new List<ContextSpecCategoryItem>();
                foreach (var item in cat.Items) {
                    var expr = MapExpressionDeep(item.Expression, mapContext);
                    itemsdesc.Add(new ContextSpecCategoryItem(expr, item.Label));
                }

                detail = new ContextSpecCategory(itemsdesc, rawSpec);
            }
            else if (descriptor is ContextDescriptorHashSegmented hash) {
                IList<ContextSpecHashItem> itemsdesc = new List<ContextSpecHashItem>();
                foreach (var item in hash.Items) {
                    var rawSpec = MapFilter(item.Filter, mapContext);
                    var singleRowMethodExpression = (SingleRowMethodExpression)item.HashFunction;
                    var func = MapChains(
                        Collections.SingletonList(singleRowMethodExpression.Chain[0]),
                        mapContext)[0];
                    itemsdesc.Add(new ContextSpecHashItem(func, rawSpec));
                }

                detail = new ContextSpecHash(itemsdesc, hash.Granularity, hash.IsPreallocate);
            }
            else {
                var nested = (ContextDescriptorNested)descriptor;
                IList<CreateContextDesc> itemsdesc = new List<CreateContextDesc>();
                foreach (var item in nested.Contexts) {
                    itemsdesc.Add(
                        new CreateContextDesc(item.ContextName, MapCreateContextDetail(item.Descriptor, mapContext)));
                }

                detail = new ContextNested(itemsdesc);
            }

            return detail;
        }

        private static ContextSpecCondition MapCreateContextRangeCondition(
            ContextDescriptorCondition condition,
            StatementSpecMapContext mapContext)
        {
            if (condition is ContextDescriptorConditionCrontab crontab) {
                IList<IList<ExprNode>> expr = new List<IList<ExprNode>>(crontab.CrontabExpressions.Count);
                foreach (var crontabItem in crontab.CrontabExpressions) {
                    expr.Add(MapExpressionDeep(crontabItem, mapContext));
                }

                return new ContextSpecConditionCrontab(expr, crontab.IsNow);
            }
            else if (condition is ContextDescriptorConditionFilter filter) {
                var filterExpr = MapFilter(filter.Filter, mapContext);
                return new ContextSpecConditionFilter(filterExpr, filter.OptionalAsName);
            }

            if (condition is ContextDescriptorConditionPattern pattern) {
                var patternExpr = MapPatternEvalDeep(pattern.Pattern, mapContext);
                return new ContextSpecConditionPattern(patternExpr, pattern.IsInclusive, pattern.IsNow, pattern.AsName);
            }

            if (condition is ContextDescriptorConditionTimePeriod timePeriod) {
                var expr = MapExpressionDeep(timePeriod.TimePeriod, mapContext);
                return new ContextSpecConditionTimePeriod((ExprTimePeriod)expr, timePeriod.IsNow);
            }

            if (condition is ContextDescriptorConditionImmediate) {
                return ContextSpecConditionImmediate.INSTANCE;
            }

            if (condition is ContextDescriptorConditionNever) {
                return ContextSpecConditionNever.INSTANCE;
            }

            throw new IllegalStateException("Unrecognized condition " + condition);
        }

        private static void MapCreateWindow(
            CreateWindowClause createWindow,
            StatementSpecRaw raw,
            StatementSpecMapContext mapContext)
        {
            if (createWindow == null) {
                return;
            }

            ExprNode insertFromWhereExpr = null;
            if (createWindow.InsertWhereClause != null) {
                insertFromWhereExpr = MapExpressionDeep(createWindow.InsertWhereClause, mapContext);
            }

            var columns = MapColumns(createWindow.Columns);
            raw.CreateWindowDesc = new CreateWindowDesc(
                createWindow.WindowName,
                MapViews(createWindow.Views, mapContext),
                new StreamSpecOptions(false, createWindow.IsRetainUnion, false),
                createWindow.IsInsert,
                insertFromWhereExpr,
                columns,
                createWindow.AsEventTypeName);
        }

        private static void MapCreateIndex(
            CreateIndexClause clause,
            StatementSpecRaw raw,
            StatementSpecMapContext mapContext)
        {
            if (clause == null) {
                return;
            }

            IList<CreateIndexItem> cols = new List<CreateIndexItem>();
            foreach (var col in clause.Columns) {
                var item = MapCreateIndexCol(col, mapContext);
                cols.Add(item);
            }

            var desc = new CreateIndexDesc(clause.IsUnique, clause.IndexName, clause.WindowName, cols);
            raw.CreateIndexDesc = desc;
        }

        private static CreateIndexItem MapCreateIndexCol(
            CreateIndexColumn col,
            StatementSpecMapContext mapContext)
        {
            var columns = MapExpressionDeep(col.Columns, mapContext);
            var parameters = MapExpressionDeep(col.Parameters, mapContext);
            return new CreateIndexItem(
                columns,
                col.IndexType ?? CreateIndexType.HASH.GetName().ToLowerInvariant(),
                parameters);
        }

        private static void MapUpdateClause(
            UpdateClause updateClause,
            StatementSpecRaw raw,
            StatementSpecMapContext mapContext)
        {
            if (updateClause == null) {
                return;
            }

            IList<OnTriggerSetAssignment> assignments = new List<OnTriggerSetAssignment>();
            foreach (var pair in updateClause.Assignments) {
                var expr = MapExpressionDeep(pair.Value, mapContext);
                assignments.Add(new OnTriggerSetAssignment(expr));
            }

            ExprNode whereClause = null;
            if (updateClause.OptionalWhereClause != null) {
                whereClause = MapExpressionDeep(updateClause.OptionalWhereClause, mapContext);
            }

            var desc = new UpdateDesc(updateClause.OptionalAsClauseStreamName, assignments, whereClause);
            raw.UpdateDesc = desc;
            var filterSpecRaw = new FilterSpecRaw(updateClause.EventType, EmptyList<ExprNode>.Instance, null);
            raw.StreamSpecs.Add(
                new FilterStreamSpecRaw(filterSpecRaw, ViewSpec.EMPTY_VIEWSPEC_ARRAY, null, StreamSpecOptions.DEFAULT));
        }

        private static void MapCreateVariable(
            CreateVariableClause createVariable,
            StatementSpecRaw raw,
            StatementSpecMapContext mapContext)
        {
            if (createVariable == null) {
                return;
            }

            ExprNode assignment = null;
            if (createVariable.OptionalAssignment != null) {
                assignment = MapExpressionDeep(createVariable.OptionalAssignment, mapContext);
            }

            var type = ClassDescriptor.ParseTypeText(createVariable.VariableType);
            raw.CreateVariableDesc = new CreateVariableDesc(
                type,
                createVariable.VariableName,
                assignment,
                createVariable.IsConstant);
        }

        private static void MapCreateTable(
            CreateTableClause createTable,
            StatementSpecRaw raw,
            StatementSpecMapContext mapContext)
        {
            if (createTable == null) {
                return;
            }

            IList<spec.CreateTableColumn> cols = new List<spec.CreateTableColumn>();
            foreach (var desc in createTable.Columns) {
                var optNode = desc.OptionalExpression != null
                    ? MapExpressionDeep(desc.OptionalExpression, mapContext)
                    : null;
                var annotations = MapAnnotations(desc.Annotations);
                var ident = desc.OptionalTypeName == null
                    ? null
                    : ClassDescriptor.ParseTypeText(desc.OptionalTypeName);
                cols.Add(new spec.CreateTableColumn(desc.ColumnName, optNode, ident, annotations, desc.PrimaryKey));
            }

            var agg = new CreateTableDesc(createTable.TableName, cols);
            raw.CreateTableDesc = agg;
        }

        private static void MapCreateSchema(
            CreateSchemaClause clause,
            StatementSpecRaw raw,
            StatementSpecMapContext mapContext)
        {
            if (clause == null) {
                return;
            }

            var desc = MapCreateSchemaInternal(clause);
            raw.CreateSchemaDesc = desc;
        }

        private static void MapCreateExpression(
            CreateExpressionClause clause,
            StatementSpecRaw raw,
            StatementSpecMapContext mapContext)
        {
            if (clause == null) {
                return;
            }

            CreateExpressionDesc desc;
            if (clause.ExpressionDeclaration != null) {
                var item = MapExpressionDeclItem(clause.ExpressionDeclaration, mapContext);
                desc = new CreateExpressionDesc(item);
            }
            else {
                var item = MapScriptExpression(clause.ScriptExpression, mapContext);
                desc = new CreateExpressionDesc(item);
            }

            raw.CreateExpressionDesc = desc;
        }

        private static void MapCreateClass(
            CreateClassClause clause,
            StatementSpecRaw raw,
            StatementSpecMapContext mapContext)
        {
            if (clause == null) {
                return;
            }

            raw.CreateClassProvided = clause.ClassProvidedExpression.ClassText;
        }

        private static CreateSchemaDesc MapCreateSchemaInternal(CreateSchemaClause clause)
        {
            var columns = MapColumns(clause.Columns);
            return new CreateSchemaDesc(
                clause.SchemaName,
                clause.Types,
                columns,
                clause.Inherits,
                AssignedTypeExtensions.MapFrom(clause.TypeDefinition),
                clause.StartTimestampPropertyName,
                clause.EndTimestampPropertyName,
                clause.CopyFrom);
        }

        private static void MapCreateGraph(
            CreateDataFlowClause clause,
            StatementSpecRaw raw,
            StatementSpecMapContext mapContext)
        {
            if (clause == null) {
                return;
            }

            IList<CreateSchemaDesc> schemas = new List<CreateSchemaDesc>();
            foreach (var schema in clause.Schemas) {
                schemas.Add(MapCreateSchemaInternal(schema));
            }

            IList<GraphOperatorSpec> ops = new List<GraphOperatorSpec>();
            foreach (var op in clause.Operators) {
                ops.Add(MapGraphOperator(op, mapContext));
            }

            var desc = new CreateDataFlowDesc(clause.DataFlowName, ops, schemas);
            raw.CreateDataFlowDesc = desc;
        }

        private static GraphOperatorSpec MapGraphOperator(
            DataFlowOperator op,
            StatementSpecMapContext mapContext)
        {
            var annotations = MapAnnotations(op.Annotations);

            var input = new GraphOperatorInput();
            foreach (var @in in op.Input) {
                input.StreamNamesAndAliases.Add(
                    new GraphOperatorInputNamesAlias(@in.InputStreamNames.ToArray(), @in.OptionalAsName));
            }

            var output = new GraphOperatorOutput();
            foreach (var @out in op.Output) {
                output.Items.Add(new GraphOperatorOutputItem(@out.StreamName, MapGraphOpType(@out.TypeInfo)));
            }

            IDictionary<string, object> detail = new LinkedHashMap<string, object>();
            foreach (var entry in op.Parameters) {
                var value = entry.ParameterValue;
                if (value is EPStatementObjectModel model) {
                    value = Map(model, mapContext);
                }
                else if (value is Expression expression) {
                    value = MapExpressionDeep(expression, mapContext);
                }

                detail.Put(entry.ParameterName, value);
            }

            return new GraphOperatorSpec(op.OperatorName, input, output, new GraphOperatorDetail(detail), annotations);
        }

        private static IList<GraphOperatorOutputItemType> MapGraphOpType(IList<DataFlowOperatorOutputType> typeInfos)
        {
            if (typeInfos == null) {
                return EmptyList<GraphOperatorOutputItemType>.Instance;
            }

            IList<GraphOperatorOutputItemType> types = new List<GraphOperatorOutputItemType>();
            foreach (var info in typeInfos) {
                var type = new GraphOperatorOutputItemType(
                    info.IsWildcard,
                    info.TypeOrClassname,
                    MapGraphOpType(info.TypeParameters));
                types.Add(type);
            }

            return types;
        }

        private static IList<ColumnDesc> MapColumns(IList<SchemaColumnDesc> columns)
        {
            if (columns == null) {
                return null;
            }

            IList<ColumnDesc> result = new List<ColumnDesc>();
            foreach (var col in columns) {
                result.Add(new ColumnDesc(col.Name, col.Type));
            }

            return result;
        }

        private static IList<SchemaColumnDesc> UnmapColumns(IList<ColumnDesc> columns)
        {
            if (columns == null) {
                return null;
            }

            IList<SchemaColumnDesc> result = new List<SchemaColumnDesc>();
            foreach (var col in columns) {
                result.Add(new SchemaColumnDesc(col.Name, col.Type));
            }

            return result;
        }

        private static InsertIntoDesc MapInsertInto(
            InsertIntoClause insertInto,
            StatementSpecMapContext mapContext)
        {
            if (insertInto == null) {
                return null;
            }

            var eventTypeName = insertInto.StreamName;
            var desc = new InsertIntoDesc(MapFromSODA(insertInto.StreamSelector), eventTypeName);

            foreach (var name in insertInto.ColumnNames) {
                desc.Add(name);
            }

            desc.EventPrecedence = MapExpressionDeep(insertInto.EventPrecedence, mapContext);
            return desc;
        }

        private static void MapSelect(
            SelectClause selectClause,
            StatementSpecRaw raw,
            StatementSpecMapContext mapContext)
        {
            if (selectClause == null) {
                return;
            }

            var spec = MapSelectRaw(selectClause, mapContext);
            raw.SelectStreamDirEnum = MapFromSODA(selectClause.StreamSelector);
            raw.SelectClauseSpec = spec;
        }

        private static IList<SelectClauseElementRaw> MapSelectClauseElements(
            IList<SelectClauseElement> elements,
            StatementSpecMapContext mapContext)
        {
            IList<SelectClauseElementRaw> result = new List<SelectClauseElementRaw>();
            foreach (var element in elements) {
                if (element is SelectClauseWildcard) {
                    result.Add(new SelectClauseElementWildcard());
                }
                else if (element is SelectClauseExpression selectExpr) {
                    var expr = selectExpr.Expression;
                    var exprNode = MapExpressionDeep(expr, mapContext);
                    var rawElement = new SelectClauseExprRawSpec(
                        exprNode,
                        selectExpr.AsName,
                        selectExpr.IsAnnotatedByEventFlag);
                    result.Add(rawElement);
                }
                else if (element is SelectClauseStreamWildcard streamWild) {
                    var rawElement = new SelectClauseStreamRawSpec(
                        streamWild.StreamName,
                        streamWild.OptionalColumnName);
                    result.Add(rawElement);
                }
            }

            return result;
        }

        private static SelectClauseSpecRaw MapSelectRaw(
            SelectClause selectClause,
            StatementSpecMapContext mapContext)
        {
            var spec = new SelectClauseSpecRaw();
            spec.AddAll(MapSelectClauseElements(selectClause.SelectList, mapContext));
            spec.IsDistinct = selectClause.IsDistinct;
            return spec;
        }

        private static Expression UnmapExpressionDeep(
            ExprNode exprNode,
            StatementSpecUnMapContext unmapContext)
        {
            if (exprNode == null) {
                return null;
            }

            var parent = UnmapExpressionFlat(exprNode, unmapContext);
            UnmapExpressionRecursive(parent, exprNode, unmapContext);
            return parent;
        }

        private static IList<ExprNode> MapExpressionDeep(
            IList<Expression> expressions,
            StatementSpecMapContext mapContext)
        {
            IList<ExprNode> result = new List<ExprNode>();
            if (expressions == null) {
                return result;
            }

            foreach (var expr in expressions) {
                if (expr == null) {
                    result.Add(null);
                    continue;
                }

                result.Add(MapExpressionDeep(expr, mapContext));
            }

            return result;
        }

        private static MatchRecognizeRegEx UnmapExpressionDeepRowRecog(
            RowRecogExprNode exprNode,
            StatementSpecUnMapContext unmapContext)
        {
            var parent = UnmapExpressionFlatRowRecog(exprNode, unmapContext);
            UnmapExpressionRecursiveRowRecog(parent, exprNode, unmapContext);
            return parent;
        }

        private static ExprNode MapExpressionDeep(
            Expression expr,
            StatementSpecMapContext mapContext)
        {
            if (expr == null) {
                return null;
            }

            var parent = MapExpressionFlat(expr, mapContext);
            MapExpressionRecursive(parent, expr, mapContext);
            return parent;
        }

        private static RowRecogExprNode MapExpressionDeepRowRecog(
            MatchRecognizeRegEx expr,
            StatementSpecMapContext mapContext)
        {
            var parent = MapExpressionFlatRowRecog(expr, mapContext);
            MapExpressionRecursiveRowRecog(parent, expr, mapContext);
            return parent;
        }

        private static ExprNode MapExpressionFlat(
            Expression expr,
            StatementSpecMapContext mapContext)
        {
            if (expr == null) {
                throw new ArgumentException("Null expression parameter");
            }

            if (expr is ArithmaticExpression arith) {
                return new ExprMathNode(
                    MathArithTypeEnumExtensions.ParseOperator(arith.Operator),
                    mapContext.Configuration.Compiler.Expression.IsIntegerDivision,
                    mapContext.Configuration.Compiler.Expression.IsDivisionByZeroReturnsNull);
            }
            else if (expr is PropertyValueExpression prop) {
                var indexDot = StringValue.UnescapedIndexOfDot(prop.PropertyName);

                // handle without nesting
                if (indexDot == -1) {
                    // properties and also indexed properties can be either just a property of may reference a table
                    var tableAccessNode = TableCompileTimeUtil.MapPropertyToTableUnnested(
                        prop.PropertyName,
                        mapContext.TableCompileTimeResolver);
                    if (tableAccessNode != null) {
                        mapContext.TableExpressions.Add(tableAccessNode);
                        return tableAccessNode;
                    }

                    // maybe variable
                    var variableMetaData =
                        mapContext.VariableCompileTimeResolver.Resolve(prop.PropertyName);
                    if (variableMetaData != null) {
                        var node = new ExprVariableNodeImpl(variableMetaData, null);
                        mapContext.VariableNames.Add(variableMetaData.VariableName);
                        var message = VariableUtil.CheckVariableContextName(
                            mapContext.ContextName,
                            variableMetaData);
                        if (message != null) {
                            throw new EPException(message);
                        }

                        return node;
                    }

                    return new ExprIdentNodeImpl(prop.PropertyName);
                }

                var stream = prop.PropertyName.Substring(0, indexDot);
                var property = prop.PropertyName.Substring(indexDot + 1);

                var tableNode = TableCompileTimeUtil.MapPropertyToTableNested(
                    mapContext.TableCompileTimeResolver,
                    stream,
                    property);
                if (tableNode != null) {
                    mapContext.TableExpressions.Add(tableNode.First);
                    return tableNode.First;
                }

                var variableMetaDataX = mapContext.VariableCompileTimeResolver.Resolve(stream);
                if (variableMetaDataX != null) {
                    var node = new ExprVariableNodeImpl(variableMetaDataX, property);
                    mapContext.VariableNames.Add(variableMetaDataX.VariableName);
                    var message = VariableUtil.CheckVariableContextName(mapContext.ContextName, variableMetaDataX);
                    if (message != null) {
                        throw new EPException(message);
                    }

                    return node;
                }

                if (mapContext.ContextName != null) {
                    var contextDescriptor = mapContext.ContextCompileTimeDescriptor;
                    if (contextDescriptor != null &&
                        contextDescriptor.ContextPropertyRegistry.IsContextPropertyPrefix(stream)) {
                        return new ExprContextPropertyNodeImpl(property);
                    }
                }

                return new ExprIdentNodeImpl(property, stream);
            }
            else if (expr is Conjunction) {
                return new ExprAndNodeImpl();
            }
            else if (expr is Disjunction) {
                return new ExprOrNode();
            }
            else if (expr is RelationalOpExpression expression) {
                if (expression.Operator.Equals("=")) {
                    return new ExprEqualsNodeImpl(false, false);
                }
                else if (expression.Operator.Equals("!=")) {
                    return new ExprEqualsNodeImpl(true, false);
                }
                else if (expression.Operator.ToUpperInvariant().Trim().Equals("IS")) {
                    return new ExprEqualsNodeImpl(false, true);
                }
                else if (expression.Operator.ToUpperInvariant().Trim().Equals("IS NOT")) {
                    return new ExprEqualsNodeImpl(true, true);
                }
                else {
                    return new ExprRelationalOpNodeImpl(RelationalOpEnumExtensions.Parse(expression.Operator));
                }
            }
            else if (expr is ConstantExpression op) {
                Type constantType = null;
                if (op.ConstantType != null) {
                    try {
                        constantType = mapContext.ImportService.TypeResolver.ResolveType(op.ConstantType);
                    }
                    catch (TypeLoadException) {
                        constantType = TypeHelper.GetPrimitiveTypeForName(op.ConstantType);
                        if (constantType == null) {
                            throw new EPException(
                                "Error looking up class name '" + op.ConstantType + "' to resolve as constant type");
                        }
                    }
                }

                if (!CodegenExpressionUtil.CanRenderConstant(op.Constant)) {
                    throw new EPException(
                        "Invalid constant of type '" +
                        op.Constant.GetType().CleanName() +
                        "' encountered as the class has no compiler representation, please use substitution parameters instead");
                }

                return new ExprConstantNodeImpl(op.Constant, constantType);
            }
            else if (expr is ConcatExpression) {
                return new ExprConcatNode();
            }
            else if (expr is SubqueryExpression subqueryExpression) {
                var rawSubselect = Map(subqueryExpression.Model, mapContext);
                return new ExprSubselectRowNode(rawSubselect);
            }
            else if (expr is SubqueryInExpression inExpression) {
                var rawSubselect = Map(inExpression.Model, mapContext);
                return new ExprSubselectInNode(rawSubselect, inExpression.IsNotIn);
            }
            else if (expr is SubqueryExistsExpression existsExpression) {
                var rawSubselect = Map(existsExpression.Model, mapContext);
                return new ExprSubselectExistsNode(rawSubselect);
            }
            else if (expr is SubqueryQualifiedExpression sub) {
                var rawSubselect = Map(sub.Model, mapContext);
                var isNot = false;
                                RelationalOpEnum? relop = null;
                
                if (sub.Operator == "!=") {
                    isNot = true;
                } else if (sub.Operator != "=") {
                    relop = RelationalOpEnumExtensions.Parse(sub.Operator);
                }

                return new ExprSubselectAllSomeAnyNode(rawSubselect, isNot, sub.IsAll, relop);
            }
            else if (expr is CountStarProjectionExpression) {
                return new ExprCountNode(false);
            }
            else if (expr is CountProjectionExpression count) {
                return new ExprCountNode(count.IsDistinct);
            }
            else if (expr is AvgProjectionExpression projectionExpression) {
                return new ExprAvgNode(projectionExpression.IsDistinct);
            }
            else if (expr is SumProjectionExpression avg) {
                return new ExprSumNode(avg.IsDistinct);
            }
            else if (expr is BetweenExpression between) {
                return new ExprBetweenNodeImpl(
                    between.IsLowEndpointIncluded,
                    between.IsHighEndpointIncluded,
                    between.IsNotBetween);
            }
            else if (expr is PriorExpression) {
                mapContext.HasPriorExpression = true;
                return new ExprPriorNode();
            }
            else if (expr is PreviousExpression prev) {
                return new ExprPreviousNode(prev.Type.Xlate<ExprPreviousNodePreviousType>());
            }
            else if (expr is StaticMethodExpression methodExpression) {
                var chained = MapChains(methodExpression.Chain, mapContext);
                chained.Insert(0, new ChainableCall(methodExpression.ClassName, EmptyList<ExprNode>.Instance));
                return new ExprDotNodeImpl(
                    chained,
                    mapContext.Configuration.Compiler.Expression.IsDuckTyping,
                    mapContext.Configuration.Compiler.Expression.IsUdfCache);
            }
            else if (expr is MinProjectionExpression minProjectionExpression) {
                return new ExprMinMaxAggrNode(
                    minProjectionExpression.IsDistinct,
                    MinMaxTypeEnum.MIN,
                    expr.Children.Count > 1,
                    minProjectionExpression.IsEver);
            }
            else if (expr is MaxProjectionExpression method) {
                return new ExprMinMaxAggrNode(
                    method.IsDistinct,
                    MinMaxTypeEnum.MAX,
                    expr.Children.Count > 1,
                    method.IsEver);
            }
            else if (expr is NotExpression) {
                return new ExprNotNode();
            }
            else if (expr is InExpression inExpr) {
                return new ExprInNodeImpl(inExpr.IsNotIn);
            }
            else if (expr is CoalesceExpression) {
                return new ExprCoalesceNode();
            }
            else if (expr is CaseWhenThenExpression) {
                return new ExprCaseNode(false);
            }
            else if (expr is CaseSwitchExpression) {
                return new ExprCaseNode(true);
            }
            else if (expr is MaxRowExpression) {
                return new ExprMinMaxRowNode(MinMaxTypeEnum.MAX);
            }
            else if (expr is MinRowExpression) {
                return new ExprMinMaxRowNode(MinMaxTypeEnum.MIN);
            }
            else if (expr is BitwiseOpExpression bit) {
                return new ExprBitWiseNode(bit.BinaryOp);
            }
            else if (expr is ArrayExpression) {
                return new ExprArrayNode();
            }
            else if (expr is LikeExpression like) {
                return new ExprLikeNode(like.IsNot);
            }
            else if (expr is RegExpExpression regexp) {
                return new ExprRegexpNode(regexp.IsNot);
            }
            else if (expr is MedianProjectionExpression median) {
                return new ExprMedianNode(median.IsDistinct);
            }
            else if (expr is AvedevProjectionExpression avedevProjectionExpression) {
                return new ExprAvedevNode(avedevProjectionExpression.IsDistinct);
            }
            else if (expr is StddevProjectionExpression stddevProjectionExpression) {
                return new ExprStddevNode(stddevProjectionExpression.IsDistinct);
            }
            else if (expr is LastEverProjectionExpression everProjectionExpression) {
                return new ExprFirstLastEverNode(everProjectionExpression.IsDistinct, false);
            }
            else if (expr is FirstEverProjectionExpression firstEverProjectionExpression) {
                return new ExprFirstLastEverNode(firstEverProjectionExpression.IsDistinct, true);
            }
            else if (expr is CountEverProjectionExpression countEverProjectionExpression) {
                return new ExprCountEverNode(countEverProjectionExpression.IsDistinct);
            }
            else if (expr is InstanceOfExpression ofExpression) {
                return new ExprInstanceofNode(ofExpression.TypeNames);
            }
            else if (expr is TypeOfExpression) {
                return new ExprTypeofNode();
            }
            else if (expr is CastExpression castExpression) {
                return new ExprCastNode(ClassDescriptor.ParseTypeText(castExpression.TypeName));
            }
            else if (expr is PropertyExistsExpression) {
                return new ExprPropertyExistsNode();
            }
            else if (expr is CurrentTimestampExpression) {
                return new ExprTimestampNode();
            }
            else if (expr is CurrentEvaluationContextExpression) {
                return new ExprCurrentEvaluationContextNode();
            }
            else if (expr is IStreamBuiltinExpression) {
                return new ExprIStreamNode();
            }
            else if (expr is TimePeriodExpression tpe) {
                return new ExprTimePeriodImpl(
                    tpe.HasDays,
                    tpe.HasMonths,
                    tpe.HasWeeks,
                    tpe.HasDays,
                    tpe.HasHours,
                    tpe.HasMinutes,
                    tpe.HasSeconds,
                    tpe.HasMilliseconds,
                    tpe.HasMicroseconds,
                    mapContext.ImportService.TimeAbacus);
            }
            else if (expr is NewOperatorExpression operatorExpression) {
                return new ExprNewStructNode(operatorExpression.ColumnNames.ToArray());
            }
            else if (expr is NewInstanceOperatorExpression noe) {
                var type = ClassDescriptor.ParseTypeText(noe.ClassName);
                return new ExprNewInstanceNode(type, noe.NumArrayDimensions);
            }
            else if (expr is CompareListExpression exp) {
                if (exp.Operator.Equals("=") || exp.Operator.Equals("!=")) {
                    return new ExprEqualsAllAnyNode(exp.Operator.Equals("!="), exp.IsAll);
                }
                else {
                    return new ExprRelationalOpAllAnyNode(RelationalOpEnumExtensions.Parse(exp.Operator), exp.IsAll);
                }
            }
            else if (expr is SubstitutionParameterExpression parameterExpression) {
                var ident = parameterExpression.OptionalType == null
                    ? null
                    : ClassDescriptor.ParseTypeText(parameterExpression.OptionalType);
                var substitutionNode = new ExprSubstitutionNode(
                    parameterExpression.OptionalName,
                    ident);
                mapContext.SubstitutionNodes.Add(substitutionNode);
                return substitutionNode;
            }
            else if (expr is SingleRowMethodExpression single) {
                if (single.Chain == null || single.Chain.Count == 0) {
                    throw new ArgumentException("Single row method expression requires one or more method calls");
                }

                var chain = MapChains(single.Chain, mapContext);
                var call = (ChainableCall)chain[0];
                var functionName = call.Name;

                Pair<Type, ImportSingleRowDesc> pair;
                try {
                    pair = mapContext.ImportService.ResolveSingleRow(functionName, mapContext.ClassProvidedExtension);
                }
                catch (Exception e) {
                    throw new ArgumentException(
                        "Function name '" +
                        functionName +
                        "' cannot be resolved to a single-row function: " +
                        e.Message,
                        e);
                }

                call.Name = pair.Second.MethodName;
                return new ExprPlugInSingleRowNode(functionName, pair.First, chain, pair.Second);
            }
            else if (expr is PlugInProjectionExpression node) {
                var exprNode = ASTAggregationHelper.TryResolveAsAggregation(
                    mapContext.ImportService,
                    node.IsDistinct,
                    node.FunctionName,
                    mapContext.PlugInAggregations,
                    mapContext.ClassProvidedExtension);
                if (exprNode == null) {
                    throw new EPException("Error resolving aggregation function named '" + node.FunctionName + "'");
                }

                return exprNode;
            }
            else if (expr is OrderedObjectParamExpression order) {
                return new ExprOrderedExpr(order.IsDescending);
            }
            else if (expr is CrontabFrequencyExpression) {
                return new ExprNumberSetFrequency();
            }
            else if (expr is CrontabRangeExpression) {
                return new ExprNumberSetRange();
            }
            else if (expr is CrontabParameterSetExpression) {
                return new ExprNumberSetList();
            }
            else if (expr is CrontabParameterExpression cronParam) {
                if (cronParam.Type == ScheduleItemType.WILDCARD) {
                    return new ExprWildcardImpl();
                }

                CronOperatorEnum @operator;
                if (cronParam.Type == ScheduleItemType.LASTDAY) {
                    @operator = CronOperatorEnum.LASTDAY;
                }
                else if (cronParam.Type == ScheduleItemType.WEEKDAY) {
                    @operator = CronOperatorEnum.WEEKDAY;
                }
                else if (cronParam.Type == ScheduleItemType.LASTWEEKDAY) {
                    @operator = CronOperatorEnum.LASTWEEKDAY;
                }
                else {
                    throw new ArgumentException("Cron parameter not recognized: " + cronParam.Type);
                }

                return new ExprNumberSetCronParam(@operator);
            }
            else if (expr is AccessProjectionExpressionBase) {
                AggregationAccessorLinearType type;
                if (expr is FirstProjectionExpression) {
                    type = AggregationAccessorLinearType.FIRST;
                }
                else if (expr is LastProjectionExpression) {
                    type = AggregationAccessorLinearType.LAST;
                }
                else {
                    type = AggregationAccessorLinearType.WINDOW;
                }

                return new ExprAggMultiFunctionLinearAccessNode(type);
            }
            else if (expr is DotExpression @base) {
                // the first chain element may itself be nested:
                //   chain.get(0)="table.a" (looks like a class name)
                //   chain.get(1)="doit()"
                var chain = MapChains(@base.Chain, mapContext);
                if (!chain.IsEmpty() && Chainable.IsPlainPropertyChain(chain[0])) {
                    var elementized = Chainable.ChainForDot(chain[0]);
                    elementized.AddAll(chain.SubList(1, chain.Count));
                    chain = elementized;
                }

                return ChainableWalkHelper.ProcessDot(true, expr.Children.IsEmpty(), chain, mapContext);
            }
            else if (expr is LambdaExpression theBase) {
                return new ExprLambdaGoesNode(new List<string>(theBase.Parameters));
            }
            else if (expr is StreamWildcardExpression sw) {
                return new ExprStreamUnderlyingNodeImpl(sw.StreamName, true);
            }
            else if (expr is GroupingExpression) {
                return new ExprGroupingNode();
            }
            else if (expr is GroupingIdExpression) {
                return new ExprGroupingIdNode();
            }
            else if (expr is TableAccessExpression accessExpression) {
                var table = mapContext.TableCompileTimeResolver.Resolve(accessExpression.TableName);
                if (table == null) {
                    throw new ArgumentException("Failed to find table by name '" + accessExpression.TableName + "'");
                }

                ExprTableAccessNode tableNode;
                var tableName = table.TableName;
                if (accessExpression.OptionalColumn != null) {
                    tableNode = new ExprTableAccessNodeSubprop(tableName, accessExpression.OptionalColumn);
                }
                else {
                    tableNode = new ExprTableAccessNodeTopLevel(tableName);
                }

                mapContext.TableExpressions.Add(tableNode);
                return tableNode;
            }
            else if (expr is WildcardExpression) {
                return new ExprWildcardImpl();
            }
            else if (expr is NamedParameterExpression named) {
                return new ExprNamedParameterNodeImpl(named.Name);
            }

            throw new ArgumentException("Could not map expression node of type " + expr.GetType().Name);
        }

        private static IList<Expression> UnmapExpressionDeep(
            ExprNode[] expressions,
            StatementSpecUnMapContext unmapContext)
        {
            return UnmapExpressionDeep(Arrays.AsList(expressions), unmapContext);
        }

        private static IList<Expression> UnmapExpressionDeep(
            IList<ExprNode> expressions,
            StatementSpecUnMapContext unmapContext)
        {
            IList<Expression> result = new List<Expression>();
            if (expressions == null) {
                return result;
            }

            foreach (var expr in expressions) {
                if (expr == null) {
                    result.Add(null);
                    continue;
                }

                result.Add(UnmapExpressionDeep(expr, unmapContext));
            }

            return result;
        }

        private static MatchRecognizeRegEx UnmapExpressionFlatRowRecog(
            RowRecogExprNode expr,
            StatementSpecUnMapContext unmapContext)
        {
            if (expr is RowRecogExprNodeAlteration) {
                return new MatchRecognizeRegExAlteration();
            }
            else if (expr is RowRecogExprNodeAtom atom) {
                var repeat = UnmapRowRecogRepeat(atom.OptionalRepeat, unmapContext);
                return new MatchRecognizeRegExAtom(
                    atom.Tag,
                    atom.Type.Xlate<MatchRecogizePatternElementType>(),
                    repeat);
            }
            else if (expr is RowRecogExprNodeConcatenation) {
                return new MatchRecognizeRegExConcatenation();
            }
            else if (expr is RowRecogExprNodePermute) {
                return new MatchRecognizeRegExPermutation();
            }
            else {
                var nested = (RowRecogExprNodeNested)expr;
                var repeat = UnmapRowRecogRepeat(nested.OptionalRepeat, unmapContext);
                return new MatchRecognizeRegExNested(
                    nested.Type.Xlate<MatchRecogizePatternElementType>(),
                    repeat);
            }
        }

        private static MatchRecognizeRegExRepeat UnmapRowRecogRepeat(
            RowRecogExprRepeatDesc optionalRepeat,
            StatementSpecUnMapContext unmapContext)
        {
            if (optionalRepeat == null) {
                return null;
            }

            return new MatchRecognizeRegExRepeat(
                UnmapExpressionDeep(optionalRepeat.Lower, unmapContext),
                UnmapExpressionDeep(optionalRepeat.Upper, unmapContext),
                UnmapExpressionDeep(optionalRepeat.Single, unmapContext)
            );
        }

        private static RowRecogExprNode MapExpressionFlatRowRecog(
            MatchRecognizeRegEx expr,
            StatementSpecMapContext mapContext)
        {
            if (expr is MatchRecognizeRegExAlteration) {
                return new RowRecogExprNodeAlteration();
            }
            else if (expr is MatchRecognizeRegExAtom atom) {
                var repeat = MapRowRecogRepeat(atom.OptionalRepeat, mapContext);
                return new RowRecogExprNodeAtom(atom.Name, atom.Type.Xlate<RowRecogNFATypeEnum>(), repeat);
            }
            else if (expr is MatchRecognizeRegExConcatenation) {
                return new RowRecogExprNodeConcatenation();
            }
            else if (expr is MatchRecognizeRegExPermutation) {
                return new RowRecogExprNodePermute();
            }
            else {
                var nested = (MatchRecognizeRegExNested)expr;
                var repeat = MapRowRecogRepeat(nested.OptionalRepeat, mapContext);
                return new RowRecogExprNodeNested(nested.Type.Xlate<RowRecogNFATypeEnum>(), repeat);
            }
        }

        private static RowRecogExprRepeatDesc MapRowRecogRepeat(
            MatchRecognizeRegExRepeat optionalRepeat,
            StatementSpecMapContext mapContext)
        {
            if (optionalRepeat == null) {
                return null;
            }

            return new RowRecogExprRepeatDesc(
                MapExpressionDeep(optionalRepeat.Low, mapContext),
                MapExpressionDeep(optionalRepeat.High, mapContext),
                MapExpressionDeep(optionalRepeat.Single, mapContext)
            );
        }

        private static Expression UnmapExpressionFlat(
            ExprNode expr,
            StatementSpecUnMapContext unmapContext)
        {
            if (expr is ExprIdentNode identNode) {
                var propertyName = identNode.UnresolvedPropertyName;
                if (identNode.StreamOrPropertyName != null) {
                    propertyName = identNode.StreamOrPropertyName + "." + identNode.UnresolvedPropertyName;
                }

                return new PropertyValueExpression(propertyName);
            }
            else if (expr is ExprMathNode math) {
                return new ArithmaticExpression(math.MathArithTypeEnum.GetExpressionText());
            }
            else if (expr is ExprVariableNode variableNode) {
                var propertyName = variableNode.VariableNameWithSubProp;
                return new PropertyValueExpression(propertyName);
            }
            else if (expr is ExprContextPropertyNodeImpl prop) {
                return new PropertyValueExpression(ContextPropertyRegistry.CONTEXT_PREFIX + "." + prop.PropertyName);
            }
            else if (expr is ExprEqualsNode equals) {
                string @operator;
                if (!equals.IsIs) {
                    @operator = "=";
                    if (equals.IsNotEquals) {
                        @operator = "!=";
                    }
                }
                else {
                    @operator = "is";
                    if (equals.IsNotEquals) {
                        @operator = "is not";
                    }
                }

                return new RelationalOpExpression(@operator);
            }
            else if (expr is ExprRelationalOpNode rel) {
                return new RelationalOpExpression(rel.RelationalOpEnum.GetExpressionText());
            }
            else if (expr is ExprAndNode) {
                return new Conjunction();
            }
            else if (expr is ExprOrNode) {
                return new Disjunction();
            }
            else if (expr is ExprConstantNodeImpl constNode) {
                string constantType = null;
                if (constNode.ConstantType != null) {
                    constantType = constNode.ConstantType.Name;
                }

                return new ConstantExpression(constNode.ConstantValue, constantType);
            }
            else if (expr is ExprConcatNode) {
                return new ConcatExpression();
            }
            else if (expr is ExprSubselectRowNode rowNode) {
                var unmapped = Unmap(rowNode.StatementSpecRaw);
                return new SubqueryExpression(unmapped.ObjectModel);
            }
            else if (expr is ExprSubselectInNode inNode) {
                var unmapped = Unmap(inNode.StatementSpecRaw);
                return new SubqueryInExpression(unmapped.ObjectModel, inNode.IsNotIn);
            }
            else if (expr is ExprSubselectExistsNode existsNode) {
                var unmapped = Unmap(existsNode.StatementSpecRaw);
                return new SubqueryExistsExpression(unmapped.ObjectModel);
            }
            else if (expr is ExprSubselectAllSomeAnyNode anyNode) {
                var unmapped = Unmap(anyNode.StatementSpecRaw);
                var @operator = "=";
                if (anyNode.IsNot) {
                    @operator = "!=";
                }

                if (anyNode.RelationalOp != null) {
                    @operator = anyNode.RelationalOp?.GetExpressionText();
                }

                return new SubqueryQualifiedExpression(unmapped.ObjectModel, @operator, anyNode.IsAll);
            }
            else if (expr is ExprCountNode countNode) {
                if (countNode.ChildNodes.Length == 0 || (countNode.ChildNodes.Length == 1 && countNode.HasFilter)) {
                    return new CountStarProjectionExpression();
                }
                else {
                    return new CountProjectionExpression(countNode.IsDistinct);
                }
            }
            else if (expr is ExprPriorNode) {
                return new PriorExpression();
            }
            else if (expr is ExprPreviousNode prev) {
                var result = new PreviousExpression();
                result.Type = prev.PreviousType.Xlate<PreviousExpressionType>();
                return result;
            }
            else if (expr is ExprSumNode sumNode) {
                return new SumProjectionExpression(sumNode.IsDistinct);
            }
            else if (expr is ExprLeavingAggNode) {
                return new PlugInProjectionExpression("leaving", false);
            }
            else if (expr is ExprRateAggNode) {
                return new PlugInProjectionExpression("rate", false);
            }
            else if (expr is ExprAvgNode avgNode) {
                return new AvgProjectionExpression(avgNode.IsDistinct);
            }
            else if (expr is ExprNthAggNode) {
                return new PlugInProjectionExpression("nth", false);
            }
            else if (expr is ExprBetweenNode between) {
                return new BetweenExpression(
                    between.IsLowEndpointIncluded,
                    between.IsHighEndpointIncluded,
                    between.IsNotBetween);
            }
            else if (expr is ExprAggMultiFunctionCountMinSketchNode cmsNode) {
                return new PlugInProjectionExpression(cmsNode.AggregationFunctionName, false);
            }
            else if (expr is ExprBitWiseNode wiseNode) {
                return new BitwiseOpExpression(wiseNode.BitWiseOpEnum);
            }
            else if (expr is ExprAggMultiFunctionSortedMinMaxByNode byNode) {
                return new PlugInProjectionExpression(byNode.AggregationFunctionName, false);
            }
            else if (expr is ExprArrayNode) {
                return new ArrayExpression();
            }
            else if (expr is ExprMinMaxAggrNode aggrNode) {
                if (aggrNode.MinMaxTypeEnum == MinMaxTypeEnum.MIN) {
                    return new MinProjectionExpression(aggrNode.IsDistinct, aggrNode.IsEver);
                }
                else {
                    return new MaxProjectionExpression(aggrNode.IsDistinct, aggrNode.IsEver);
                }
            }
            else if (expr is ExprMinMaxRowNode maxRowNode) {
                if (maxRowNode.MinMaxTypeEnum == MinMaxTypeEnum.MAX) {
                    return new MaxRowExpression();
                }

                return new MinRowExpression();
            }
            else if (expr is ExprCaseNode mycase) {
                if (mycase.IsCase2) {
                    return new CaseSwitchExpression();
                }
                else {
                    return new CaseWhenThenExpression();
                }
            }
            else if (expr is ExprCoalesceNode) {
                return new CoalesceExpression();
            }
            else if (expr is ExprLikeNode exprLikeNode) {
                return new LikeExpression(exprLikeNode.IsNot);
            }
            else if (expr is ExprNotNode) {
                return new NotExpression();
            }
            else if (expr is ExprInNode inExpr) {
                return new InExpression(inExpr.IsNotIn);
            }
            else if (expr is ExprRegexpNode exprRegexNode) {
                return new RegExpExpression(exprRegexNode.IsNot);
            }
            else if (expr is ExprTimestampNode) {
                return new CurrentTimestampExpression();
            }
            else if (expr is ExprCastNode) {
                var node = (ExprCastNode)expr;
                return new CastExpression(node.ClassIdentifierWArray.ToEPL());
            }
            else if (expr is ExprInstanceofNode instanceofNode) {
                return new InstanceOfExpression(instanceofNode.ClassIdentifiers);
            }
            else if (expr is ExprPropertyExistsNode) {
                return new PropertyExistsExpression();
            }
            else if (expr is ExprCurrentEvaluationContextNode) {
                return new CurrentEvaluationContextExpression();
            }
            else if (expr is ExprAvedevNode avedevNode) {
                return new AvedevProjectionExpression(avedevNode.IsDistinct);
            }
            else if (expr is ExprStddevNode stddevNode) {
                return new StddevProjectionExpression(stddevNode.IsDistinct);
            }
            else if (expr is ExprMedianNode median) {
                return new MedianProjectionExpression(median.IsDistinct);
            }
            else if (expr is ExprFirstLastEverNode firstlast) {
                if (firstlast.IsFirst) {
                    return new FirstEverProjectionExpression(firstlast.IsDistinct);
                }
                return new LastEverProjectionExpression(firstlast.IsDistinct);
            }
            else if (expr is ExprCountEverNode countEver) {
                return new CountEverProjectionExpression(countEver.IsDistinct);
            }
            else if (expr is ExprTypeofNode) {
                return new TypeOfExpression();
            }
            else if (expr is ExprPlugInSingleRowNode singleRowNode) {
                var chain = UnmapChains(singleRowNode.ChainSpec, unmapContext);
                if (chain[0] is DotExpressionItemCall dotExpressionItemCall) {
                    dotExpressionItemCall.Name = singleRowNode.FunctionName; // we use the actual function name
                }

                return new SingleRowMethodExpression(chain);
            }
            else if (expr is ExprPlugInAggNode aggNode) {
                return new PlugInProjectionExpression(aggNode.AggregationFunctionName, aggNode.IsDistinct);
            }
            else if (expr is ExprPlugInMultiFunctionAggNode functionAggNode) {
                return new PlugInProjectionExpression(
                    functionAggNode.AggregationFunctionName,
                    functionAggNode.IsDistinct);
            }
            else if (expr is ExprIStreamNode) {
                return new IStreamBuiltinExpression();
            }
            else if (expr is ExprSubstitutionNode substitutionNode) {
                return new SubstitutionParameterExpression(
                    substitutionNode.OptionalName,
                    substitutionNode.OptionalType?.ToEPL());
            }
            else if (expr is ExprTimePeriod period) {
                return new TimePeriodExpression(
                    period.HasYear,
                    period.HasMonth,
                    period.HasWeek,
                    period.HasDay,
                    period.HasHour,
                    period.HasMinute,
                    period.HasSecond,
                    period.HasMillisecond,
                    period.HasMicrosecond);
            }
            else if (expr is ExprWildcard) {
                return new CrontabParameterExpression(ScheduleItemType.WILDCARD);
            }
            else if (expr is ExprNewInstanceNode instanceNode) {
                return new NewInstanceOperatorExpression(
                    instanceNode.ClassIdentNoDimensions.ToEPL(),
                    instanceNode.NumArrayDimensions);
            }
            else if (expr is ExprNewStructNode newNode) {
                return new NewOperatorExpression(new List<string>(Arrays.AsList(newNode.ColumnNames)));
            }
            else if (expr is ExprNumberSetFrequency) {
                return new CrontabFrequencyExpression();
            }
            else if (expr is ExprNumberSetRange) {
                return new CrontabRangeExpression();
            }
            else if (expr is ExprNumberSetList) {
                return new CrontabParameterSetExpression();
            }
            else if (expr is ExprOrderedExpr order) {
                return new OrderedObjectParamExpression(order.IsDescending);
            }
            else if (expr is ExprEqualsAllAnyNode allAnyNode) {
                var @operator = allAnyNode.IsNot ? "!=" : "=";
                return new CompareListExpression(allAnyNode.IsAll, @operator);
            }
            else if (expr is ExprRelationalOpAllAnyNode node) {
                return new CompareListExpression(node.IsAll, node.RelationalOpEnum.GetExpressionText());
            }
            else if (expr is ExprNumberSetCronParam cronParam) {
                ScheduleItemType type;
                if (cronParam.CronOperator == CronOperatorEnum.LASTDAY) {
                    type = ScheduleItemType.LASTDAY;
                }
                else if (cronParam.CronOperator == CronOperatorEnum.LASTWEEKDAY) {
                    type = ScheduleItemType.LASTWEEKDAY;
                }
                else if (cronParam.CronOperator == CronOperatorEnum.WEEKDAY) {
                    type = ScheduleItemType.WEEKDAY;
                }
                else {
                    throw new ArgumentException("Cron parameter not recognized: " + cronParam.CronOperator);
                }

                return new CrontabParameterExpression(type);
            }
            else if (expr is ExprAggMultiFunctionLinearAccessNode accessNode) {
                AccessProjectionExpressionBase ape;
                if (accessNode.StateType == AggregationAccessorLinearType.FIRST) {
                    ape = new FirstProjectionExpression();
                }
                else if (accessNode.StateType == AggregationAccessorLinearType.WINDOW) {
                    ape = new WindowProjectionExpression();
                }
                else {
                    ape = new LastProjectionExpression();
                }

                return ape;
            }
            else if (expr is ExprDotNode dotNode) {
                var dotExpr = new DotExpression();
                foreach (var chain in dotNode.ChainSpec) {
                    dotExpr.Add(UnmapChainItem(chain, unmapContext));
                }

                return dotExpr;
            }
            else if (expr is ExprStreamUnderlyingNodeImpl streamNode) {
                return new StreamWildcardExpression(streamNode.StreamName);
            }
            else if (expr is ExprLambdaGoesNode lambdaNode) {
                return new LambdaExpression(new List<string>(lambdaNode.GoesToNames));
            }
            else if (expr is ExprDeclaredNode declNode) {
                var dotExpr = new DotExpression();
                dotExpr.Add(
                    declNode.Prototype.Name,
                    UnmapExpressionDeep(declNode.ChainParameters, unmapContext));
                return dotExpr;
            }
            else if (expr is ExprNodeScript scriptNode) {
                var dotExpr = new DotExpression();
                dotExpr.Add(scriptNode.Script.Name, UnmapExpressionDeep(scriptNode.Parameters, unmapContext));
                return dotExpr;
            }
            else if (expr is ExprGroupingNode) {
                return new GroupingExpression();
            }
            else if (expr is ExprGroupingIdNode) {
                return new GroupingIdExpression();
            }
            else if (expr is ExprNamedParameterNode named) {
                return new NamedParameterExpression(named.ParameterName);
            }
            else if (expr is ExprTableAccessNode table) {
                if (table is ExprTableAccessNodeTopLevel topLevel) {
                    return new TableAccessExpression(
                        topLevel.TableName,
                        UnmapExpressionDeep(topLevel.ChildNodes, unmapContext),
                        null);
                }

                if (table is ExprTableAccessNodeSubprop sub) {
                    if (sub.ChildNodes.Length == 0) {
                        return new PropertyValueExpression(table.TableName + "." + sub.SubpropName);
                    }
                    else {
                        return new TableAccessExpression(
                            sub.TableName,
                            UnmapExpressionDeep(sub.ChildNodes, unmapContext),
                            sub.SubpropName);
                    }
                }

                if (table is ExprTableAccessNodeKeys) {
                    var dotExpression = new DotExpression();
                    dotExpression.Add(table.TableName, EmptyList<Expression>.Instance, true);
                    dotExpression.Add("keys", Collections.GetEmptyList<Expression>());
                    return dotExpression;
                }
            }

            throw new ArgumentException("Could not map expression node of type " + expr.GetType().Name);
        }

        private static void UnmapExpressionRecursive(
            Expression parent,
            ExprNode expr,
            StatementSpecUnMapContext unmapContext)
        {
            foreach (var child in expr.ChildNodes) {
                var result = UnmapExpressionFlat(child, unmapContext);
                parent.Children.Add(result);
                UnmapExpressionRecursive(result, child, unmapContext);
            }
        }

        private static void UnmapExpressionRecursiveRowRecog(
            MatchRecognizeRegEx parent,
            RowRecogExprNode expr,
            StatementSpecUnMapContext unmapContext)
        {
            foreach (var child in expr.ChildNodes) {
                var result = UnmapExpressionFlatRowRecog(child, unmapContext);
                parent.Children.Add(result);
                UnmapExpressionRecursiveRowRecog(result, child, unmapContext);
            }
        }

        private static void MapExpressionRecursive(
            ExprNode parent,
            Expression expr,
            StatementSpecMapContext mapContext)
        {
            foreach (var child in expr.Children) {
                var result = MapExpressionFlat(child, mapContext);
                parent.AddChildNode(result);
                MapExpressionRecursive(result, child, mapContext);
            }
        }

        private static void MapExpressionRecursiveRowRecog(
            RowRecogExprNode parent,
            MatchRecognizeRegEx expr,
            StatementSpecMapContext mapContext)
        {
            foreach (var child in expr.Children) {
                var result = MapExpressionFlatRowRecog(child, mapContext);
                parent.AddChildNode(result);
                MapExpressionRecursiveRowRecog(result, child, mapContext);
            }
        }

        private static void MapFrom(
            FromClause fromClause,
            StatementSpecRaw raw,
            StatementSpecMapContext mapContext)
        {
            if (fromClause == null) {
                return;
            }

            foreach (var stream in fromClause.Streams) {
                StreamSpecRaw spec;

                var views = ViewSpec.EMPTY_VIEWSPEC_ARRAY;
                if (stream is ProjectedStream projectedStream) {
                    views = ViewSpec.ToArray(MapViews(projectedStream.Views, mapContext));
                }

                if (stream is FilterStream filterStream) {
                    var filterSpecRaw = MapFilter(filterStream.Filter, mapContext);
                    var options = MapStreamOpts(filterStream);
                    spec = new FilterStreamSpecRaw(filterSpecRaw, views, filterStream.StreamName, options);
                }
                else if (stream is SQLStream sqlStream) {
                    spec = new DBStatementStreamSpec(
                        sqlStream.StreamName,
                        views,
                        sqlStream.DatabaseName,
                        sqlStream.SqlWithSubsParams,
                        sqlStream.OptionalMetadataSQL);
                }
                else if (stream is PatternStream patternStream) {
                    var child = MapPatternEvalDeep(patternStream.Expression, mapContext);
                    var options = MapStreamOpts(patternStream);
                    var flags =
                        PatternLevelAnnotationUtil.AnnotationsToSpec(patternStream.Annotations);
                    spec = new PatternStreamSpecRaw(
                        child,
                        views,
                        patternStream.StreamName,
                        options,
                        flags.IsSuppressSameEventMatches,
                        flags.IsDiscardPartialsOnMatch);
                }
                else if (stream is MethodInvocationStream methodStream) {
                    IList<ExprNode> expressions = new List<ExprNode>();
                    foreach (var expr in methodStream.ParameterExpressions) {
                        var exprNode = MapExpressionDeep(expr, mapContext);
                        expressions.Add(exprNode);
                    }

                    if (mapContext.VariableCompileTimeResolver.Resolve(methodStream.ClassName) != null) {
                        mapContext.VariableNames.Add(methodStream.ClassName);
                    }

                    spec = new MethodStreamSpec(
                        methodStream.StreamName,
                        views,
                        "method",
                        methodStream.ClassName,
                        methodStream.MethodName,
                        expressions,
                        methodStream.OptionalEventTypeName);
                }
                else {
                    throw new ArgumentException(
                        "Could not map from stream " + stream + " to an internal representation");
                }

                raw.StreamSpecs.Add(spec);
            }

            foreach (var qualifier in fromClause.OuterJoinQualifiers) {
                ExprIdentNode left = null;
                ExprIdentNode right = null;
                ExprIdentNode[] additionalLeft = null;
                ExprIdentNode[] additionalRight = null;

                if (qualifier.Left != null) {
                    left = (ExprIdentNode)MapExpressionFlat(qualifier.Left, mapContext);
                    right = (ExprIdentNode)MapExpressionFlat(qualifier.Right, mapContext);

                    if (qualifier.AdditionalProperties.Count != 0) {
                        additionalLeft = new ExprIdentNode[qualifier.AdditionalProperties.Count];
                        additionalRight = new ExprIdentNode[qualifier.AdditionalProperties.Count];
                        var count = 0;
                        foreach (var pair in qualifier.AdditionalProperties) {
                            additionalLeft[count] = (ExprIdentNode)MapExpressionFlat(pair.Left, mapContext);
                            additionalRight[count] = (ExprIdentNode)MapExpressionFlat(pair.Right, mapContext);
                            count++;
                        }
                    }
                }

                raw.OuterJoinDescList.Add(
                    new OuterJoinDesc(qualifier.Type, left, right, additionalLeft, additionalRight));
            }
        }

        private static IList<ViewSpec> MapViews(
            IList<View> views,
            StatementSpecMapContext mapContext)
        {
            IList<ViewSpec> viewSpecs = new List<ViewSpec>();
            foreach (var view in views) {
                var viewExpressions = MapExpressionDeep(view.Parameters, mapContext);
                viewSpecs.Add(new ViewSpec(view.Namespace, view.Name, viewExpressions));
            }

            return viewSpecs;
        }

        private static IList<View> UnmapViews(
            IList<ViewSpec> viewSpecs,
            StatementSpecUnMapContext unmapContext)
        {
            IList<View> views = new List<View>();
            foreach (var viewSpec in viewSpecs) {
                var viewExpressions = UnmapExpressionDeep(viewSpec.ObjectParameters, unmapContext);
                views.Add(View.Create(viewSpec.ObjectNamespace, viewSpec.ObjectName, viewExpressions));
            }

            return views;
        }

        private static EvalForgeNode MapPatternEvalFlat(
            PatternExpr eval,
            StatementSpecMapContext mapContext)
        {
            if (eval == null) {
                throw new ArgumentException("Null expression parameter");
            }

            var attachPatternText = mapContext.IsAttachPatternText;
            if (eval is PatternAndExpr) {
                return new EvalAndForgeNode(attachPatternText);
            }
            else if (eval is PatternFilterExpr filterExpr) {
                var filterSpec = MapFilter(filterExpr.Filter, mapContext);
                return new EvalFilterForgeNode(
                    attachPatternText,
                    filterSpec,
                    filterExpr.TagName,
                    filterExpr.OptionalConsumptionLevel);
            }
            else if (eval is PatternEveryExpr) {
                return new EvalEveryForgeNode(attachPatternText);
            }
            else if (eval is PatternOrExpr) {
                return new EvalOrForgeNode(attachPatternText);
            }
            else if (eval is PatternNotExpr) {
                return new EvalNotForgeNode(attachPatternText);
            }
            else if (eval is PatternFollowedByExpr fb) {
                var maxExpr = MapExpressionDeep(fb.OptionalMaxPerSubexpression, mapContext);
                return new EvalFollowedByForgeNode(attachPatternText, maxExpr);
            }

            if (eval is PatternObserverExpr observer) {
                var expressions = MapExpressionDeep(observer.Parameters, mapContext);
                return new EvalObserverForgeNode(
                    attachPatternText,
                    new PatternObserverSpec(observer.Namespace, observer.Name, expressions));
            }
            else if (eval is PatternGuardExpr guard) {
                var expressions = MapExpressionDeep(guard.Parameters, mapContext);
                return new EvalGuardForgeNode(
                    attachPatternText,
                    new PatternGuardSpec(guard.Namespace, guard.Name, expressions));
            }
            else if (eval is PatternMatchUntilExpr until) {
                var low = until.Low != null ? MapExpressionDeep(until.Low, mapContext) : null;
                var high = until.High != null ? MapExpressionDeep(until.High, mapContext) : null;
                var single = until.Single != null ? MapExpressionDeep(until.Single, mapContext) : null;
                return new EvalMatchUntilForgeNode(attachPatternText, low, high, single);
            }
            else if (eval is PatternEveryDistinctExpr everyDist) {
                var expressions = MapExpressionDeep(everyDist.Expressions, mapContext);
                return new EvalEveryDistinctForgeNode(attachPatternText, expressions);
            }

            throw new ArgumentException("Could not map pattern expression node of type " + eval.GetType().Name);
        }

        private static PatternExpr UnmapPatternEvalFlat(
            EvalForgeNode eval,
            StatementSpecUnMapContext unmapContext)
        {
            if (eval is EvalFilterForgeNode filterNode) {
                var filter = UnmapFilter(filterNode.RawFilterSpec, unmapContext);
                var expr = new PatternFilterExpr(filter, filterNode.EventAsName);
                expr.OptionalConsumptionLevel = filterNode.ConsumptionLevel;
                return expr;
            }
            else if (eval is EvalAndForgeNode) {
                return new PatternAndExpr();
            }
            else if (eval is EvalEveryForgeNode) {
                return new PatternEveryExpr();
            }
            else if (eval is EvalOrForgeNode) {
                return new PatternOrExpr();
            }
            else if (eval is EvalNotForgeNode) {
                return new PatternNotExpr();
            }
            else if (eval is EvalFollowedByForgeNode fb) {
                var expressions = UnmapExpressionDeep(fb.OptionalMaxExpressions, unmapContext);
                return new PatternFollowedByExpr(expressions);
            }
            else if (eval is EvalObserverForgeNode observerNode) {
                var expressions = UnmapExpressionDeep(
                    observerNode.PatternObserverSpec.ObjectParameters,
                    unmapContext);
                return new PatternObserverExpr(
                    observerNode.PatternObserverSpec.ObjectNamespace,
                    observerNode.PatternObserverSpec.ObjectName,
                    expressions);
            }
            else if (eval is EvalGuardForgeNode guardNode) {
                var expressions = UnmapExpressionDeep(
                    guardNode.PatternGuardSpec.ObjectParameters,
                    unmapContext);
                return new PatternGuardExpr(
                    guardNode.PatternGuardSpec.ObjectNamespace,
                    guardNode.PatternGuardSpec.ObjectName,
                    expressions);
            }
            else if (eval is EvalMatchUntilForgeNode matchUntilNode) {
                var low = matchUntilNode.LowerBounds != null
                    ? UnmapExpressionDeep(matchUntilNode.LowerBounds, unmapContext)
                    : null;
                var high = matchUntilNode.UpperBounds != null
                    ? UnmapExpressionDeep(matchUntilNode.UpperBounds, unmapContext)
                    : null;
                var single = matchUntilNode.SingleBound != null
                    ? UnmapExpressionDeep(matchUntilNode.SingleBound, unmapContext)
                    : null;
                return new PatternMatchUntilExpr(low, high, single);
            }
            else if (eval is EvalEveryDistinctForgeNode everyDistinctNode) {
                var expressions = UnmapExpressionDeep(everyDistinctNode.Expressions, unmapContext);
                return new PatternEveryDistinctExpr(expressions);
            }

            throw new ArgumentException("Could not map pattern expression node of type " + eval.GetType().Name);
        }

        private static void UnmapPatternEvalRecursive(
            PatternExpr parent,
            EvalForgeNode eval,
            StatementSpecUnMapContext unmapContext)
        {
            foreach (var child in eval.ChildNodes) {
                var result = UnmapPatternEvalFlat(child, unmapContext);
                parent.Children.Add(result);
                UnmapPatternEvalRecursive(result, child, unmapContext);
            }
        }

        private static void MapPatternEvalRecursive(
            EvalForgeNode parent,
            PatternExpr expr,
            StatementSpecMapContext mapContext)
        {
            foreach (var child in expr.Children) {
                var result = MapPatternEvalFlat(child, mapContext);
                parent.AddChildNode(result);
                MapPatternEvalRecursive(result, child, mapContext);
            }
        }

        private static PatternExpr UnmapPatternEvalDeep(
            EvalForgeNode exprNode,
            StatementSpecUnMapContext unmapContext)
        {
            var parent = UnmapPatternEvalFlat(exprNode, unmapContext);
            UnmapPatternEvalRecursive(parent, exprNode, unmapContext);
            return parent;
        }

        private static EvalForgeNode MapPatternEvalDeep(
            PatternExpr expr,
            StatementSpecMapContext mapContext)
        {
            var parent = MapPatternEvalFlat(expr, mapContext);
            MapPatternEvalRecursive(parent, expr, mapContext);
            return parent;
        }

        private static FilterSpecRaw MapFilter(
            Filter filter,
            StatementSpecMapContext mapContext)
        {
            IList<ExprNode> expr = new List<ExprNode>();
            if (filter.FilterExpression != null) {
                var exprNode = MapExpressionDeep(filter.FilterExpression, mapContext);
                expr.Add(exprNode);
            }

            PropertyEvalSpec evalSpec = null;
            if (filter.OptionalPropertySelects != null) {
                evalSpec = MapPropertySelects(filter.OptionalPropertySelects, mapContext);
            }

            return new FilterSpecRaw(filter.EventTypeName, expr, evalSpec);
        }

        private static PropertyEvalSpec MapPropertySelects(
            IList<ContainedEventSelect> propertySelects,
            StatementSpecMapContext mapContext)
        {
            var evalSpec = new PropertyEvalSpec();
            foreach (var propertySelect in propertySelects) {
                SelectClauseSpecRaw selectSpec = null;
                if (propertySelect.SelectClause != null) {
                    selectSpec = MapSelectRaw(propertySelect.SelectClause, mapContext);
                }

                ExprNode exprNodeWhere = null;
                if (propertySelect.WhereClause != null) {
                    exprNodeWhere = MapExpressionDeep(propertySelect.WhereClause, mapContext);
                }

                ExprNode splitterExpr = null;
                if (propertySelect.SplitExpression != null) {
                    splitterExpr = MapExpressionDeep(propertySelect.SplitExpression, mapContext);
                }

                evalSpec.Add(
                    new PropertyEvalAtom(
                        splitterExpr,
                        propertySelect.OptionalSplitExpressionTypeName,
                        propertySelect.OptionalAsName,
                        selectSpec,
                        exprNodeWhere));
            }

            return evalSpec;
        }

        private static Filter UnmapFilter(
            FilterSpecRaw filter,
            StatementSpecUnMapContext unmapContext)
        {
            Expression expr = null;
            if (filter.FilterExpressions.Count > 1) {
                expr = new Conjunction();
                foreach (var exprNode in filter.FilterExpressions) {
                    var expression = UnmapExpressionDeep(exprNode, unmapContext);
                    expr.Children.Add(expression);
                }
            }
            else if (filter.FilterExpressions.Count == 1) {
                expr = UnmapExpressionDeep(filter.FilterExpressions[0], unmapContext);
            }

            var filterDef = new Filter(filter.EventTypeName, expr);

            if (filter.OptionalPropertyEvalSpec != null) {
                var propertySelects = UnmapPropertySelects(
                    filter.OptionalPropertyEvalSpec,
                    unmapContext);
                filterDef.OptionalPropertySelects = propertySelects;
            }

            return filterDef;
        }

        private static IList<ContainedEventSelect> UnmapPropertySelects(
            PropertyEvalSpec propertyEvalSpec,
            StatementSpecUnMapContext unmapContext)
        {
            IList<ContainedEventSelect> propertySelects = new List<ContainedEventSelect>();
            foreach (var atom in propertyEvalSpec.Atoms) {
                SelectClause selectClause = null;
                if (atom.OptionalSelectClause != null && !atom.OptionalSelectClause.SelectExprList.IsEmpty()) {
                    selectClause = UnmapSelect(
                        atom.OptionalSelectClause,
                        SelectClauseStreamSelectorEnum.ISTREAM_ONLY,
                        unmapContext);
                }

                Expression filterExpression = null;
                if (atom.OptionalWhereClause != null) {
                    filterExpression = UnmapExpressionDeep(atom.OptionalWhereClause, unmapContext);
                }

                var splitExpression = UnmapExpressionDeep(atom.SplitterExpression, unmapContext);

                var contained = new ContainedEventSelect(splitExpression);
                contained.OptionalSplitExpressionTypeName = atom.OptionalResultEventType;
                contained.SelectClause = selectClause;
                contained.WhereClause = filterExpression;
                contained.OptionalAsName = atom.OptionalAsName;

                if (atom.SplitterExpression != null) {
                    contained.SplitExpression = UnmapExpressionDeep(atom.SplitterExpression, unmapContext);
                }

                propertySelects.Add(contained);
            }

            return propertySelects;
        }

        private static IList<AnnotationPart> UnmapAnnotations(IList<AnnotationDesc> annotations)
        {
            IList<AnnotationPart> result = new List<AnnotationPart>();
            foreach (var desc in annotations) {
                result.Add(UnmapAnnotation(desc));
            }

            return result;
        }

        private static IList<ExpressionDeclaration> UnmapExpressionDeclarations(
            ExpressionDeclDesc expr,
            StatementSpecUnMapContext unmapContext)
        {
            if (expr == null || expr.Expressions.IsEmpty()) {
                return EmptyList<ExpressionDeclaration>.Instance;
            }

            IList<ExpressionDeclaration> result = new List<ExpressionDeclaration>();
            foreach (var desc in expr.Expressions) {
                result.Add(UnmapExpressionDeclItem(desc));
            }

            return result;
        }

        private static ExpressionDeclaration UnmapExpressionDeclItem(ExpressionDeclItem desc)
        {
            return new ExpressionDeclaration(
                desc.Name,
                new List<string>(Arrays.AsList(desc.ParametersNames)),
                desc.OptionalSoda,
                desc.IsAlias);
        }

        private static IList<ScriptExpression> UnmapScriptExpressions(
            IList<ExpressionScriptProvided> scripts,
            StatementSpecUnMapContext unmapContext)
        {
            if (scripts == null || scripts.IsEmpty()) {
                return EmptyList<ScriptExpression>.Instance;
            }

            IList<ScriptExpression> result = new List<ScriptExpression>();
            foreach (var script in scripts) {
                var e = UnmapScriptExpression(script);
                result.Add(e);
            }

            return result;
        }

        private static IList<ClassProvidedExpression> UnmapClassProvideds(
            IList<string> classProvideds,
            StatementSpecUnMapContext unmapContext)
        {
            if (classProvideds == null || classProvideds.IsEmpty()) {
                return EmptyList<ClassProvidedExpression>.Instance;
            }

            IList<ClassProvidedExpression> result = new List<ClassProvidedExpression>(classProvideds.Count);
            foreach (var text in classProvideds) {
                var e = new ClassProvidedExpression(text);
                result.Add(e);
            }

            return result;
        }

        private static ScriptExpression UnmapScriptExpression(ExpressionScriptProvided script)
        {
            var returnType = script.OptionalReturnTypeName;
            IList<string> @params = new List<string>(Arrays.AsList(script.ParameterNames));
            return new ScriptExpression(
                script.Name,
                @params,
                script.Expression,
                returnType,
                script.OptionalDialect,
                script.OptionalEventTypeName);
        }

        private static AnnotationPart UnmapAnnotation(AnnotationDesc desc)
        {
            if (desc.Attributes == null || desc.Attributes.IsEmpty()) {
                return new AnnotationPart(desc.Name);
            }

            IList<AnnotationAttribute> attributes = new List<AnnotationAttribute>();
            foreach (var pair in desc.Attributes) {
                if (pair.Second is AnnotationDesc second) {
                    attributes.Add(new AnnotationAttribute(pair.First, UnmapAnnotation(second)));
                }
                else {
                    attributes.Add(new AnnotationAttribute(pair.First, pair.Second));
                }
            }

            return new AnnotationPart(desc.Name, attributes);
        }

        public static IList<AnnotationDesc> MapAnnotations(IList<AnnotationPart> annotations)
        {
            IList<AnnotationDesc> result;
            if (annotations != null) {
                result = new List<AnnotationDesc>();
                foreach (var part in annotations) {
                    result.Add(MapAnnotation(part));
                }
            }
            else {
                result = EmptyList<AnnotationDesc>.Instance;
            }

            return result;
        }

        private static void MapContextName(
            string contextName,
            StatementSpecRaw raw,
            StatementSpecMapContext mapContext)
        {
            raw.OptionalContextName = contextName;
        }

        private static void MapExpressionDeclaration(
            IList<ExpressionDeclaration> expressionDeclarations,
            StatementSpecRaw raw,
            StatementSpecMapContext mapContext)
        {
            if (expressionDeclarations == null || expressionDeclarations.IsEmpty()) {
                return;
            }

            var desc = new ExpressionDeclDesc();
            raw.ExpressionDeclDesc = desc;

            foreach (var decl in expressionDeclarations) {
                var item = MapExpressionDeclItem(decl, mapContext);
                desc.Expressions.Add(item);
                mapContext.AddExpressionDeclaration(item);
            }
        }

        private static ExpressionDeclItem MapExpressionDeclItem(
            ExpressionDeclaration decl,
            StatementSpecMapContext mapContext)
        {
            var item = new ExpressionDeclItem(
                decl.Name,
                decl.IsAlias ? Array.Empty<string>() : decl.ParameterNames.ToArray(),
                decl.IsAlias);
            item.OptionalSoda = decl.Expression;
            return item;
        }

        private static void MapScriptExpressions(
            IList<ScriptExpression> scriptExpressions,
            StatementSpecRaw raw,
            StatementSpecMapContext mapContext)
        {
            if (scriptExpressions == null || scriptExpressions.IsEmpty()) {
                return;
            }

            IList<ExpressionScriptProvided> scripts = new List<ExpressionScriptProvided>();
            raw.ScriptExpressions = scripts;

            foreach (var decl in scriptExpressions) {
                var scriptProvided = MapScriptExpression(decl, mapContext);
                scripts.Add(scriptProvided);
                mapContext.AddScript(scriptProvided);
            }
        }

        private static void MapClassProvidedExpressions(
            IList<ClassProvidedExpression> classProvidedExpressions,
            StatementSpecRaw raw,
            StatementSpecMapContext mapContext)
        {
            if (classProvidedExpressions == null || classProvidedExpressions.IsEmpty()) {
                return;
            }

            IList<string> classes = new List<string>(classProvidedExpressions.Count);
            raw.ClassProvidedList = classes;

            foreach (var decl in classProvidedExpressions) {
                classes.Add(decl.ClassText);
            }
        }

        private static ExpressionScriptProvided MapScriptExpression(
            ScriptExpression decl,
            StatementSpecMapContext mapContext)
        {
            var @params = decl.ParameterNames == null ? Array.Empty<string>() : decl.ParameterNames.ToArray();
            return new ExpressionScriptProvided(
                decl.Name,
                decl.ExpressionText,
                @params,
                decl.OptionalReturnType,
                decl.OptionalEventTypeName,
                decl.OptionalDialect);
        }

        private static AnnotationDesc MapAnnotation(AnnotationPart part)
        {
            if (part.Attributes == null || part.Attributes.IsEmpty()) {
                return new AnnotationDesc(part.Name, EmptyList<Pair<string, object>>.Instance);
            }

            IList<Pair<string, object>> attributes = new List<Pair<string, object>>();
            foreach (var pair in part.Attributes) {
                if (pair.Value is AnnotationPart value) {
                    attributes.Add(new Pair<string, object>(pair.Name, MapAnnotation(value)));
                }
                else {
                    attributes.Add(new Pair<string, object>(pair.Name, pair.Value));
                }
            }

            return new AnnotationDesc(part.Name, attributes);
        }

        private static void MapSQLParameters(
            FromClause fromClause,
            StatementSpecRaw raw,
            StatementSpecMapContext mapContext)
        {
            if (fromClause == null || fromClause.Streams == null) {
                return;
            }

            var streamNum = -1;
            foreach (var stream in fromClause.Streams) {
                streamNum++;
                if (!(stream is SQLStream sqlStream)) {
                    continue;
                }

                IList<PlaceholderParser.Fragment> sqlFragments = null;
                try {
                    sqlFragments = PlaceholderParser.ParsePlaceholder(sqlStream.SqlWithSubsParams);
                }
                catch (PlaceholderParseException) {
                    throw new EPRuntimeException(
                        "Error parsing SQL placeholder expression '" + sqlStream.SqlWithSubsParams + "': ");
                }

                foreach (var fragment in sqlFragments) {
                    if (!(fragment is PlaceholderParser.ParameterFragment)) {
                        continue;
                    }

                    // Parse expression, store for substitution parameters
                    var expression = fragment.Value;
                    if (string.Equals(expression, SAMPLE_WHERECLAUSE_PLACEHOLDER, StringComparison.InvariantCultureIgnoreCase)) {
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(expression)) {
                        throw new EPException("Missing expression within ${...} in SQL statement");
                    }

                    var toCompile = "select * from System.Object where " + expression;
                    StatementSpecRaw rawSqlExpr;
                    try {
                        rawSqlExpr = mapContext.MapEnv.CompilerServices.ParseWalk(toCompile, mapContext.MapEnv);
                    }
                    catch (StatementSpecCompileException e) {
                        throw new EPException(
                            "Failed to compile SQL parameter '" + expression + "': " + e.Expression,
                            e);
                    }

                    mapContext.SubstitutionNodes.AddAll(rawSqlExpr.SubstitutionParameters);
                    mapContext.VariableNames.AddAll(rawSqlExpr.ReferencedVariables);
                    mapContext.TableExpressions.AddAll(rawSqlExpr.TableExpressions);

                    // add expression
                    if (raw.SqlParameters == null) {
                        raw.SqlParameters = new Dictionary<int, IList<ExprNode>>();
                    }

                    var listExp = raw.SqlParameters.Get(streamNum);
                    if (listExp == null) {
                        listExp = new List<ExprNode>();
                        raw.SqlParameters.Put(streamNum, listExp);
                    }

                    listExp.Add(rawSqlExpr.WhereClause);
                }
            }
        }

        private static IList<Chainable> MapChains(
            IList<DotExpressionItem> pairs,
            StatementSpecMapContext mapContext)
        {
            IList<Chainable> chains = new List<Chainable>();
            foreach (var item in pairs) {
                chains.Add(MapChainItem(item, mapContext));
            }

            return chains;
        }

        private static IList<DotExpressionItem> UnmapChains(
            IList<Chainable> pairs,
            StatementSpecUnMapContext unmapContext)
        {
            IList<DotExpressionItem> result = new List<DotExpressionItem>();
            foreach (var chain in pairs) {
                result.Add(UnmapChainItem(chain, unmapContext));
            }

            return result;
        }

        private static DotExpressionItem UnmapChainItem(
            Chainable chain,
            StatementSpecUnMapContext unmapContext)
        {
            if (chain is ChainableName name) {
                return new DotExpressionItemName(name.Name);
            }
            else if (chain is ChainableArray array) {
                var indexes = array.Indexes;
                return new DotExpressionItemArray(UnmapExpressionDeep(indexes, unmapContext));
            }
            else if (chain is ChainableCall call) {
                return new DotExpressionItemCall(call.Name, UnmapExpressionDeep(call.Parameters, unmapContext));
            }
            else {
                throw new IllegalStateException("Unrecognized chainable " + chain);
            }
        }

        private static Chainable MapChainItem(
            DotExpressionItem item,
            StatementSpecMapContext mapContext)
        {
            if (item is DotExpressionItemName name) {
                return new ChainableName(name.Name);
            }
            else if (item is DotExpressionItemArray array) {
                var indexes = array.Indexes;
                return new ChainableArray(MapExpressionDeep(indexes, mapContext));
            }
            else if (item is DotExpressionItemCall call) {
                return new ChainableCall(call.Name, MapExpressionDeep(call.Parameters, mapContext));
            }
            else {
                throw new IllegalStateException("Unrecognized item " + item);
            }
        }

        public static ExprNode MapExpression(
            Expression expression,
            StatementSpecMapContext env)
        {
            return MapExpressionDeep(expression, env);
        }
    }
} // end of namespace