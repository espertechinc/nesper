///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.epl.expression.subquery;
using com.espertech.esper.common.@internal.epl.resultset.select.core;
using com.espertech.esper.common.@internal.epl.table.compiletime;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.compile.stage2
{
    public class StatementLifecycleSvcUtil
    {
        public static bool DetermineHasTableAccess(
            IList<ExprSubselectNode> subselectNodes,
            StatementSpecRaw statementSpecRaw,
            TableCompileTimeResolver tableCompileTimeResolver)
        {
            var hasTableAccess =
                statementSpecRaw.TableExpressions != null && !statementSpecRaw.TableExpressions.IsEmpty() ||
                statementSpecRaw.IntoTableSpec != null;
            hasTableAccess = hasTableAccess ||
                             IsJoinWithTable(statementSpecRaw, tableCompileTimeResolver) ||
                             IsSubqueryWithTable(subselectNodes, tableCompileTimeResolver) ||
                             IsInsertIntoTable(statementSpecRaw, tableCompileTimeResolver);
            return hasTableAccess;
        }

        private static bool IsInsertIntoTable(
            StatementSpecRaw statementSpecRaw,
            TableCompileTimeResolver tableCompileTimeResolver)
        {
            if (statementSpecRaw.InsertIntoDesc == null) {
                return false;
            }

            return tableCompileTimeResolver.Resolve(statementSpecRaw.InsertIntoDesc.EventTypeName) != null;
        }

        public static bool IsSubqueryWithTable(
            IList<ExprSubselectNode> subselectNodes,
            TableCompileTimeResolver tableCompileTimeResolver)
        {
            foreach (var node in subselectNodes) {
                var spec = (FilterStreamSpecRaw) node.StatementSpecRaw.StreamSpecs[0];
                if (tableCompileTimeResolver.Resolve(spec.RawFilterSpec.EventTypeName) != null) {
                    return true;
                }
            }

            return false;
        }

        private static bool IsJoinWithTable(
            StatementSpecRaw statementSpecRaw,
            TableCompileTimeResolver tableCompileTimeResolver)
        {
            foreach (var stream in statementSpecRaw.StreamSpecs) {
                if (stream is FilterStreamSpecRaw) {
                    var filter = (FilterStreamSpecRaw) stream;
                    if (tableCompileTimeResolver.Resolve(filter.RawFilterSpec.EventTypeName) != null) {
                        return true;
                    }
                }
            }

            return false;
        }

        public static SelectClauseSpecCompiled CompileSelectClause(SelectClauseSpecRaw spec)
        {
            IList<SelectClauseElementCompiled> selectElements = new List<SelectClauseElementCompiled>();
            foreach (var raw in spec.SelectExprList) {
                if (raw is SelectClauseExprRawSpec) {
                    var rawExpr = (SelectClauseExprRawSpec) raw;
                    selectElements.Add(
                        new SelectClauseExprCompiledSpec(
                            rawExpr.SelectExpression,
                            rawExpr.OptionalAsName,
                            rawExpr.OptionalAsName,
                            rawExpr.IsEvents));
                }
                else if (raw is SelectClauseStreamRawSpec) {
                    var rawExpr = (SelectClauseStreamRawSpec) raw;
                    selectElements.Add(new SelectClauseStreamCompiledSpec(rawExpr.StreamName, rawExpr.OptionalAsName));
                }
                else if (raw is SelectClauseElementWildcard) {
                    var wildcard = (SelectClauseElementWildcard) raw;
                    selectElements.Add(wildcard);
                }
                else {
                    throw new IllegalStateException("Unexpected select clause element class : " + raw.GetType().Name);
                }
            }

            return new SelectClauseSpecCompiled(selectElements.ToArray(), spec.IsDistinct);
        }

        public static bool IsWritesToTables(
            StatementSpecRaw statementSpec,
            TableCompileTimeResolver tableCompileTimeResolver)
        {
            // determine if writing to a table:

            // insert-into (single)
            if (statementSpec.InsertIntoDesc != null) {
                if (IsTable(statementSpec.InsertIntoDesc.EventTypeName, tableCompileTimeResolver)) {
                    return true;
                }
            }

            // into-table
            if (statementSpec.IntoTableSpec != null) {
                return true;
            }

            // triggers
            if (statementSpec.OnTriggerDesc != null) {
                var onTriggerDesc = statementSpec.OnTriggerDesc;

                // split-stream insert-into
                if (onTriggerDesc.OnTriggerType == OnTriggerType.ON_SPLITSTREAM) {
                    var split = (OnTriggerSplitStreamDesc) onTriggerDesc;
                    foreach (var stream in split.SplitStreams) {
                        if (stream.InsertInto != null &&
                            IsTable(stream.InsertInto.EventTypeName, tableCompileTimeResolver)) {
                            return true;
                        }
                    }
                }

                // on-delete/update/merge/on-selectdelete
                if (onTriggerDesc is OnTriggerWindowDesc) {
                    var window = (OnTriggerWindowDesc) onTriggerDesc;
                    if (onTriggerDesc.OnTriggerType == OnTriggerType.ON_DELETE ||
                        onTriggerDesc.OnTriggerType == OnTriggerType.ON_UPDATE ||
                        onTriggerDesc.OnTriggerType == OnTriggerType.ON_MERGE ||
                        window.IsDeleteAndSelect) {
                        if (IsTable(window.WindowName, tableCompileTimeResolver)) {
                            return true;
                        }
                    }
                }

                // on-merge with insert-action
                if (onTriggerDesc is OnTriggerMergeDesc) {
                    var merge = (OnTriggerMergeDesc) onTriggerDesc;
                    foreach (var item in merge.Items) {
                        foreach (var action in item.Actions) {
                            if (CheckOnTriggerMergeAction(action, tableCompileTimeResolver)) {
                                return true;
                            }
                        }
                    }

                    if (merge.OptionalInsertNoMatch != null &&
                        CheckOnTriggerMergeAction(merge.OptionalInsertNoMatch, tableCompileTimeResolver)) {
                        return true;
                    }
                }
            } // end of trigger handling

            // fire-and-forget insert/update/delete
            if (statementSpec.FireAndForgetSpec != null) {
                var faf = statementSpec.FireAndForgetSpec;
                if (faf is FireAndForgetSpecDelete ||
                    faf is FireAndForgetSpecInsert ||
                    faf is FireAndForgetSpecUpdate) {
                    if (statementSpec.StreamSpecs.Count == 1) {
                        return IsTable(
                            ((FilterStreamSpecRaw) statementSpec.StreamSpecs[0]).RawFilterSpec.EventTypeName,
                            tableCompileTimeResolver);
                    }
                }
            }

            return false;
        }

        private static bool CheckOnTriggerMergeAction(
            OnTriggerMergeAction action,
            TableCompileTimeResolver tableCompileTimeResolver)
        {
            var insert = action as OnTriggerMergeActionInsert;
            if (insert?.OptionalStreamName != null && IsTable(insert.OptionalStreamName, tableCompileTimeResolver)) {
                return true;
            }

            return false;
        }

        private static bool IsTable(
            string name,
            TableCompileTimeResolver tableCompileTimeResolver)
        {
            return tableCompileTimeResolver.Resolve(name) != null;
        }
    }
} // end of namespace