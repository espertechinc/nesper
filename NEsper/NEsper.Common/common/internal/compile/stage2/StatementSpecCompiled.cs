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
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.epl.expression.declared.compiletime;
using com.espertech.esper.common.@internal.epl.expression.subquery;
using com.espertech.esper.common.@internal.epl.expression.table;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.compile.stage2
{
    /// <summary>
    ///     Specification object representing a complete EPL statement including all EPL constructs.
    /// </summary>
    public class StatementSpecCompiled
    {
        private readonly IList<ExprDeclaredNode> exprDeclaredNodes;

        public StatementSpecCompiled()
        {
            Raw = new StatementSpecRaw(SelectClauseStreamSelectorEnum.ISTREAM_ONLY);
            StreamSpecs = StreamSpecCompiledConstants.EMPTY_STREAM_ARRAY;
            SelectClauseCompiled = new SelectClauseSpecCompiled(false);
            Annotations = null;
            GroupByExpressions = null;
            SubselectNodes = Collections.GetEmptyList<ExprSubselectNode>();
            exprDeclaredNodes = Collections.GetEmptyList<ExprDeclaredNode>();
            TableAccessNodes = Collections.GetEmptyList<ExprTableAccessNode>();
        }

        public StatementSpecCompiled(
            StatementSpecRaw raw,
            StreamSpecCompiled[] streamSpecs,
            SelectClauseSpecCompiled selectClauseCompiled,
            Attribute[] annotations,
            GroupByClauseExpressions groupByExpressions,
            IList<ExprSubselectNode> subselectNodes,
            IList<ExprDeclaredNode> exprDeclaredNodes,
            IList<ExprTableAccessNode> tableAccessNodes)
        {
            Raw = raw;
            StreamSpecs = streamSpecs;
            SelectClauseCompiled = selectClauseCompiled;
            Annotations = annotations;
            GroupByExpressions = groupByExpressions;
            SubselectNodes = subselectNodes;
            this.exprDeclaredNodes = exprDeclaredNodes;
            TableAccessNodes = tableAccessNodes;
        }

        public StatementSpecCompiled(StatementSpecCompiled spec, StreamSpecCompiled[] streamSpecCompileds)
            : this(
                spec.Raw, streamSpecCompileds, spec.SelectClauseCompiled, spec.Annotations, spec.GroupByExpressions,
                spec.SubselectNodes, spec.exprDeclaredNodes, spec.TableAccessNodes)
        {
        }

        public StatementSpecRaw Raw { get; }

        public StreamSpecCompiled[] StreamSpecs { get; }

        public SelectClauseSpecCompiled SelectClauseCompiled { get; set; }

        public Attribute[] Annotations { get; }

        public GroupByClauseExpressions GroupByExpressions { get; }

        public IList<ExprSubselectNode> SubselectNodes { get; }

        public IList<ExprTableAccessNode> TableAccessNodes { get; }

        public ExprDeclaredNode[] DeclaredExpressions => exprDeclaredNodes.ToArray();
    }
} // end of namespace