///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
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
        protected FilterBooleanExpressionFactory filterBooleanExpressionFactory; // subclasses by generated code

        public FilterSpecParamExprNode(
            ExprFilterSpecLookupable lkupable,
            FilterOperator filterOperator)
            : base(lkupable, filterOperator)
        {
        }

        public bool HasVariable {
            set => IsVariable = value;
        }

        public bool HasFilterStreamSubquery {
            set => IsFilterStreamSubquery = value;
        }

        public bool HasTableAccess {
            set => IsTableAccess = value;
        }

        public ExprEvaluator ExprNode { get; set; }

        public int FilterBoolExprId { get; set; }

        public EventBeanTypedEventFactory EventBeanTypedEventFactory { get; set; }

        public FilterBooleanExpressionFactory FilterBooleanExpressionFactory {
            get => filterBooleanExpressionFactory;
            set => filterBooleanExpressionFactory = value;
        }

        public bool IsVariable { get; private set; }

        public bool IsUseLargeThreadingProfile { get; private set; }

        public bool IsFilterStreamSubquery { get; private set; }

        public bool IsTableAccess { get; private set; }

        public string ExprText { get; set; }

        public EventType[] EventTypesProvidedBy { get; set; }

        public int StatementIdBooleanExpr { get; set; }

        public bool UseLargeThreadingProfile {
            set => IsUseLargeThreadingProfile = value;
        }
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
            return ProcGetFilterValue(matchedEvents, exprEvaluatorContext, filterEvalEnv);
        }
    }
} // end of namespace