///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.visitor;
using com.espertech.esper.pattern;

namespace com.espertech.esper.epl.spec.util
{
    public class StatementSpecRawAnalyzer
    {
        public static IList<FilterSpecRaw> AnalyzeFilters(StatementSpecRaw spec) {
            IList<FilterSpecRaw> result = new List<FilterSpecRaw>();
            AddFilters(spec, result);
    
            var subselects = WalkSubselectAndDeclaredDotExpr(spec);
            foreach (var subselect in subselects.Subselects) {
                AddFilters(subselect.StatementSpecRaw, result);
            }
            return result;
        }
    
        private static void AddFilters(StatementSpecRaw spec, IList<FilterSpecRaw> filters) {
            foreach (var raw in spec.StreamSpecs) {
                if (raw is FilterStreamSpecRaw) {
                    var r = (FilterStreamSpecRaw) raw;
                    filters.Add(r.RawFilterSpec);
                }
                if (raw is PatternStreamSpecRaw) {
                    var r = (PatternStreamSpecRaw) raw;
                    var evalNodeAnalysisResult = EvalNodeUtil.RecursiveAnalyzeChildNodes(r.EvalFactoryNode);
                    var filterNodes = evalNodeAnalysisResult.FilterNodes;
                    foreach (var filterNode in filterNodes)
                    {
                        filters.Add(filterNode.RawFilterSpec);
                    }
                }
            }
        }
    
        public static ExprNodeSubselectDeclaredDotVisitor WalkSubselectAndDeclaredDotExpr(StatementSpecRaw spec)
        {
            // Look for expressions with sub-selects in select expression list and filter expression
            // Recursively compile the statement within the statement.
            var visitor = new ExprNodeSubselectDeclaredDotVisitor();
            foreach (var raw in spec.SelectClauseSpec.SelectExprList)
            {
                if (raw is SelectClauseExprRawSpec)
                {
                    var rawExpr = (SelectClauseExprRawSpec) raw;
                    rawExpr.SelectExpression.Accept(visitor);
                }
            }
            if (spec.FilterRootNode != null)
            {
                spec.FilterRootNode.Accept(visitor);
            }
            if (spec.HavingExprRootNode != null)
            {
                spec.HavingExprRootNode.Accept(visitor);
            }
            if (spec.UpdateDesc != null)
            {
                if (spec.UpdateDesc.OptionalWhereClause != null)
                {
                    spec.UpdateDesc.OptionalWhereClause.Accept(visitor);
                }
                foreach (var assignment in spec.UpdateDesc.Assignments)
                {
                    assignment.Expression.Accept(visitor);
                }
            }
            if (spec.OnTriggerDesc != null) {
                VisitSubselectOnTrigger(spec.OnTriggerDesc, visitor);
            }
            // Determine pattern-filter subqueries
            foreach (var streamSpecRaw in spec.StreamSpecs) {
                if (streamSpecRaw is PatternStreamSpecRaw) {
                    var patternStreamSpecRaw = (PatternStreamSpecRaw) streamSpecRaw;
                    var analysisResult = EvalNodeUtil.RecursiveAnalyzeChildNodes(patternStreamSpecRaw.EvalFactoryNode);
                    foreach (var evalNode in analysisResult.ActiveNodes) {
                        if (evalNode is EvalFilterFactoryNode) {
                            var filterNode = (EvalFilterFactoryNode) evalNode;
                            foreach (var filterExpr in filterNode.RawFilterSpec.FilterExpressions) {
                                filterExpr.Accept(visitor);
                            }
                        }
                        else if (evalNode is EvalObserverFactoryNode) {
                            var beforeCount = visitor.Subselects.Count;
                            var observerNode = (EvalObserverFactoryNode) evalNode;
                            foreach (var param in observerNode.PatternObserverSpec.ObjectParameters) {
                                param.Accept(visitor);
                            }
                            if (visitor.Subselects.Count != beforeCount) {
                                throw new ExprValidationException("Subselects are not allowed within pattern observer parameters, please consider using a variable instead");
                            }
                        }
                    }
                }
            }
            // Determine filter streams
            foreach (var rawSpec in spec.StreamSpecs)
            {
                if (rawSpec is FilterStreamSpecRaw) {
                    var raw = (FilterStreamSpecRaw) rawSpec;
                    foreach (var filterExpr in raw.RawFilterSpec.FilterExpressions) {
                        filterExpr.Accept(visitor);
                    }
                }
            }
    
            return visitor;
        }
    
