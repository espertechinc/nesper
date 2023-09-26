///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.visitor;
using com.espertech.esper.common.@internal.epl.pattern.core;
using com.espertech.esper.common.@internal.epl.pattern.filter;
using com.espertech.esper.common.@internal.epl.pattern.observer;


namespace com.espertech.esper.common.@internal.epl.util
{
    public class StatementSpecRawWalkerSubselectAndDeclaredDot
    {
        public static ExprNodeSubselectDeclaredDotVisitor WalkSubselectAndDeclaredDotExpr(StatementSpecRaw spec)
        {
            var visitor = new ExprNodeSubselectDeclaredDotVisitor();
            WalkSubselectAndDeclaredDotExpr(spec, visitor);
            return visitor;
        }

        public static void WalkSubselectAndDeclaredDotExpr(
            StatementSpecRaw spec,
            ExprNodeSubselectDeclaredDotVisitor visitor)
        {
            // Look for expressions with sub-selects in select expression list and filter expression
            // Recursively compile the statement within the statement.
            WalkSubselectSelectClause(spec.SelectClauseSpec.SelectExprList, visitor);

            if (spec.WhereClause != null) {
                spec.WhereClause.Accept(visitor);
            }

            if (spec.HavingClause != null) {
                spec.HavingClause.Accept(visitor);
            }

            if (spec.UpdateDesc != null) {
                if (spec.UpdateDesc.OptionalWhereClause != null) {
                    spec.UpdateDesc.OptionalWhereClause.Accept(visitor);
                }

                foreach (var assignment in spec.UpdateDesc.Assignments) {
                    assignment.Expression.Accept(visitor);
                }
            }

            if (spec.OnTriggerDesc != null) {
                VisitSubselectOnTrigger(spec.OnTriggerDesc, visitor);
            }

            // walk streams
            WalkStreamSpecs(spec, visitor);

            if (spec.InsertIntoDesc != null) {
                if (spec.InsertIntoDesc.EventPrecedence != null) {
                    spec.InsertIntoDesc.EventPrecedence.Accept(visitor);
                }
            }

            // walk FAF
            WalkFAFSpec(spec.FireAndForgetSpec, visitor);

            // walk SQL-parameters
            var sqlParams = spec.SqlParameters;
            if (sqlParams != null) {
                foreach (var entry in sqlParams) {
                    foreach (var node in entry.Value) {
                        node.Accept(visitor);
                    }
                }
            }
        }

        private static void WalkFAFSpec(
            FireAndForgetSpec fireAndForgetSpec,
            ExprNodeSubselectDeclaredDotVisitor visitor)
        {
            if (fireAndForgetSpec == null) {
                return;
            }

            if (fireAndForgetSpec is FireAndForgetSpecUpdate update) {
                foreach (var assignment in update.Assignments) {
                    assignment.Expression.Accept(visitor);
                }
            }

            if (fireAndForgetSpec is FireAndForgetSpecInsert insert) {
                foreach (var row in insert.Multirow) {
                    foreach (var col in row) {
                        col.Accept(visitor);
                    }
                }
            }
        }

        public static void WalkStreamSpecs(
            StatementSpecRaw spec,
            ExprNodeSubselectDeclaredDotVisitor visitor)
        {
            // determine pattern-filter subqueries
            foreach (var streamSpecRaw in spec.StreamSpecs) {
                if (streamSpecRaw is PatternStreamSpecRaw patternStreamSpecRaw) {
                    var analysisResult = EvalNodeUtil.RecursiveAnalyzeChildNodes(patternStreamSpecRaw.EvalForgeNode);
                    foreach (var evalNode in analysisResult.ActiveNodes) {
                        if (evalNode is EvalFilterForgeNode filterNode) {
                            foreach (var filterExpr in filterNode.RawFilterSpec.FilterExpressions) {
                                filterExpr.Accept(visitor);
                            }
                        }
                        else if (evalNode is EvalObserverForgeNode observerNode) {
                            var beforeCount = visitor.Subselects.Count;
                            foreach (var param in observerNode.PatternObserverSpec.ObjectParameters) {
                                param.Accept(visitor);
                            }

                            if (visitor.Subselects.Count != beforeCount) {
                                throw new ExprValidationException(
                                    "Subselects are not allowed within pattern observer parameters, please consider using a variable instead");
                            }
                        }
                    }
                }
            }

            // determine filter streams
            foreach (var rawSpec in spec.StreamSpecs) {
                if (rawSpec is FilterStreamSpecRaw raw) {
                    foreach (var filterExpr in raw.RawFilterSpec.FilterExpressions) {
                        filterExpr.Accept(visitor);
                    }
                }
            }
        }

        private static void VisitSubselectOnTrigger(
            OnTriggerDesc onTriggerDesc,
            ExprNodeSubselectDeclaredDotVisitor visitor)
        {
            if (onTriggerDesc is OnTriggerWindowUpdateDesc updates) {
                foreach (var assignment in updates.Assignments) {
                    assignment.Expression.Accept(visitor);
                }
            }
            else if (onTriggerDesc is OnTriggerSetDesc sets) {
                foreach (var assignment in sets.Assignments) {
                    assignment.Expression.Accept(visitor);
                }
            }
            else if (onTriggerDesc is OnTriggerSplitStreamDesc splits) {
                foreach (var split in splits.SplitStreams) {
                    if (split.WhereClause != null) {
                        split.WhereClause.Accept(visitor);
                    }

                    if (split.SelectClause.SelectExprList != null) {
                        WalkSubselectSelectClause(split.SelectClause.SelectExprList, visitor);
                    }

                    if (split.InsertInto != null) {
                        if (split.InsertInto.EventPrecedence != null) {
                            split.InsertInto.EventPrecedence.Accept(visitor);
                        }
                    }
                }
            }
            else if (onTriggerDesc is OnTriggerMergeDesc merge) {
                foreach (var matched in merge.Items) {
                    if (matched.OptionalMatchCond != null) {
                        matched.OptionalMatchCond.Accept(visitor);
                    }

                    foreach (var action in matched.Actions) {
                        if (action.OptionalWhereClause != null) {
                            action.OptionalWhereClause.Accept(visitor);
                        }

                        if (action is OnTriggerMergeActionUpdate update) {
                            foreach (var assignment in update.Assignments) {
                                assignment.Expression.Accept(visitor);
                            }
                        }

                        if (action is OnTriggerMergeActionInsert insert) {
                            WalkOnMergeActionInsert(insert, visitor);
                        }
                    }
                }

                if (merge.OptionalInsertNoMatch != null) {
                    WalkOnMergeActionInsert(merge.OptionalInsertNoMatch, visitor);
                }
            }
        }

        private static void WalkOnMergeActionInsert(
            OnTriggerMergeActionInsert action,
            ExprNodeSubselectDeclaredDotVisitor visitor)
        {
            WalkSubselectSelectClause(action.SelectClause, visitor);
            if (action.EventPrecedence != null) {
                action.EventPrecedence.Accept(visitor);
            }
        }

        private static void WalkSubselectSelectClause(
            IList<SelectClauseElementRaw> selectClause,
            ExprNodeSubselectDeclaredDotVisitor visitor)
        {
            foreach (var element in selectClause) {
                if (element is SelectClauseExprRawSpec selectExpr) {
                    selectExpr.SelectExpression.Accept(visitor);
                }
            }
        }
    }
} // end of namespace