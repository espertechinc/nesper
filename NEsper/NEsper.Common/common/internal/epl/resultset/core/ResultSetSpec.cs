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
	public class ResultSetSpec {
	    private readonly SelectClauseStreamSelectorEnum selectClauseStreamSelector;
	    private readonly IList<OrderByItem> orderByList;
	    private readonly SelectClauseSpecCompiled selectClauseSpec;
	    private readonly InsertIntoDesc insertIntoDesc;
	    private readonly GroupByClauseExpressions groupByClauseExpressions;
	    private readonly ExprNode whereClause;
	    private readonly ExprNode havingClause;
	    private readonly OutputLimitSpec optionalOutputLimitSpec;
	    private readonly RowLimitSpec rowLimitSpec;
	    private readonly string contextName;
	    private readonly ForClauseSpec forClauseSpec;
	    private readonly IntoTableSpec intoTableSpec;
	    private readonly StreamSpecCompiled[] streamSpecs;
	    private readonly Attribute[] annotations;

	    public ResultSetSpec(SelectClauseStreamSelectorEnum selectClauseStreamSelector, IList<OrderByItem> orderByList, SelectClauseSpecCompiled selectClauseSpec, InsertIntoDesc insertIntoDesc, GroupByClauseExpressions groupByClauseExpressions, ExprNode whereClause, ExprNode havingClause, OutputLimitSpec optionalOutputLimitSpec, RowLimitSpec rowLimitSpec, string contextName, ForClauseSpec forClauseSpec, IntoTableSpec intoTableSpec, StreamSpecCompiled[] streamSpecs, Attribute[] annotations) {
	        this.selectClauseStreamSelector = selectClauseStreamSelector;
	        this.orderByList = orderByList;
	        this.selectClauseSpec = selectClauseSpec;
	        this.insertIntoDesc = insertIntoDesc;
	        this.groupByClauseExpressions = groupByClauseExpressions;
	        this.whereClause = whereClause;
	        this.havingClause = havingClause;
	        this.optionalOutputLimitSpec = optionalOutputLimitSpec;
	        this.rowLimitSpec = rowLimitSpec;
	        this.contextName = contextName;
	        this.forClauseSpec = forClauseSpec;
	        this.intoTableSpec = intoTableSpec;
	        this.streamSpecs = streamSpecs;
	        this.annotations = annotations;
	    }

	    public ResultSetSpec(StatementSpecCompiled statementSpec) {
	        This(statementSpec.Raw.SelectStreamSelectorEnum, statementSpec.Raw.OrderByList, statementSpec.SelectClauseCompiled,
	                statementSpec.Raw.InsertIntoDesc, statementSpec.GroupByExpressions, statementSpec.Raw.WhereClause, statementSpec.Raw.HavingClause, statementSpec.Raw.OutputLimitSpec,
	                statementSpec.Raw.RowLimitSpec, statementSpec.Raw.OptionalContextName, statementSpec.Raw.ForClauseSpec, statementSpec.Raw.IntoTableSpec,
	                statementSpec.StreamSpecs, statementSpec.Annotations);
	    }

	    public IList<OrderByItem> GetOrderByList() {
	        return orderByList;
	    }

	    public SelectClauseSpecCompiled SelectClauseSpec {
	        get => selectClauseSpec;
	    }

	    public InsertIntoDesc InsertIntoDesc {
	        get => insertIntoDesc;
	    }

	    public ExprNode HavingClause {
	        get => havingClause;
	    }

	    public OutputLimitSpec OptionalOutputLimitSpec {
	        get => optionalOutputLimitSpec;
	    }

	    public SelectClauseStreamSelectorEnum SelectClauseStreamSelector {
	        get => selectClauseStreamSelector;
	    }

	    public GroupByClauseExpressions GroupByClauseExpressions {
	        get => groupByClauseExpressions;
	    }

	    public RowLimitSpec RowLimitSpec {
	        get => rowLimitSpec;
	    }

	    public string ContextName {
	        get => contextName;
	    }

	    public ExprNode WhereClause {
	        get => whereClause;
	    }

	    public ForClauseSpec ForClauseSpec {
	        get => forClauseSpec;
	    }

	    public IntoTableSpec IntoTableSpec {
	        get => intoTableSpec;
	    }

	    public StreamSpecCompiled[] GetStreamSpecs() {
	        return streamSpecs;
	    }

	    public Attribute[] GetAnnotations() {
	        return annotations;
	    }
	}
} // end of namespace