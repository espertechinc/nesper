///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.core;

namespace com.espertech.esper.common.@internal.filterspec
{
    /// <summary>
    ///     This class represents an arbitrary expression node returning a boolean value as a filter parameter in an
    ///     <seealso cref="FilterSpecActivatable" /> filter specification.
    /// </summary>
    public abstract class FilterSpecParamExprNode : FilterSpecParam
    {
        public FilterSpecParamExprNode(
            ExprFilterSpecLookupable lkupable,
            FilterOperator filterOperator)
            : base(lkupable, filterOperator)
        {
        }

        public bool HasVariable {
            get => IsVariable;
            set => IsVariable = value;
        }

        public bool HasFilterStreamSubquery {
            get => IsFilterStreamSubquery;
            set => IsFilterStreamSubquery = value;
        }

        public bool HasTableAccess {
            get => IsTableAccess;
            set => IsTableAccess = value;
        }

        public ExprEvaluator ExprNode { get; set; }

        public int FilterBoolExprId { get; set; }

        public EventBeanTypedEventFactory EventBeanTypedEventFactory { get; set; }

        public FilterBooleanExpressionFactory FilterBooleanExpressionFactory { get; set; }

        public bool IsVariable { get; set; }

        public bool UseLargeThreadingProfile { get; set; }

        public bool IsFilterStreamSubquery { get; set; }

        public bool IsTableAccess { get; set; }

        public string ExprText { get; set; }

        public EventType[] EventTypesProvidedBy { get; set; }

        public int StatementIdBooleanExpr { get; set; }
    }

    public class ProxyFilterSpecParamExprNode : FilterSpecParamExprNode
    {
        public delegate FilterValueSetParam GetFilterValueFunc(
            MatchedEventMap matchedEvents,
            ExprEvaluatorContext exprEvaluatorContext,
            StatementContextFilterEvalEnv filterEvalEnv);

        public ProxyFilterSpecParamExprNode(
            ExprFilterSpecLookupable lkupable,
            FilterOperator filterOperator) : base(lkupable, filterOperator)
        {
        }

        public ProxyFilterSpecParamExprNode(
            GetFilterValueFunc getFilterValue,
            ExprFilterSpecLookupable lkupable,
            FilterOperator filterOperator) : base(lkupable, filterOperator)
        {
            ProcGetFilterValue = getFilterValue;
        }

        public GetFilterValueFunc ProcGetFilterValue { get; set; }

        public override FilterValueSetParam GetFilterValue(
            MatchedEventMap matchedEvents,
            ExprEvaluatorContext exprEvaluatorContext,
            StatementContextFilterEvalEnv filterEvalEnv)
        {
            return ProcGetFilterValue?.Invoke(matchedEvents, exprEvaluatorContext, filterEvalEnv);
        }
    }
} // end of namespace