///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.annotation;
using com.espertech.esper.common.@internal.compile.stage1;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.stage1.specmapper;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.agg.rollup;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.subquery;
using com.espertech.esper.common.@internal.epl.expression.table;
using com.espertech.esper.common.@internal.epl.expression.visitor;
using com.espertech.esper.common.@internal.epl.resultset.select.core;
using com.espertech.esper.common.@internal.epl.rowrecog.core;
using com.espertech.esper.common.@internal.epl.rowrecog.expr;
using com.espertech.esper.common.@internal.epl.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.common.@internal.compile.stage2
{
    public class StatementRawCompiler
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static StatementSpecCompiledDesc Compile(
            StatementSpecRaw spec,
            Compilable compilable,
            bool isSubquery,
            bool isOnDemandQuery,
            Attribute[] annotations,
            IList<ExprSubselectNode> subselectNodes,
            IList<ExprTableAccessNode> tableAccessNodes,
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices compileTimeServices)
        {
            IList<StreamSpecCompiled> compiledStreams;
            IList<StmtClassForgeableFactory> additionalForgeables = new List<StmtClassForgeableFactory>(2);

            if (!isOnDemandQuery && spec.FireAndForgetSpec != null) {
                throw new StatementSpecCompileException(
                    "Provided EPL expression is an on-demand query expression (not a continuous query)",
                    compilable.ToEPL());
            }

            // If not using a join and not specifying a data window, make the where-clause, if present, the filter of the stream
            // if selecting using filter spec, and not subquery in where clause
            if (spec.StreamSpecs.Count == 1 &&
                spec.StreamSpecs[0] is FilterStreamSpecRaw &&
                spec.StreamSpecs[0].ViewSpecs.Length == 0 &&
                spec.WhereClause != null &&
                spec.OnTriggerDesc == null &&
                !isSubquery &&
                !isOnDemandQuery &&
                (tableAccessNodes == null || tableAccessNodes.IsEmpty())) {
                bool disqualified;
                var whereClause = spec.WhereClause;

                var visitorX = new ExprNodeSubselectDeclaredDotVisitor();
                whereClause.Accept(visitorX);
                disqualified = visitorX.Subselects.Count > 0 ||
                               HintEnum.DISABLE_WHEREEXPR_MOVETO_FILTER.GetHint(annotations) != null;

                if (!disqualified) {
                    var viewResourceVisitor = new ExprNodeViewResourceVisitor();
                    whereClause.Accept(viewResourceVisitor);
                    disqualified = viewResourceVisitor.ExprNodes.Count > 0;
                }

                var streamSpec = (FilterStreamSpecRaw)spec.StreamSpecs[0];
                if (streamSpec.RawFilterSpec.OptionalPropertyEvalSpec != null) {
                    disqualified = true;
                }

                if (!disqualified) {
                    spec.WhereClause = null;
                    streamSpec.RawFilterSpec.FilterExpressions.Add(whereClause);
                }
            }

            // compile select-clause
            var selectClauseCompiled = CompileSelectClause(spec.SelectClauseSpec);

            // Determine subselects in filter streams, these may need special handling for locking
            var visitor = new ExprNodeSubselectDeclaredDotVisitor();
            try {
                StatementSpecRawWalkerSubselectAndDeclaredDot.WalkStreamSpecs(spec, visitor);
            }
            catch (ExprValidationException ex) {
                throw new StatementSpecCompileException(ex.Message, ex, compilable.ToEPL());
            }

            foreach (var subselectNode in visitor.Subselects) {
                subselectNode.IsFilterStreamSubselect = true;
            }

            // Determine subselects for compilation, and lambda-expression shortcut syntax for named windows
            visitor.Reset();
            GroupByClauseExpressions groupByRollupExpressions;
            try {
                StatementSpecRawWalkerSubselectAndDeclaredDot.WalkSubselectAndDeclaredDotExpr(spec, visitor);

                var expressionCopier = new ExpressionCopier(
                    spec,
                    statementRawInfo.OptionalContextDescriptor,
                    compileTimeServices,
                    visitor);
                groupByRollupExpressions = GroupByExpressionHelper.GetGroupByRollupExpressions(
                    spec.GroupByExpressions,
                    spec.SelectClauseSpec,
                    spec.HavingClause,
                    spec.OrderByList,
                    expressionCopier);
            }
            catch (ExprValidationException ex) {
                throw new StatementSpecCompileException(ex.Message, ex, compilable.ToEPL());
            }

            // Expand match-recognize patterns
            if (spec.MatchRecognizeSpec != null) {
                RowRecogExprNode expandedPatternNode;
                try {
                    var copier = new ExpressionCopier(
                        spec,
                        statementRawInfo.OptionalContextDescriptor,
                        compileTimeServices,
                        visitor);
                    expandedPatternNode = RowRecogPatternExpandUtil.Expand(spec.MatchRecognizeSpec.Pattern, copier);
                }
                catch (ExprValidationException ex) {
                    throw new StatementSpecCompileException(ex.Message, ex, compilable.ToEPL());
                }

                spec.MatchRecognizeSpec.Pattern = expandedPatternNode;
            }

            if (isSubquery && !visitor.Subselects.IsEmpty()) {
                throw new StatementSpecCompileException(
                    "Invalid nested subquery, subquery-within-subquery is not supported",
                    compilable.ToEPL());
            }

            foreach (var subselectNode in visitor.Subselects) {
                if (!subselectNodes.Contains(subselectNode)) {
                    subselectNodes.Add(subselectNode);
                }
            }

            // Compile subselects found
            var subselectNumber = 0;
            foreach (var subselect in subselectNodes) {
                var raw = subselect.StatementSpecRaw;
                var desc = Compile(
                    raw,
                    compilable,
                    true,
                    isOnDemandQuery,
                    annotations,
                    Collections.GetEmptyList<ExprSubselectNode>(),
                    Collections.GetEmptyList<ExprTableAccessNode>(),
                    statementRawInfo,
                    compileTimeServices);
                additionalForgeables.AddAll(desc.AdditionalForgeables);
                subselect.SetStatementSpecCompiled(desc.Compiled, subselectNumber);
                subselectNumber++;
            }

            // Set table-access number
            var tableAccessNumber = 0;
            foreach (var tableAccess in tableAccessNodes) {
                tableAccess.TableAccessNumber = tableAccessNumber;
                tableAccessNumber++;
            }

            // compile each stream used
            try {
                compiledStreams = new List<StreamSpecCompiled>(spec.StreamSpecs.Count);
                var streamNum = 0;
                foreach (var rawSpec in spec.StreamSpecs) {
                    streamNum++;
                    var desc = StreamSpecCompiler.Compile(
                        rawSpec,
                        spec.StreamSpecs.Count > 1,
                        false,
                        spec.OnTriggerDesc != null,
                        rawSpec.OptionalStreamName,
                        streamNum,
                        statementRawInfo,
                        compileTimeServices);

                    additionalForgeables.AddAll(desc.AdditionalForgeables);
                    compiledStreams.Add(desc.StreamSpecCompiled);
                }
            }
            catch (ExprValidationException ex) {
                if (ex.Message == null) {
                    throw new StatementSpecCompileException(
                        "Unexpected exception compiling statement, please consult the log file and report the exception",
                        ex,
                        compilable.ToEPL());
                }

                throw new StatementSpecCompileException(ex.Message, ex, compilable.ToEPL());
            }
            catch (EPException ex) {
                throw new StatementSpecCompileException(ex.Message, ex, compilable.ToEPL());
            }
            catch (Exception ex) {
                var text = "Unexpected error compiling statement";
                Log.Error(text, ex);
                throw new StatementSpecCompileException(
                    text + ": " + ex.GetType().Name + ":" + ex.Message,
                    ex,
                    compilable.ToEPL());
            }

            var compiled = new StatementSpecCompiled(
                spec,
                compiledStreams.ToArray(),
                selectClauseCompiled,
                annotations,
                groupByRollupExpressions,
                subselectNodes,
                visitor.DeclaredExpressions,
                tableAccessNodes);

            return new StatementSpecCompiledDesc(compiled, additionalForgeables);
        }

        public static SelectClauseSpecCompiled CompileSelectClause(SelectClauseSpecRaw spec)
        {
            IList<SelectClauseElementCompiled> selectElements = new List<SelectClauseElementCompiled>();
            foreach (var raw in spec.SelectExprList) {
                if (raw is SelectClauseExprRawSpec expr) {
                    selectElements.Add(
                        new SelectClauseExprCompiledSpec(
                            expr.SelectExpression,
                            expr.OptionalAsName,
                            expr.OptionalAsName,
                            expr.IsEvents));
                }
                else if (raw is SelectClauseStreamRawSpec rawExpr) {
                    selectElements.Add(new SelectClauseStreamCompiledSpec(rawExpr.StreamName, rawExpr.OptionalAsName));
                }
                else if (raw is SelectClauseElementWildcard wildcard) {
                    selectElements.Add(wildcard);
                }
                else {
                    throw new IllegalStateException("Unexpected select clause element class : " + raw.GetType().Name);
                }
            }

            return new SelectClauseSpecCompiled(selectElements.ToArray(), spec.IsDistinct);
        }
    }
} // end of namespace