        private static void VisitSubselectOnTrigger(OnTriggerDesc onTriggerDesc, ExprNodeSubselectDeclaredDotVisitor visitor) {
            if (onTriggerDesc is OnTriggerWindowUpdateDesc) {
                var updates = (OnTriggerWindowUpdateDesc) onTriggerDesc;
                foreach (var assignment in updates.Assignments)
                {
                    assignment.Expression.Accept(visitor);
                }
            }
            else if (onTriggerDesc is OnTriggerSetDesc) {
                var sets = (OnTriggerSetDesc) onTriggerDesc;
                foreach (var assignment in sets.Assignments)
                {
                    assignment.Expression.Accept(visitor);
                }
            }
            else if (onTriggerDesc is OnTriggerSplitStreamDesc) {
                var splits = (OnTriggerSplitStreamDesc) onTriggerDesc;
                foreach (var split in splits.SplitStreams)
                {
                    if (split.WhereClause != null) {
                        split.WhereClause.Accept(visitor);
                    }
                    if (split.SelectClause.SelectExprList != null) {
                        foreach (var element in split.SelectClause.SelectExprList) {
                            if (element is SelectClauseExprRawSpec) {
                                var selectExpr = (SelectClauseExprRawSpec) element;
                                selectExpr.SelectExpression.Accept(visitor);
                            }
                        }
                    }
                }
            }
            else if (onTriggerDesc is OnTriggerMergeDesc) {
                var merge = (OnTriggerMergeDesc) onTriggerDesc;
                foreach (var matched in merge.Items) {
                    if (matched.OptionalMatchCond != null) {
                        matched.OptionalMatchCond.Accept(visitor);
                    }
                    foreach (var action in matched.Actions)
                    {
                        if (action.OptionalWhereClause != null) {
                            action.OptionalWhereClause.Accept(visitor);
                        }
    
                        if (action is OnTriggerMergeActionUpdate) {
                            var update = (OnTriggerMergeActionUpdate) action;
                            foreach (var assignment in update.Assignments)
                            {
                                assignment.Expression.Accept(visitor);
                            }
                        }
                        if (action is OnTriggerMergeActionInsert) {
                            var insert = (OnTriggerMergeActionInsert) action;
                            foreach (var element in insert.SelectClause) {
                                if (element is SelectClauseExprRawSpec) {
                                    var selectExpr = (SelectClauseExprRawSpec) element;
                                    selectExpr.SelectExpression.Accept(visitor);
                                }
                            }
                        }
                    }
                }
            }
        }
    
