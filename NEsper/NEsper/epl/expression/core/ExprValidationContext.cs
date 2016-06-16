///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client.annotation;
using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.epl.variable;
using com.espertech.esper.events;
using com.espertech.esper.schedule;
using com.espertech.esper.script;

namespace com.espertech.esper.epl.expression.core
{
	public class ExprValidationContext
	{
	    public ExprValidationContext(
	        StreamTypeService streamTypeService,
	        MethodResolutionService methodResolutionService,
	        ViewResourceDelegateUnverified viewResourceDelegate,
	        TimeProvider timeProvider,
	        VariableService variableService,
	        TableService tableService,
	        ExprEvaluatorContext exprEvaluatorContext,
	        EventAdapterService eventAdapterService,
	        string statementName,
            int statementId,
	        Attribute[] annotations,
	        ContextDescriptor contextDescriptor,
            ScriptingService scriptingService,
	        bool disablePropertyExpressionEventCollCache,
	        bool allowRollupFunctions,
	        bool allowBindingConsumption,
	        bool isUnidirectionalJoin,
	        string intoTableName,
            bool isFilterExpression)
        {
	        StreamTypeService = streamTypeService;
	        MethodResolutionService = methodResolutionService;
	        ViewResourceDelegate = viewResourceDelegate;
	        TimeProvider = timeProvider;
	        VariableService = variableService;
	        TableService = tableService;
	        ExprEvaluatorContext = exprEvaluatorContext;
	        EventAdapterService = eventAdapterService;
	        StatementName = statementName;
	        StatementId = statementId;
	        Annotations = annotations;
	        ContextDescriptor = contextDescriptor;
	        ScriptingService = scriptingService;
	        IsDisablePropertyExpressionEventCollCache = disablePropertyExpressionEventCollCache;
	        IsAllowRollupFunctions = allowRollupFunctions;
	        IsAllowBindingConsumption = allowBindingConsumption;
	        IsResettingAggregations = isUnidirectionalJoin;
	        IntoTableName = intoTableName;
	        IsFilterExpression = isFilterExpression;

	        IsExpressionAudit = AuditEnum.EXPRESSION.GetAudit(annotations) != null;
	        IsExpressionNestedAudit = AuditEnum.EXPRESSION_NESTED.GetAudit(annotations) != null;
	    }

	    public ExprValidationContext(StreamTypeServiceImpl types, ExprValidationContext ctx)
	        : this(
                types,
	            ctx.MethodResolutionService,
	            ctx.ViewResourceDelegate,
	            ctx.TimeProvider,
	            ctx.VariableService,
	            ctx.TableService,
	            ctx.ExprEvaluatorContext,
	            ctx.EventAdapterService,
	            ctx.StatementName,
	            ctx.StatementId,
	            ctx.Annotations,
	            ctx.ContextDescriptor,
	            ctx.ScriptingService,
	            ctx.IsDisablePropertyExpressionEventCollCache, false,
	            ctx.IsAllowBindingConsumption,
	            ctx.IsResettingAggregations,
	            ctx.IntoTableName,
	            false)
        {
	    }

	    public StreamTypeService StreamTypeService { get; private set; }

	    public MethodResolutionService MethodResolutionService { get; private set; }

	    public ViewResourceDelegateUnverified ViewResourceDelegate { get; private set; }

	    public TimeProvider TimeProvider { get; private set; }

	    public VariableService VariableService { get; private set; }

	    public ExprEvaluatorContext ExprEvaluatorContext { get; private set; }

	    public EventAdapterService EventAdapterService { get; private set; }

	    public string StatementName { get; private set; }

	    public Attribute[] Annotations { get; private set; }

	    public bool IsExpressionNestedAudit { get; private set; }

	    public bool IsExpressionAudit { get; private set; }

	    public int StatementId { get; private set; }

	    public ContextDescriptor ContextDescriptor { get; private set; }

	    public bool IsDisablePropertyExpressionEventCollCache { get; private set; }

	    public bool IsAllowRollupFunctions { get; private set; }

	    public TableService TableService { get; private set; }

	    public bool IsAllowBindingConsumption { get; private set; }

	    public bool IsResettingAggregations { get; private set; }

	    public string IntoTableName { get; private set; }

        public ScriptingService ScriptingService { get; private set; }

        public bool IsFilterExpression { get; private set; }
	}
} // end of namespace
