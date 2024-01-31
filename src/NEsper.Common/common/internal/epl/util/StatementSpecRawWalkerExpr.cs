///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.pattern.core;
using com.espertech.esper.common.@internal.epl.pattern.filter;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.util
{
    public class StatementSpecRawWalkerExpr
    {
        public static IList<ExprNode> CollectExpressionsShallow(StatementSpecRaw raw)
        {
            IList<ExprNode> expressions = new List<ExprNode>();

            if (raw.CreateContextDesc != null) {
                var detail = raw.CreateContextDesc.ContextDetail;
                if (detail is ContextSpecKeyed ks) {
                    foreach (var item in ks.Items) {
                        if (item.FilterSpecRaw.FilterExpressions != null) {
                            expressions.AddAll(item.FilterSpecRaw.FilterExpressions);
                        }
                    }

                    if (ks.OptionalInit != null) {
                        foreach (var filter in ks.OptionalInit) {
                            CollectExpressions(expressions, filter);
                        }
                    }

                    if (ks.OptionalTermination != null) {
                        CollectExpressions(expressions, ks.OptionalTermination);
                    }
                }
                else if (detail is ContextSpecCategory cat) {
                    foreach (var item in cat.Items) {
                        if (item.Expression != null) {
                            expressions.Add(item.Expression);
                        }
                    }

                    if (cat.FilterSpecRaw.FilterExpressions != null) {
                        expressions.AddAll(cat.FilterSpecRaw.FilterExpressions);
                    }
                }
                else if (detail is ContextSpecInitiatedTerminated ts) {
                    CollectExpressions(expressions, ts.StartCondition);
                    CollectExpressions(expressions, ts.EndCondition);
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
                if (raw.OnTriggerDesc is OnTriggerSplitStreamDesc onSplit) {
                    foreach (var item in onSplit.SplitStreams) {
                        if (item.SelectClause != null) {
                            AddSelectClause(expressions, item.SelectClause.SelectExprList);
                        }

                        if (item.WhereClause != null) {
                            expressions.Add(item.WhereClause);
                        }
                    }
                }

                var onSet = raw.OnTriggerDesc as OnTriggerSetDesc;
                if (onSet?.Assignments != null) {
                    foreach (var aitem in onSet.Assignments) {
                        expressions.Add(aitem.Expression);
                    }
                }

                var onUpdate = raw.OnTriggerDesc as OnTriggerWindowUpdateDesc;
                if (onUpdate?.Assignments != null) {
                    foreach (var bitem in onUpdate.Assignments) {
                        expressions.Add(bitem.Expression);
                    }
                }

                if (raw.OnTriggerDesc is OnTriggerMergeDesc onMerge) {
                    foreach (var item in onMerge.Items) {
                        if (item.OptionalMatchCond != null) {
                            expressions.Add(item.OptionalMatchCond);
                        }

                        foreach (var action in item.Actions) {
                            if (action.OptionalWhereClause != null) {
                                expressions.Add(action.OptionalWhereClause);
                            }

                            if (action is OnTriggerMergeActionUpdate update) {
                                foreach (var assignment in update.Assignments) {
                                    expressions.Add(assignment.Expression);
                                }
                            }
                            else if (action is OnTriggerMergeActionInsert insert) {
                                AddSelectClause(expressions, insert.SelectClause);
                            }
                        }
                    }

                    if (onMerge.OptionalInsertNoMatch != null) {
                        AddSelectClause(expressions, onMerge.OptionalInsertNoMatch.SelectClause);
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
                    if (stream is FilterStreamSpecRaw filterStream) {
                        var filter = filterStream.RawFilterSpec;
                        if (filter != null && filter.FilterExpressions != null) {
                            expressions.AddAll(filter.FilterExpressions);
                        }

                        if (filter != null && filter.OptionalPropertyEvalSpec != null) {
                            foreach (var contained in filter.OptionalPropertyEvalSpec.Atoms) {
                                AddSelectClause(
                                    expressions,
                                    contained.OptionalSelectClause?.SelectExprList);
                                if (contained.OptionalWhereClause != null) {
                                    expressions.Add(contained.OptionalWhereClause);
                                }
                            }
                        }
                    }

                    // pattern stream
                    if (stream is PatternStreamSpecRaw patternStream) {
                        CollectPatternExpressions(expressions, patternStream.EvalForgeNode);
                    }

                    // method stream
                    var methodStream = stream as MethodStreamSpec;
                    if (methodStream?.Expressions != null) {
                        expressions.AddAll(methodStream.Expressions);
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

            if (raw.WhereClause != null) {
                expressions.Add(raw.WhereClause);
            }

            if (raw.GroupByExpressions != null) {
                foreach (var element in raw.GroupByExpressions) {
                    if (element is GroupByClauseElementExpr expr) {
                        expressions.Add(expr.Expr);
                    }
                    else if (element is GroupByClauseElementRollupOrCube rollup) {
                        AnalyzeRollup(rollup, expressions);
                    }
                    else {
                        var set = (GroupByClauseElementGroupingSet)element;
                        foreach (var inner in set.Elements) {
                            if (inner is GroupByClauseElementExpr elementExpr) {
                                expressions.Add(elementExpr.Expr);
                            }
                            else if (inner is GroupByClauseElementCombinedExpr combinedExpr) {
                                expressions.AddAll(combinedExpr.Expressions);
                            }
                            else {
                                AnalyzeRollup((GroupByClauseElementRollupOrCube)inner, expressions);
                            }
                        }
                    }
                }
            }

            if (raw.HavingClause != null) {
                expressions.Add(raw.HavingClause);
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

                if (raw.MatchRecognizeSpec.Interval?.TimePeriodExpr != null) {
                    expressions.Add(raw.MatchRecognizeSpec.Interval.TimePeriodExpr);
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

        private static void AnalyzeRollup(
            GroupByClauseElementRollupOrCube rollup,
            IList<ExprNode> expressions)
        {
            foreach (var ex in rollup.RollupExpressions) {
                if (ex is GroupByClauseElementExpr expr) {
                    expressions.Add(expr.Expr);
                }
                else {
                    var combined = (GroupByClauseElementCombinedExpr)ex;
                    expressions.AddAll(combined.Expressions);
                }
            }
        }

        private static void AddSelectClause(
            IList<ExprNode> expressions,
            IList<SelectClauseElementRaw> selectClause)
        {
            if (selectClause == null) {
                return;
            }

            foreach (var selement in selectClause) {
                if (!(selement is SelectClauseExprRawSpec sexpr)) {
                    continue;
                }

                expressions.Add(sexpr.SelectExpression);
            }
        }

        private static void CollectPatternExpressions(
            IList<ExprNode> expressions,
            EvalForgeNode patternExpression)
        {
            var filter = patternExpression as EvalFilterForgeNode;
            if (filter?.RawFilterSpec.FilterExpressions != null) {
                expressions.AddAll(filter.RawFilterSpec.FilterExpressions);
            }

            foreach (var child in patternExpression.ChildNodes) {
                CollectPatternExpressions(expressions, child);
            }
        }

        private static void CollectExpressions(
            IList<ExprNode> expressions,
            ContextSpecCondition endpoint)
        {
            if (endpoint is ContextSpecConditionCrontab crontab) {
                foreach (var crontabItem in crontab.Crontabs) {
                    expressions.AddAll(crontabItem);
                }
            }
        }
    }
} // end of namespace