        public static IList<ExprNode> CollectExpressionsShallow(StatementSpecRaw raw) {
            var expressions = new List<ExprNode>();
    
            if (raw.ExpressionDeclDesc != null) {
                foreach (var decl in raw.ExpressionDeclDesc.Expressions) {
                    expressions.Add(decl.Inner);
                }
            }
    
            if (raw.CreateExpressionDesc != null) {
                if (raw.CreateExpressionDesc.Expression != null) {
                    expressions.Add(raw.CreateExpressionDesc.Expression.Inner);
                }
            }
    
            if (raw.CreateContextDesc != null) {
                var detail = raw.CreateContextDesc.ContextDetail;
                if (detail is ContextDetailPartitioned) {
                    var ks = (ContextDetailPartitioned) detail;
                    foreach (var item in ks.Items) {
                        if (item.FilterSpecRaw.FilterExpressions != null) {
                            expressions.AddAll(item.FilterSpecRaw.FilterExpressions);
                        }
                    }
                }
                else if (detail is ContextDetailCategory) {
                    var cat = (ContextDetailCategory) detail;
                    foreach (var item in cat.Items) {
                        if (item.Expression != null) {
                            expressions.Add(item.Expression);
                        }
                    }
                    if (cat.FilterSpecRaw.FilterExpressions != null) {
                        expressions.AddAll(cat.FilterSpecRaw.FilterExpressions);
                    }
                }
                else if (detail is ContextDetailInitiatedTerminated) {
                    var ts = (ContextDetailInitiatedTerminated) detail;
                    CollectExpressions(expressions, ts.Start);
                    CollectExpressions(expressions, ts.End);
                }
                else {
                    throw new EPException("Failed to obtain expressions from context detail " + detail);
                }
            }
    
            if (raw.CreateVariableDesc != null) {
                var expr = raw.CreateVariableDesc.Assignment;
                if (expr != null) {
                    expressions.Add(expr);
                }
            }
    
            if (raw.CreateWindowDesc != null) {
                var expr = raw.CreateWindowDesc.InsertFilter;
                if (expr != null) {
                    expressions.Add(expr);
                }
                foreach (var view in raw.CreateWindowDesc.ViewSpecs) {
                    expressions.AddAll(view.ObjectParameters);
                }
            }
    
            if (raw.UpdateDesc != null) {
                if (raw.UpdateDesc.OptionalWhereClause != null) {
                    expressions.Add(raw.UpdateDesc.OptionalWhereClause);
                }
                if (raw.UpdateDesc.Assignments != null) {
                    foreach (var pair in raw.UpdateDesc.Assignments) {
                        expressions.Add(pair.Expression);
                    } 
                }
            }
                                
            // on-expr
            if (raw.OnTriggerDesc != null) {
                if (raw.OnTriggerDesc is OnTriggerSplitStreamDesc) {
                    var onSplit = (OnTriggerSplitStreamDesc) raw.OnTriggerDesc;
                    foreach (var item in onSplit.SplitStreams) {
                        if (item.SelectClause != null) {
                            AddSelectClause(expressions, item.SelectClause.SelectExprList);
                        }
                        if (item.WhereClause != null) {
                            expressions.Add(item.WhereClause);
                        }
                    }
                }
                if (raw.OnTriggerDesc is OnTriggerSetDesc) {
                    var onSet = (OnTriggerSetDesc) raw.OnTriggerDesc;
                    if (onSet.Assignments != null) {
                        foreach (var aitem in onSet.Assignments) {
                            expressions.Add(aitem.Expression);
                        }
                    }
                }
                if (raw.OnTriggerDesc is OnTriggerWindowUpdateDesc) {
                    var onUpdate = (OnTriggerWindowUpdateDesc) raw.OnTriggerDesc;
                    if (onUpdate.Assignments != null) {
                        foreach (var bitem in onUpdate.Assignments) {
                            expressions.Add(bitem.Expression);
                        }
                    }
                }
                if (raw.OnTriggerDesc is OnTriggerMergeDesc) {
                    var onMerge = (OnTriggerMergeDesc) raw.OnTriggerDesc;
                    foreach (var item in onMerge.Items) {
                        if (item.OptionalMatchCond != null) {
                            expressions.Add(item.OptionalMatchCond);
                        }
                        foreach (var action in item.Actions) {
                            if (action is OnTriggerMergeActionDelete) {
                                var delete = (OnTriggerMergeActionDelete) action;
                                if (delete.OptionalWhereClause != null) {
                                    expressions.Add(delete.OptionalWhereClause);
                                }
                            }
                            else if (action is OnTriggerMergeActionUpdate) {
                                var update = (OnTriggerMergeActionUpdate) action;
                                if (update.OptionalWhereClause != null) {
                                    expressions.Add(update.OptionalWhereClause);
                                }
                                foreach (var assignment in update.Assignments) {
                                    expressions.Add(assignment.Expression);
                                }
                            }
                            else if (action is OnTriggerMergeActionInsert) {
                                var insert = (OnTriggerMergeActionInsert) action;
                                if (insert.OptionalWhereClause != null) {
                                    expressions.Add(insert.OptionalWhereClause);
                                }
                                AddSelectClause(expressions, insert.SelectClause);
                            }
                        }
                    }
                }
            }
            
            // select clause
            if (raw.SelectClauseSpec != null) {
                AddSelectClause(expressions, raw.SelectClauseSpec.SelectExprList);
            }
    
            // from clause
            if (raw.StreamSpecs != null) {
                foreach (var stream in raw.StreamSpecs) {
                    // filter stream
                    if (stream is FilterStreamSpecRaw) {
                        var filterStream = (FilterStreamSpecRaw) stream;
                        var filter = filterStream.RawFilterSpec;
                        if ((filter != null) && (filter.FilterExpressions != null)){
                            expressions.AddAll(filter.FilterExpressions);
                        }
                        if ((filter != null) && (filter.OptionalPropertyEvalSpec != null)) {
                            foreach (var contained in filter.OptionalPropertyEvalSpec.Atoms) {
                                AddSelectClause(expressions, contained.OptionalSelectClause == null ? null : contained.OptionalSelectClause.SelectExprList);
                                if (contained.OptionalWhereClause != null) {
                                    expressions.Add(contained.OptionalWhereClause);
                                }
                            }
                        }
                    }
                    // pattern stream
                    if (stream is PatternStreamSpecRaw) {
                        var patternStream = (PatternStreamSpecRaw) stream;
                        CollectPatternExpressions(expressions, patternStream.EvalFactoryNode);
                    }
                    // method stream
                    if (stream is MethodStreamSpec) {
                        var methodStream = (MethodStreamSpec) stream;
                        if (methodStream.Expressions != null) {
                            expressions.AddAll(methodStream.Expressions);
                        }
                    }
                    if (stream.ViewSpecs != null) {
                        foreach (var view in stream.ViewSpecs) {
                            expressions.AddAll(view.ObjectParameters);
                        }
                    }
                }
    
                if (raw.OuterJoinDescList != null) {
                    foreach (var q in raw.OuterJoinDescList) {
                        if (q.OptLeftNode != null) {
                            expressions.Add(q.OptLeftNode);
                            expressions.Add(q.OptRightNode);
                            foreach (var ident in q.AdditionalLeftNodes) {
                                expressions.Add(ident);
                            }
                            foreach (var ident in q.AdditionalRightNodes) {
                                expressions.Add(ident);
                            }
                        }
                    }
                }
            }
            
            if (raw.FilterRootNode != null) {
                expressions.Add(raw.FilterRootNode);
            }
            
            if (raw.GroupByExpressions != null) {
                foreach (GroupByClauseElement element in raw.GroupByExpressions) {
                    if (element is GroupByClauseElementExpr) {
                        expressions.Add( ((GroupByClauseElementExpr) element).Expr);
                    }
                    else if (element is GroupByClauseElementRollupOrCube) {
                        var rollup = (GroupByClauseElementRollupOrCube) element;
                        AnalyzeRollup(rollup, expressions);
                    }
                    else {
                        var set = (GroupByClauseElementGroupingSet) element;
                        foreach (GroupByClauseElement inner in set.Elements) {
                            if (inner is GroupByClauseElementExpr) {
                                expressions.Add( ((GroupByClauseElementExpr) inner).Expr);
                            }
                            else if (inner is GroupByClauseElementCombinedExpr)
                            {
                                expressions.AddAll( ((GroupByClauseElementCombinedExpr) inner).Expressions);
                            }
                            else {
                                AnalyzeRollup((GroupByClauseElementRollupOrCube) inner, expressions);
                            }
                        }
                    }
                }
            }
    
            if (raw.HavingExprRootNode != null) {
                expressions.Add(raw.HavingExprRootNode);
            }
    
            if (raw.OutputLimitSpec != null) {
                if (raw.OutputLimitSpec.WhenExpressionNode != null) {
                    expressions.Add(raw.OutputLimitSpec.WhenExpressionNode);
                }
                if (raw.OutputLimitSpec.ThenExpressions != null) {
                    foreach (var thenAssign in raw.OutputLimitSpec.ThenExpressions) {
                        expressions.Add(thenAssign.Expression);
                    }					
                }
                if (raw.OutputLimitSpec.CrontabAtSchedule != null) {
                    expressions.AddAll(raw.OutputLimitSpec.CrontabAtSchedule);
                }
                if (raw.OutputLimitSpec.TimePeriodExpr != null) {
                    expressions.Add(raw.OutputLimitSpec.TimePeriodExpr);
                }
                if (raw.OutputLimitSpec.AfterTimePeriodExpr != null) {
                    expressions.Add(raw.OutputLimitSpec.AfterTimePeriodExpr);
                }
            }
            
            if (raw.OrderByList != null) {
                foreach (var orderByElement in raw.OrderByList) {
                    expressions.Add(orderByElement.ExprNode);
                }									
            }
            
            if (raw.MatchRecognizeSpec != null) {
                if (raw.MatchRecognizeSpec.PartitionByExpressions != null) {
                    expressions.AddAll(raw.MatchRecognizeSpec.PartitionByExpressions);
                }
                foreach (var selectItemMR in raw.MatchRecognizeSpec.Measures) {
                    expressions.Add(selectItemMR.Expr);
                }
                foreach (var define in raw.MatchRecognizeSpec.Defines) {
                    expressions.Add(define.Expression);
                }
                if (raw.MatchRecognizeSpec.Interval != null) {
                    if (raw.MatchRecognizeSpec.Interval.TimePeriodExpr != null) {
                        expressions.Add(raw.MatchRecognizeSpec.Interval.TimePeriodExpr);
                    }
                }
            }
    
            if (raw.ForClauseSpec != null) {
                foreach (var item in raw.ForClauseSpec.Clauses) {
                    if (item.Expressions != null) {
                        expressions.AddAll(item.Expressions);
                    }
                }
            }
    
            return expressions;
        }
    
