///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
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
            ExprNodeSubselectDeclaredDotVisitor visitor = new ExprNodeSubselectDeclaredDotVisitor();
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

            spec.WhereClause?.Accept(visitor);

            spec.HavingClause?.Accept(visitor);

            if (spec.UpdateDesc != null) {
                spec.UpdateDesc.OptionalWhereClause?.Accept(visitor);

                foreach (OnTriggerSetAssignment assignment in spec.UpdateDesc.Assignments) {
                    assignment.Expression.Accept(visitor);
                }
            }

            if (spec.OnTriggerDesc != null) {
                VisitSubselectOnTrigger(spec.OnTriggerDesc, visitor);
            }

            // walk streams
            WalkStreamSpecs(spec, visitor);

            // walk FAF
            WalkFAFSpec(spec.FireAndForgetSpec, visitor);
        }

        private static void WalkFAFSpec(
            FireAndForgetSpec fireAndForgetSpec,
            ExprNodeSubselectDeclaredDotVisitor visitor)
        {
            if (fireAndForgetSpec == null) {
                return;
            }

            if (fireAndForgetSpec is FireAndForgetSpecUpdate update) {
                foreach (OnTriggerSetAssignment assignment in update.Assignments) {
                    assignment.Expression.Accept(visitor);
                }
            }
        }

        public static void WalkStreamSpecs(
            StatementSpecRaw spec,
            ExprNodeSubselectDeclaredDotVisitor visitor)
        {
            // determine pattern-filter subqueries
            foreach (StreamSpecRaw streamSpecRaw in spec.StreamSpecs) {
                if (streamSpecRaw is PatternStreamSpecRaw) {
                    PatternStreamSpecRaw patternStreamSpecRaw = (PatternStreamSpecRaw) streamSpecRaw;
                    EvalNodeAnalysisResult analysisResult =
                        EvalNodeUtil.RecursiveAnalyzeChildNodes(patternStreamSpecRaw.EvalForgeNode);
                    foreach (EvalForgeNode evalNode in analysisResult.ActiveNodes) {
                        if (evalNode is EvalFilterForgeNode) {
                            EvalFilterForgeNode filterNode = (EvalFilterForgeNode) evalNode;
                            foreach (ExprNode filterExpr in filterNode.RawFilterSpec.FilterExpressions) {
                                filterExpr.Accept(visitor);
                            }
                        }
                        else if (evalNode is EvalObserverForgeNode) {
                            int beforeCount = visitor.Subselects.Count;
                            EvalObserverForgeNode observerNode = (EvalObserverForgeNode) evalNode;
                            foreach (ExprNode param in observerNode.PatternObserverSpec.ObjectParameters) {
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
            foreach (StreamSpecRaw rawSpec in spec.StreamSpecs) {
                if (rawSpec is FilterStreamSpecRaw) {
                    FilterStreamSpecRaw raw = (FilterStreamSpecRaw) rawSpec;
                    foreach (ExprNode filterExpr in raw.RawFilterSpec.FilterExpressions) {
                        filterExpr.Accept(visitor);
                    }
                }
            }
        }

        private static void VisitSubselectOnTrigger(
            OnTriggerDesc onTriggerDesc,
            ExprNodeSubselectDeclaredDotVisitor visitor)
        {
            if (onTriggerDesc is OnTriggerWindowUpdateDesc) {
                OnTriggerWindowUpdateDesc updates = (OnTriggerWindowUpdateDesc) onTriggerDesc;
                foreach (OnTriggerSetAssignment assignment in updates.Assignments) {
                    assignment.Expression.Accept(visitor);
                }
            }
            else if (onTriggerDesc is OnTriggerSetDesc) {
                OnTriggerSetDesc sets = (OnTriggerSetDesc) onTriggerDesc;
                foreach (OnTriggerSetAssignment assignment in sets.Assignments) {
                    assignment.Expression.Accept(visitor);
                }
            }
            else if (onTriggerDesc is OnTriggerSplitStreamDesc) {
                OnTriggerSplitStreamDesc splits = (OnTriggerSplitStreamDesc) onTriggerDesc;
                foreach (OnTriggerSplitStream split in splits.SplitStreams) {
                    split.WhereClause?.Accept(visitor);

                    if (split.SelectClause.SelectExprList != null) {
                        WalkSubselectSelectClause(split.SelectClause.SelectExprList, visitor);
                    }
                }
            }
            else if (onTriggerDesc is OnTriggerMergeDesc) {
                OnTriggerMergeDesc merge = (OnTriggerMergeDesc) onTriggerDesc;
                foreach (OnTriggerMergeMatched matched in merge.Items) {
                    matched.OptionalMatchCond?.Accept(visitor);

                    foreach (OnTriggerMergeAction action in matched.Actions) {
                        action.OptionalWhereClause?.Accept(visitor);

                        if (action is OnTriggerMergeActionUpdate) {
                            OnTriggerMergeActionUpdate update = (OnTriggerMergeActionUpdate) action;
                            foreach (OnTriggerSetAssignment assignment in update.Assignments) {
                                assignment.Expression.Accept(visitor);
                            }
                        }

                        if (action is OnTriggerMergeActionInsert) {
                            OnTriggerMergeActionInsert insert = (OnTriggerMergeActionInsert) action;
                            WalkSubselectSelectClause(insert.SelectClause, visitor);
                        }
                    }
                }

                if (merge.OptionalInsertNoMatch != null) {
                    WalkSubselectSelectClause(merge.OptionalInsertNoMatch.SelectClause, visitor);
                }
            }
        }

        private static void WalkSubselectSelectClause(
            IList<SelectClauseElementRaw> selectClause,
            ExprNodeSubselectDeclaredDotVisitor visitor)
        {
            foreach (SelectClauseElementRaw element in selectClause) {
                if (element is SelectClauseExprRawSpec) {
                    SelectClauseExprRawSpec selectExpr = (SelectClauseExprRawSpec) element;
                    selectExpr.SelectExpression.Accept(visitor);
                }
            }
        }
    }
} // end of namespace