///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.compat;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.visitor;
using com.espertech.esper.epl.spec;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.filter;
using com.espertech.esper.pattern;

namespace com.espertech.esper.core.service
{
    public class StatementLifecycleSvcUtil
    {
        public static void AssignFilterSpecIds(FilterSpecCompiled filterSpec, FilterSpecCompiled[] filterSpecsAll)
        {
            for (int path = 0; path < filterSpec.Parameters.Length; path++)
            {
                foreach (FilterSpecParam param in filterSpec.Parameters[path])
                {
                    if (param is FilterSpecParamExprNode)
                    {
                        var index = filterSpec.GetFilterSpecIndexAmongAll(filterSpecsAll);
                        var exprNode = (FilterSpecParamExprNode) param;
                        exprNode.FilterSpecId = index;
                        exprNode.FilterSpecParamPathNum = path;
                    }
                }
            }
        }

        public static void WalkStatement(StatementSpecRaw spec, ExprNodeSubselectDeclaredDotVisitor visitor)
        {
            // Look for expressions with sub-selects in select expression list and filter expression
            // Recursively compile the statement within the statement.
            foreach (SelectClauseElementRaw raw in spec.SelectClauseSpec.SelectExprList)
            {
                if (raw is SelectClauseExprRawSpec)
                {
                    SelectClauseExprRawSpec rawExpr = (SelectClauseExprRawSpec) raw;
                    rawExpr.SelectExpression.Accept(visitor);
                }
                else
                {
                    continue;
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
                foreach (OnTriggerSetAssignment assignment in spec.UpdateDesc.Assignments)
                {
                    assignment.Expression.Accept(visitor);
                }
            }
            if (spec.OnTriggerDesc != null) {
                VisitSubselectOnTrigger(spec.OnTriggerDesc, visitor);
            }
            // Determine pattern-filter subqueries
            foreach (StreamSpecRaw streamSpecRaw in spec.StreamSpecs) {
                if (streamSpecRaw is PatternStreamSpecRaw) {
                    PatternStreamSpecRaw patternStreamSpecRaw = (PatternStreamSpecRaw) streamSpecRaw;
                    EvalNodeAnalysisResult analysisResult = EvalNodeUtil.RecursiveAnalyzeChildNodes(patternStreamSpecRaw.EvalFactoryNode);
                    foreach (EvalFactoryNode evalNode in analysisResult.ActiveNodes) {
                        if (evalNode is EvalFilterFactoryNode) {
                            EvalFilterFactoryNode filterNode = (EvalFilterFactoryNode) evalNode;
                            foreach (ExprNode filterExpr in filterNode.RawFilterSpec.FilterExpressions) {
                                filterExpr.Accept(visitor);
                            }
                        }
                        else if (evalNode is EvalObserverFactoryNode) {
                            int beforeCount = visitor.Subselects.Count;
                            EvalObserverFactoryNode observerNode = (EvalObserverFactoryNode) evalNode;
                            foreach (ExprNode param in observerNode.PatternObserverSpec.ObjectParameters) {
                                param.Accept(visitor);
                            }
                            if (visitor.Subselects.Count != beforeCount) {
                                throw new ExprValidationException("Subselects are not allowed within pattern observer parameters, please consider using a variable instead");
                            }
                        }
                    }
                }
            }
    
            // walk streams
            WalkStreamSpecs(spec, visitor);
        }
    
        public static void WalkStreamSpecs(StatementSpecRaw spec, ExprNodeSubselectDeclaredDotVisitor visitor) {
    
            // Determine filter streams
            foreach (StreamSpecRaw rawSpec in spec.StreamSpecs)
            {
                if (rawSpec is FilterStreamSpecRaw) {
                    FilterStreamSpecRaw raw = (FilterStreamSpecRaw) rawSpec;
                    foreach (ExprNode filterExpr in raw.RawFilterSpec.FilterExpressions) {
                        filterExpr.Accept(visitor);
                    }
                }
                if (rawSpec is PatternStreamSpecRaw) {
                    PatternStreamSpecRaw patternStreamSpecRaw = (PatternStreamSpecRaw) rawSpec;
                    EvalNodeAnalysisResult analysisResult = EvalNodeUtil.RecursiveAnalyzeChildNodes(patternStreamSpecRaw.EvalFactoryNode);
                    foreach (EvalFactoryNode evalNode in analysisResult.ActiveNodes) {
                        if (evalNode is EvalFilterFactoryNode) {
                            EvalFilterFactoryNode filterNode = (EvalFilterFactoryNode) evalNode;
                            foreach (ExprNode filterExpr in filterNode.RawFilterSpec.FilterExpressions) {
                                filterExpr.Accept(visitor);
                            }
                        }
                    }
                }
            }
        }
    
        private static void VisitSubselectOnTrigger(OnTriggerDesc onTriggerDesc, ExprNodeSubselectDeclaredDotVisitor visitor) {
            if (onTriggerDesc is OnTriggerWindowUpdateDesc) {
                OnTriggerWindowUpdateDesc updates = (OnTriggerWindowUpdateDesc) onTriggerDesc;
                foreach (OnTriggerSetAssignment assignment in updates.Assignments)
                {
                    assignment.Expression.Accept(visitor);
                }
            }
            else if (onTriggerDesc is OnTriggerSetDesc) {
                OnTriggerSetDesc sets = (OnTriggerSetDesc) onTriggerDesc;
                foreach (OnTriggerSetAssignment assignment in sets.Assignments)
                {
                    assignment.Expression.Accept(visitor);
                }
            }
            else if (onTriggerDesc is OnTriggerSplitStreamDesc) {
                OnTriggerSplitStreamDesc splits = (OnTriggerSplitStreamDesc) onTriggerDesc;
                foreach (OnTriggerSplitStream split in splits.SplitStreams)
                {
                    if (split.WhereClause != null) {
                        split.WhereClause.Accept(visitor);
                    }
                    if (split.SelectClause.SelectExprList != null) {
                        foreach (SelectClauseElementRaw element in split.SelectClause.SelectExprList) {
                            if (element is SelectClauseExprRawSpec) {
                                SelectClauseExprRawSpec selectExpr = (SelectClauseExprRawSpec) element;
                                selectExpr.SelectExpression.Accept(visitor);
                            }
                        }
                    }
                }
            }
            else if (onTriggerDesc is OnTriggerMergeDesc) {
                OnTriggerMergeDesc merge = (OnTriggerMergeDesc) onTriggerDesc;
                foreach (OnTriggerMergeMatched matched in merge.Items) {
                    if (matched.OptionalMatchCond != null) {
                        matched.OptionalMatchCond.Accept(visitor);
                    }
                    foreach (OnTriggerMergeAction action in matched.Actions)
                    {
                        if (action.OptionalWhereClause != null) {
                            action.OptionalWhereClause.Accept(visitor);
                        }
    
                        if (action is OnTriggerMergeActionUpdate) {
                            OnTriggerMergeActionUpdate update = (OnTriggerMergeActionUpdate) action;
                            foreach (OnTriggerSetAssignment assignment in update.Assignments)
                            {
                                assignment.Expression.Accept(visitor);
                            }
                        }
                        if (action is OnTriggerMergeActionInsert) {
                            OnTriggerMergeActionInsert insert = (OnTriggerMergeActionInsert) action;
                            foreach (SelectClauseElementRaw element in insert.SelectClause) {
                                if (element is SelectClauseExprRawSpec) {
                                    SelectClauseExprRawSpec selectExpr = (SelectClauseExprRawSpec) element;
                                    selectExpr.SelectExpression.Accept(visitor);
                                }
                            }
                        }
                    }
                }
            }
        }
    
        public static SelectClauseSpecCompiled CompileSelectClause(SelectClauseSpecRaw spec) {
            IList<SelectClauseElementCompiled> selectElements = new List<SelectClauseElementCompiled>();
            foreach (SelectClauseElementRaw raw in spec.SelectExprList)
            {
                if (raw is SelectClauseExprRawSpec)
                {
                    SelectClauseExprRawSpec rawExpr = (SelectClauseExprRawSpec) raw;
                    selectElements.Add(new SelectClauseExprCompiledSpec(rawExpr.SelectExpression, rawExpr.OptionalAsName, rawExpr.OptionalAsName, rawExpr.IsEvents));
                }
                else if (raw is SelectClauseStreamRawSpec)
                {
                    SelectClauseStreamRawSpec rawExpr = (SelectClauseStreamRawSpec) raw;
                    selectElements.Add(new SelectClauseStreamCompiledSpec(rawExpr.StreamName, rawExpr.OptionalAsName));
                }
                else if (raw is SelectClauseElementWildcard)
                {
                    SelectClauseElementWildcard wildcard = (SelectClauseElementWildcard) raw;
                    selectElements.Add(wildcard);
                }
                else
                {
                    throw new IllegalStateException("Unexpected select clause element class : " + raw.GetType().FullName);
                }
            }
            return new SelectClauseSpecCompiled(selectElements.ToArray(), spec.IsDistinct);
        }

        public static bool IsWritesToTables(StatementSpecRaw statementSpec, TableService tableService)
        {
            // determine if writing to a table:

            // insert-into (single)
            if (statementSpec.InsertIntoDesc != null) {
                if (IsTable(statementSpec.InsertIntoDesc.EventTypeName, tableService)) {
                    return true;
                }
            }

            // into-table
            if (statementSpec.IntoTableSpec != null) {
                return true;
            }

            // triggers
            if (statementSpec.OnTriggerDesc != null) {
                OnTriggerDesc onTriggerDesc = statementSpec.OnTriggerDesc;

                // split-stream insert-into
                if (onTriggerDesc.OnTriggerType == OnTriggerType.ON_SPLITSTREAM) {
                    OnTriggerSplitStreamDesc split = (OnTriggerSplitStreamDesc) onTriggerDesc;
                    foreach (OnTriggerSplitStream stream in split.SplitStreams) {
                        if (stream.InsertInto != null && IsTable(stream.InsertInto.EventTypeName, tableService)) {
                            return true;
                        }
                    }
                }

                // on-delete/update/merge/on-selectdelete
                if (onTriggerDesc is OnTriggerWindowDesc) {
                    OnTriggerWindowDesc window = (OnTriggerWindowDesc) onTriggerDesc;
                    if (onTriggerDesc.OnTriggerType == OnTriggerType.ON_DELETE ||
                        onTriggerDesc.OnTriggerType == OnTriggerType.ON_UPDATE ||
                        onTriggerDesc.OnTriggerType == OnTriggerType.ON_MERGE ||
                        window.IsDeleteAndSelect) {
                        if (IsTable(window.WindowName, tableService)) {
                            return true;
                        }
                    }
                }

                // on-merge with insert-action
                if (onTriggerDesc is OnTriggerMergeDesc) {
                    OnTriggerMergeDesc merge = (OnTriggerMergeDesc) onTriggerDesc;
                    foreach (OnTriggerMergeMatched item in merge.Items) {
                        foreach (OnTriggerMergeAction action in item.Actions) {
                            if (action is OnTriggerMergeActionInsert) {
                                OnTriggerMergeActionInsert insert = (OnTriggerMergeActionInsert) action;
                                if (insert.OptionalStreamName != null && IsTable(insert.OptionalStreamName, tableService)) {
                                    return true;
                                }
                            }
                        }
                    }
                }
            } // end of trigger handling

            // fire-and-forget insert/update/delete
            if (statementSpec.FireAndForgetSpec != null) {
                FireAndForgetSpec faf = statementSpec.FireAndForgetSpec;
                if (faf is FireAndForgetSpecDelete ||
                        faf is FireAndForgetSpecInsert ||
                        faf is FireAndForgetSpecUpdate) {
                    if (statementSpec.StreamSpecs.Count == 1) {
                        return IsTable(((FilterStreamSpecRaw) statementSpec.StreamSpecs[0]).RawFilterSpec.EventTypeName, tableService);
                    }
                }
            }

            return false;
        }

        private static bool IsTable(string name, TableService tableService)
        {
            return tableService.GetTableMetadata(name) != null;
        }
    }
}