        private static void AnalyzeRollup(GroupByClauseElementRollupOrCube rollup, List<ExprNode> expressions) {
            foreach (GroupByClauseElement ex in rollup.RollupExpressions) {
                if (ex is GroupByClauseElementExpr) {
                    expressions.Add( ((GroupByClauseElementExpr) ex).Expr);
                }
                else {
                    var combined = (GroupByClauseElementCombinedExpr) ex;
                    expressions.AddAll(combined.Expressions);
                }
            }
        }

        private static void CollectExpressions(IList<ExprNode> expressions, ContextDetailCondition endpoint) {
            if (endpoint is ContextDetailConditionCrontab) {
                var crontab = (ContextDetailConditionCrontab) endpoint;
                expressions.AddAll(crontab.Crontab);
            }
        }
    
        private static void AddSelectClause(IList<ExprNode> expressions, IList<SelectClauseElementRaw> selectClause) {
            if (selectClause == null) {
                return;
            }
            foreach (var selement in selectClause) {
                if (!(selement is SelectClauseExprRawSpec)) {
                    continue;
                }
                var sexpr = (SelectClauseExprRawSpec) selement;
                expressions.Add(sexpr.SelectExpression);
            }
        }
    
        private static void CollectPatternExpressions(IList<ExprNode> expressions, EvalFactoryNode patternExpression) {
    
            if (patternExpression is EvalFilterFactoryNode) {
                var filter = (EvalFilterFactoryNode) patternExpression;
                if (filter.RawFilterSpec.FilterExpressions != null) {
                    expressions.AddAll(filter.RawFilterSpec.FilterExpressions);
                }
            }
    
            foreach (var child in patternExpression.ChildNodes) {
                CollectPatternExpressions(expressions, child);
            }
        }
    }
}
