///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.epl.expression.codegen.ExprForgeCodegenNames;

namespace com.espertech.esper.common.@internal.filterspec
{
    /// <summary>
    ///     This class represents one filter parameter in an <seealso cref="FilterSpecActivatable" /> filter specification.
    ///     <para />
    ///     Each filerting parameter has an attribute name and operator type.
    /// </summary>
    public abstract class FilterSpecParam
    {
        public static readonly CodegenExpressionRef REF_MATCHEDEVENTMAP = new CodegenExpressionRef("matchedEvents");
        public static readonly CodegenExpressionRef REF_STMTCTXFILTEREVALENV = new CodegenExpressionRef("stmtCtxFilterEnv");
        public static readonly CodegenExpressionRef REF_LOOKUPABLE = new CodegenExpressionRef("lkupable"); // see name below
        public static readonly CodegenExpressionRef REF_FILTEROPERATOR = new CodegenExpressionRef("filterOperator"); // see name below

        public static readonly IList<CodegenNamedParam> GET_FILTER_VALUE_FP = CodegenNamedParam.From(
            typeof(MatchedEventMap),
            REF_MATCHEDEVENTMAP.Ref,
            typeof(ExprEvaluatorContext),
            REF_EXPREVALCONTEXT.Ref,
            typeof(StatementContextFilterEvalEnv),
            REF_STMTCTXFILTEREVALENV.Ref);

        public static readonly CodegenExpression[] GET_FILTER_VALUE_REFS = {
            REF_MATCHEDEVENTMAP,
            REF_EXPREVALCONTEXT,
            REF_STMTCTXFILTEREVALENV
        };

        public static readonly FilterSpecParam[] EMPTY_PARAM_ARRAY = new FilterSpecParam[0];
        public static readonly FilterValueSetParam[] EMPTY_VALUE_ARRAY = new FilterValueSetParam[0];

        internal readonly ExprFilterSpecLookupable lkupable;

        protected FilterSpecParam(
            ExprFilterSpecLookupable lookupable,
            FilterOperator filterOperator)
        {
            this.lkupable = lookupable;
            FilterOperator = filterOperator;
        }

        public ExprFilterSpecLookupable Lkupable => lkupable;

        public FilterOperator FilterOperator { get; }

        public abstract FilterValueSetParam GetFilterValue(
            MatchedEventMap matchedEvents,
            ExprEvaluatorContext exprEvaluatorContext,
            StatementContextFilterEvalEnv filterEvalEnv);

        public override string ToString()
        {
            return "FilterSpecParam" +
                   " lookupable=" +
                   lkupable +
                   " filterOp=" +
                   FilterOperator;
        }

        public static FilterSpecParam[] ToArray(ICollection<FilterSpecParam> coll)
        {
            if (coll.IsEmpty()) {
                return EMPTY_PARAM_ARRAY;
            }

            return coll.ToArray();
        }
    }

    public class ProxyFilterSpecParam : FilterSpecParam
    {
        public delegate FilterValueSetParam GetFilterValueFunc(
            MatchedEventMap matchedEvents,
            ExprEvaluatorContext exprEvaluatorContext,
            StatementContextFilterEvalEnv filterEvalEnv);

        public ProxyFilterSpecParam(
            ExprFilterSpecLookupable lkupable,
            FilterOperator filterOperator,
            GetFilterValueFunc getFilterValueFunc) : base(lkupable, filterOperator)
        {
            ProcGetFilterValue = getFilterValueFunc;
        }


        public GetFilterValueFunc ProcGetFilterValue;

        public override FilterValueSetParam GetFilterValue(
            MatchedEventMap matchedEvents,
            ExprEvaluatorContext exprEvaluatorContext,
            StatementContextFilterEvalEnv filterEvalEnv)
        {
            return ProcGetFilterValue.Invoke(matchedEvents, exprEvaluatorContext, filterEvalEnv);
        }
    }
} // end of namespace