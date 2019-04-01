///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.filterspec
{
	/// <summary>
	/// This class represents an arbitrary expression node returning a boolean value as a filter parameter in an <seealso cref="FilterSpecActivatable" /> filter specification.
	/// </summary>
	public abstract class FilterSpecParamExprNode : FilterSpecParam {
	    private ExprEvaluator exprNode;
	    private string exprText;
	    private EventBeanTypedEventFactory eventBeanTypedEventFactory;
	    protected FilterBooleanExpressionFactory filterBooleanExpressionFactory; // subclasses by generated code
	    private bool hasVariable;
	    private bool useLargeThreadingProfile;
	    private bool hasFilterStreamSubquery;
	    private bool hasTableAccess;
	    private int statementIdBooleanExpr;
	    private int filterBoolExprId;
	    private EventType[] eventTypesProvidedBy;

	    public FilterSpecParamExprNode(ExprFilterSpecLookupable lookupable, FilterOperator filterOperator) : base(lookupable, filterOperator)
	        {
	    }

	    public bool HasVariable {
	        set { this.hasVariable = value; }
	    }

	    public bool HasFilterStreamSubquery {
	        set { this.hasFilterStreamSubquery = value; }
	    }

	    public bool HasTableAccess {
	        set { this.hasTableAccess = value; }
	    }

	    public ExprEvaluator ExprNode {
	        get => exprNode;
	        set { this.exprNode = value; }
	    }

	    public int FilterBoolExprId
	    {
	        get => filterBoolExprId;
	    }

	    public void SetFilterBoolExprId(int filterBoolExprId) {
	        this.filterBoolExprId = filterBoolExprId;
	    }

	    public EventBeanTypedEventFactory EventBeanTypedEventFactory {
	        get => eventBeanTypedEventFactory;
	        set { this.eventBeanTypedEventFactory = value; }
	    }

	    public FilterBooleanExpressionFactory FilterBooleanExpressionFactory {
	        get => filterBooleanExpressionFactory;
	        set { this.filterBooleanExpressionFactory = value; }
	    }

	    public bool IsVariable
	    {
	        get => hasVariable;
	    }

	    public bool IsUseLargeThreadingProfile
	    {
	        get => useLargeThreadingProfile;
	    }

	    public bool IsFilterStreamSubquery
	    {
	        get => hasFilterStreamSubquery;
	    }

	    public bool IsTableAccess
	    {
	        get => hasTableAccess;
	    }

	    public string ExprText {
	        get => exprText;
	        set { this.exprText = value; }
	    }

	    public EventType[] EventTypesProvidedBy {
	        get => eventTypesProvidedBy;
	        set { this.eventTypesProvidedBy = value; }
	    }

	    public int StatementIdBooleanExpr {
	        get => statementIdBooleanExpr;
	        set { this.statementIdBooleanExpr = value; }
	    }

	    public bool UseLargeThreadingProfile {
	        set { this.useLargeThreadingProfile = value; }
	    }
	}
} // end of namespace