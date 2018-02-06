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
using com.espertech.esper.client.soda;
using com.espertech.esper.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.container;
using com.espertech.esper.compat.threading;
using com.espertech.esper.core.context.mgr;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.agg.access;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.db;
using com.espertech.esper.epl.declexpr;
using com.espertech.esper.epl.enummethod.dot;
using com.espertech.esper.epl.expression.accessagg;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.dot;
using com.espertech.esper.epl.expression.funcs;
using com.espertech.esper.epl.expression.methodagg;
using com.espertech.esper.epl.expression.ops;
using com.espertech.esper.epl.expression.prev;
using com.espertech.esper.epl.expression.prior;
using com.espertech.esper.epl.expression.subquery;
using com.espertech.esper.epl.expression.table;
using com.espertech.esper.epl.expression.time;
using com.espertech.esper.epl.named;
using com.espertech.esper.epl.parse;
using com.espertech.esper.epl.script;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.epl.variable;
using com.espertech.esper.pattern;
using com.espertech.esper.rowregex;
using com.espertech.esper.schedule;
using com.espertech.esper.type;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.spec
{
    using ContextDescriptor = client.soda.ContextDescriptor;
    using OrderByElement = com.espertech.esper.client.soda.OrderByElement;

    /// <summary>
    /// Helper for mapping internal representations of a statement to the SODA object model for statements.
    /// </summary>
    public class StatementSpecMapper
    {
        /// <summary>
        /// Unmap expresission.
        /// </summary>
        /// <param name="expression">to unmap</param>
        /// <returns>expression</returns>
        public static Expression Unmap(ExprNode expression)
        {
            return UnmapExpressionDeep(expression, new StatementSpecUnMapContext());
        }

        /// <summary>
        /// Unmap pattern.
        /// </summary>
        /// <param name="node">to unmap</param>
        /// <returns>pattern</returns>
        public static PatternExpr Unmap(EvalFactoryNode node)
        {
            return UnmapPatternEvalDeep(node, new StatementSpecUnMapContext());
        }

        /// <summary>
        /// Unmap annotation.
        /// </summary>
        /// <param name="node">to unmap</param>
        /// <returns>annotation</returns>
        public static AnnotationPart Unmap(AnnotationDesc node)
        {
            var list = UnmapAnnotations(new List<AnnotationDesc>(Collections.SingletonList(node)));
            return list[0];
        }

        /// <summary>
        /// Unmap match recognize pattern.
        /// </summary>
        /// <param name="pattern">recognize pattern to unmap</param>
        /// <returns>match recognize pattern</returns>
        public static MatchRecognizeRegEx Unmap(RowRegexExprNode pattern)
        {
            return UnmapExpressionDeepRowRegex(pattern, new StatementSpecUnMapContext());
        }

        public static StatementSpecRaw Map(
            IContainer container,
            EPStatementObjectModel sodaStatement,
            EngineImportService engineImportService,
            VariableService variableService,
            ConfigurationInformation configuration,
            SchedulingService schedulingService,
            string engineURI,
            PatternNodeFactory patternNodeFactory,
            NamedWindowMgmtService namedWindowMgmtService,
            ContextManagementService contextManagementService,
            ExprDeclaredService exprDeclaredService,
            TableService tableService)
        {
            esper.core.context.util.ContextDescriptor contextDescriptor = null;
            if (sodaStatement.ContextName != null)
            {
                contextDescriptor = contextManagementService.GetContextDescriptor(sodaStatement.ContextName);
            }

            var mapContext = new StatementSpecMapContext(
                container,
                engineImportService,
                variableService,
                configuration,
                schedulingService,
                engineURI,
                patternNodeFactory,
                namedWindowMgmtService,
                contextManagementService,
                exprDeclaredService,
                contextDescriptor,
                tableService);

            var raw = Map(sodaStatement, mapContext);
            if (mapContext.HasVariables)
            {
                raw.HasVariables = true;
            }
            raw.ReferencedVariables = mapContext.VariableNames;
            raw.TableExpressions = mapContext.TableExpressions;
            return raw;
        }

        private static StatementSpecRaw Map(
            EPStatementObjectModel sodaStatement, 
            StatementSpecMapContext mapContext)
        {
            var raw = new StatementSpecRaw(SelectClauseStreamSelectorEnum.ISTREAM_ONLY);

            var annotations = MapAnnotations(sodaStatement.Annotations);
            raw.Annotations = annotations;
            MapFireAndForget(sodaStatement.FireAndForgetClause, raw, mapContext);
            MapExpressionDeclaration(sodaStatement.ExpressionDeclarations, raw, mapContext);
            MapScriptExpressions(sodaStatement.ScriptExpressions, raw, mapContext);
            MapContextName(sodaStatement.ContextName, raw, mapContext);
            MapUpdateClause(sodaStatement.UpdateClause, raw, mapContext);
            MapCreateContext(sodaStatement.CreateContext, raw, mapContext);
            MapCreateWindow(sodaStatement.CreateWindow, sodaStatement.FromClause, raw, mapContext);
            MapCreateIndex(sodaStatement.CreateIndex, raw, mapContext);
            MapCreateVariable(sodaStatement.CreateVariable, raw, mapContext);
            MapCreateTable(sodaStatement.CreateTable, raw, mapContext);
            MapCreateSchema(sodaStatement.CreateSchema, raw, mapContext);
            MapCreateExpression(sodaStatement.CreateExpression, raw, mapContext);
            MapCreateGraph(sodaStatement.CreateDataFlow, raw, mapContext);
            MapOnTrigger(sodaStatement.OnExpr, raw, mapContext);
            var desc = MapInsertInto(sodaStatement.InsertInto);
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
            MapIntoVariableClause(sodaStatement.IntoTableClause, raw, mapContext);

            // from clause is required for create-window
            if (sodaStatement.CreateWindow != null && raw.StreamSpecs.Count == 0)
            {
                var spec = new FilterSpecRaw("System.Object", Collections.GetEmptyList<ExprNode>(), null);
                raw.StreamSpecs.Add(
                    new FilterStreamSpecRaw(spec, ViewSpec.EMPTY_VIEWSPEC_ARRAY, null, StreamSpecOptions.DEFAULT));
            }
            return raw;
        }

        private static void MapIntoVariableClause(
            IntoTableClause intoClause,
            StatementSpecRaw raw,
            StatementSpecMapContext mapContext)
        {
            if (intoClause != null)
            {
                raw.IntoTableSpec = new IntoTableSpec(intoClause.TableName);
            }
        }

        private static void MapFireAndForget(
            FireAndForgetClause fireAndForgetClause,
            StatementSpecRaw raw,
            StatementSpecMapContext mapContext)
        {
            if (fireAndForgetClause == null)
            {
                return;
            }
            else if (fireAndForgetClause is FireAndForgetDelete)
            {
                raw.FireAndForgetSpec = new FireAndForgetSpecDelete();
            }
            else if (fireAndForgetClause is FireAndForgetInsert)
            {
                var insert = (FireAndForgetInsert) fireAndForgetClause;
                raw.FireAndForgetSpec = new FireAndForgetSpecInsert(insert.IsUseValuesKeyword);
            }
            else if (fireAndForgetClause is FireAndForgetUpdate)
            {
                var upd = (FireAndForgetUpdate) fireAndForgetClause;
                var assignments = upd.Assignments
                    .Select(pair => MapExpressionDeep(pair.Value, mapContext))
                    .Select(expr => new OnTriggerSetAssignment(expr))
                    .ToList();
                var updspec = new FireAndForgetSpecUpdate(assignments);
                raw.FireAndForgetSpec = updspec;
            }
            else
            {
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
            return new StatementSpecUnMapResult(model, unmapContext.SubstitutionParams);
        }

        private static EPStatementObjectModel UnmapInternal(
            StatementSpecRaw statementSpec,
            StatementSpecUnMapContext unmapContext)
        {
            var model = new EPStatementObjectModel();
            var annotations = UnmapAnnotations(statementSpec.Annotations);
            model.Annotations = annotations;
            UnmapFireAndForget(statementSpec.FireAndForgetSpec, model, unmapContext);
            var expressionDeclarations = UnmapExpressionDeclarations(statementSpec.ExpressionDeclDesc, unmapContext);
            model.ExpressionDeclarations = expressionDeclarations;
            var scripts = UnmapScriptExpressions(statementSpec.ScriptExpressions, unmapContext);
            model.ScriptExpressions = scripts;
            UnmapContextName(statementSpec.OptionalContextName, model);
            UnmapCreateContext(statementSpec.CreateContextDesc, model, unmapContext);
            UnmapCreateWindow(statementSpec.CreateWindowDesc, model, unmapContext);
            UnmapCreateIndex(statementSpec.CreateIndexDesc, model, unmapContext);
            UnmapCreateVariable(statementSpec.CreateVariableDesc, model, unmapContext);
            UnmapCreateTable(statementSpec.CreateTableDesc, model, unmapContext);
            UnmapCreateSchema(statementSpec.CreateSchemaDesc, model, unmapContext);
            UnmapCreateExpression(statementSpec.CreateExpressionDesc, model, unmapContext);
            UnmapCreateGraph(statementSpec.CreateDataFlowDesc, model, unmapContext);
            UnmapUpdateClause(statementSpec.StreamSpecs, statementSpec.UpdateDesc, model, unmapContext);
            UnmapOnClause(statementSpec.OnTriggerDesc, model, unmapContext);
            var insertIntoClause = UnmapInsertInto(statementSpec.InsertIntoDesc);
            model.InsertInto = insertIntoClause;
            var selectClause = UnmapSelect(
                statementSpec.SelectClauseSpec, statementSpec.SelectStreamSelectorEnum, unmapContext);
            model.SelectClause = selectClause;
            UnmapFrom(statementSpec.StreamSpecs, statementSpec.OuterJoinDescList, model, unmapContext);
            UnmapWhere(statementSpec.FilterRootNode, model, unmapContext);
            UnmapGroupBy(statementSpec.GroupByExpressions, model, unmapContext);
            UnmapHaving(statementSpec.HavingExprRootNode, model, unmapContext);
            UnmapOutputLimit(statementSpec.OutputLimitSpec, model, unmapContext);
            UnmapOrderBy(statementSpec.OrderByList, model, unmapContext);
            UnmapRowLimit(statementSpec.RowLimitSpec, model, unmapContext);
            UnmapMatchRecognize(statementSpec.MatchRecognizeSpec, model, unmapContext);
            UnmapForClause(statementSpec.ForClauseSpec, model, unmapContext);
            UnmapSQLParameters(statementSpec.SqlParameters, unmapContext);
            UnmapIntoVariableClause(statementSpec.IntoTableSpec, model, unmapContext);
            return model;
        }

        private static void UnmapIntoVariableClause(
            IntoTableSpec intoTableSpec,
            EPStatementObjectModel model,
            StatementSpecUnMapContext unmapContext)
        {
            if (intoTableSpec == null)
            {
                return;
            }
            model.IntoTableClause = new IntoTableClause(intoTableSpec.Name);
        }

        private static void UnmapCreateTable(
            CreateTableDesc desc,
            EPStatementObjectModel model,
            StatementSpecUnMapContext unmapContext)
        {
            if (desc == null)
            {
                return;
            }
            var clause = new CreateTableClause(desc.TableName);
            var cols = new List<com.espertech.esper.client.soda.CreateTableColumn>();
            foreach (var col in desc.Columns)
            {
                var optExpr = col.OptExpression != null ? UnmapExpressionDeep(col.OptExpression, unmapContext) : null;
                var annots = UnmapAnnotations(col.Annotations);
                var coldesc = new com.espertech.esper.client.soda.CreateTableColumn(
                    col.ColumnName, optExpr, col.OptTypeName, col.OptTypeIsArray, col.OptTypeIsPrimitiveArray, annots,
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
            if (fireAndForgetSpec == null)
            {
                return;
            }
            else if (fireAndForgetSpec is FireAndForgetSpecDelete)
            {
                model.FireAndForgetClause = new FireAndForgetDelete();
            }
            else if (fireAndForgetSpec is FireAndForgetSpecInsert)
            {
                var insert = (FireAndForgetSpecInsert) fireAndForgetSpec;
                model.FireAndForgetClause = new FireAndForgetInsert(insert.IsUseValuesKeyword);
            }
            else if (fireAndForgetSpec is FireAndForgetSpecUpdate)
            {
                var upd = (FireAndForgetSpecUpdate) fireAndForgetSpec;
                var faf = new FireAndForgetUpdate();
                foreach (var assignment in upd.Assignments)
                {
                    var expr = UnmapExpressionDeep(assignment.Expression, unmapContext);
                    faf.AddAssignment(new Assignment(expr));
                }
                model.FireAndForgetClause = faf;
            }
            else
            {
                throw new IllegalStateException("Unrecognized type of fire-and-forget: " + fireAndForgetSpec);
            }
        }

        // Collect substitution parameters
        private static void UnmapSQLParameters(
            IEnumerable<KeyValuePair<int, IList<ExprNode>>> sqlParameters,
            StatementSpecUnMapContext unmapContext)
        {
            if (sqlParameters == null)
            {
                return;
            }
            foreach (var pair in sqlParameters)
            {
                foreach (var node in pair.Value)
                {
                    UnmapExpressionDeep(node, unmapContext);
                }
            }
        }

        private static void UnmapOnClause(
            OnTriggerDesc onTriggerDesc,
            EPStatementObjectModel model,
            StatementSpecUnMapContext unmapContext)
        {
            if (onTriggerDesc == null)
            {
                return;
            }
            if (onTriggerDesc.OnTriggerType == OnTriggerType.ON_DELETE)
            {
                var window = (OnTriggerWindowDesc) onTriggerDesc;
                model.OnExpr = new OnDeleteClause(window.WindowName, window.OptionalAsName);
            }
            else if (onTriggerDesc.OnTriggerType == OnTriggerType.ON_UPDATE)
            {
                var window = (OnTriggerWindowUpdateDesc) onTriggerDesc;
                var clause = new OnUpdateClause(window.WindowName, window.OptionalAsName);
                foreach (var assignment in window.Assignments)
                {
                    var expr = UnmapExpressionDeep(assignment.Expression, unmapContext);
                    clause.AddAssignment(expr);
                }
                model.OnExpr = clause;
            }
            else if (onTriggerDesc.OnTriggerType == OnTriggerType.ON_SELECT)
            {
                var window = (OnTriggerWindowDesc) onTriggerDesc;
                var onSelect = new OnSelectClause(window.WindowName, window.OptionalAsName);
                onSelect.IsDeleteAndSelect = window.IsDeleteAndSelect;
                model.OnExpr = onSelect;
            }
            else if (onTriggerDesc.OnTriggerType == OnTriggerType.ON_SET)
            {
                var trigger = (OnTriggerSetDesc) onTriggerDesc;
                var clause = new OnSetClause();
                foreach (var assignment in trigger.Assignments)
                {
                    var expr = UnmapExpressionDeep(assignment.Expression, unmapContext);
                    clause.AddAssignment(expr);
                }
                model.OnExpr = clause;
            }
            else if (onTriggerDesc.OnTriggerType == OnTriggerType.ON_SPLITSTREAM)
            {
                var trigger = (OnTriggerSplitStreamDesc) onTriggerDesc;
                var clause = OnInsertSplitStreamClause.Create();
                foreach (var stream in trigger.SplitStreams)
                {
                    Expression whereClause = null;
                    if (stream.WhereClause != null)
                    {
                        whereClause = UnmapExpressionDeep(stream.WhereClause, unmapContext);
                    }
                    IList<ContainedEventSelect> propertySelects = null;
                    string propertySelectStreamName = null;
                    if (stream.FromClause != null)
                    {
                        propertySelects = UnmapPropertySelects(stream.FromClause.PropertyEvalSpec, unmapContext);
                        propertySelectStreamName = stream.FromClause.OptionalStreamName;
                    }
                    var insertIntoClause = UnmapInsertInto(stream.InsertInto);
                    var selectClause = UnmapSelect(
                        stream.SelectClause, SelectClauseStreamSelectorEnum.ISTREAM_ONLY, unmapContext);
                    clause.AddItem(
                        OnInsertSplitStreamItem.Create(
                            insertIntoClause, selectClause, propertySelects, propertySelectStreamName, whereClause));
                }
                model.OnExpr = clause;
                clause.IsFirst = trigger.IsFirst;
            }
            else if (onTriggerDesc.OnTriggerType == OnTriggerType.ON_MERGE)
            {
                var trigger = (OnTriggerMergeDesc) onTriggerDesc;
                var matchItems = new List<OnMergeMatchItem>();
                foreach (var matched in trigger.Items)
                {

                    var actions = new List<OnMergeMatchedAction>();
                    var matchCond = matched.OptionalMatchCond != null
                        ? UnmapExpressionDeep(matched.OptionalMatchCond, unmapContext)
                        : null;
                    var matchItem = new OnMergeMatchItem(matched.IsMatchedUnmatched, matchCond, actions);
                    foreach (var actionitem in matched.Actions)
                    {
                        OnMergeMatchedAction action;
                        if (actionitem is OnTriggerMergeActionDelete)
                        {
                            var delete = (OnTriggerMergeActionDelete) actionitem;
                            var optionalCondition = delete.OptionalWhereClause == null
                                ? null
                                : UnmapExpressionDeep(delete.OptionalWhereClause, unmapContext);
                            action = new OnMergeMatchedDeleteAction(optionalCondition);
                        }
                        else if (actionitem is OnTriggerMergeActionUpdate)
                        {
                            var merge = (OnTriggerMergeActionUpdate) actionitem;
                            var assignments = merge.Assignments
                                .Select(pair => UnmapExpressionDeep(pair.Expression, unmapContext))
                                .Select(expr => new Assignment(expr))
                                .ToList();
                            var optionalCondition = merge.OptionalWhereClause == null
                                ? null
                                : UnmapExpressionDeep(merge.OptionalWhereClause, unmapContext);
                            action = new OnMergeMatchedUpdateAction(assignments, optionalCondition);
                        }
                        else if (actionitem is OnTriggerMergeActionInsert)
                        {
                            var insert = (OnTriggerMergeActionInsert) actionitem;
                            var columnNames = new List<string>(insert.Columns);
                            var select = UnmapSelectClauseElements(insert.SelectClause, unmapContext);
                            var optionalCondition = insert.OptionalWhereClause == null
                                ? null
                                : UnmapExpressionDeep(insert.OptionalWhereClause, unmapContext);
                            action = new OnMergeMatchedInsertAction(
                                columnNames, select, optionalCondition, insert.OptionalStreamName);
                        }
                        else
                        {
                            throw new ArgumentException(
                                "Unrecognized merged action type '" + actionitem.GetType() + "'");
                        }
                        actions.Add(action);
                    }
                    matchItems.Add(matchItem);
                }
                model.OnExpr = OnMergeClause.Create(trigger.WindowName, trigger.OptionalAsName, matchItems);
            }
            else
            {
                throw new ArgumentException("Type of on-clause not handled: " + onTriggerDesc.OnTriggerType);
            }
        }

        private static void UnmapUpdateClause(
            IList<StreamSpecRaw> desc,
            UpdateDesc updateDesc,
            EPStatementObjectModel model,
            StatementSpecUnMapContext unmapContext)
        {
            if (updateDesc == null)
            {
                return;
            }
            var type = ((FilterStreamSpecRaw) desc[0]).RawFilterSpec.EventTypeName;
            var clause = new UpdateClause(type, updateDesc.OptionalStreamName);
            foreach (var assignment in updateDesc.Assignments)
            {
                var expr = UnmapExpressionDeep(assignment.Expression, unmapContext);
                clause.AddAssignment(expr);
            }
            model.UpdateClause = clause;

            if (updateDesc.OptionalWhereClause != null)
            {
                var expr = UnmapExpressionDeep(updateDesc.OptionalWhereClause, unmapContext);
                model.UpdateClause.OptionalWhereClause = expr;
            }
        }

        private static void UnmapContextName(string contextName, EPStatementObjectModel model)
        {
            model.ContextName = contextName;
        }

        private static void UnmapCreateContext(
            CreateContextDesc createContextDesc,
            EPStatementObjectModel model,
            StatementSpecUnMapContext unmapContext)
        {
            if (createContextDesc == null)
            {
                return;
            }

            var desc = UnmapCreateContextDetail(createContextDesc.ContextDetail, unmapContext);
            var clause = new CreateContextClause(createContextDesc.ContextName, desc);
            model.CreateContext = clause;
        }

        private static ContextDescriptor UnmapCreateContextDetail(
            ContextDetail contextDetail,
            StatementSpecUnMapContext unmapContext)
        {
            ContextDescriptor desc;
            if (contextDetail is ContextDetailInitiatedTerminated)
            {
                var spec = (ContextDetailInitiatedTerminated) contextDetail;
                var startCondition = UnmapCreateContextRangeCondition(spec.Start, unmapContext);
                var endCondition = UnmapCreateContextRangeCondition(spec.End, unmapContext);
                IList<Expression> distinctExpressions = null;
                if (spec.DistinctExpressions != null && spec.DistinctExpressions.Length > 0)
                {
                    distinctExpressions = UnmapExpressionDeep(spec.DistinctExpressions, unmapContext);
                }
                desc = new ContextDescriptorInitiatedTerminated(
                    startCondition, endCondition, spec.IsOverlapping, distinctExpressions);
            }
            else if (contextDetail is ContextDetailPartitioned)
            {
                var seg = (ContextDetailPartitioned) contextDetail;
                var segmentedItems = new List<ContextDescriptorKeyedSegmentedItem>();
                foreach (var item in seg.Items)
                {
                    var filter = UnmapFilter(item.FilterSpecRaw, unmapContext);
                    segmentedItems.Add(new ContextDescriptorKeyedSegmentedItem(item.PropertyNames, filter));
                }
                desc = new ContextDescriptorKeyedSegmented(segmentedItems);
            }
            else if (contextDetail is ContextDetailCategory)
            {
                var category = (ContextDetailCategory) contextDetail;
                var categoryItems = new List<ContextDescriptorCategoryItem>();
                var filter = UnmapFilter(category.FilterSpecRaw, unmapContext);
                foreach (var item in category.Items)
                {
                    var expr = UnmapExpressionDeep(item.Expression, unmapContext);
                    categoryItems.Add(new ContextDescriptorCategoryItem(expr, item.Name));
                }
                desc = new ContextDescriptorCategory(categoryItems, filter);
            }
            else if (contextDetail is ContextDetailHash)
            {
                var init = (ContextDetailHash) contextDetail;
                var hashes = new List<ContextDescriptorHashSegmentedItem>();
                foreach (var item in init.Items)
                {
                    var dot =
                        UnmapChains(
                            new List<ExprChainedSpec>(Collections.SingletonList(item.Function)), unmapContext, false)[0];
                    var dotExpression =
                        new SingleRowMethodExpression(new List<DotExpressionItem>(Collections.SingletonList(dot)));
                    var filter = UnmapFilter(item.FilterSpecRaw, unmapContext);
                    hashes.Add(new ContextDescriptorHashSegmentedItem(dotExpression, filter));
                }
                desc = new ContextDescriptorHashSegmented(hashes, init.Granularity, init.IsPreallocate);
            }
            else
            {
                var nested = (ContextDetailNested) contextDetail;
                var contexts = new List<CreateContextClause>();
                foreach (var item in nested.Contexts)
                {
                    var detail = UnmapCreateContextDetail(item.ContextDetail, unmapContext);
                    contexts.Add(new CreateContextClause(item.ContextName, detail));
                }
                desc = new ContextDescriptorNested(contexts);
            }
            return desc;
        }

        private static ContextDescriptorCondition UnmapCreateContextRangeCondition(
            ContextDetailCondition endpoint,
            StatementSpecUnMapContext unmapContext)
        {
            if (endpoint is ContextDetailConditionCrontab)
            {
                var crontab = (ContextDetailConditionCrontab) endpoint;
                IList<Expression> crontabExpr = UnmapExpressionDeep(crontab.Crontab, unmapContext);
                return new ContextDescriptorConditionCrontab(crontabExpr, crontab.IsImmediate);
            }
            else if (endpoint is ContextDetailConditionPattern)
            {
                var pattern = (ContextDetailConditionPattern) endpoint;
                var patternExpr = UnmapPatternEvalDeep(pattern.PatternRaw, unmapContext);
                return new ContextDescriptorConditionPattern(patternExpr, pattern.IsInclusive, pattern.IsImmediate);
            }
            else if (endpoint is ContextDetailConditionFilter)
            {
                var filter = (ContextDetailConditionFilter) endpoint;
                var filterExpr = UnmapFilter(filter.FilterSpecRaw, unmapContext);
                return new ContextDescriptorConditionFilter(filterExpr, filter.OptionalFilterAsName);
            }
            else if (endpoint is ContextDetailConditionTimePeriod)
            {
                var period = (ContextDetailConditionTimePeriod) endpoint;
                var expression = (TimePeriodExpression) UnmapExpressionDeep(period.TimePeriod, unmapContext);
                return new ContextDescriptorConditionTimePeriod(expression, period.IsImmediate);
            }
            else if (endpoint is ContextDetailConditionImmediate)
            {
                return new ContextDescriptorConditionImmediate();
            }
            else if (endpoint is ContextDetailConditionNever)
            {
                return new ContextDescriptorConditionNever();
            }
            throw new IllegalStateException("Unrecognized endpoint " + endpoint);
        }

        private static void UnmapCreateWindow(
            CreateWindowDesc createWindowDesc,
            EPStatementObjectModel model,
            StatementSpecUnMapContext unmapContext)
        {
            if (createWindowDesc == null)
            {
                return;
            }
            Expression filter = null;
            if (createWindowDesc.InsertFilter != null)
            {
                filter = UnmapExpressionDeep(createWindowDesc.InsertFilter, unmapContext);
            }

            var clause = new CreateWindowClause(
                createWindowDesc.WindowName, UnmapViews(createWindowDesc.ViewSpecs, unmapContext));
            clause.IsInsert = createWindowDesc.IsInsert;
            clause.InsertWhereClause = filter;
            clause.Columns = UnmapColumns(createWindowDesc.Columns);
            model.CreateWindow = clause;
        }

        private static void UnmapCreateIndex(
            CreateIndexDesc createIndexDesc,
            EPStatementObjectModel model,
            StatementSpecUnMapContext unmapContext)
        {
            if (createIndexDesc == null)
            {
                return;
            }

            var cols = createIndexDesc.Columns
                .Select(item => UnmapCreateIndexColumn(item, unmapContext))
                .ToList();
            model.CreateIndex = new CreateIndexClause(
                createIndexDesc.IndexName, createIndexDesc.WindowName, cols, createIndexDesc.IsUnique);
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
            if (createVariableDesc == null)
            {
                return;
            }
            Expression assignment = null;
            if (createVariableDesc.Assignment != null)
            {
                assignment = UnmapExpressionDeep(createVariableDesc.Assignment, unmapContext);
            }
            var clause = new CreateVariableClause(
                createVariableDesc.VariableType, createVariableDesc.VariableName, assignment,
                createVariableDesc.IsConstant);
            clause.IsArray = createVariableDesc.IsArray;
            model.CreateVariable = clause;
        }

        private static void UnmapCreateSchema(
            CreateSchemaDesc desc,
            EPStatementObjectModel model,
            StatementSpecUnMapContext unmapContext)
        {
            if (desc == null)
            {
                return;
            }
            model.CreateSchema = UnmapCreateSchemaInternal(desc, unmapContext);
        }

        private static void UnmapCreateExpression(
            CreateExpressionDesc desc,
            EPStatementObjectModel model,
            StatementSpecUnMapContext unmapContext)
        {
            if (desc == null)
            {
                return;
            }
            CreateExpressionClause clause;
            if (desc.Expression != null)
            {
                clause = new CreateExpressionClause(UnmapExpressionDeclItem(desc.Expression, unmapContext));
            }
            else
            {
                clause = new CreateExpressionClause(UnmapScriptExpression(desc.Script, unmapContext));
            }
            model.CreateExpression = clause;
        }

        private static CreateSchemaClause UnmapCreateSchemaInternal(
            CreateSchemaDesc desc,
            StatementSpecUnMapContext unmapContext)
        {
            var columns = UnmapColumns(desc.Columns);
            var clause = new CreateSchemaClause(
                desc.SchemaName, desc.Types, columns, desc.Inherits, desc.AssignedType.MapToSoda());
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
            if (desc == null)
            {
                return;
            }

            var schemas = desc.Schemas.Select(schema => UnmapCreateSchemaInternal(schema, unmapContext)).ToList();
            var operators = desc.Operators.Select(spec => UnmapGraphOperator(spec, unmapContext)).ToList();
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

            op.Input = spec.Input.StreamNamesAndAliases
                .Select(@in => new DataFlowOperatorInput(@in.InputStreamNames, @in.OptionalAsName))
                .ToList();

            var outputs = new List<DataFlowOperatorOutput>();
            foreach (var @out in spec.Output.Items)
            {
                IList<DataFlowOperatorOutputType> types = @out.TypeInfo.IsEmpty()
                    ? null
                    : new List<DataFlowOperatorOutputType>(Collections.SingletonList(UnmapTypeInfo(@out.TypeInfo[0])));
                outputs.Add(new DataFlowOperatorOutput(@out.StreamName, types));
            }
            op.Output = outputs;

            if (spec.Detail != null)
            {
                var parameters = new List<DataFlowOperatorParameter>();
                foreach (var param in spec.Detail.Configs)
                {
                    var value = param.Value;
                    if (value is StatementSpecRaw)
                    {
                        value = UnmapInternal((StatementSpecRaw) value, unmapContext);
                    }
                    if (value is ExprNode)
                    {
                        value = UnmapExpressionDeep((ExprNode) value, unmapContext);
                    }
                    parameters.Add(new DataFlowOperatorParameter(param.Key, value));
                }
                op.Parameters = parameters;
            }
            else
            {
                op.Parameters = Collections.GetEmptyList<DataFlowOperatorParameter>();
            }

            return op;
        }

        private static DataFlowOperatorOutputType UnmapTypeInfo(GraphOperatorOutputItemType typeInfo)
        {
            IList<DataFlowOperatorOutputType> types = Collections.GetEmptyList<DataFlowOperatorOutputType>();
            if (typeInfo.TypeParameters != null && !typeInfo.TypeParameters.IsEmpty())
            {
                types = typeInfo.TypeParameters
                    .Select(UnmapTypeInfo)
                    .ToList();
            }
            return new DataFlowOperatorOutputType(typeInfo.IsWildcard, typeInfo.TypeOrClassname, types);
        }

        private static void UnmapOrderBy(
            IList<OrderByItem> orderByList,
            EPStatementObjectModel model,
            StatementSpecUnMapContext unmapContext)
        {
            if ((orderByList == null) || (orderByList.Count == 0))
            {
                return;
            }

            var clause = new OrderByClause();
            foreach (var item in orderByList)
            {
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
            if (outputLimitSpec == null)
            {
                return;
            }

            var selector = OutputLimitSelector.DEFAULT;
            if (outputLimitSpec.DisplayLimit == OutputLimitLimitType.FIRST)
            {
                selector = OutputLimitSelector.FIRST;
            }
            if (outputLimitSpec.DisplayLimit == OutputLimitLimitType.LAST)
            {
                selector = OutputLimitSelector.LAST;
            }
            if (outputLimitSpec.DisplayLimit == OutputLimitLimitType.SNAPSHOT)
            {
                selector = OutputLimitSelector.SNAPSHOT;
            }
            if (outputLimitSpec.DisplayLimit == OutputLimitLimitType.ALL)
            {
                selector = OutputLimitSelector.ALL;
            }

            OutputLimitClause clause;
            var unit = OutputLimitUnit.EVENTS;
            if (outputLimitSpec.RateType == OutputLimitRateType.TIME_PERIOD)
            {
                unit = OutputLimitUnit.TIME_PERIOD;
                var timePeriod =
                    (TimePeriodExpression) UnmapExpressionDeep(outputLimitSpec.TimePeriodExpr, unmapContext);
                clause = new OutputLimitClause(selector, timePeriod);
            }
            else if (outputLimitSpec.RateType == OutputLimitRateType.AFTER)
            {
                unit = OutputLimitUnit.AFTER;
                if (outputLimitSpec.AfterTimePeriodExpr != null)
                {
                    var after =
                        (TimePeriodExpression) UnmapExpressionDeep(outputLimitSpec.AfterTimePeriodExpr, unmapContext);
                    clause = new OutputLimitClause(OutputLimitSelector.DEFAULT, OutputLimitUnit.AFTER, after, null);
                }
                else
                {
                    clause = new OutputLimitClause(
                        OutputLimitSelector.DEFAULT, OutputLimitUnit.AFTER, null, outputLimitSpec.AfterNumberOfEvents);
                }
            }
            else if (outputLimitSpec.RateType == OutputLimitRateType.WHEN_EXPRESSION)
            {
                unit = OutputLimitUnit.WHEN_EXPRESSION;
                var whenExpression = UnmapExpressionDeep(outputLimitSpec.WhenExpressionNode, unmapContext);
                var thenAssignments = new List<Assignment>();
                clause = new OutputLimitClause(selector, whenExpression, thenAssignments);
                if (outputLimitSpec.ThenExpressions != null)
                {
                    foreach (var assignment in outputLimitSpec.ThenExpressions)
                    {
                        var expr = UnmapExpressionDeep(assignment.Expression, unmapContext);
                        clause.AddThenAssignment(expr);
                    }
                }
            }
            else if (outputLimitSpec.RateType == OutputLimitRateType.CRONTAB)
            {
                unit = OutputLimitUnit.CRONTAB_EXPRESSION;
                IList<ExprNode> timerAtExpressions = outputLimitSpec.CrontabAtSchedule;
                var mappedExpr = UnmapExpressionDeep(timerAtExpressions, unmapContext);
                clause = new OutputLimitClause(selector, mappedExpr.ToArray());
            }
            else if (outputLimitSpec.RateType == OutputLimitRateType.TERM)
            {
                clause = new OutputLimitClause(selector, OutputLimitUnit.CONTEXT_PARTITION_TERM);
            }
            else
            {
                clause = new OutputLimitClause(selector, outputLimitSpec.Rate, outputLimitSpec.VariableName, unit);
            }

            clause.AfterNumberOfEvents = outputLimitSpec.AfterNumberOfEvents;
            if (outputLimitSpec.AfterTimePeriodExpr != null)
            {
                clause.AfterTimePeriodExpression = UnmapExpressionDeep(
                    outputLimitSpec.AfterTimePeriodExpr, unmapContext);
            }
            clause.IsAndAfterTerminate = outputLimitSpec.IsAndAfterTerminate;
            if (outputLimitSpec.AndAfterTerminateExpr != null)
            {
                clause.AndAfterTerminateAndExpr = UnmapExpressionDeep(
                    outputLimitSpec.AndAfterTerminateExpr, unmapContext);
            }
            if (outputLimitSpec.AndAfterTerminateThenExpressions != null)
            {
                var thenAssignments = outputLimitSpec.AndAfterTerminateThenExpressions
                    .Select(assignment => UnmapExpressionDeep(assignment.Expression, unmapContext))
                    .Select(expr => new Assignment(expr))
                    .ToList();
                clause.AndAfterTerminateThenAssignments = thenAssignments;
            }
            model.OutputLimitClause = clause;
        }

        private static void UnmapRowLimit(
            RowLimitSpec rowLimitSpec,
            EPStatementObjectModel model,
            StatementSpecUnMapContext unmapContext)
        {
            if (rowLimitSpec == null)
            {
                return;
            }
            var spec = new RowLimitClause(
                rowLimitSpec.NumRows, rowLimitSpec.OptionalOffset,
                rowLimitSpec.NumRowsVariable, rowLimitSpec.OptionalOffsetVariable);
            model.RowLimitClause = spec;
        }

        private static void UnmapForClause(
            ForClauseSpec spec,
            EPStatementObjectModel model,
            StatementSpecUnMapContext unmapContext)
        {
            if ((spec == null) || (spec.Clauses == null) || (spec.Clauses.Count == 0))
            {
                return;
            }
            var clause = new ForClause();
            foreach (var itemSpec in spec.Clauses)
            {
                var item = new ForClauseItem(EnumHelper.Parse<ForClauseKeyword>(itemSpec.Keyword));
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
            if (spec == null)
            {
                return;
            }
            var clause = new MatchRecognizeClause();
            clause.PartitionExpressions = UnmapExpressionDeep(spec.PartitionByExpressions, unmapContext);

            var measures = spec.Measures
                .Select(item => new SelectClauseExpression(UnmapExpressionDeep(item.Expr, unmapContext), item.Name))
                .ToList();

            clause.Measures = measures;
            clause.IsAll = spec.IsAllMatches;
            clause.SkipClause = spec.Skip.Skip.Xlate<MatchRecognizeSkipClause>();

            var defines = spec.Defines
                .Select(
                    define =>
                        new MatchRecognizeDefine(
                            define.Identifier, UnmapExpressionDeep(define.Expression, unmapContext)))
                .ToList();

            clause.Defines = defines;

            if (spec.Interval != null)
            {
                clause.IntervalClause = new MatchRecognizeIntervalClause(
                    (TimePeriodExpression) UnmapExpressionDeep(
                        spec.Interval.TimePeriodExpr, unmapContext), spec.Interval.IsOrTerminated);
            }
            clause.Pattern = UnmapExpressionDeepRowRegex(spec.Pattern, unmapContext);
            model.MatchRecognizeClause = clause;
        }

        private static void MapOrderBy(
            OrderByClause orderByClause,
            StatementSpecRaw raw,
            StatementSpecMapContext mapContext)
        {
            if (orderByClause == null)
            {
                return;
            }
            foreach (var element in orderByClause.OrderByExpressions)
            {
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
            if (outputLimitClause == null)
            {
                return;
            }

            OutputLimitLimitType displayLimit =
                EnumHelper.Parse<OutputLimitLimitType>(outputLimitClause.Selector.ToString());

            OutputLimitRateType rateType;
            if (outputLimitClause.Unit == OutputLimitUnit.EVENTS)
            {
                rateType = OutputLimitRateType.EVENTS;
            }
            else if (outputLimitClause.Unit == OutputLimitUnit.TIME_PERIOD)
            {
                rateType = OutputLimitRateType.TIME_PERIOD;
            }
            else if (outputLimitClause.Unit == OutputLimitUnit.CRONTAB_EXPRESSION)
            {
                rateType = OutputLimitRateType.CRONTAB;
            }
            else if (outputLimitClause.Unit == OutputLimitUnit.WHEN_EXPRESSION)
            {
                rateType = OutputLimitRateType.WHEN_EXPRESSION;
            }
            else if (outputLimitClause.Unit == OutputLimitUnit.AFTER)
            {
                rateType = OutputLimitRateType.AFTER;
            }
            else if (outputLimitClause.Unit == OutputLimitUnit.CONTEXT_PARTITION_TERM)
            {
                rateType = OutputLimitRateType.TERM;
            }
            else
            {
                throw new ArgumentException("Unknown output limit unit " + outputLimitClause.Unit);
            }

            double? frequency = outputLimitClause.Frequency;
            string frequencyVariable = outputLimitClause.FrequencyVariable;

            if (frequencyVariable != null)
            {
                mapContext.HasVariables = true;
            }

            ExprNode whenExpression = null;
            IList<OnTriggerSetAssignment> assignments = null;
            if (outputLimitClause.WhenExpression != null)
            {
                whenExpression = MapExpressionDeep(
                    outputLimitClause.WhenExpression, mapContext);

                assignments = outputLimitClause.ThenAssignments
                    .Select(pair => MapExpressionDeep(pair.Value, mapContext))
                    .Select(expr => new OnTriggerSetAssignment(expr))
                    .ToList();
            }

            IList<ExprNode> timerAtExprList = null;
            if (outputLimitClause.CrontabAtParameters != null)
            {
                timerAtExprList = MapExpressionDeep(outputLimitClause.CrontabAtParameters, mapContext);
            }

            ExprTimePeriod timePeriod = null;
            if (outputLimitClause.TimePeriodExpression != null)
            {
                timePeriod = (ExprTimePeriod) MapExpressionDeep(
                    outputLimitClause.TimePeriodExpression, mapContext);
            }

            ExprTimePeriod afterTimePeriod = null;
            if (outputLimitClause.AfterTimePeriodExpression != null)
            {
                afterTimePeriod =
                    (ExprTimePeriod) MapExpressionDeep(
                        outputLimitClause.AfterTimePeriodExpression, mapContext);
            }

            ExprNode andAfterTerminateAndExpr = null;
            if (outputLimitClause.AndAfterTerminateAndExpr != null)
            {
                andAfterTerminateAndExpr = MapExpressionDeep(
                    outputLimitClause.AndAfterTerminateAndExpr, mapContext);
            }

            IList<OnTriggerSetAssignment> afterTerminateAssignments = null;
            if (outputLimitClause.AndAfterTerminateThenAssignments != null)
            {
                afterTerminateAssignments = new List<OnTriggerSetAssignment>();
                foreach (Assignment pair in outputLimitClause.AndAfterTerminateThenAssignments)
                {
                    var expr = MapExpressionDeep(
                        pair.Value, mapContext);
                    afterTerminateAssignments.Add(new OnTriggerSetAssignment(expr));
                }
            }

            var spec = new OutputLimitSpec(
                frequency, frequencyVariable, rateType, displayLimit, whenExpression, assignments, timerAtExprList,
                timePeriod, afterTimePeriod, outputLimitClause.AfterNumberOfEvents,
                outputLimitClause.IsAndAfterTerminate, andAfterTerminateAndExpr, afterTerminateAssignments);
            raw.OutputLimitSpec = spec;
        }

        private static void MapOnTrigger(
            OnClause onExpr, StatementSpecRaw raw, StatementSpecMapContext mapContext)
        {
            if (onExpr == null)
            {
                return;
            }

            if (onExpr is OnDeleteClause)
            {
                var onDeleteClause = (OnDeleteClause) onExpr;
                raw.OnTriggerDesc = new OnTriggerWindowDesc(
                    onDeleteClause.WindowName, onDeleteClause.OptionalAsName, OnTriggerType.ON_DELETE, false);
            }
            else if (onExpr is OnSelectClause)
            {
                var onSelectClause = (OnSelectClause) onExpr;
                raw.OnTriggerDesc = new OnTriggerWindowDesc(
                    onSelectClause.WindowName, onSelectClause.OptionalAsName, OnTriggerType.ON_SELECT,
                    onSelectClause.IsDeleteAndSelect);
            }
            else if (onExpr is OnSetClause)
            {
                var setClause = (OnSetClause) onExpr;
                mapContext.HasVariables = true;
                var assignments = setClause.Assignments
                    .Select(pair => MapExpressionDeep(pair.Value, mapContext))
                    .Select(expr => new OnTriggerSetAssignment(expr))
                    .ToList();
                var desc = new OnTriggerSetDesc(assignments);
                raw.OnTriggerDesc = desc;
            }
            else if (onExpr is OnUpdateClause)
            {
                var updateClause = (OnUpdateClause) onExpr;
                var assignments = updateClause.Assignments
                    .Select(pair => MapExpressionDeep(pair.Value, mapContext))
                    .Select(expr => new OnTriggerSetAssignment(expr))
                    .ToList();
                var desc = new OnTriggerWindowUpdateDesc(
                    updateClause.WindowName, updateClause.OptionalAsName, assignments);
                raw.OnTriggerDesc = desc;
            }
            else if (onExpr is OnInsertSplitStreamClause)
            {
                var splitClause = (OnInsertSplitStreamClause) onExpr;
                mapContext.HasVariables = true;
                var streams = new List<OnTriggerSplitStream>();
                foreach (var item in splitClause.Items)
                {
                    OnTriggerSplitStreamFromClause fromClause = null;
                    if (item.PropertySelects != null)
                    {
                        var propertyEvalSpec = MapPropertySelects(item.PropertySelects, mapContext);
                        fromClause = new OnTriggerSplitStreamFromClause(
                            propertyEvalSpec, item.PropertySelectsStreamName);
                    }

                    ExprNode whereClause = null;
                    if (item.WhereClause != null)
                    {
                        whereClause = MapExpressionDeep(item.WhereClause, mapContext);
                    }

                    var insertDesc = MapInsertInto(item.InsertInto);
                    var selectDesc = MapSelectRaw(item.SelectClause, mapContext);

                    streams.Add(new OnTriggerSplitStream(insertDesc, selectDesc, fromClause, whereClause));
                }
                var desc = new OnTriggerSplitStreamDesc(OnTriggerType.ON_SPLITSTREAM, splitClause.IsFirst, streams);
                raw.OnTriggerDesc = desc;
            }
            else if (onExpr is OnMergeClause)
            {
                var merge = (OnMergeClause) onExpr;
                var matcheds = new List<OnTriggerMergeMatched>();
                foreach (var matchItem in merge.MatchItems)
                {
                    var actions = new List<OnTriggerMergeAction>();
                    foreach (var action in matchItem.Actions)
                    {
                        OnTriggerMergeAction actionItem;
                        if (action is OnMergeMatchedDeleteAction)
                        {
                            var delete = (OnMergeMatchedDeleteAction) action;
                            var optionalCondition = delete.WhereClause == null
                                ? null
                                : MapExpressionDeep(delete.WhereClause, mapContext);
                            actionItem = new OnTriggerMergeActionDelete(optionalCondition);
                        }
                        else if (action is OnMergeMatchedUpdateAction)
                        {
                            var update = (OnMergeMatchedUpdateAction) action;
                            var assignments = update.Assignments
                                .Select(pair => MapExpressionDeep(pair.Value, mapContext))
                                .Select(expr => new OnTriggerSetAssignment(expr))
                                .ToList();
                            var optionalCondition = update.WhereClause == null
                                ? null
                                : MapExpressionDeep(update.WhereClause, mapContext);
                            actionItem = new OnTriggerMergeActionUpdate(optionalCondition, assignments);
                        }
                        else if (action is OnMergeMatchedInsertAction)
                        {
                            var insert = (OnMergeMatchedInsertAction) action;
                            var columnNames = new List<string>(insert.ColumnNames);
                            var select = MapSelectClauseElements(insert.SelectList, mapContext);
                            var optionalCondition = insert.WhereClause == null
                                ? null
                                : MapExpressionDeep(insert.WhereClause, mapContext);
                            actionItem = new OnTriggerMergeActionInsert(
                                optionalCondition, insert.OptionalStreamName, columnNames, select);
                        }
                        else
                        {
                            throw new ArgumentException("Unrecognized merged action type '" + action.GetType() + "'");
                        }
                        actions.Add(actionItem);
                    }
                    var optionalConditionX = matchItem.OptionalCondition == null
                        ? null
                        : MapExpressionDeep(matchItem.OptionalCondition, mapContext);
                    matcheds.Add(new OnTriggerMergeMatched(matchItem.IsMatched, optionalConditionX, actions));
                }
                var mergeDesc = new OnTriggerMergeDesc(merge.WindowName, merge.OptionalAsName, matcheds);
                raw.OnTriggerDesc = mergeDesc;
            }
            else
            {
                throw new ArgumentException("Cannot map on-clause expression type : " + onExpr);
            }
        }

        private static void MapRowLimit(
            RowLimitClause rowLimitClause,
            StatementSpecRaw raw,
            StatementSpecMapContext mapContext)
        {
            if (rowLimitClause == null)
            {
                return;
            }
            if (rowLimitClause.NumRowsVariable != null)
            {
                raw.HasVariables = true;
                mapContext.VariableNames.Add(rowLimitClause.NumRowsVariable);
            }
            if (rowLimitClause.OptionalOffsetRowsVariable != null)
            {
                raw.HasVariables = true;
                mapContext.VariableNames.Add(rowLimitClause.OptionalOffsetRowsVariable);
            }
            raw.RowLimitSpec = new RowLimitSpec(
                rowLimitClause.NumRows, rowLimitClause.OptionalOffsetRows,
                rowLimitClause.NumRowsVariable, rowLimitClause.OptionalOffsetRowsVariable);
        }

        private static void MapForClause(ForClause clause, StatementSpecRaw raw, StatementSpecMapContext mapContext)
        {
            if ((clause == null) || (clause.Items.Count == 0))
            {
                return;
            }
            raw.ForClauseSpec = new ForClauseSpec();
            foreach (var item in clause.Items)
            {
                var specItem = new ForClauseItemSpec(
                    item.Keyword.Value.GetName(), MapExpressionDeep(item.Expressions, mapContext));
                raw.ForClauseSpec.Clauses.Add(specItem);
            }
        }

        private static void MapMatchRecognize(
            MatchRecognizeClause clause,
            StatementSpecRaw raw,
            StatementSpecMapContext mapContext)
        {
            if (clause == null)
            {
                return;
            }
            var spec = new MatchRecognizeSpec();
            spec.PartitionByExpressions = MapExpressionDeep(clause.PartitionExpressions, mapContext);

            var measures = clause.Measures
                .Select(item => new MatchRecognizeMeasureItem(MapExpressionDeep(
                    item.Expression, mapContext), item.AsName))
                .ToList();

            spec.Measures = measures;
            spec.IsAllMatches = clause.IsAll;
            spec.Skip = new MatchRecognizeSkip(clause.SkipClause.Xlate<MatchRecognizeSkipEnum>());

            var defines =
                clause.Defines
                    .Select(define => new MatchRecognizeDefineItem(define.Name, MapExpressionDeep(
                        define.Expression, mapContext)))
                    .ToList();
            spec.Defines = defines;

            if (clause.IntervalClause != null)
            {
                var timePeriod = (ExprTimePeriod) MapExpressionDeep(
                    clause.IntervalClause.Expression, mapContext);
                try
                {
                    timePeriod.Validate(
                        new ExprValidationContext(
                            mapContext.Container,
                            null, null, null, null, null, null, null, null, null, null, -1, null, null, null, false,
                            false, false, false, null, false));
                }
                catch (ExprValidationException e)
                {
                    throw new EPException("Error validating time-period expression: " + e.Message, e);
                }
                spec.Interval = new MatchRecognizeInterval(timePeriod, clause.IntervalClause.IsOrTerminated);
            }
            spec.Pattern = MapExpressionDeepRowRegex(clause.Pattern, mapContext);
            raw.MatchRecognizeSpec = spec;
        }

        private static void MapHaving(Expression havingClause, StatementSpecRaw raw, StatementSpecMapContext mapContext)
        {
            if (havingClause == null)
            {
                return;
            }
            var node = MapExpressionDeep(havingClause, mapContext);
            raw.HavingExprRootNode = node;
        }

        private static void UnmapHaving(
            ExprNode havingExprRootNode,
            EPStatementObjectModel model,
            StatementSpecUnMapContext unmapContext)
        {
            if (havingExprRootNode == null)
            {
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
            if (groupByClause == null)
            {
                return;
            }
            foreach (var expr in groupByClause.GroupByExpressions)
            {
                var element = MapGroupByExpr(expr, mapContext);
                raw.GroupByExpressions.Add(element);
            }
        }

        private static GroupByClauseElement MapGroupByExpr(
            GroupByClauseExpression expr,
            StatementSpecMapContext mapContext)
        {
            if (expr is GroupByClauseExpressionSingle)
            {
                var node = MapExpressionDeep(((GroupByClauseExpressionSingle) expr).Expression, mapContext);
                return new GroupByClauseElementExpr(node);
            }
            if (expr is GroupByClauseExpressionCombination)
            {
                IList<ExprNode> nodes = MapExpressionDeep(
                    ((GroupByClauseExpressionCombination) expr).Expressions, mapContext);
                return new GroupByClauseElementCombinedExpr(nodes);
            }
            if (expr is GroupByClauseExpressionGroupingSet)
            {
                var set = (GroupByClauseExpressionGroupingSet) expr;
                return new GroupByClauseElementGroupingSet(MapGroupByElements(set.Expressions, mapContext));
            }
            if (expr is GroupByClauseExpressionRollupOrCube)
            {
                var rollup = (GroupByClauseExpressionRollupOrCube) expr;
                return new GroupByClauseElementRollupOrCube(
                    rollup.IsCube, MapGroupByElements(rollup.Expressions, mapContext));
            }
            throw new IllegalStateException("Group by expression not recognized: " + expr);
        }

        private static IList<GroupByClauseElement> MapGroupByElements(
            IList<GroupByClauseExpression> elements,
            StatementSpecMapContext mapContext)
        {
            var @out = new List<GroupByClauseElement>();
            foreach (var element in elements)
            {
                @out.Add(MapGroupByExpr(element, mapContext));
            }
            return @out;
        }

        private static void UnmapGroupBy(
            IList<GroupByClauseElement> groupByExpressions,
            EPStatementObjectModel model,
            StatementSpecUnMapContext unmapContext)
        {
            if (groupByExpressions.Count == 0)
            {
                return;
            }
            var expressions = new List<GroupByClauseExpression>();
            foreach (var element in groupByExpressions)
            {
                expressions.Add(UnmapGroupByExpression(element, unmapContext));
            }
            model.GroupByClause = new GroupByClause(expressions);
        }

        private static GroupByClauseExpression UnmapGroupByExpression(
            GroupByClauseElement element,
            StatementSpecUnMapContext unmapContext)
        {
            if (element is GroupByClauseElementExpr)
            {
                var expr = (GroupByClauseElementExpr) element;
                var unmapped = UnmapExpressionDeep(expr.Expr, unmapContext);
                return new GroupByClauseExpressionSingle(unmapped);
            }
            if (element is GroupByClauseElementCombinedExpr)
            {
                var expr = (GroupByClauseElementCombinedExpr) element;
                IList<Expression> unmapped = UnmapExpressionDeep(expr.Expressions, unmapContext);
                return new GroupByClauseExpressionCombination(unmapped);
            }
            else if (element is GroupByClauseElementRollupOrCube)
            {
                var rollup = (GroupByClauseElementRollupOrCube) element;
                var elements = UnmapGroupByExpressions(rollup.RollupExpressions, unmapContext);
                return new GroupByClauseExpressionRollupOrCube(rollup.IsCube, elements);
            }
            else if (element is GroupByClauseElementGroupingSet)
            {
                var set = (GroupByClauseElementGroupingSet) element;
                var elements = UnmapGroupByExpressions(set.Elements, unmapContext);
                return new GroupByClauseExpressionGroupingSet(elements);
            }
            else
            {
                throw new IllegalStateException("Unrecognized group-by element " + element);
            }
        }

        private static IList<GroupByClauseExpression> UnmapGroupByExpressions(
            IList<GroupByClauseElement> rollupExpressions,
            StatementSpecUnMapContext unmapContext)
        {
            var @out = new List<GroupByClauseExpression>();
            foreach (var e in rollupExpressions)
            {
                @out.Add(UnmapGroupByExpression(e, unmapContext));
            }
            return @out;
        }


        private static void MapWhere(
            Expression whereClause, 
            StatementSpecRaw raw, 
            StatementSpecMapContext mapContext)
        {
            if (whereClause == null)
            {
                return;
            }
            var node = MapExpressionDeep(whereClause, mapContext);
            raw.FilterExprRootNode = node;
        }

        private static void UnmapWhere(
            ExprNode filterRootNode,
            EPStatementObjectModel model,
            StatementSpecUnMapContext unmapContext)
        {
            if (filterRootNode == null)
            {
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

            foreach (var stream in streamSpecs)
            {
                Stream targetStream;
                if (stream is FilterStreamSpecRaw)
                {
                    var filterStreamSpec = (FilterStreamSpecRaw) stream;
                    var filter = UnmapFilter(filterStreamSpec.RawFilterSpec, unmapContext);
                    var filterStream = new FilterStream(filter, filterStreamSpec.OptionalStreamName);
                    UnmapStreamOpts(stream.Options, filterStream);
                    targetStream = filterStream;
                }
                else if (stream is DBStatementStreamSpec)
                {
                    var db = (DBStatementStreamSpec) stream;
                    targetStream = new SQLStream(
                        db.DatabaseName, db.SqlWithSubsParams, db.OptionalStreamName, db.MetadataSQL);
                }
                else if (stream is PatternStreamSpecRaw)
                {
                    var pattern = (PatternStreamSpecRaw) stream;
                    var patternExpr = UnmapPatternEvalDeep(pattern.EvalFactoryNode, unmapContext);
                    var annotationParts = PatternLevelAnnotationUtil.AnnotationsFromSpec(pattern);
                    var patternStream = new PatternStream(patternExpr, pattern.OptionalStreamName, annotationParts);
                    UnmapStreamOpts(stream.Options, patternStream);
                    targetStream = patternStream;
                }
                else if (stream is MethodStreamSpec)
                {
                    var method = (MethodStreamSpec) stream;
                    var methodStream = new MethodInvocationStream(
                        method.ClassName, method.MethodName, method.OptionalStreamName);
                    foreach (var exprNode in method.Expressions)
                    {
                        var expr = UnmapExpressionDeep(exprNode, unmapContext);
                        methodStream.AddParameter(expr);
                    }
                    methodStream.OptionalEventTypeName = method.EventTypeName;
                    targetStream = methodStream;
                }
                else
                {
                    throw new ArgumentException("Stream modelled by " + stream.GetType() + " cannot be unmapped");
                }

                if (targetStream is ProjectedStream)
                {
                    var projStream = (ProjectedStream) targetStream;
                    foreach (var viewSpec in stream.ViewSpecs)
                    {
                        IList<Expression> viewExpressions = UnmapExpressionDeep(viewSpec.ObjectParameters, unmapContext);
                        projStream.AddView(View.Create(viewSpec.ObjectNamespace, viewSpec.ObjectName, viewExpressions));
                    }
                }
                from.Add(targetStream);
            }

            foreach (var desc in outerJoinDescList)
            {
                PropertyValueExpression left = null;
                PropertyValueExpression right = null;
                var additionalProperties = new List<PropertyValueExpressionPair>();

                if (desc.OptLeftNode != null)
                {
                    left = (PropertyValueExpression) UnmapExpressionFlat(desc.OptLeftNode, unmapContext);
                    right = (PropertyValueExpression) UnmapExpressionFlat(desc.OptRightNode, unmapContext);

                    if (desc.AdditionalLeftNodes != null)
                    {
                        for (var i = 0; i < desc.AdditionalLeftNodes.Length; i++)
                        {
                            var leftNode = desc.AdditionalLeftNodes[i];
                            var rightNode = desc.AdditionalRightNodes[i];
                            var propLeft = (PropertyValueExpression) UnmapExpressionFlat(leftNode, unmapContext);
                            var propRight = (PropertyValueExpression) UnmapExpressionFlat(rightNode, unmapContext);
                            additionalProperties.Add(new PropertyValueExpressionPair(propLeft, propRight));
                        }
                    }
                }
                from.Add(new OuterJoinQualifier(desc.OuterJoinType, left, right, additionalProperties));
            }
        }

        private static void UnmapStreamOpts(StreamSpecOptions options, ProjectedStream stream)
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
            clause.StreamSelector = SelectClauseStreamSelectorEnumExtensions.MapFromSODA(selectStreamSelectorEnum);
            clause.AddElements(UnmapSelectClauseElements(selectClauseSpec.SelectExprList, unmapContext));
            clause.Distinct(selectClauseSpec.IsDistinct);
            return clause;
        }

        private static IList<SelectClauseElement> UnmapSelectClauseElements(
            IList<SelectClauseElementRaw> selectExprList,
            StatementSpecUnMapContext unmapContext)
        {
            var elements = new List<SelectClauseElement>();
            foreach (var raw in selectExprList)
            {
                if (raw is SelectClauseStreamRawSpec)
                {
                    var streamSpec = (SelectClauseStreamRawSpec) raw;
                    elements.Add(new SelectClauseStreamWildcard(streamSpec.StreamName, streamSpec.OptionalAsName));
                }
                else if (raw is SelectClauseElementWildcard)
                {
                    elements.Add(new SelectClauseWildcard());
                }
                else if (raw is SelectClauseExprRawSpec)
                {
                    var rawSpec = (SelectClauseExprRawSpec) raw;
                    var expression = UnmapExpressionDeep(rawSpec.SelectExpression, unmapContext);
                    var selectExpr = new SelectClauseExpression(expression, rawSpec.OptionalAsName);
                    selectExpr.IsAnnotatedByEventFlag = rawSpec.IsEvents;
                    elements.Add(selectExpr);
                }
                else
                {
                    throw new IllegalStateException("Unexpected select clause element typed " + raw.GetType().FullName);
                }
            }
            return elements;
        }

        private static InsertIntoClause UnmapInsertInto(InsertIntoDesc insertIntoDesc)
        {
            if (insertIntoDesc == null)
            {
                return null;
            }
            StreamSelector selector = SelectClauseStreamSelectorEnumExtensions.MapFromSODA(
                insertIntoDesc.StreamSelector);
            return InsertIntoClause.Create(
                insertIntoDesc.EventTypeName,
                insertIntoDesc.ColumnNames.ToArray(), selector);
        }

        private static void MapCreateContext(
            CreateContextClause createContext,
            StatementSpecRaw raw,
            StatementSpecMapContext mapContext)
        {
            if (createContext == null)
            {
                return;
            }

            var detail = MapCreateContextDetail(
                createContext.Descriptor, mapContext);

            var desc = new CreateContextDesc(createContext.ContextName, detail);
            raw.CreateContextDesc = desc;
        }

        private static ContextDetail MapCreateContextDetail(
            ContextDescriptor descriptor,
            StatementSpecMapContext mapContext)
        {
            ContextDetail detail;
            if (descriptor is ContextDescriptorInitiatedTerminated)
            {
                var desc = (ContextDescriptorInitiatedTerminated) descriptor;
                var start = MapCreateContextRangeCondition(
                    desc.StartCondition, mapContext);
                var end = MapCreateContextRangeCondition(
                    desc.EndCondition, mapContext);
                ExprNode[] distinctExpressions = null;
                if (desc.OptionalDistinctExpressions != null && desc.OptionalDistinctExpressions.Count > 0)
                {
                    distinctExpressions =
                        ExprNodeUtility.ToArray(MapExpressionDeep(desc.OptionalDistinctExpressions, mapContext));
                }
                detail = new ContextDetailInitiatedTerminated(start, end, desc.IsOverlapping, distinctExpressions);
            }
            else if (descriptor is ContextDescriptorKeyedSegmented)
            {
                var seg = (ContextDescriptorKeyedSegmented) descriptor;
                var itemsdesc = new List<ContextDetailPartitionItem>();
                foreach (var item in seg.Items)
                {
                    var rawSpec = MapFilter(item.Filter, mapContext);
                    itemsdesc.Add(new ContextDetailPartitionItem(rawSpec, item.PropertyNames));
                }
                detail = new ContextDetailPartitioned(itemsdesc);
            }
            else if (descriptor is ContextDescriptorCategory)
            {
                var cat = (ContextDescriptorCategory) descriptor;
                var rawSpec = MapFilter(cat.Filter, mapContext);
                var itemsdesc = new List<ContextDetailCategoryItem>();
                foreach (var item in cat.Items)
                {
                    var expr = MapExpressionDeep(
                        item.Expression, mapContext);
                    itemsdesc.Add(new ContextDetailCategoryItem(expr, item.Label));
                }
                detail = new ContextDetailCategory(itemsdesc, rawSpec);
            }
            else if (descriptor is ContextDescriptorHashSegmented)
            {
                var hash = (ContextDescriptorHashSegmented) descriptor;
                var itemsdesc = new List<ContextDetailHashItem>();
                foreach (var item in hash.Items)
                {
                    var rawSpec = MapFilter(item.Filter, mapContext);
                    var singleRowMethodExpression = (SingleRowMethodExpression) item.HashFunction;
                    var func = MapChains(Collections.SingletonList(singleRowMethodExpression.Chain[0]), mapContext)[0];
                    itemsdesc.Add(new ContextDetailHashItem(func, rawSpec));
                }
                detail = new ContextDetailHash(itemsdesc, hash.Granularity, hash.IsPreallocate);
            }
            else
            {
                var nested = (ContextDescriptorNested) descriptor;
                var itemsdesc = new List<CreateContextDesc>();
                foreach (var item in nested.Contexts)
                {
                    itemsdesc.Add(new CreateContextDesc(item.ContextName, MapCreateContextDetail(
                        item.Descriptor, mapContext)));
                }
                detail = new ContextDetailNested(itemsdesc);
            }
            return detail;
        }

        private static ContextDetailCondition MapCreateContextRangeCondition(
            ContextDescriptorCondition condition,
            StatementSpecMapContext mapContext)
        {
            if (condition is ContextDescriptorConditionCrontab)
            {
                var crontab = (ContextDescriptorConditionCrontab) condition;
                IList<ExprNode> expr = MapExpressionDeep(crontab.CrontabExpressions, mapContext);
                return new ContextDetailConditionCrontab(expr, crontab.IsNow);
            }
            else if (condition is ContextDescriptorConditionFilter)
            {
                var filter = (ContextDescriptorConditionFilter) condition;
                var filterExpr = MapFilter(filter.Filter, mapContext);
                return new ContextDetailConditionFilter(filterExpr, filter.OptionalAsName);
            }
            if (condition is ContextDescriptorConditionPattern)
            {
                var pattern = (ContextDescriptorConditionPattern) condition;
                var patternExpr = MapPatternEvalDeep(pattern.Pattern, mapContext);
                return new ContextDetailConditionPattern(patternExpr, pattern.IsInclusive, pattern.IsNow);
            }
            if (condition is ContextDescriptorConditionTimePeriod)
            {
                var timePeriod = (ContextDescriptorConditionTimePeriod) condition;
                var expr = MapExpressionDeep(timePeriod.TimePeriod, mapContext);
                return new ContextDetailConditionTimePeriod((ExprTimePeriod) expr, timePeriod.IsNow);
            }
            if (condition is ContextDescriptorConditionImmediate)
            {
                return ContextDetailConditionImmediate.INSTANCE;
            }
            if (condition is ContextDescriptorConditionNever)
            {
                return ContextDetailConditionNever.INSTANCE;
            }
            throw new IllegalStateException("Unrecognized condition " + condition);
        }

        private static void MapCreateWindow(
            CreateWindowClause createWindow,
            FromClause fromClause,
            StatementSpecRaw raw,
            StatementSpecMapContext mapContext)
        {
            if (createWindow == null)
            {
                return;
            }

            ExprNode insertFromWhereExpr = null;
            if (createWindow.InsertWhereClause != null)
            {
                insertFromWhereExpr = MapExpressionDeep(createWindow.InsertWhereClause, mapContext);
            }
            var columns = MapColumns(createWindow.Columns);

            string asEventTypeName = null;
            if (fromClause != null && !fromClause.Streams.IsEmpty() && fromClause.Streams[0] is FilterStream)
            {
                asEventTypeName = ((FilterStream) fromClause.Streams[0]).Filter.EventTypeName;
            }
            raw.CreateWindowDesc = new CreateWindowDesc(
                createWindow.WindowName,
                MapViews(createWindow.Views, mapContext),
                StreamSpecOptions.DEFAULT, createWindow.IsInsert, insertFromWhereExpr, columns, asEventTypeName);
        }

        private static void MapCreateIndex(
            CreateIndexClause clause,
            StatementSpecRaw raw,
            StatementSpecMapContext mapContext)
        {
            if (clause == null)
            {
                return;
            }

            var cols = clause.Columns
                .Select(col => MapCreateIndexCol(col, mapContext))
                .ToList();

            var desc = new CreateIndexDesc(clause.IsUnique, clause.IndexName, clause.WindowName, cols);
            raw.CreateIndexDesc = desc;
        }

        private static CreateIndexItem MapCreateIndexCol(
            CreateIndexColumn col, 
            StatementSpecMapContext mapContext)
        {
            var columns = MapExpressionDeep(col.Columns, mapContext);
            var parameters = MapExpressionDeep(col.Parameters, mapContext);
            return new CreateIndexItem(columns, col.IndexType == null ? CreateIndexType.HASH.GetNameInvariant() : col.IndexType, parameters);
        }

        private static void MapUpdateClause(
            UpdateClause updateClause,
            StatementSpecRaw raw,
            StatementSpecMapContext mapContext)
        {
            if (updateClause == null)
            {
                return;
            }
            var assignments = new List<OnTriggerSetAssignment>();
            foreach (var pair in updateClause.Assignments)
            {
                var expr = MapExpressionDeep(pair.Value, mapContext);
                assignments.Add(new OnTriggerSetAssignment(expr));
            }
            ExprNode whereClause = null;
            if (updateClause.OptionalWhereClause != null)
            {
                whereClause = MapExpressionDeep(updateClause.OptionalWhereClause, mapContext);
            }
            var desc = new UpdateDesc(updateClause.OptionalAsClauseStreamName, assignments, whereClause);
            raw.UpdateDesc = desc;
            var filterSpecRaw = new FilterSpecRaw(updateClause.EventType, Collections.GetEmptyList<ExprNode>(), null);
            raw.StreamSpecs.Add(
                new FilterStreamSpecRaw(filterSpecRaw, ViewSpec.EMPTY_VIEWSPEC_ARRAY, null, StreamSpecOptions.DEFAULT));
        }

        private static void MapCreateVariable(
            CreateVariableClause createVariable,
            StatementSpecRaw raw,
            StatementSpecMapContext mapContext)
        {
            if (createVariable == null)
            {
                return;
            }

            ExprNode assignment = null;
            if (createVariable.OptionalAssignment != null)
            {
                assignment = MapExpressionDeep(createVariable.OptionalAssignment, mapContext);
            }
            raw.CreateVariableDesc = new CreateVariableDesc(
                createVariable.VariableType, createVariable.VariableName, assignment, createVariable.IsConstant,
                createVariable.IsArray, createVariable.IsArrayOfPrimitive);
        }

        private static void MapCreateTable(
            CreateTableClause createTable,
            StatementSpecRaw raw,
            StatementSpecMapContext mapContext)
        {
            if (createTable == null)
            {
                return;
            }

            var cols = new List<CreateTableColumn>();
            foreach (var desc in createTable.Columns)
            {
                var optNode = desc.OptionalExpression != null
                    ? MapExpressionDeep(desc.OptionalExpression, mapContext)
                    : null;
                var annotations = MapAnnotations(desc.Annotations);
                cols.Add(
                    new CreateTableColumn(
                        desc.ColumnName, optNode, desc.OptionalTypeName, desc.OptionalTypeIsArray,
                        desc.OptionalTypeIsPrimitiveArray, annotations, desc.PrimaryKey));
            }

            var agg = new CreateTableDesc(createTable.TableName, cols);
            raw.CreateTableDesc = agg;
        }

        private static void MapCreateSchema(
            CreateSchemaClause clause,
            StatementSpecRaw raw,
            StatementSpecMapContext mapContext)
        {
            if (clause == null)
            {
                return;
            }
            var desc = MapCreateSchemaInternal(clause, raw, mapContext);
            raw.CreateSchemaDesc = desc;
        }

        private static void MapCreateExpression(
            CreateExpressionClause clause,
            StatementSpecRaw raw,
            StatementSpecMapContext mapContext)
        {
            if (clause == null)
            {
                return;
            }

            CreateExpressionDesc desc;
            if (clause.ExpressionDeclaration != null)
            {
                var item = MapExpressionDeclItem(clause.ExpressionDeclaration, mapContext);
                desc = new CreateExpressionDesc(item);
            }
            else
            {
                var item = MapScriptExpression(clause.ScriptExpression, mapContext);
                desc = new CreateExpressionDesc(item);
            }
            raw.CreateExpressionDesc = desc;
        }

        private static CreateSchemaDesc MapCreateSchemaInternal(
            CreateSchemaClause clause,
            StatementSpecRaw raw,
            StatementSpecMapContext mapContext)
        {
            var columns = MapColumns(clause.Columns);
            return new CreateSchemaDesc(
                clause.SchemaName, clause.Types, columns, clause.Inherits,
                AssignedTypeExtensions.MapFrom(clause.TypeDefinition.Value),
                clause.StartTimestampPropertyName,
                clause.EndTimestampPropertyName, clause.CopyFrom);
        }

        private static void MapCreateGraph(
            CreateDataFlowClause clause,
            StatementSpecRaw raw,
            StatementSpecMapContext mapContext)
        {
            if (clause == null)
            {
                return;
            }

            var schemas = new List<CreateSchemaDesc>();
            foreach (var schema in clause.Schemas)
            {
                schemas.Add(MapCreateSchemaInternal(schema, raw, mapContext));
            }

            var ops = new List<GraphOperatorSpec>();
            foreach (var op in clause.Operators)
            {
                ops.Add(MapGraphOperator(op, mapContext));
            }

            var desc = new CreateDataFlowDesc(clause.DataFlowName, ops, schemas);
            raw.CreateDataFlowDesc = desc;
        }

        private static GraphOperatorSpec MapGraphOperator(
            DataFlowOperator op, StatementSpecMapContext mapContext)
        {
            var annotations = MapAnnotations(op.Annotations);

            var input = new GraphOperatorInput();
            foreach (DataFlowOperatorInput @in in op.Input)
            {
                input.StreamNamesAndAliases.Add(
                    new GraphOperatorInputNamesAlias(@in.InputStreamNames.ToArray(), @in.OptionalAsName));
            }

            var output = new GraphOperatorOutput();
            foreach (var @out in op.Output)
            {
                output.Items.Add(new GraphOperatorOutputItem(@out.StreamName, MapGraphOpType(@out.TypeInfo)));
            }

            var detail = new LinkedHashMap<string, Object>();
            foreach (var entry in op.Parameters)
            {
                var value = entry.ParameterValue;
                if (value is EPStatementObjectModel)
                {
                    value = Map((EPStatementObjectModel) value, mapContext);
                }
                else if (value is Expression)
                {
                    value = MapExpressionDeep(
                        (Expression) value, mapContext);
                }
                else
                {
                    // no action
                }
                detail.Put(entry.ParameterName, value);
            }

            return new GraphOperatorSpec(op.OperatorName, input, output, new GraphOperatorDetail(detail), annotations);
        }

        private static IList<GraphOperatorOutputItemType> MapGraphOpType(IList<DataFlowOperatorOutputType> typeInfos)
        {
            if (typeInfos == null)
            {
                return Collections.GetEmptyList<GraphOperatorOutputItemType>();
            }
            var types = new List<GraphOperatorOutputItemType>();
            foreach (var info in typeInfos)
            {
                var type = new GraphOperatorOutputItemType(
                    info.IsWildcard, info.TypeOrClassname, MapGraphOpType(info.TypeParameters));
                types.Add(type);
            }
            return types;
        }

        private static IList<ColumnDesc> MapColumns(IList<SchemaColumnDesc> columns)
        {
            if (columns == null)
            {
                return null;
            }
            var result = new List<ColumnDesc>();
            foreach (var col in columns)
            {
                result.Add(new ColumnDesc(col.Name, col.Type, col.IsArray, col.IsPrimitiveArray));
            }
            return result;
        }

        private static IList<SchemaColumnDesc> UnmapColumns(IList<ColumnDesc> columns)
        {
            if (columns == null)
            {
                return null;
            }
            var result = new List<SchemaColumnDesc>();
            foreach (var col in columns)
            {
                result.Add(new SchemaColumnDesc(col.Name, col.Type, col.IsArray, col.IsPrimitiveArray));
            }
            return result;
        }

        private static InsertIntoDesc MapInsertInto(InsertIntoClause insertInto)
        {
            if (insertInto == null)
            {
                return null;
            }

            var eventTypeName = insertInto.StreamName;
            var desc =
                new InsertIntoDesc(
                    SelectClauseStreamSelectorEnumExtensions.MapFromSODA(insertInto.StreamSelector), eventTypeName);

            foreach (var name in insertInto.ColumnNames)
            {
                desc.Add(name);
            }
            return desc;
        }

        private static void MapSelect(
            SelectClause selectClause,
            StatementSpecRaw raw,
            StatementSpecMapContext mapContext)
        {
            if (selectClause == null)
            {
                return;
            }
            var spec = MapSelectRaw(selectClause, mapContext);
            raw.SelectStreamDirEnum = SelectClauseStreamSelectorEnumExtensions.MapFromSODA(selectClause.StreamSelector);
            raw.SelectClauseSpec = spec;
        }

        private static IList<SelectClauseElementRaw> MapSelectClauseElements(
            IList<SelectClauseElement> elements,
            StatementSpecMapContext mapContext)
        {
            var result = new List<SelectClauseElementRaw>();
            foreach (var element in elements)
            {
                if (element is SelectClauseWildcard)
                {
                    result.Add(new SelectClauseElementWildcard());
                }
                else if (element is SelectClauseExpression)
                {
                    var selectExpr = (SelectClauseExpression) element;
                    var expr = selectExpr.Expression;
                    var exprNode = MapExpressionDeep(expr, mapContext);
                    var rawElement = new SelectClauseExprRawSpec(
                        exprNode, selectExpr.AsName, selectExpr.IsAnnotatedByEventFlag);
                    result.Add(rawElement);
                }
                else if (element is SelectClauseStreamWildcard)
                {
                    var streamWild = (SelectClauseStreamWildcard) element;
                    var rawElement = new SelectClauseStreamRawSpec(streamWild.StreamName, streamWild.OptionalColumnName);
                    result.Add(rawElement);
                }
            }
            return result;
        }

        private static SelectClauseSpecRaw MapSelectRaw(
            SelectClause selectClause, StatementSpecMapContext mapContext)
        {
            var spec = new SelectClauseSpecRaw();
            spec.AddAll(MapSelectClauseElements(selectClause.SelectList, mapContext));
            spec.IsDistinct = selectClause.IsDistinct;
            return spec;
        }

        private static Expression UnmapExpressionDeep(
            ExprNode exprNode, StatementSpecUnMapContext unmapContext)
        {
            if (exprNode == null)
            {
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
            var result = new List<ExprNode>();
            if (expressions == null)
            {
                return result;
            }
            foreach (var expr in expressions)
            {
                if (expr == null)
                {
                    result.Add(null);
                    continue;
                }
                result.Add(MapExpressionDeep(expr, mapContext));
            }
            return result;
        }

        private static MatchRecognizeRegEx UnmapExpressionDeepRowRegex(
            RowRegexExprNode exprNode,
            StatementSpecUnMapContext unmapContext)
        {
            var parent = UnmapExpressionFlatRowregex(exprNode, unmapContext);
            UnmapExpressionRecursiveRowregex(parent, exprNode, unmapContext);
            return parent;
        }

        private static ExprNode MapExpressionDeep(
            Expression expr, 
            StatementSpecMapContext mapContext)
        {
            if (expr == null)
            {
                return null;
            }
            var parent = MapExpressionFlat(expr, mapContext);
            MapExpressionRecursive(parent, expr, mapContext);
            return parent;
        }

        private static RowRegexExprNode MapExpressionDeepRowRegex(
            MatchRecognizeRegEx expr,
            StatementSpecMapContext mapContext)
        {
            var parent = MapExpressionFlatRowregex(expr, mapContext);
            MapExpressionRecursiveRowregex(parent, expr, mapContext);
            return parent;
        }

        private static ExprNode MapExpressionFlat(
            Expression expr, 
            StatementSpecMapContext mapContext)
        {
            if (expr == null)
            {
                throw new ArgumentException("Null expression parameter");
            }
            if (expr is ArithmaticExpression)
            {
                var arith = (ArithmaticExpression) expr;
                return new ExprMathNode(
                    MathArithTypeEnumExtensions.ParseOperator(arith.Operator),
                    mapContext.Configuration.EngineDefaults.Expression.IsIntegerDivision,
                    mapContext.Configuration.EngineDefaults.Expression.IsDivisionByZeroReturnsNull);
            }
            else if (expr is PropertyValueExpression)
            {
                var prop = (PropertyValueExpression) expr;
                var indexDot = ASTUtil.UnescapedIndexOfDot(prop.PropertyName);

                // handle without nesting
                if (indexDot == -1)
                {

                    // maybe table
                    if (mapContext.TableService.GetTableMetadata(prop.PropertyName) != null)
                    {
                        var tableNodeX = new ExprTableAccessNodeTopLevel(prop.PropertyName);
                        mapContext.TableExpressions.Add(tableNodeX);
                        return tableNodeX;
                    }

                    // maybe variable
                    var variableMetaDataX = mapContext.VariableService.GetVariableMetaData(prop.PropertyName);
                    if (variableMetaDataX != null)
                    {
                        mapContext.HasVariables = true;
                        var node = new ExprVariableNodeImpl(variableMetaDataX, null);
                        mapContext.VariableNames.Add(variableMetaDataX.VariableName);
                        var message = VariableServiceUtil.CheckVariableContextName(
                            mapContext.ContextName, variableMetaDataX);
                        if (message != null)
                        {
                            throw new EPException(message);
                        }
                        return node;
                    }

                    return new ExprIdentNodeImpl(prop.PropertyName);
                }

                var stream = prop.PropertyName.Substring(0, indexDot);
                var property = prop.PropertyName.Substring(indexDot + 1);

                var tableNode = ASTTableExprHelper.CheckTableNameGetExprForSubproperty(
                    mapContext.TableService, stream, property);
                if (tableNode != null)
                {
                    mapContext.TableExpressions.Add(tableNode.First);
                    return tableNode.First;
                }

                var variableMetaData = mapContext.VariableService.GetVariableMetaData(stream);
                if (variableMetaData != null)
                {
                    mapContext.HasVariables = true;
                    var node = new ExprVariableNodeImpl(variableMetaData, property);
                    mapContext.VariableNames.Add(variableMetaData.VariableName);
                    var message = VariableServiceUtil.CheckVariableContextName(mapContext.ContextName, variableMetaData);
                    if (message != null)
                    {
                        throw new EPException(message);
                    }
                    return node;
                }

                if (mapContext.ContextName != null)
                {
                    var contextDescriptor =
                        mapContext.ContextManagementService.GetContextDescriptor(mapContext.ContextName);
                    if (contextDescriptor != null &&
                        contextDescriptor.ContextPropertyRegistry.IsContextPropertyPrefix(stream))
                    {
                        return new ExprContextPropertyNode(property);
                    }
                }

                return new ExprIdentNodeImpl(property, stream);
            }
            else if (expr is Conjunction)
            {
                return new ExprAndNodeImpl();
            }
            else if (expr is Disjunction)
            {
                return new ExprOrNode();
            }
            else if (expr is RelationalOpExpression)
            {
                var op = (RelationalOpExpression) expr;
                if (op.Operator.Equals("="))
                {
                    return new ExprEqualsNodeImpl(false, false);
                }
                else if (op.Operator.Equals("!="))
                {
                    return new ExprEqualsNodeImpl(true, false);
                }
                else if (op.Operator.ToUpperInvariant().Trim().Equals("IS"))
                {
                    return new ExprEqualsNodeImpl(false, true);
                }
                else if (op.Operator.ToUpperInvariant().Trim().Equals("IS NOT"))
                {
                    return new ExprEqualsNodeImpl(true, true);
                }
                else
                {
                    return new ExprRelationalOpNodeImpl(RelationalOpEnumExtensions.Parse(op.Operator));
                }
            }
            else if (expr is ConstantExpression)
            {
                var op = (ConstantExpression) expr;
                Type constantType = null;
                if (op.ConstantType != null)
                {
                    try
                    {
                        constantType = mapContext.EngineImportService.GetClassForNameProvider().ClassForName(op.ConstantType);
                    }
                    catch (TypeLoadException e)
                    {
                        throw new EPException(
                            "Error looking up class name '" + op.ConstantType + "' to resolve as constant type", e);
                    }
                }
                return new ExprConstantNodeImpl(op.Constant, constantType);
            }
            else if (expr is ConcatExpression)
            {
                return new ExprConcatNode();
            }
            else if (expr is SubqueryExpression)
            {
                var sub = (SubqueryExpression) expr;
                var rawSubselect = Map(sub.Model, mapContext);
                return new ExprSubselectRowNode(rawSubselect);
            }
            else if (expr is SubqueryInExpression)
            {
                var sub = (SubqueryInExpression) expr;
                var rawSubselect = Map(sub.Model, mapContext);
                var inSub = new ExprSubselectInNode(rawSubselect, sub.IsNotIn);
                return inSub;
            }
            else if (expr is SubqueryExistsExpression)
            {
                var sub = (SubqueryExistsExpression) expr;
                var rawSubselect = Map(sub.Model, mapContext);
                return new ExprSubselectExistsNode(rawSubselect);
            }
            else if (expr is SubqueryQualifiedExpression)
            {
                var sub = (SubqueryQualifiedExpression) expr;
                var rawSubselect = Map(sub.Model, mapContext);
                var isNot = false;
                RelationalOpEnum? relop = null;
                if (sub.Operator.Equals("!="))
                {
                    isNot = true;
                }
                if (sub.Operator.Equals("="))
                {
                }
                else
                {
                    relop = RelationalOpEnumExtensions.Parse(sub.Operator);
                }
                return new ExprSubselectAllSomeAnyNode(rawSubselect, isNot, sub.IsAll, relop);
            }
            else if (expr is CountStarProjectionExpression)
            {
                return new ExprCountNode(false);
            }
            else if (expr is CountProjectionExpression)
            {
                var count = (CountProjectionExpression) expr;
                return new ExprCountNode(count.IsDistinct);
            }
            else if (expr is AvgProjectionExpression)
            {
                var avg = (AvgProjectionExpression) expr;
                return new ExprAvgNode(avg.IsDistinct);
            }
            else if (expr is SumProjectionExpression)
            {
                var avg = (SumProjectionExpression) expr;
                return new ExprSumNode(avg.IsDistinct);
            }
            else if (expr is BetweenExpression)
            {
                var between = (BetweenExpression) expr;
                return new ExprBetweenNodeImpl(
                    between.IsLowEndpointIncluded, between.IsHighEndpointIncluded, between.IsNotBetween);
            }
            else if (expr is PriorExpression)
            {
                return new ExprPriorNode();
            }
            else if (expr is PreviousExpression)
            {
                var prev = (PreviousExpression) expr;
                return
                    new ExprPreviousNode(EnumHelper.Parse<ExprPreviousNodePreviousType>(prev.ExpressionType.ToString()));
            }
            else if (expr is StaticMethodExpression)
            {
                var method = (StaticMethodExpression) expr;
                var chained = MapChains(method.Chain, mapContext);
                chained.Insert(0, new ExprChainedSpec(method.ClassName, Collections.GetEmptyList<ExprNode>(), false));
                return new ExprDotNodeImpl(
                    chained,
                    mapContext.Configuration.EngineDefaults.Expression.IsDuckTyping,
                    mapContext.Configuration.EngineDefaults.Expression.IsUdfCache);
            }
            else if (expr is MinProjectionExpression)
            {
                var method = (MinProjectionExpression) expr;
                return new ExprMinMaxAggrNode(
                    method.IsDistinct, MinMaxTypeEnum.MIN, expr.Children.Count > 1, method.IsEver);
            }
            else if (expr is MaxProjectionExpression)
            {
                var method = (MaxProjectionExpression) expr;
                return new ExprMinMaxAggrNode(
                    method.IsDistinct, MinMaxTypeEnum.MAX, expr.Children.Count > 1, method.IsEver);
            }
            else if (expr is NotExpression)
            {
                return new ExprNotNode();
            }
            else if (expr is InExpression)
            {
                var inExpr = (InExpression) expr;
                return new ExprInNodeImpl(inExpr.IsNotIn);
            }
            else if (expr is CoalesceExpression)
            {
                return new ExprCoalesceNode();
            }
            else if (expr is CaseWhenThenExpression)
            {
                return new ExprCaseNode(false);
            }
            else if (expr is CaseSwitchExpression)
            {
                return new ExprCaseNode(true);
            }
            else if (expr is MaxRowExpression)
            {
                return new ExprMinMaxRowNode(MinMaxTypeEnum.MAX);
            }
            else if (expr is MinRowExpression)
            {
                return new ExprMinMaxRowNode(MinMaxTypeEnum.MIN);
            }
            else if (expr is BitwiseOpExpression)
            {
                var bit = (BitwiseOpExpression) expr;
                return new ExprBitWiseNode(bit.BinaryOp);
            }
            else if (expr is ArrayExpression)
            {
                return new ExprArrayNode();
            }
            else if (expr is LikeExpression)
            {
                var like = (LikeExpression) expr;
                return new ExprLikeNode(like.IsNot);
            }
            else if (expr is RegExpExpression)
            {
                var regexp = (RegExpExpression) expr;
                return new ExprRegexpNode(regexp.IsNot);
            }
            else if (expr is MedianProjectionExpression)
            {
                var median = (MedianProjectionExpression) expr;
                return new ExprMedianNode(median.IsDistinct);
            }
            else if (expr is AvedevProjectionExpression)
            {
                var node = (AvedevProjectionExpression) expr;
                return new ExprAvedevNode(node.IsDistinct);
            }
            else if (expr is StddevProjectionExpression)
            {
                var node = (StddevProjectionExpression) expr;
                return new ExprStddevNode(node.IsDistinct);
            }
            else if (expr is LastEverProjectionExpression)
            {
                var node = (LastEverProjectionExpression) expr;
                return new ExprLastEverNode(node.IsDistinct);
            }
            else if (expr is FirstEverProjectionExpression)
            {
                var node = (FirstEverProjectionExpression) expr;
                return new ExprFirstEverNode(node.IsDistinct);
            }
            else if (expr is CountEverProjectionExpression)
            {
                var node = (CountEverProjectionExpression) expr;
                return new ExprCountEverNode(node.IsDistinct);
            }
            else if (expr is InstanceOfExpression)
            {
                var node = (InstanceOfExpression) expr;
                return new ExprInstanceofNode(node.TypeNames, mapContext.LockManager);
            }
            else if (expr is TypeOfExpression)
            {
                return new ExprTypeofNode();
            }
            else if (expr is CastExpression)
            {
                var node = (CastExpression) expr;
                return new ExprCastNode(node.TypeName);
            }
            else if (expr is PropertyExistsExpression)
            {
                return new ExprPropertyExistsNode();
            }
            else if (expr is CurrentTimestampExpression)
            {
                return new ExprTimestampNode();
            }
            else if (expr is CurrentEvaluationContextExpression)
            {
                return new ExprCurrentEvaluationContextNode();
            }
            else if (expr is IStreamBuiltinExpression)
            {
                return new ExprIStreamNode();
            }
            else if (expr is TimePeriodExpression)
            {
                var tpe = (TimePeriodExpression) expr;
                return new ExprTimePeriodImpl(
                    mapContext.Configuration.EngineDefaults.Expression.TimeZone,
                    tpe.HasYears, 
                    tpe.HasMonths,
                    tpe.HasWeeks,
                    tpe.HasDays,
                    tpe.HasHours,
                    tpe.HasMinutes,
                    tpe.HasSeconds,
                    tpe.HasMilliseconds,
                    tpe.HasMicroseconds,
                    mapContext.EngineImportService.TimeAbacus,
                    mapContext.LockManager);
            }
            else if (expr is NewOperatorExpression)
            {
                var noe = (NewOperatorExpression) expr;
                return new ExprNewStructNode(noe.ColumnNames.ToArray());
            }
            else if (expr is NewInstanceOperatorExpression)
            {
                var noe = (NewInstanceOperatorExpression) expr;
                return new ExprNewInstanceNode(noe.ClassName);
            }
            else if (expr is CompareListExpression)
            {
                var exp = (CompareListExpression) expr;
                if ((exp.Operator.Equals("=")) || (exp.Operator.Equals("!=")))
                {
                    return new ExprEqualsAllAnyNode(exp.Operator.Equals("!="), exp.IsAll);
                }
                else
                {
                    return new ExprRelationalOpAllAnyNode(RelationalOpEnumExtensions.Parse(exp.Operator), exp.IsAll);
                }
            }
            else if (expr is SubstitutionParameterExpressionBase)
            {
                var node = (SubstitutionParameterExpressionBase) expr;
                if (!(node.IsSatisfied))
                {
                    if (node is SubstitutionParameterExpressionIndexed)
                    {
                        var indexed = (SubstitutionParameterExpressionIndexed) node;
                        throw new EPException(
                            "Substitution parameter value for index " + indexed.Index +
                            " not set, please provide a value for this parameter");
                    }
                    var named = (SubstitutionParameterExpressionNamed) node;
                    throw new EPException(
                        "Substitution parameter value for name '" + named.Name +
                        "' not set, please provide a value for this parameter");
                }
                return new ExprConstantNodeImpl(node.Constant);
            }
            else if (expr is SingleRowMethodExpression)
            {
                var single = (SingleRowMethodExpression) expr;
                if ((single.Chain == null) || (single.Chain.Count == 0))
                {
                    throw new ArgumentException("Single row method expression requires one or more method calls");
                }
                var chain = MapChains(single.Chain, mapContext);
                var functionName = chain[0].Name;
                Pair<Type, EngineImportSingleRowDesc> pair;
                try
                {
                    pair = mapContext.EngineImportService.ResolveSingleRow(functionName);
                }
                catch (Exception e)
                {
                    throw new ArgumentException(
                        "Function name '" + functionName + "' cannot be resolved to a single-row function: " + e.Message,
                        e);
                }
                chain[0].Name = pair.Second.MethodName;
                return new ExprPlugInSingleRowNode(functionName, pair.First, chain, pair.Second);
            }
            else if (expr is PlugInProjectionExpression)
            {
                var node = (PlugInProjectionExpression) expr;
                var exprNode = ASTAggregationHelper.TryResolveAsAggregation(
                    mapContext.EngineImportService, node.IsDistinct, node.FunctionName, mapContext.PlugInAggregations,
                    mapContext.EngineURI);
                if (exprNode == null)
                {
                    throw new EPException("Error resolving aggregation function named '" + node.FunctionName + "'");
                }
                return exprNode;
            }
            else if (expr is OrderedObjectParamExpression)
            {
                var order = (OrderedObjectParamExpression) expr;
                return new ExprOrderedExpr(order.IsDescending);
            }
            else if (expr is CrontabFrequencyExpression)
            {
                return new ExprNumberSetFrequency();
            }
            else if (expr is CrontabRangeExpression)
            {
                return new ExprNumberSetRange();
            }
            else if (expr is CrontabParameterSetExpression)
            {
                return new ExprNumberSetList();
            }
            else if (expr is CrontabParameterExpression)
            {
                var cronParam = (CrontabParameterExpression) expr;
                if (cronParam.ItemType == ScheduleItemType.WILDCARD)
                {
                    return new ExprWildcardImpl();
                }
                CronOperatorEnum @operator;
                switch (cronParam.ItemType)
                {
                    case ScheduleItemType.LASTDAY:
                        @operator = CronOperatorEnum.LASTDAY;
                        break;
                    case ScheduleItemType.WEEKDAY:
                        @operator = CronOperatorEnum.WEEKDAY;
                        break;
                    case ScheduleItemType.LASTWEEKDAY:
                        @operator = CronOperatorEnum.LASTWEEKDAY;
                        break;
                    default:
                        throw new ArgumentException("Cron parameter not recognized: " + cronParam.ItemType);
                }
                return new ExprNumberSetCronParam(@operator);
            }
            else if (expr is AccessProjectionExpressionBase)
            {
                var theBase = (AccessProjectionExpressionBase) expr;
                AggregationStateType type;
                if (expr is FirstProjectionExpression)
                {
                    type = AggregationStateType.FIRST;
                }
                else if (expr is LastProjectionExpression)
                {
                    type = AggregationStateType.LAST;
                }
                else
                {
                    type = AggregationStateType.WINDOW;
                }
                return new ExprAggMultiFunctionLinearAccessNode(type);
            }
            else if (expr is DotExpression)
            {
                var theBase = (DotExpression) expr;
                var chain = MapChains(theBase.Chain, mapContext);

                // determine table use
                var workChain = new List<ExprChainedSpec>(chain);
                var tableNameCandidate = workChain[0].Name;
                Pair<ExprTableAccessNode, IList<ExprChainedSpec>> pair =
                    ASTTableExprHelper.CheckTableNameGetLibFunc(
                        mapContext.TableService, mapContext.EngineImportService, mapContext.PlugInAggregations,
                        mapContext.EngineURI, tableNameCandidate, workChain);
                if (pair != null)
                {
                    mapContext.TableExpressions.Add(pair.First);
                    return pair.First;
                }

                if (chain.Count == 1)
                {
                    var name = chain[0].Name;
                    var declared = ExprDeclaredHelper.GetExistsDeclaredExpr(
                        mapContext.Container, name, chain[0].Parameters, 
                        mapContext.ExpressionDeclarations.Values,
                        mapContext.ExprDeclaredService, 
                        mapContext.ContextDescriptor);
                    if (declared != null)
                    {
                        return declared;
                    }
                    var script = ExprDeclaredHelper.GetExistsScript(
                        mapContext.Configuration.EngineDefaults.Scripts.DefaultDialect,
                        name, chain[0].Parameters, mapContext.Scripts.Values, mapContext.ExprDeclaredService);
                    if (script != null)
                    {
                        return script;
                    }
                }
                var dotNode = new ExprDotNodeImpl(
                    chain,
                    mapContext.Configuration.EngineDefaults.Expression.IsDuckTyping,
                    mapContext.Configuration.EngineDefaults.Expression.IsUdfCache);
                if (dotNode.IsVariableOpGetName(mapContext.VariableService) != null)
                {
                    mapContext.HasVariables = true;
                }
                return dotNode;
            }
            else if (expr is LambdaExpression)
            {
                var theBase = (LambdaExpression) expr;
                return new ExprLambdaGoesNode(new List<string>(theBase.Parameters));
            }
            else if (expr is StreamWildcardExpression)
            {
                var sw = (StreamWildcardExpression) expr;
                return new ExprStreamUnderlyingNodeImpl(sw.StreamName, true);
            }
            else if (expr is GroupingExpression)
            {
                return new ExprGroupingNode();
            }
            else if (expr is GroupingIdExpression)
            {
                return new ExprGroupingIdNode();
            }
            else if (expr is TableAccessExpression)
            {
                var b = (TableAccessExpression) expr;
                ExprTableAccessNode tableNode;
                if (b.OptionalAggregate != null)
                {
                    var exprNode = MapExpressionDeep(b.OptionalAggregate, mapContext);
                    tableNode = new ExprTableAccessNodeSubpropAccessor(b.TableName, b.OptionalColumn, exprNode);
                }
                else if (b.OptionalColumn != null)
                {
                    tableNode = new ExprTableAccessNodeSubprop(b.TableName, b.OptionalColumn);
                }
                else
                {
                    tableNode = new ExprTableAccessNodeTopLevel(b.TableName);
                }
                mapContext.TableExpressions.Add(tableNode);
                return tableNode;
            }
            else if (expr is WildcardExpression)
            {
                return new ExprWildcardImpl();
            }
            else if (expr is NamedParameterExpression)
            {
                var named = (NamedParameterExpression) expr;
                return new ExprNamedParameterNodeImpl(named.Name);
            }
            throw new ArgumentException("Could not map expression node of type " + expr.GetType().Name);
        }

        private static IList<Expression> UnmapExpressionDeep(
            IList<ExprNode> expressions,
            StatementSpecUnMapContext unmapContext)
        {
            var result = new List<Expression>();
            if (expressions == null)
            {
                return result;
            }
            foreach (var expr in expressions)
            {
                if (expr == null)
                {
                    result.Add(null);
                    continue;
                }
                result.Add(UnmapExpressionDeep(expr, unmapContext));
            }
            return result;
        }

        private static MatchRecognizeRegEx UnmapExpressionFlatRowregex(
            RowRegexExprNode expr,
            StatementSpecUnMapContext unmapContext)
        {
            if (expr is RowRegexExprNodeAlteration)
            {
                return new MatchRecognizeRegExAlteration();
            }
            else if (expr is RowRegexExprNodeAtom)
            {
                var atom = (RowRegexExprNodeAtom) expr;
                var repeat = UnmapRowRegexRepeat(atom.OptionalRepeat, unmapContext);
                return new MatchRecognizeRegExAtom(
                    atom.Tag, atom.NFAType.Xlate<MatchRecognizePatternElementType>(), repeat);
            }
            else if (expr is RowRegexExprNodeConcatenation)
            {
                return new MatchRecognizeRegExConcatenation();
            }
            else if (expr is RowRegexExprNodePermute)
            {
                return new MatchRecognizeRegExPermutation();
            }
            else
            {
                var nested = (RowRegexExprNodeNested) expr;
                var repeat = UnmapRowRegexRepeat(nested.OptionalRepeat, unmapContext);
                return new MatchRecognizeRegExNested(nested.NFAType.Xlate<MatchRecognizePatternElementType>(), repeat);
            }
        }

        private static MatchRecognizeRegExRepeat UnmapRowRegexRepeat(
            RowRegexExprRepeatDesc optionalRepeat,
            StatementSpecUnMapContext unmapContext)
        {
            if (optionalRepeat == null)
            {
                return null;
            }
            return new MatchRecognizeRegExRepeat(
                UnmapExpressionDeep(optionalRepeat.Lower, unmapContext),
                UnmapExpressionDeep(optionalRepeat.Upper, unmapContext),
                UnmapExpressionDeep(optionalRepeat.Single, unmapContext)
                );
        }

        private static RowRegexExprNode MapExpressionFlatRowregex(
            MatchRecognizeRegEx expr,
            StatementSpecMapContext mapContext)
        {
            if (expr is MatchRecognizeRegExAlteration)
            {
                return new RowRegexExprNodeAlteration();
            }
            else if (expr is MatchRecognizeRegExAtom)
            {
                var atom = (MatchRecognizeRegExAtom) expr;
                var repeat = MapRowRegexRepeat(atom.OptionalRepeat, mapContext);
                return new RowRegexExprNodeAtom(atom.Name, atom.ElementType.Xlate<RegexNFATypeEnum>(), repeat);
            }
            else if (expr is MatchRecognizeRegExConcatenation)
            {
                return new RowRegexExprNodeConcatenation();
            }
            else if (expr is MatchRecognizeRegExPermutation)
            {
                return new RowRegexExprNodePermute();
            }
            else
            {
                var nested = (MatchRecognizeRegExNested) expr;
                var repeat = MapRowRegexRepeat(nested.OptionalRepeat, mapContext);
                return new RowRegexExprNodeNested(nested.ElementType.Xlate<RegexNFATypeEnum>(), repeat);
            }
        }

        private static RowRegexExprRepeatDesc MapRowRegexRepeat(
            MatchRecognizeRegExRepeat optionalRepeat,
            StatementSpecMapContext mapContext)
        {
            if (optionalRepeat == null)
            {
                return null;
            }
            return new RowRegexExprRepeatDesc(
                MapExpressionDeep(optionalRepeat.Low, mapContext),
                MapExpressionDeep(optionalRepeat.High, mapContext),
                MapExpressionDeep(optionalRepeat.Single, mapContext)
                );
        }

        private static Expression UnmapExpressionFlat(ExprNode expr, StatementSpecUnMapContext unmapContext)
        {
            if (expr is ExprMathNode)
            {
                var math = (ExprMathNode) expr;
                return new ArithmaticExpression(math.MathArithTypeEnum.GetExpressionText());
            }
            else if (expr is ExprIdentNode)
            {
                var prop = (ExprIdentNode) expr;
                var propertyName = prop.UnresolvedPropertyName;
                if (prop.StreamOrPropertyName != null)
                {
                    propertyName = prop.StreamOrPropertyName + "." + prop.UnresolvedPropertyName;
                }
                return new PropertyValueExpression(propertyName);
            }
            else if (expr is ExprVariableNode)
            {
                var prop = (ExprVariableNode) expr;
                var propertyName = prop.VariableNameWithSubProp;
                return new PropertyValueExpression(propertyName);
            }
            else if (expr is ExprContextPropertyNode)
            {
                var prop = (ExprContextPropertyNode) expr;
                return
                    new PropertyValueExpression(
                        ContextPropertyRegistryConstants.CONTEXT_PREFIX + "." + prop.PropertyName);
            }
            else if (expr is ExprEqualsNode)
            {
                var equals = (ExprEqualsNode) expr;
                string @operator;
                if (!equals.IsIs)
                {
                    @operator = "=";
                    if (equals.IsNotEquals)
                    {
                        @operator = "!=";
                    }
                }
                else
                {
                    @operator = "is";
                    if (equals.IsNotEquals)
                    {
                        @operator = "is not";
                    }
                }
                return new RelationalOpExpression(@operator);
            }
            else if (expr is ExprRelationalOpNode)
            {
                var rel = (ExprRelationalOpNode) expr;
                return new RelationalOpExpression(rel.RelationalOpEnum.GetExpressionText());
            }
            else if (expr is ExprAndNode)
            {
                return new Conjunction();
            }
            else if (expr is ExprOrNode)
            {
                return new Disjunction();
            }
            else if (expr is ExprConstantNodeImpl)
            {
                var constNode = (ExprConstantNodeImpl) expr;
                string constantType = null;
                if (constNode.ConstantType != null)
                {
                    constantType = constNode.ConstantType.AssemblyQualifiedName;
                }
                return new ConstantExpression(constNode.GetConstantValue(null), constantType);
            }
            else if (expr is ExprConcatNode)
            {
                return new ConcatExpression();
            }
            else if (expr is ExprSubselectRowNode)
            {
                var sub = (ExprSubselectRowNode) expr;
                var unmapped = Unmap(sub.StatementSpecRaw);
                unmapContext.AddAll(unmapped.SubstitutionParams);
                return new SubqueryExpression(unmapped.ObjectModel);
            }
            else if (expr is ExprSubselectInNode)
            {
                var sub = (ExprSubselectInNode) expr;
                var unmapped = Unmap(sub.StatementSpecRaw);
                unmapContext.AddAll(unmapped.SubstitutionParams);
                return new SubqueryInExpression(unmapped.ObjectModel, sub.IsNotIn);
            }
            else if (expr is ExprSubselectExistsNode)
            {
                var sub = (ExprSubselectExistsNode) expr;
                var unmapped = Unmap(sub.StatementSpecRaw);
                unmapContext.AddAll(unmapped.SubstitutionParams);
                return new SubqueryExistsExpression(unmapped.ObjectModel);
            }
            else if (expr is ExprSubselectAllSomeAnyNode)
            {
                var sub = (ExprSubselectAllSomeAnyNode) expr;
                var unmapped = Unmap(sub.StatementSpecRaw);
                unmapContext.AddAll(unmapped.SubstitutionParams);
                var @operator = "=";
                if (sub.IsNot)
                {
                    @operator = "!=";
                }
                if (sub.RelationalOp != null)
                {
                    @operator = sub.RelationalOp.Value.GetExpressionText();
                }
                return new SubqueryQualifiedExpression(unmapped.ObjectModel, @operator, sub.IsAll);
            }
            else if (expr is ExprCountNode)
            {
                var sub = (ExprCountNode) expr;
                if (sub.ChildNodes.Count == 0 || (sub.ChildNodes.Count == 1 && sub.HasFilter))
                {
                    return new CountStarProjectionExpression();
                }
                else
                {
                    return new CountProjectionExpression(sub.IsDistinct);
                }
            }
            else if (expr is ExprAvgNode)
            {
                var sub = (ExprAvgNode) expr;
                return new AvgProjectionExpression(sub.IsDistinct);
            }
            else if (expr is ExprSumNode)
            {
                var sub = (ExprSumNode) expr;
                return new SumProjectionExpression(sub.IsDistinct);
            }
            else if (expr is ExprBetweenNode)
            {
                var between = (ExprBetweenNode) expr;
                return new BetweenExpression(
                    between.IsLowEndpointIncluded, between.IsHighEndpointIncluded, between.IsNotBetween);
            }
            else if (expr is ExprPriorNode)
            {
                return new PriorExpression();
            }
            else if (expr is ExprRateAggNode)
            {
                return new PlugInProjectionExpression("rate", false);
            }
            else if (expr is ExprNthAggNode)
            {
                return new PlugInProjectionExpression("nth", false);
            }
            else if (expr is ExprLeavingAggNode)
            {
                return new PlugInProjectionExpression("leaving", false);
            }
            else if (expr is ExprAggCountMinSketchNode)
            {
                var cmsNode = (ExprAggCountMinSketchNode) expr;
                return new PlugInProjectionExpression(cmsNode.AggregationFunctionName, false);
            }
            else if (expr is ExprAggMultiFunctionSortedMinMaxByNode)
            {
                var node = (ExprAggMultiFunctionSortedMinMaxByNode) expr;
                return new PlugInProjectionExpression(node.AggregationFunctionName, false);
            }
            else if (expr is ExprPreviousNode)
            {
                var prev = (ExprPreviousNode) expr;
                var result = new PreviousExpression();
                result.ExpressionType = EnumHelper.Parse<PreviousExpressionType>(prev.PreviousType.ToString());
                return result;
            }
            else if (expr is ExprMinMaxAggrNode)
            {
                var node = (ExprMinMaxAggrNode) expr;
                if (node.MinMaxTypeEnum == MinMaxTypeEnum.MIN)
                {
                    return new MinProjectionExpression(node.IsDistinct, node.IsEver);
                }
                else
                {
                    return new MaxProjectionExpression(node.IsDistinct, node.IsEver);
                }
            }
            else if (expr is ExprNotNode)
            {
                return new NotExpression();
            }
            else if (expr is ExprInNode)
            {
                var inExpr = (ExprInNode) expr;
                return new InExpression(inExpr.IsNotIn);
            }
            else if (expr is ExprCoalesceNode)
            {
                return new CoalesceExpression();
            }
            else if (expr is ExprCaseNode)
            {
                var mycase = (ExprCaseNode) expr;
                if (mycase.IsCase2)
                {
                    return new CaseSwitchExpression();
                }
                else
                {
                    return new CaseWhenThenExpression();
                }
            }
            else if (expr is ExprMinMaxRowNode)
            {
                var node = (ExprMinMaxRowNode) expr;
                if (node.MinMaxTypeEnum == MinMaxTypeEnum.MAX)
                {
                    return new MaxRowExpression();
                }
                return new MinRowExpression();
            }
            else if (expr is ExprBitWiseNode)
            {
                var node = (ExprBitWiseNode) expr;
                return new BitwiseOpExpression(node.BitWiseOpEnum);
            }
            else if (expr is ExprArrayNode)
            {
                return new ArrayExpression();
            }
            else if (expr is ExprLikeNode)
            {
                var exprLikeNode = (ExprLikeNode) expr;
                return new LikeExpression(exprLikeNode.IsNot);
            }
            else if (expr is ExprRegexpNode)
            {
                var exprRegexNode = (ExprRegexpNode) expr;
                return new RegExpExpression(exprRegexNode.IsNot);
            }
            else if (expr is ExprMedianNode)
            {
                var median = (ExprMedianNode) expr;
                return new MedianProjectionExpression(median.IsDistinct);
            }
            else if (expr is ExprLastEverNode)
            {
                var last = (ExprLastEverNode) expr;
                return new LastEverProjectionExpression(last.IsDistinct);
            }
            else if (expr is ExprFirstEverNode)
            {
                var first = (ExprFirstEverNode) expr;
                return new FirstEverProjectionExpression(first.IsDistinct);
            }
            else if (expr is ExprCountEverNode)
            {
                var countEver = (ExprCountEverNode) expr;
                return new CountEverProjectionExpression(countEver.IsDistinct);
            }
            else if (expr is ExprAvedevNode)
            {
                var node = (ExprAvedevNode) expr;
                return new AvedevProjectionExpression(node.IsDistinct);
            }
            else if (expr is ExprStddevNode)
            {
                var node = (ExprStddevNode) expr;
                return new StddevProjectionExpression(node.IsDistinct);
            }
            else if (expr is ExprPlugInAggNode)
            {
                var node = (ExprPlugInAggNode) expr;
                return new PlugInProjectionExpression(node.AggregationFunctionName, node.IsDistinct);
            }
            else if (expr is ExprPlugInAggMultiFunctionNode)
            {
                var node = (ExprPlugInAggMultiFunctionNode) expr;
                return new PlugInProjectionExpression(node.AggregationFunctionName, node.IsDistinct);
            }
            else if (expr is ExprPlugInSingleRowNode)
            {
                var node = (ExprPlugInSingleRowNode) expr;
                var chain = UnmapChains(node.ChainSpec, unmapContext, false);
                chain[0].Name = node.FunctionName; // starts with actual function name not mapped on
                return new SingleRowMethodExpression(chain);
            }
            else if (expr is ExprInstanceofNode)
            {
                var node = (ExprInstanceofNode) expr;
                return new InstanceOfExpression(node.ClassIdentifiers);
            }
            else if (expr is ExprTypeofNode)
            {
                return new TypeOfExpression();
            }
            else if (expr is ExprCastNode)
            {
                var node = (ExprCastNode) expr;
                return new CastExpression(node.ClassIdentifier);
            }
            else if (expr is ExprPropertyExistsNode)
            {
                return new PropertyExistsExpression();
            }
            else if (expr is ExprTimestampNode)
            {
                return new CurrentTimestampExpression();
            }
            else if (expr is ExprCurrentEvaluationContextNode)
            {
                return new CurrentEvaluationContextExpression();
            }
            else if (expr is ExprIStreamNode)
            {
                return new IStreamBuiltinExpression();
            }
            else if (expr is ExprSubstitutionNode)
            {
                var node = (ExprSubstitutionNode) expr;
                SubstitutionParameterExpressionBase subs;
                if (node.Index == null)
                {
                    subs = new SubstitutionParameterExpressionNamed(node.Name);
                }
                else
                {
                    subs = new SubstitutionParameterExpressionIndexed(node.Index.Value);
                }
                unmapContext.Add(subs);
                return subs;
            }
            else if (expr is ExprTimePeriod)
            {
                var node = (ExprTimePeriod) expr;
                return new TimePeriodExpression(
                    node.HasYear,
                    node.HasMonth,
                    node.HasWeek,
                    node.HasDay,
                    node.HasHour,
                    node.HasMinute,
                    node.HasSecond,
                    node.HasMillisecond,
                    node.HasMicrosecond);
            }
            else if (expr is ExprWildcard)
            {
                return new CrontabParameterExpression(ScheduleItemType.WILDCARD);
            }
            else if (expr is ExprNumberSetFrequency)
            {
                return new CrontabFrequencyExpression();
            }
            else if (expr is ExprNumberSetRange)
            {
                return new CrontabRangeExpression();
            }
            else if (expr is ExprNumberSetList)
            {
                return new CrontabParameterSetExpression();
            }
            else if (expr is ExprNewStructNode)
            {
                var newNode = (ExprNewStructNode) expr;
                return new NewOperatorExpression(new List<string>(newNode.ColumnNames));
            }
            else if (expr is ExprNewInstanceNode)
            {
                var newNode = (ExprNewInstanceNode) expr;
                return new NewInstanceOperatorExpression(newNode.ClassIdent);
            }
            else if (expr is ExprOrderedExpr)
            {
                var order = (ExprOrderedExpr) expr;
                return new OrderedObjectParamExpression(order.IsDescending);
            }
            else if (expr is ExprEqualsAllAnyNode)
            {
                var node = (ExprEqualsAllAnyNode) expr;
                var @operator = node.IsNot ? "!=" : "=";
                return new CompareListExpression(node.IsAll, @operator);
            }
            else if (expr is ExprRelationalOpAllAnyNode)
            {
                var node = (ExprRelationalOpAllAnyNode) expr;
                return new CompareListExpression(node.IsAll, node.RelationalOp.GetExpressionText());
            }
            else if (expr is ExprNumberSetCronParam)
            {
                var cronParam = (ExprNumberSetCronParam) expr;
                ScheduleItemType type;
                if (cronParam.CronOperator == CronOperatorEnum.LASTDAY)
                {
                    type = ScheduleItemType.LASTDAY;
                }
                else if (cronParam.CronOperator == CronOperatorEnum.LASTWEEKDAY)
                {
                    type = ScheduleItemType.LASTWEEKDAY;
                }
                else if (cronParam.CronOperator == CronOperatorEnum.WEEKDAY)
                {
                    type = ScheduleItemType.WEEKDAY;
                }
                else
                {
                    throw new ArgumentException("Cron parameter not recognized: " + cronParam.CronOperator);
                }
                return new CrontabParameterExpression(type);
            }
            else if (expr is ExprAggMultiFunctionLinearAccessNode)
            {
                var accessNode = (ExprAggMultiFunctionLinearAccessNode) expr;
                AccessProjectionExpressionBase ape;
                if (accessNode.StateType == AggregationStateType.FIRST)
                {
                    ape = new FirstProjectionExpression();
                }
                else if (accessNode.StateType == AggregationStateType.WINDOW)
                {
                    ape = new WindowProjectionExpression();
                }
                else
                {
                    ape = new LastProjectionExpression();
                }
                return ape;
            }
            else if (expr is ExprDotNode)
            {
                var dotNode = (ExprDotNode) expr;
                var dotExpr = new DotExpression();
                foreach (var chain in dotNode.ChainSpec)
                {
                    dotExpr.Add(chain.Name, UnmapExpressionDeep(chain.Parameters, unmapContext), chain.IsProperty);
                }
                return dotExpr;
            }
            else if (expr is ExprDeclaredNode)
            {
                var declNode = (ExprDeclaredNode) expr;
                var dotExpr = new DotExpression();
                dotExpr.Add(
                    declNode.Prototype.Name,
                    UnmapExpressionDeep(declNode.ChainParameters, unmapContext));
                return dotExpr;
            }
            else if (expr is ExprStreamUnderlyingNodeImpl)
            {
                var streamNode = (ExprStreamUnderlyingNodeImpl) expr;
                return new StreamWildcardExpression(streamNode.StreamName);
            }
            else if (expr is ExprLambdaGoesNode)
            {
                var lambdaNode = (ExprLambdaGoesNode) expr;
                var lambdaExpr = new LambdaExpression(new List<string>(lambdaNode.GoesToNames));
                return lambdaExpr;
            }
            else if (expr is ExprNodeScript)
            {
                var scriptNode = (ExprNodeScript) expr;
                var dotExpr = new DotExpression();
                dotExpr.Add(scriptNode.Script.Name, UnmapExpressionDeep(scriptNode.Parameters, unmapContext));
                return dotExpr;
            }
            else if (expr is ExprGroupingNode)
            {
                return new GroupingExpression();
            }
            else if (expr is ExprGroupingIdNode)
            {
                return new GroupingIdExpression();
            }
            else if (expr is ExprNamedParameterNode)
            {
                var named = (ExprNamedParameterNode) expr;
                return new NamedParameterExpression(named.ParameterName);
            }
            else if (expr is ExprTableAccessNode)
            {
                var table = (ExprTableAccessNode) expr;
                if (table is ExprTableAccessNodeTopLevel)
                {
                    var topLevel = (ExprTableAccessNodeTopLevel) table;
                    return new TableAccessExpression(
                        topLevel.TableName, UnmapExpressionDeep(topLevel.ChildNodes, unmapContext), null, null);
                }
                if (table is ExprTableAccessNodeSubprop)
                {
                    var sub = (ExprTableAccessNodeSubprop) table;
                    if (sub.ChildNodes.Count == 0)
                    {
                        return new PropertyValueExpression(table.TableName + "." + sub.SubpropName);
                    }
                    else
                    {
                        return new TableAccessExpression(
                            sub.TableName, UnmapExpressionDeep(sub.ChildNodes, unmapContext), sub.SubpropName, null);
                    }
                }
                if (table is ExprTableAccessNodeKeys)
                {
                    var dotExpression = new DotExpression();
                    dotExpression.Add(table.TableName, Collections.GetEmptyList<Expression>(), true);
                    dotExpression.Add("keys", Collections.GetEmptyList<Expression>());
                    return dotExpression;
                }
                if (table is ExprTableAccessNodeSubpropAccessor)
                {
                    var sub = (ExprTableAccessNodeSubpropAccessor) table;
                    if (sub.ChildNodes.Count == 0)
                    {
                        var dotExpression = new DotExpression();
                        dotExpression.Add(
                            table.TableName + "." + sub.SubpropName, Collections.GetEmptyList<Expression>(), true);
                        IList<Expression> @params = UnmapExpressionDeep(
                            sub.AggregateAccessMultiValueNode.ChildNodes, unmapContext);
                        var functionName = sub.AggregateAccessMultiValueNode.AggregationFunctionName;
                        if (AggregationStateTypeExtensions.FromString(functionName) != null && @params.IsEmpty())
                        {
                            @params.Add(new WildcardExpression());
                        }
                        dotExpression.Add(functionName, @params);
                        return dotExpression;
                    }
                    else
                    {
                        var aggregate = UnmapExpressionDeep(sub.AggregateAccessMultiValueNode, unmapContext);
                        return new TableAccessExpression(
                            sub.TableName, UnmapExpressionDeep(sub.ChildNodes, unmapContext), sub.SubpropName, aggregate);
                    }
                }
            }
            throw new ArgumentException("Could not map expression node of type " + expr.GetType().Name);
        }

        private static void UnmapExpressionRecursive(
            Expression parent,
            ExprNode expr,
            StatementSpecUnMapContext unmapContext)
        {
            foreach (var child in expr.ChildNodes)
            {
                var result = UnmapExpressionFlat(child, unmapContext);
                parent.Children.Add(result);
                UnmapExpressionRecursive(result, child, unmapContext);
            }
        }

        private static void UnmapExpressionRecursiveRowregex(
            MatchRecognizeRegEx parent,
            RowRegexExprNode expr,
            StatementSpecUnMapContext unmapContext)
        {
            foreach (var child in expr.ChildNodes)
            {
                var result = UnmapExpressionFlatRowregex(child, unmapContext);
                parent.Children.Add(result);
                UnmapExpressionRecursiveRowregex(result, child, unmapContext);
            }
        }

        private static void MapExpressionRecursive(ExprNode parent, Expression expr, StatementSpecMapContext mapContext)
        {
            foreach (var child in expr.Children)
            {
                var result = MapExpressionFlat(child, mapContext);
                parent.AddChildNode(result);
                MapExpressionRecursive(result, child, mapContext);
            }
        }

        private static void MapExpressionRecursiveRowregex(
            RowRegexExprNode parent,
            MatchRecognizeRegEx expr,
            StatementSpecMapContext mapContext)
        {
            foreach (var child in expr.Children)
            {
                var result = MapExpressionFlatRowregex(child, mapContext);
                parent.AddChildNode(result);
                MapExpressionRecursiveRowregex(result, child, mapContext);
            }
        }

        private static void MapFrom(FromClause fromClause, StatementSpecRaw raw, StatementSpecMapContext mapContext)
        {
            if (fromClause == null)
            {
                return;
            }

            foreach (var stream in fromClause.Streams)
            {
                StreamSpecRaw spec;

                var views = ViewSpec.EMPTY_VIEWSPEC_ARRAY;
                if (stream is ProjectedStream)
                {
                    var projectedStream = (ProjectedStream) stream;
                    views = ViewSpec.ToArray(MapViews(projectedStream.Views, mapContext));
                }

                if (stream is FilterStream)
                {
                    var filterStream = (FilterStream) stream;
                    var filterSpecRaw = MapFilter(filterStream.Filter, mapContext);
                    var options = MapStreamOpts(filterStream);
                    spec = new FilterStreamSpecRaw(filterSpecRaw, views, filterStream.StreamName, options);
                }
                else if (stream is SQLStream)
                {
                    var sqlStream = (SQLStream) stream;
                    spec = new DBStatementStreamSpec(
                        sqlStream.StreamName, views,
                        sqlStream.DatabaseName, sqlStream.SqlWithSubsParams, sqlStream.OptionalMetadataSQL);
                }
                else if (stream is PatternStream)
                {
                    var patternStream = (PatternStream) stream;
                    var child = MapPatternEvalDeep(patternStream.Expression, mapContext);
                    var options = MapStreamOpts(patternStream);
                    var flags = PatternLevelAnnotationUtil.AnnotationsToSpec(patternStream.Annotations);
                    spec = new PatternStreamSpecRaw(
                        child, views, patternStream.StreamName, options, flags.IsSuppressSameEventMatches,
                        flags.IsDiscardPartialsOnMatch);
                }
                else if (stream is MethodInvocationStream)
                {
                    var methodStream = (MethodInvocationStream) stream;
                    var expressions = new List<ExprNode>();
                    foreach (Expression expr in methodStream.ParameterExpressions)
                    {
                        var exprNode = MapExpressionDeep(expr, mapContext);
                        expressions.Add(exprNode);
                    }

                    if (mapContext.VariableService.GetVariableMetaData(methodStream.ClassName) != null)
                    {
                        mapContext.HasVariables = true;
                    }

                    spec = new MethodStreamSpec(
                        methodStream.StreamName, views, "method",
                        methodStream.ClassName, methodStream.MethodName, expressions, methodStream.OptionalEventTypeName);
                }
                else
                {
                    throw new ArgumentException(
                        "Could not map from stream " + stream + " to an internal representation");
                }

                raw.StreamSpecs.Add(spec);
            }

            foreach (var qualifier in fromClause.OuterJoinQualifiers)
            {
                ExprIdentNode left = null;
                ExprIdentNode right = null;
                ExprIdentNode[] additionalLeft = null;
                ExprIdentNode[] additionalRight = null;

                if (qualifier.Left != null)
                {

                    left = (ExprIdentNode) MapExpressionFlat(qualifier.Left, mapContext);
                    right = (ExprIdentNode) MapExpressionFlat(qualifier.Right, mapContext);

                    if (qualifier.AdditionalProperties.Count != 0)
                    {
                        additionalLeft = new ExprIdentNode[qualifier.AdditionalProperties.Count];
                        additionalRight = new ExprIdentNode[qualifier.AdditionalProperties.Count];
                        var count = 0;
                        foreach (var pair in qualifier.AdditionalProperties)
                        {
                            additionalLeft[count] = (ExprIdentNode) MapExpressionFlat(pair.Left, mapContext);
                            additionalRight[count] = (ExprIdentNode) MapExpressionFlat(pair.Right, mapContext);
                            count++;
                        }
                    }
                }

                raw.OuterJoinDescList.Add(
                    new OuterJoinDesc(qualifier.JoinType, left, right, additionalLeft, additionalRight));
            }
        }

        private static IList<ViewSpec> MapViews(IList<View> views, StatementSpecMapContext mapContext)
        {
            var viewSpecs = new List<ViewSpec>();
            foreach (var view in views)
            {
                IList<ExprNode> viewExpressions = MapExpressionDeep(view.Parameters, mapContext);
                viewSpecs.Add(new ViewSpec(view.Namespace, view.Name, viewExpressions));
            }
            return viewSpecs;
        }

        private static IList<View> UnmapViews(IList<ViewSpec> viewSpecs, StatementSpecUnMapContext unmapContext)
        {
            var views = new List<View>();
            foreach (var viewSpec in viewSpecs)
            {
                IList<Expression> viewExpressions = UnmapExpressionDeep(viewSpec.ObjectParameters, unmapContext);
                views.Add(View.Create(viewSpec.ObjectNamespace, viewSpec.ObjectName, viewExpressions));
            }
            return views;
        }

        private static EvalFactoryNode MapPatternEvalFlat(PatternExpr eval, StatementSpecMapContext mapContext)
        {
            if (eval == null)
            {
                throw new ArgumentException("Null expression parameter");
            }
            if (eval is PatternAndExpr)
            {
                return mapContext.PatternNodeFactory.MakeAndNode();
            }
            else if (eval is PatternOrExpr)
            {
                return mapContext.PatternNodeFactory.MakeOrNode();
            }
            else if (eval is PatternFollowedByExpr)
            {
                var fb = (PatternFollowedByExpr) eval;
                IList<ExprNode> maxExpr = MapExpressionDeep(fb.OptionalMaxPerSubexpression, mapContext);
                return mapContext.PatternNodeFactory.MakeFollowedByNode(
                    maxExpr, mapContext.Configuration.EngineDefaults.Patterns.MaxSubexpressions != null);
            }
            else if (eval is PatternEveryExpr)
            {
                return mapContext.PatternNodeFactory.MakeEveryNode();
            }
            else if (eval is PatternFilterExpr)
            {
                var filterExpr = (PatternFilterExpr) eval;
                var filterSpec = MapFilter(filterExpr.Filter, mapContext);
                return mapContext.PatternNodeFactory.MakeFilterNode(
                    filterSpec, filterExpr.TagName, filterExpr.OptionalConsumptionLevel);
            }
            else if (eval is PatternObserverExpr)
            {
                var observer = (PatternObserverExpr) eval;
                IList<ExprNode> expressions = MapExpressionDeep(observer.Parameters, mapContext);
                return
                    mapContext.PatternNodeFactory.MakeObserverNode(
                        new PatternObserverSpec(observer.Namespace, observer.Name, expressions));
            }
            else if (eval is PatternGuardExpr)
            {
                var guard = (PatternGuardExpr) eval;
                IList<ExprNode> expressions = MapExpressionDeep(guard.Parameters, mapContext);
                return
                    mapContext.PatternNodeFactory.MakeGuardNode(
                        new PatternGuardSpec(guard.Namespace, guard.Name, expressions));
            }
            else if (eval is PatternNotExpr)
            {
                return mapContext.PatternNodeFactory.MakeNotNode();
            }
            else if (eval is PatternMatchUntilExpr)
            {
                var until = (PatternMatchUntilExpr) eval;
                var low = until.Low != null ? MapExpressionDeep(until.Low, mapContext) : null;
                var high = until.High != null ? MapExpressionDeep(until.High, mapContext) : null;
                var single = until.Single != null ? MapExpressionDeep(until.Single, mapContext) : null;
                return mapContext.PatternNodeFactory.MakeMatchUntilNode(low, high, single);
            }
            else if (eval is PatternEveryDistinctExpr)
            {
                var everyDist = (PatternEveryDistinctExpr) eval;
                IList<ExprNode> expressions = MapExpressionDeep(everyDist.Expressions, mapContext);
                return mapContext.PatternNodeFactory.MakeEveryDistinctNode(expressions);
            }
            throw new ArgumentException("Could not map pattern expression node of type " + eval.GetType().Name);
        }

        private static PatternExpr UnmapPatternEvalFlat(EvalFactoryNode eval, StatementSpecUnMapContext unmapContext)
        {
            if (eval is EvalAndFactoryNode)
            {
                return new PatternAndExpr();
            }
            else if (eval is EvalOrFactoryNode)
            {
                return new PatternOrExpr();
            }
            else if (eval is EvalFollowedByFactoryNode)
            {
                var fb = (EvalFollowedByFactoryNode) eval;
                IList<Expression> expressions = UnmapExpressionDeep(fb.OptionalMaxExpressions, unmapContext);
                return new PatternFollowedByExpr(expressions);
            }
            else if (eval is EvalEveryFactoryNode)
            {
                return new PatternEveryExpr();
            }
            else if (eval is EvalNotFactoryNode)
            {
                return new PatternNotExpr();
            }
            else if (eval is EvalFilterFactoryNode)
            {
                var filterNode = (EvalFilterFactoryNode) eval;
                var filter = UnmapFilter(filterNode.RawFilterSpec, unmapContext);
                var expr = new PatternFilterExpr(filter, filterNode.EventAsName);
                expr.OptionalConsumptionLevel = filterNode.ConsumptionLevel;
                return expr;
            }
            else if (eval is EvalObserverFactoryNode)
            {
                var observerNode = (EvalObserverFactoryNode) eval;
                IList<Expression> expressions = UnmapExpressionDeep(
                    observerNode.PatternObserverSpec.ObjectParameters, unmapContext);
                return new PatternObserverExpr(
                    observerNode.PatternObserverSpec.ObjectNamespace,
                    observerNode.PatternObserverSpec.ObjectName, expressions);
            }
            else if (eval is EvalGuardFactoryNode)
            {
                var guardNode = (EvalGuardFactoryNode) eval;
                IList<Expression> expressions = UnmapExpressionDeep(
                    guardNode.PatternGuardSpec.ObjectParameters, unmapContext);
                return new PatternGuardExpr(
                    guardNode.PatternGuardSpec.ObjectNamespace,
                    guardNode.PatternGuardSpec.ObjectName, expressions);
            }
            else if (eval is EvalMatchUntilFactoryNode)
            {
                var matchUntilNode = (EvalMatchUntilFactoryNode) eval;
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
            else if (eval is EvalEveryDistinctFactoryNode)
            {
                var everyDistinctNode = (EvalEveryDistinctFactoryNode) eval;
                IList<Expression> expressions = UnmapExpressionDeep(everyDistinctNode.Expressions, unmapContext);
                return new PatternEveryDistinctExpr(expressions);
            }
            else if (eval is EvalAuditFactoryNode)
            {
                return null;
            }
            throw new ArgumentException("Could not map pattern expression node of type " + eval.GetType().Name);
        }

        private static void UnmapPatternEvalRecursive(
            PatternExpr parent,
            EvalFactoryNode eval,
            StatementSpecUnMapContext unmapContext)
        {
            foreach (var child in eval.ChildNodes)
            {
                var result = UnmapPatternEvalFlat(child, unmapContext);
                parent.Children.Add(result);
                UnmapPatternEvalRecursive(result, child, unmapContext);
            }
        }

        private static void MapPatternEvalRecursive(
            EvalFactoryNode parent,
            PatternExpr expr,
            StatementSpecMapContext mapContext)
        {
            foreach (var child in expr.Children)
            {
                var result = MapPatternEvalFlat(child, mapContext);
                parent.AddChildNode(result);
                MapPatternEvalRecursive(result, child, mapContext);
            }
        }

        private static PatternExpr UnmapPatternEvalDeep(
            EvalFactoryNode exprNode,
            StatementSpecUnMapContext unmapContext)
        {
            var parent = UnmapPatternEvalFlat(exprNode, unmapContext);
            UnmapPatternEvalRecursive(parent, exprNode, unmapContext);
            return parent;
        }

        private static EvalFactoryNode MapPatternEvalDeep(PatternExpr expr, StatementSpecMapContext mapContext)
        {
            var parent = MapPatternEvalFlat(expr, mapContext);
            MapPatternEvalRecursive(parent, expr, mapContext);
            return parent;
        }

        private static FilterSpecRaw MapFilter(Filter filter, StatementSpecMapContext mapContext)
        {
            var expr = new List<ExprNode>();
            if (filter.FilterExpression != null)
            {
                ExprNode exprNode = MapExpressionDeep(filter.FilterExpression, mapContext);
                expr.Add(exprNode);
            }

            PropertyEvalSpec evalSpec = null;
            if (filter.OptionalPropertySelects != null)
            {
                evalSpec = MapPropertySelects(filter.OptionalPropertySelects, mapContext);
            }

            return new FilterSpecRaw(filter.EventTypeName, expr, evalSpec);
        }

        private static PropertyEvalSpec MapPropertySelects(
            IList<ContainedEventSelect> propertySelects,
            StatementSpecMapContext mapContext)
        {
            var evalSpec = new PropertyEvalSpec();
            foreach (var propertySelect in propertySelects)
            {
                SelectClauseSpecRaw selectSpec = null;
                if (propertySelect.SelectClause != null)
                {
                    selectSpec = MapSelectRaw(propertySelect.SelectClause, mapContext);
                }

                ExprNode exprNodeWhere = null;
                if (propertySelect.WhereClause != null)
                {
                    exprNodeWhere = MapExpressionDeep(propertySelect.WhereClause, mapContext);
                }

                ExprNode splitterExpr = null;
                if (propertySelect.SplitExpression != null)
                {
                    splitterExpr = MapExpressionDeep(propertySelect.SplitExpression, mapContext);
                }

                evalSpec.Add(
                    new PropertyEvalAtom(
                        splitterExpr, propertySelect.OptionalSplitExpressionTypeName, propertySelect.OptionalAsName,
                        selectSpec, exprNodeWhere));
            }
            return evalSpec;
        }

        private static Filter UnmapFilter(FilterSpecRaw filter, StatementSpecUnMapContext unmapContext)
        {
            Expression expr = null;
            if (filter.FilterExpressions.Count > 1)
            {
                expr = new Conjunction();
                foreach (var exprNode in filter.FilterExpressions)
                {
                    var expression = UnmapExpressionDeep(exprNode, unmapContext);
                    expr.Children.Add(expression);
                }
            }
            else if (filter.FilterExpressions.Count == 1)
            {
                expr = UnmapExpressionDeep(filter.FilterExpressions[0], unmapContext);
            }

            var filterDef = new Filter(filter.EventTypeName, expr);

            if (filter.OptionalPropertyEvalSpec != null)
            {
                var propertySelects = UnmapPropertySelects(filter.OptionalPropertyEvalSpec, unmapContext);
                filterDef.OptionalPropertySelects = propertySelects;
            }
            return filterDef;
        }

        private static IList<ContainedEventSelect> UnmapPropertySelects(
            PropertyEvalSpec propertyEvalSpec,
            StatementSpecUnMapContext unmapContext)
        {
            var propertySelects = new List<ContainedEventSelect>();
            foreach (var atom in propertyEvalSpec.Atoms)
            {
                SelectClause selectClause = null;
                if (atom.OptionalSelectClause != null && !atom.OptionalSelectClause.SelectExprList.IsEmpty())
                {
                    selectClause = UnmapSelect(
                        atom.OptionalSelectClause, SelectClauseStreamSelectorEnum.ISTREAM_ONLY, unmapContext);
                }

                Expression filterExpression = null;
                if (atom.OptionalWhereClause != null)
                {
                    filterExpression = UnmapExpressionDeep(atom.OptionalWhereClause, unmapContext);
                }

                var splitExpression = UnmapExpressionDeep(atom.SplitterExpression, unmapContext);

                var contained = new ContainedEventSelect(splitExpression);
                contained.OptionalSplitExpressionTypeName = atom.OptionalResultEventType;
                contained.SelectClause = selectClause;
                contained.WhereClause = filterExpression;
                contained.OptionalAsName = atom.OptionalAsName;

                if (atom.SplitterExpression != null)
                {
                    contained.SplitExpression = UnmapExpressionDeep(atom.SplitterExpression, unmapContext);
                }
                propertySelects.Add(contained);
            }
            return propertySelects;
        }

        private static IList<AnnotationPart> UnmapAnnotations(IList<AnnotationDesc> annotations)
        {
            return annotations.Select(UnmapAnnotation).ToList();
        }

        private static IList<ExpressionDeclaration> UnmapExpressionDeclarations(
            ExpressionDeclDesc expr,
            StatementSpecUnMapContext unmapContext)
        {
            if (expr == null || expr.Expressions.IsEmpty())
            {
                return Collections.GetEmptyList<ExpressionDeclaration>();
            }
            return expr.Expressions.Select(desc => UnmapExpressionDeclItem(desc, unmapContext)).ToList();
        }

        private static ExpressionDeclaration UnmapExpressionDeclItem(
            ExpressionDeclItem desc,
            StatementSpecUnMapContext unmapContext)
        {
            return new ExpressionDeclaration(
                desc.Name, desc.ParametersNames, UnmapExpressionDeep(desc.Inner, unmapContext), desc.IsAlias);
        }

        private static IList<ScriptExpression> UnmapScriptExpressions(
            IList<ExpressionScriptProvided> scripts,
            StatementSpecUnMapContext unmapContext)
        {
            if (scripts == null || scripts.IsEmpty())
            {
                return Collections.GetEmptyList<ScriptExpression>();
            }
            var result = new List<ScriptExpression>();
            foreach (var script in scripts)
            {
                var e = UnmapScriptExpression(script, unmapContext);
                result.Add(e);
            }
            return result;
        }

        private static ScriptExpression UnmapScriptExpression(
            ExpressionScriptProvided script,
            StatementSpecUnMapContext unmapContext)
        {
            var returnType = script.OptionalReturnTypeName;
            if (returnType != null && script.IsOptionalReturnTypeIsArray)
            {
                returnType = returnType + "[]";
            }
            return new ScriptExpression(
                script.Name, script.ParameterNames, script.Expression, returnType, script.OptionalDialect,
                script.OptionalEventTypeName);
        }

        private static AnnotationPart UnmapAnnotation(AnnotationDesc desc)
        {
            if ((desc.Attributes == null) || (desc.Attributes.IsEmpty()))
            {
                return new AnnotationPart(desc.Name);
            }

            var attributes = new List<AnnotationAttribute>();
            foreach (var pair in desc.Attributes)
            {
                if (pair.Second is AnnotationDesc)
                {
                    attributes.Add(new AnnotationAttribute(pair.First, UnmapAnnotation((AnnotationDesc) pair.Second)));
                }
                else
                {
                    attributes.Add(new AnnotationAttribute(pair.First, pair.Second));
                }
            }
            return new AnnotationPart(desc.Name, attributes);
        }

        public static IList<AnnotationDesc> MapAnnotations(IList<AnnotationPart> annotations)
        {
            IList<AnnotationDesc> result;
            if (annotations != null)
            {
                result = annotations.Select(part => MapAnnotation(part)).ToList();
            }
            else
            {
                result = Collections.GetEmptyList<AnnotationDesc>();
            }
            return result;
        }

        private static void MapContextName(string contextName, StatementSpecRaw raw, StatementSpecMapContext mapContext)
        {
            raw.OptionalContextName = contextName;
            mapContext.ContextName = contextName;
        }

        private static void MapExpressionDeclaration(
            IList<ExpressionDeclaration> expressionDeclarations,
            StatementSpecRaw raw,
            StatementSpecMapContext mapContext)
        {
            if (expressionDeclarations == null || expressionDeclarations.IsEmpty())
            {
                return;
            }

            var desc = new ExpressionDeclDesc();
            raw.ExpressionDeclDesc = desc;

            foreach (var decl in expressionDeclarations)
            {
                var item = MapExpressionDeclItem(decl, mapContext);
                desc.Expressions.Add(item);
                mapContext.AddExpressionDeclarations(item);
            }
        }

        private static ExpressionDeclItem MapExpressionDeclItem(
            ExpressionDeclaration decl,
            StatementSpecMapContext mapContext)
        {
            return new ExpressionDeclItem(
                decl.Name,
                decl.IsAlias ? Collections.GetEmptyList<string>() : decl.ParameterNames,
                MapExpressionDeep(decl.Expression, mapContext), decl.IsAlias);
        }

        private static void MapScriptExpressions(
            IList<ScriptExpression> scriptExpressions,
            StatementSpecRaw raw,
            StatementSpecMapContext mapContext)
        {
            if (scriptExpressions == null || scriptExpressions.IsEmpty())
            {
                return;
            }

            var scripts = new List<ExpressionScriptProvided>();
            raw.ScriptExpressions = scripts;

            foreach (var decl in scriptExpressions)
            {
                var scriptProvided = MapScriptExpression(decl, mapContext);
                scripts.Add(scriptProvided);
                mapContext.AddScript(scriptProvided);
            }
        }

        private static ExpressionScriptProvided MapScriptExpression(
            ScriptExpression decl,
            StatementSpecMapContext mapContext)
        {
            string returnType = decl.OptionalReturnType != null ? decl.OptionalReturnType.Replace("[]", "") : null;
            bool isArray = decl.OptionalReturnType != null && decl.OptionalReturnType.Contains("[]");
            return new ExpressionScriptProvided(
                decl.Name, decl.ExpressionText, decl.ParameterNames, returnType, isArray, decl.OptionalEventTypeName,
                decl.OptionalDialect);
        }

        private static AnnotationDesc MapAnnotation(AnnotationPart part)
        {
            if ((part.Attributes == null) || (part.Attributes.IsEmpty()))
            {
                return new AnnotationDesc(part.Name, Collections.GetEmptyList<Pair<string, object>>());
            }

            var attributes = new List<Pair<string, Object>>();
            foreach (var pair in part.Attributes)
            {
                if (pair.Value is AnnotationPart)
                {
                    attributes.Add(new Pair<string, Object>(pair.Name, MapAnnotation((AnnotationPart) pair.Value)));
                }
                else
                {
                    attributes.Add(new Pair<string, Object>(pair.Name, pair.Value));
                }
            }
            return new AnnotationDesc(part.Name, attributes);
        }

        private static void MapSQLParameters(
            FromClause fromClause,
            StatementSpecRaw raw,
            StatementSpecMapContext mapContext)
        {
            if ((fromClause == null) || (fromClause.Streams == null))
            {
                return;
            }
            var streamNum = -1;
            foreach (var stream in fromClause.Streams)
            {
                streamNum++;
                if (!(stream is SQLStream))
                {
                    continue;
                }
                var sqlStream = (SQLStream) stream;

                IList<PlaceholderParser.Fragment> sqlFragments = null;
                try
                {
                    sqlFragments = PlaceholderParser.ParsePlaceholder(sqlStream.SqlWithSubsParams);
                }
                catch (PlaceholderParseException)
                {
                    throw new EPException(
                        "Error parsing SQL placeholder expression '" + sqlStream.SqlWithSubsParams + "': ");
                }

                foreach (var fragment in sqlFragments)
                {
                    if (!(fragment is PlaceholderParser.ParameterFragment))
                    {
                        continue;
                    }

                    // Parse expression, store for substitution parameters
                    var expression = fragment.Value;
                    if (
                        expression.ToUpperInvariant()
                            .Equals(DatabasePollingViewableFactory.SAMPLE_WHERECLAUSE_PLACEHOLDER))
                    {
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(expression))
                    {
                        throw ASTWalkException.From("Missing expression within ${...} in SQL statement");
                    }
                    var toCompile = "select * from System.Object where " + expression;
                    var rawSqlExpr = EPAdministratorHelper.CompileEPL(
                        mapContext.Container,
                        toCompile, expression,
                        false, null,
                        SelectClauseStreamSelectorEnum.ISTREAM_ONLY,
                        mapContext.EngineImportService,
                        mapContext.VariableService,
                        mapContext.SchedulingService,
                        mapContext.EngineURI,
                        mapContext.Configuration,
                        mapContext.PatternNodeFactory,
                        mapContext.ContextManagementService,
                        mapContext.ExprDeclaredService,
                        mapContext.TableService);

                    if ((rawSqlExpr.SubstitutionParameters != null) && (rawSqlExpr.SubstitutionParameters.Count > 0))
                    {
                        throw ASTWalkException.From(
                            "EPL substitution parameters are not allowed in SQL ${...} expressions, consider using a variable instead");
                    }

                    if (rawSqlExpr.HasVariables)
                    {
                        mapContext.HasVariables = true;
                    }

                    // add expression
                    if (raw.SqlParameters == null)
                    {
                        raw.SqlParameters = new Dictionary<int, IList<ExprNode>>();
                    }
                    IList<ExprNode> listExp = raw.SqlParameters.Get(streamNum);
                    if (listExp == null)
                    {
                        listExp = new List<ExprNode>();
                        raw.SqlParameters.Put(streamNum, listExp);
                    }
                    listExp.Add(rawSqlExpr.FilterRootNode);
                }
            }
        }

        private static IList<ExprChainedSpec> MapChains(
            IEnumerable<DotExpressionItem> pairs,
            StatementSpecMapContext mapContext)
        {
            return pairs
                .Select(item => new ExprChainedSpec(item.Name, MapExpressionDeep(item.Parameters, mapContext), item.IsProperty))
                .ToList();
        }

        private static IList<DotExpressionItem> UnmapChains(
            IEnumerable<ExprChainedSpec> pairs,
            StatementSpecUnMapContext unmapContext,
            bool isProperty)
        {
            return pairs
                .Select(chain => new DotExpressionItem(chain.Name, UnmapExpressionDeep(chain.Parameters, unmapContext), chain.IsProperty))
                .ToList();
        }
    }
} // end of namespace
