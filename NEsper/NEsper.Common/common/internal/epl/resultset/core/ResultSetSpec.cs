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
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.resultset.core
{
    public class ResultSetSpec
    {
        public ResultSetSpec(
            SelectClauseStreamSelectorEnum selectClauseStreamSelector,
            IList<OrderByItem> orderByList,
            SelectClauseSpecCompiled selectClauseSpec,
            InsertIntoDesc insertIntoDesc,
            GroupByClauseExpressions groupByClauseExpressions,
            ExprNode whereClause,
            ExprNode havingClause,
            OutputLimitSpec optionalOutputLimitSpec,
            RowLimitSpec rowLimitSpec,
            string contextName,
            ForClauseSpec forClauseSpec,
            IntoTableSpec intoTableSpec,
            StreamSpecCompiled[] streamSpecs,
            Attribute[] annotations)
        {
            this.SelectClauseStreamSelector = selectClauseStreamSelector;
            this.OrderByList = orderByList;
            this.SelectClauseSpec = selectClauseSpec;
            this.InsertIntoDesc = insertIntoDesc;
            this.GroupByClauseExpressions = groupByClauseExpressions;
            this.WhereClause = whereClause;
            this.HavingClause = havingClause;
            this.OptionalOutputLimitSpec = optionalOutputLimitSpec;
            this.RowLimitSpec = rowLimitSpec;
            this.ContextName = contextName;
            this.ForClauseSpec = forClauseSpec;
            this.IntoTableSpec = intoTableSpec;
            this.StreamSpecs = streamSpecs;
            this.Annotations = annotations;
        }

        public ResultSetSpec(StatementSpecCompiled statementSpec)
            : this(
                statementSpec.Raw.SelectStreamSelectorEnum,
                statementSpec.Raw.OrderByList,
                statementSpec.SelectClauseCompiled,
                statementSpec.Raw.InsertIntoDesc,
                statementSpec.GroupByExpressions,
                statementSpec.Raw.WhereClause,
                statementSpec.Raw.HavingClause,
                statementSpec.Raw.OutputLimitSpec,
                statementSpec.Raw.RowLimitSpec,
                statementSpec.Raw.OptionalContextName,
                statementSpec.Raw.ForClauseSpec,
                statementSpec.Raw.IntoTableSpec,
                statementSpec.StreamSpecs,
                statementSpec.Annotations)
        {
        }

        public IList<OrderByItem> OrderByList { get; }

        public SelectClauseSpecCompiled SelectClauseSpec { get; }

        public InsertIntoDesc InsertIntoDesc { get; }

        public ExprNode HavingClause { get; }

        public OutputLimitSpec OptionalOutputLimitSpec { get; }

        public SelectClauseStreamSelectorEnum SelectClauseStreamSelector { get; }

        public GroupByClauseExpressions GroupByClauseExpressions { get; }

        public RowLimitSpec RowLimitSpec { get; }

        public string ContextName { get; }

        public ExprNode WhereClause { get; }

        public ForClauseSpec ForClauseSpec { get; }

        public IntoTableSpec IntoTableSpec { get; }

        public StreamSpecCompiled[] StreamSpecs { get; }

        public Attribute[] Annotations { get; }
    }
} // end of namespace