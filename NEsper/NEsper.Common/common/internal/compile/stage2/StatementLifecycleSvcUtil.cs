///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.epl.expression.subquery;
using com.espertech.esper.common.@internal.epl.resultset.select.core;
using com.espertech.esper.common.@internal.epl.table.compiletime;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.compile.stage2
{
	public class StatementLifecycleSvcUtil {

	    public static bool DetermineHasTableAccess(IList<ExprSubselectNode> subselectNodes, StatementSpecRaw statementSpecRaw, TableCompileTimeResolver tableCompileTimeResolver) {
	        bool hasTableAccess = (statementSpecRaw.TableExpressions != null && !statementSpecRaw.TableExpressions.IsEmpty()) ||
	                statementSpecRaw.IntoTableSpec != null;
	        hasTableAccess = hasTableAccess || IsJoinWithTable(statementSpecRaw, tableCompileTimeResolver) || IsSubqueryWithTable(subselectNodes, tableCompileTimeResolver) || IsInsertIntoTable(statementSpecRaw, tableCompileTimeResolver);
	        return hasTableAccess;
	    }

	    private static bool IsInsertIntoTable(StatementSpecRaw statementSpecRaw, TableCompileTimeResolver tableCompileTimeResolver) {
	        if (statementSpecRaw.InsertIntoDesc == null) {
	            return false;
	        }
	        return tableCompileTimeResolver.Resolve(statementSpecRaw.InsertIntoDesc.EventTypeName) != null;
	    }

	    private static bool IsSubqueryWithTable(IList<ExprSubselectNode> subselectNodes, TableCompileTimeResolver tableCompileTimeResolver) {
	        foreach (ExprSubselectNode node in subselectNodes) {
	            FilterStreamSpecRaw spec = (FilterStreamSpecRaw) node.StatementSpecRaw.StreamSpecs.Get(0);
	            if (tableCompileTimeResolver.Resolve(spec.RawFilterSpec.EventTypeName) != null) {
	                return true;
	            }
	        }
	        return false;
	    }

	    private static bool IsJoinWithTable(StatementSpecRaw statementSpecRaw, TableCompileTimeResolver tableCompileTimeResolver) {
	        foreach (StreamSpecRaw stream in statementSpecRaw.StreamSpecs) {
	            if (stream is FilterStreamSpecRaw) {
	                FilterStreamSpecRaw filter = (FilterStreamSpecRaw) stream;
	                if (tableCompileTimeResolver.Resolve(filter.RawFilterSpec.EventTypeName) != null) {
	                    return true;
	                }
	            }
	        }
	        return false;
	    }

	    public static SelectClauseSpecCompiled CompileSelectClause(SelectClauseSpecRaw spec) {
	        IList<SelectClauseElementCompiled> selectElements = new List<SelectClauseElementCompiled>();
	        foreach (SelectClauseElementRaw raw in spec.SelectExprList) {
	            if (raw is SelectClauseExprRawSpec) {
	                SelectClauseExprRawSpec rawExpr = (SelectClauseExprRawSpec) raw;
	                selectElements.Add(new SelectClauseExprCompiledSpec(rawExpr.SelectExpression, rawExpr.OptionalAsName, rawExpr.OptionalAsName, rawExpr.IsEvents));
	            } else if (raw is SelectClauseStreamRawSpec) {
	                SelectClauseStreamRawSpec rawExpr = (SelectClauseStreamRawSpec) raw;
	                selectElements.Add(new SelectClauseStreamCompiledSpec(rawExpr.StreamName, rawExpr.OptionalAsName));
	            } else if (raw is SelectClauseElementWildcard) {
	                SelectClauseElementWildcard wildcard = (SelectClauseElementWildcard) raw;
	                selectElements.Add(wildcard);
	            } else {
	                throw new IllegalStateException("Unexpected select clause element class : " + raw.GetType().Name);
	            }
	        }
	        return new SelectClauseSpecCompiled(selectElements.ToArray(), spec.IsDistinct);
	    }

	    public static bool IsWritesToTables(StatementSpecRaw statementSpec, TableCompileTimeResolver tableCompileTimeResolver) {
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
	            OnTriggerDesc onTriggerDesc = statementSpec.OnTriggerDesc;

	            // split-stream insert-into
	            if (onTriggerDesc.OnTriggerType == OnTriggerType.ON_SPLITSTREAM) {
	                OnTriggerSplitStreamDesc split = (OnTriggerSplitStreamDesc) onTriggerDesc;
	                foreach (OnTriggerSplitStream stream in split.SplitStreams) {
	                    if (stream.InsertInto != null && IsTable(stream.InsertInto.EventTypeName, tableCompileTimeResolver)) {
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
	                    if (IsTable(window.WindowName, tableCompileTimeResolver)) {
	                        return true;
	                    }
	                }
	            }

	            // on-merge with insert-action
	            if (onTriggerDesc is OnTriggerMergeDesc) {
	                OnTriggerMergeDesc merge = (OnTriggerMergeDesc) onTriggerDesc;
	                foreach (OnTriggerMergeMatched item in merge.Items) {
	                    foreach (OnTriggerMergeAction action in item.Actions) {
	                        if (CheckOnTriggerMergeAction(action, tableCompileTimeResolver)) {
	                            return true;
	                        }
	                    }
	                }
	                if (merge.OptionalInsertNoMatch != null && CheckOnTriggerMergeAction(merge.OptionalInsertNoMatch, tableCompileTimeResolver)) {
	                    return true;
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
	                    return IsTable(((FilterStreamSpecRaw) statementSpec.StreamSpecs.Get(0)).RawFilterSpec.EventTypeName, tableCompileTimeResolver);
	                }
	            }
	        }

	        return false;
	    }

	    private static bool CheckOnTriggerMergeAction(OnTriggerMergeAction action, TableCompileTimeResolver tableCompileTimeResolver) {
	        if (action is OnTriggerMergeActionInsert) {
	            OnTriggerMergeActionInsert insert = (OnTriggerMergeActionInsert) action;
	            if (insert.OptionalStreamName != null && IsTable(insert.OptionalStreamName, tableCompileTimeResolver)) {
	                return true;
	            }
	        }
	        return false;
	    }

	    private static bool IsTable(string name, TableCompileTimeResolver tableCompileTimeResolver) {
	        return tableCompileTimeResolver.Resolve(name) != null;
	    }
	}
} // end of namespace