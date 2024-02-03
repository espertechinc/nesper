///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.agg.rollup;
using com.espertech.esper.common.@internal.epl.expression.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.expression.codegen.ExprForgeCodegenNames;
using static com.espertech.esper.common.@internal.epl.resultset.codegen.ResultSetProcessorCodegenNames;

namespace com.espertech.esper.common.@internal.epl.resultset.order
{
    public class OrderByProcessorCodegenNames
    {
        public const string CLASSNAME_ORDERBYPROCESSORFACTORY = "OrderProcFactory";
        public const string CLASSNAME_ORDERBYPROCESSOR = "OrderProc";

        public static readonly CodegenExpressionRef REF_OUTGOINGEVENTS = Ref("orderOutgoingEvents");
        public static readonly CodegenExpressionRef REF_GENERATINGEVENTS = Ref("orderGeneratingEvents");
        public static readonly CodegenExpressionRef REF_ORDERGROUPBYKEYS = Ref("orderGroupByKeys");
        public static readonly CodegenExpressionRef REF_ORDERCURRENTGENERATORS = Ref("orderCurrentGenerators");
        public static readonly CodegenExpressionRef REF_ORDERROLLUPLEVEL = Ref("orderlevel");
        public static readonly CodegenExpressionRef REF_ORDERKEYS = Ref("orderKeys");

        public static readonly CodegenExpressionRef REF_ORDERFIRSTEVENT = Ref("first");
        public static readonly CodegenExpressionRef REF_ORDERFIRSTSORTKEY = Ref("firstSortKey");
        public static readonly CodegenExpressionRef REF_ORDERSECONDEVENT = Ref("second");
        public static readonly CodegenExpressionRef REF_ORDERSECONDSORTKEY = Ref("secondSortKey");

        public static readonly IList<CodegenNamedParam> SORTPLAIN_PARAMS = CodegenNamedParam.From(
            typeof(EventBean[]),
            REF_OUTGOINGEVENTS.Ref,
            typeof(EventBean[][]),
            REF_GENERATINGEVENTS.Ref,
            typeof(bool),
            REF_ISNEWDATA.Ref,
            typeof(ExprEvaluatorContext),
            REF_EXPREVALCONTEXT.Ref,
            typeof(AggregationService),
            MEMBER_AGGREGATIONSVC.Ref);

        public static readonly IList<CodegenNamedParam> SORTWGROUPKEYS_PARAMS = CodegenNamedParam.From(
            typeof(EventBean[]),
            REF_OUTGOINGEVENTS.Ref,
            typeof(EventBean[][]),
            REF_GENERATINGEVENTS.Ref,
            typeof(object[]),
            REF_ORDERGROUPBYKEYS.Ref,
            typeof(bool),
            REF_ISNEWDATA.Ref,
            typeof(ExprEvaluatorContext),
            REF_EXPREVALCONTEXT.Ref,
            typeof(AggregationService),
            MEMBER_AGGREGATIONSVC.Ref);

        public static readonly IList<CodegenNamedParam> SORTROLLUP_PARAMS = CodegenNamedParam.From(
            typeof(EventBean[]),
            REF_OUTGOINGEVENTS.Ref,
            typeof(IList<GroupByRollupKey>),
            REF_ORDERCURRENTGENERATORS.Ref,
            typeof(bool),
            REF_ISNEWDATA.Ref,
            typeof(ExprEvaluatorContext),
            MEMBER_EXPREVALCONTEXT.Ref,
            typeof(AggregationService),
            MEMBER_AGGREGATIONSVC.Ref);

        public static readonly IList<CodegenNamedParam> SORTTWOKEYS_PARAMS = CodegenNamedParam.From(
            typeof(EventBean),
            REF_ORDERFIRSTEVENT.Ref,
            typeof(object),
            REF_ORDERFIRSTSORTKEY.Ref,
            typeof(EventBean),
            REF_ORDERSECONDEVENT.Ref,
            typeof(object),
            REF_ORDERSECONDSORTKEY.Ref);
    }
} // end of namespace