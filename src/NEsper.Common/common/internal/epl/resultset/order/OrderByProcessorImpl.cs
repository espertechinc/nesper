///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.agg.rollup;
using com.espertech.esper.common.@internal.epl.enummethod.codegen;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.resultset.core;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionRelational.CodegenRelational;
using static com.espertech.esper.common.@internal.epl.expression.codegen.ExprForgeCodegenNames;
using static com.espertech.esper.common.@internal.epl.resultset.codegen.ResultSetProcessorCodegenNames;
using static com.espertech.esper.common.@internal.epl.resultset.order.OrderByProcessorCodegenNames;
using static com.espertech.esper.common.@internal.epl.util.EPTypeCollectionConst;
using static com.espertech.esper.common.@internal.metrics.instrumentation.InstrumentationCode;

namespace com.espertech.esper.common.@internal.epl.resultset.order
{
    /// <summary>
    /// An order-by processor that sorts events according to the expressions
    /// in the order_by clause.
    /// </summary>
    public class OrderByProcessorImpl
    {
        public static void GetSortKeyCodegen(
            OrderByProcessorForgeImpl forge,
            CodegenMethod method,
            CodegenClassScope classScope,
            CodegenNamedMethods namedMethods)
        {
            string[] expressions = null;
            bool[] descending = null;
            if (classScope.IsInstrumented) {
                expressions = forge.ExpressionTexts;
                descending = forge.DescendingFlags;
            }

            method.Block.Apply(Instblock(classScope, "qOrderBy", REF_EPS, Constant(expressions), Constant(descending)));
            var getSortKey = GenerateOrderKeyCodegen("getSortKeyInternal", forge.OrderBy, classScope, namedMethods);
            method.Block
                .DeclareVar<object>("key", LocalMethod(getSortKey, REF_EPS, REF_ISNEWDATA, REF_EXPREVALCONTEXT))
                .Apply(Instblock(classScope, "aOrderBy", Ref("key")))
                .MethodReturn(Ref("key"));
        }

        public static void GetSortKeyRollupCodegen(
            OrderByProcessorForgeImpl forge,
            CodegenMethod method,
            CodegenClassScope classScope,
            CodegenNamedMethods namedMethods)
        {
            method.Block.DeclareVar(
                typeof(int),
                "num",
                ExprDotMethod(REF_ORDERROLLUPLEVEL, "getLevelNumber"));
            var blocks = method.Block.SwitchBlockOfLength(Ref("num"), forge.OrderByRollup.Length, true);
            for (var i = 0; i < blocks.Length; i++) {
                var getSortKey = GenerateOrderKeyCodegen(
                    "getSortKeyInternal_" + i,
                    forge.OrderByRollup[i],
                    classScope,
                    namedMethods);
                blocks[i].BlockReturn(LocalMethod(getSortKey, REF_EPS, REF_ISNEWDATA, REF_EXPREVALCONTEXT));
            }
        }

        internal static void SortPlainCodegen(
            OrderByProcessorForgeImpl forge,
            CodegenMethod method,
            CodegenClassScope classScope,
            CodegenNamedMethods namedMethods)
        {
            var node = SortWGroupKeysInternalCodegen(forge, classScope, namedMethods);
            method.Block.IfCondition(
                    Or(EqualsNull(REF_OUTGOINGEVENTS), Relational(ArrayLength(REF_OUTGOINGEVENTS), LT, Constant(2))))
                .BlockReturn(REF_OUTGOINGEVENTS);

            method.Block.MethodReturn(
                LocalMethod(
                    node,
                    REF_OUTGOINGEVENTS,
                    REF_GENERATINGEVENTS,
                    ConstantNull(),
                    REF_ISNEWDATA,
                    REF_EXPREVALCONTEXT,
                    MEMBER_AGGREGATIONSVC));
        }

        internal static void SortRollupCodegen(
            OrderByProcessorForgeImpl forge,
            CodegenMethod method,
            CodegenClassScope classScope,
            CodegenNamedMethods namedMethods)
        {
            var createSortPropertiesWRollup = CreateSortPropertiesWRollupCodegen(forge, classScope, namedMethods);
            CodegenExpression comparator = classScope.AddOrGetDefaultFieldSharable(forge.IComparer);
            method.Block.DeclareVar(
                    typeof(IList<object>),
                    "sortValuesMultiKeys",
                    LocalMethod(
                        createSortPropertiesWRollup,
                        REF_ORDERCURRENTGENERATORS,
                        REF_ISNEWDATA,
                        MEMBER_EXPREVALCONTEXT,
                        MEMBER_AGGREGATIONSVC))
                .MethodReturn(
                    StaticMethod(
                        typeof(OrderByProcessorUtil),
                        "sortGivenOutgoingAndSortKeys",
                        REF_OUTGOINGEVENTS,
                        Ref("sortValuesMultiKeys"),
                        comparator));
        }

        internal static void SortWGroupKeysCodegen(
            OrderByProcessorForgeImpl forge,
            CodegenMethod method,
            CodegenClassScope classScope,
            CodegenNamedMethods namedMethods)
        {
            var sortWGroupKeysInternal = SortWGroupKeysInternalCodegen(forge, classScope, namedMethods);
            method.Block.IfCondition(
                    Or(EqualsNull(REF_OUTGOINGEVENTS), Relational(ArrayLength(REF_OUTGOINGEVENTS), LT, Constant(2))))
                .BlockReturn(REF_OUTGOINGEVENTS)
                .MethodReturn(
                    LocalMethod(
                        sortWGroupKeysInternal,
                        REF_OUTGOINGEVENTS,
                        REF_GENERATINGEVENTS,
                        REF_ORDERGROUPBYKEYS,
                        REF_ISNEWDATA,
                        REF_EXPREVALCONTEXT,
                        MEMBER_AGGREGATIONSVC));
        }

        internal static CodegenMethod SortWGroupKeysInternalCodegen(
            OrderByProcessorForgeImpl forge,
            CodegenClassScope classScope,
            CodegenNamedMethods namedMethods)
        {
            var createSortProperties = CreateSortPropertiesCodegen(forge, classScope, namedMethods);
            CodegenExpression comparator = classScope.AddOrGetDefaultFieldSharable(forge.IComparer);
            Consumer<CodegenMethod> code = method => {
                method.Block.DeclareVar(
                        typeof(IList<object>),
                        "sortValuesMultiKeys",
                        LocalMethod(
                            createSortProperties,
                            REF_GENERATINGEVENTS,
                            Ref("groupByKeys"),
                            REF_ISNEWDATA,
                            REF_EXPREVALCONTEXT,
                            MEMBER_AGGREGATIONSVC))
                    .MethodReturn(
                        StaticMethod(
                            typeof(OrderByProcessorUtil),
                            "SortGivenOutgoingAndSortKeys",
                            REF_OUTGOINGEVENTS,
                            Ref("sortValuesMultiKeys"),
                            comparator));
            };
            return namedMethods.AddMethod(
                typeof(EventBean[]),
                "SortWGroupKeysInternal",
                CodegenNamedParam.From(
                    typeof(EventBean[]),
                    REF_OUTGOINGEVENTS.Ref,
                    typeof(EventBean[][]),
                    REF_GENERATINGEVENTS.Ref,
                    typeof(object[]),
                    "groupByKeys",
                    typeof(bool),
                    REF_ISNEWDATA.Ref,
                    typeof(ExprEvaluatorContext),
                    REF_EXPREVALCONTEXT.Ref,
                    typeof(AggregationService),
                    MEMBER_AGGREGATIONSVC.Ref),
                typeof(OrderByProcessorImpl),
                classScope,
                code);
        }

        internal static CodegenMethod CreateSortPropertiesCodegen(
            OrderByProcessorForgeImpl forge,
            CodegenClassScope classScope,
            CodegenNamedMethods namedMethods)
        {
            Consumer<CodegenMethod> code = method => {
                string[] expressions = null;
                bool[] descending = null;
                if (classScope.IsInstrumented) {
                    expressions = forge.ExpressionTexts;
                    descending = forge.DescendingFlags;
                }

                method.Block.DeclareVar<object[]>(
                    "sortProperties",
                    NewArrayByLength(typeof(object), ArrayLength(REF_GENERATINGEVENTS)));

                var elements = forge.OrderBy;
                var forEach = method.Block.DeclareVar(typeof(int), "count", Constant(0))
                    .ForEach(typeof(EventBean[]), "eventsPerStream", REF_GENERATINGEVENTS);

                if (forge.IsNeedsGroupByKeys) {
                    forEach.ExprDotMethod(
                        MEMBER_AGGREGATIONSVC,
                        "SetCurrentAccess",
                        ArrayAtIndex(Ref("groupByKeys"), Ref("count")),
                        ExprDotName(REF_EXPREVALCONTEXT, "AgentInstanceId"),
                        ConstantNull());
                }

                forEach.Apply(
                    Instblock(
                        classScope,
                        "qOrderBy",
                        Ref("eventsPerStream"),
                        Constant(expressions),
                        Constant(descending)));
                if (elements.Length == 1) {
                    forEach.AssignArrayElement(
                        "sortProperties",
                        Ref("count"),
                        LocalMethod(
                            CodegenLegoMethodExpression.CodegenExpression(
                                elements[0].ExprNode.Forge,
                                method,
                                classScope),
                            Ref("eventsPerStream"),
                            REF_ISNEWDATA,
                            REF_EXPREVALCONTEXT));
                }
                else {
                    forEach.DeclareVar<object[]>(
                        "values",
                        NewArrayByLength(typeof(object), Constant(forge.OrderBy.Length)));
                    for (var i = 0; i < forge.OrderBy.Length; i++) {
                        forEach.AssignArrayElement(
                            "values",
                            Constant(i),
                            LocalMethod(
                                CodegenLegoMethodExpression.CodegenExpression(
                                    elements[i].ExprNode.Forge,
                                    method,
                                    classScope),
                                Ref("eventsPerStream"),
                                REF_ISNEWDATA,
                                REF_EXPREVALCONTEXT));
                    }

                    forEach.AssignArrayElement(
                        "sortProperties",
                        Ref("count"),
                        NewInstance(typeof(HashableMultiKey), Ref("values")));
                }

                forEach.Apply(Instblock(classScope, "aOrderBy", Ref("sortProperties")))
                    .IncrementRef("count");
                method.Block.MethodReturn(StaticMethod(typeof(CompatExtensions), "AsList", Ref("sortProperties")));
            };
            return namedMethods.AddMethod(
                typeof(IList<object>),
                "CreateSortProperties",
                CodegenNamedParam.From(
                    typeof(EventBean[][]),
                    REF_GENERATINGEVENTS.Ref,
                    typeof(object[]),
                    "groupByKeys",
                    typeof(bool),
                    REF_ISNEWDATA.Ref,
                    typeof(ExprEvaluatorContext),
                    REF_EXPREVALCONTEXT.Ref,
                    typeof(AggregationService),
                    MEMBER_AGGREGATIONSVC.Ref),
                typeof(OrderByProcessorImpl),
                classScope,
                code);
        }

        internal static void SortWOrderKeysCodegen(
            OrderByProcessorForgeImpl forge,
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            CodegenExpression comparator = classScope.AddOrGetDefaultFieldSharable(forge.IComparer);
            method.Block.MethodReturn(
                StaticMethod(
                    typeof(OrderByProcessorUtil),
                    "sortWOrderKeys",
                    REF_OUTGOINGEVENTS,
                    REF_ORDERKEYS,
                    comparator));
        }

        internal static void SortTwoKeysCodegen(
            OrderByProcessorForgeImpl forge,
            CodegenMethod method,
            CodegenClassScope classScope,
            CodegenNamedMethods namedMethods)
        {
            CodegenExpression comparator = classScope.AddOrGetDefaultFieldSharable(forge.IComparer);
            var compare = ExprDotMethod(comparator, "compare", REF_ORDERFIRSTSORTKEY, REF_ORDERSECONDSORTKEY);
            method.Block.IfCondition(Relational(compare, LE, Constant(0)))
                .BlockReturn(NewArrayWithInit(typeof(EventBean), REF_ORDERFIRSTEVENT, REF_ORDERSECONDEVENT))
                .MethodReturn(NewArrayWithInit(typeof(EventBean), REF_ORDERSECONDEVENT, REF_ORDERFIRSTEVENT));
        }

        internal static CodegenMethod CreateSortPropertiesWRollupCodegen(
            OrderByProcessorForgeImpl forge,
            CodegenClassScope classScope,
            CodegenNamedMethods namedMethods)
        {
            Consumer<CodegenMethod> code = method => {
                method.Block.DeclareVar<object[]>(
                        "sortProperties",
                        NewArrayByLength(typeof(object), ExprDotName(REF_ORDERCURRENTGENERATORS, "Count")))
                    .DeclareVar<int>("count", Constant(0));

                var forEach = method.Block.ForEach(typeof(GroupByRollupKey), "rollup", REF_ORDERCURRENTGENERATORS);

                if (forge.IsNeedsGroupByKeys) {
                    forEach.ExprDotMethod(
                        MEMBER_AGGREGATIONSVC,
                        "SetCurrentAccess",
                        ExprDotName(Ref("rollup"), "GroupKey"),
                        ExprDotName(REF_EXPREVALCONTEXT, "AgentInstanceId"),
                        ExprDotName(Ref("rollup"), "Level"));
                }

                forEach.DeclareVar<int>("num", ExprDotMethodChain(Ref("rollup")).Add("getLevel").Add("getLevelNumber"));
                var blocks = forEach.SwitchBlockOfLength(Ref("num"), forge.OrderByRollup.Length, false);
                for (var i = 0; i < blocks.Length; i++) {
                    var getSortKey = GenerateOrderKeyCodegen(
                        "GetSortKeyInternal_" + i,
                        forge.OrderByRollup[i],
                        classScope,
                        namedMethods);
                    blocks[i]
                        .AssignArrayElement(
                            "sortProperties",
                            Ref("count"),
                            LocalMethod(
                                getSortKey,
                                ExprDotName(Ref("rollup"), "Generator"),
                                REF_ISNEWDATA,
                                REF_EXPREVALCONTEXT));
                }

                forEach.IncrementRef("count");
                method.Block.MethodReturn(StaticMethod(typeof(CompatExtensions), "AsList", Ref("sortProperties")));
            };
            return namedMethods.AddMethod(
                typeof(IList<object>),
                "CreateSortPropertiesWRollup",
                CodegenNamedParam.From(
                    EPTYPE_LIST_GROUPBYROLLUPKEY,
                    REF_ORDERCURRENTGENERATORS.Ref,
                    typeof(bool),
                    REF_ISNEWDATA.Ref,
                    typeof(ExprEvaluatorContext),
                    REF_EXPREVALCONTEXT.Ref,
                    typeof(AggregationService),
                    MEMBER_AGGREGATIONSVC.Ref),
                typeof(OrderByProcessorImpl),
                classScope,
                code);
        }

        public static CodegenMethod DetermineLocalMinMaxCodegen(
            OrderByProcessorForgeImpl forge,
            CodegenClassScope classScope,
            CodegenNamedMethods namedMethods)
        {
            var elements = forge.OrderBy;
            CodegenExpression comparator = classScope.AddOrGetDefaultFieldSharable(forge.IComparer);

            Consumer<CodegenMethod> code = method => {
                method.Block.DeclareVar<object>("localMinMax", ConstantNull())
                    .DeclareVar<EventBean>("outgoingMinMaxBean", ConstantNull())
                    .DeclareVar<int>("count", Constant(0));

                if (elements.Length == 1) {
                    var forEach = method.Block.ForEach(typeof(EventBean[]), "eventsPerStream", REF_GENERATINGEVENTS);

                    forEach.DeclareVar<object>(
                            "sortKey",
                            LocalMethod(
                                CodegenLegoMethodExpression.CodegenExpression(
                                    elements[0].ExprNode.Forge,
                                    method,
                                    classScope),
                                Ref("eventsPerStream"),
                                REF_ISNEWDATA,
                                REF_EXPREVALCONTEXT))
                        .IfCondition(
                            Or(
                                EqualsNull(Ref("localMinMax")),
                                Relational(
                                    ExprDotMethod(comparator, "compare", Ref("localMinMax"), Ref("sortKey")),
                                    GT,
                                    Constant(0))))
                        .AssignRef("localMinMax", Ref("sortKey"))
                        .AssignRef("outgoingMinMaxBean", ArrayAtIndex(REF_OUTGOINGEVENTS, Ref("count")))
                        .BlockEnd()
                        .IncrementRef("count");
                }
                else {
                    method.Block.DeclareVar<object[]>(
                            "values",
                            NewArrayByLength(typeof(object), Constant(elements.Length)))
                        .DeclareVar<HashableMultiKey>("valuesMk", NewInstance(typeof(HashableMultiKey), Ref("values")));

                    var forEach = method.Block.ForEach(typeof(EventBean[]), "eventsPerStream", REF_GENERATINGEVENTS);

                    if (forge.IsNeedsGroupByKeys) {
                        forEach.ExprDotMethod(
                            MEMBER_AGGREGATIONSVC,
                            "SetCurrentAccess",
                            ArrayAtIndex(Ref("groupByKeys"), Ref("count")),
                            ExprDotMethod(REF_EXPREVALCONTEXT, "getAgentInstanceId", ConstantNull()));
                    }

                    for (var i = 0; i < elements.Length; i++) {
                        forEach.AssignArrayElement(
                            "values",
                            Constant(i),
                            LocalMethod(
                                CodegenLegoMethodExpression.CodegenExpression(
                                    elements[i].ExprNode.Forge,
                                    method,
                                    classScope),
                                Ref("eventsPerStream"),
                                REF_ISNEWDATA,
                                REF_EXPREVALCONTEXT));
                    }

                    forEach.IfCondition(
                            Or(
                                EqualsNull(Ref("localMinMax")),
                                Relational(
                                    ExprDotMethod(comparator, "compare", Ref("localMinMax"), Ref("valuesMk")),
                                    GT,
                                    Constant(0))))
                        .AssignRef("localMinMax", Ref("valuesMk"))
                        .AssignRef("values", NewArrayByLength(typeof(object), Constant(elements.Length)))
                        .AssignRef("valuesMk", NewInstance(typeof(HashableMultiKey), Ref("values")))
                        .AssignRef("outgoingMinMaxBean", ArrayAtIndex(REF_OUTGOINGEVENTS, Ref("count")))
                        .BlockEnd()
                        .IncrementRef("count");
                }

                method.Block.MethodReturn(Ref("outgoingMinMaxBean"));
            };

            return namedMethods.AddMethod(
                typeof(EventBean),
                "determineLocalMinMax",
                CodegenNamedParam.From(
                    typeof(EventBean[]),
                    REF_OUTGOINGEVENTS.Ref,
                    typeof(EventBean[][]),
                    REF_GENERATINGEVENTS.Ref,
                    typeof(bool),
                    NAME_ISNEWDATA,
                    typeof(ExprEvaluatorContext),
                    NAME_EXPREVALCONTEXT,
                    typeof(AggregationService),
                    MEMBER_AGGREGATIONSVC.Ref),
                typeof(OrderByProcessorImpl),
                classScope,
                code);
        }

        internal static CodegenMethod GenerateOrderKeyCodegen(
            string methodName,
            OrderByElementForge[] orderBy,
            CodegenClassScope classScope,
            CodegenNamedMethods namedMethods)
        {
            Consumer<CodegenMethod> code = methodNode => {
                if (orderBy.Length == 1) {
                    var expression = CodegenLegoMethodExpression.CodegenExpression(
                        orderBy[0].ExprNode.Forge,
                        methodNode,
                        classScope);
                    methodNode.Block.MethodReturn(
                        LocalMethod(expression, EnumForgeCodegenNames.REF_EPS, REF_ISNEWDATA, REF_EXPREVALCONTEXT));
                    return;
                }

                methodNode.Block.DeclareVar<object[]>(
                    "keys",
                    NewArrayByLength(typeof(object), Constant(orderBy.Length)));
                for (var i = 0; i < orderBy.Length; i++) {
                    var expression = CodegenLegoMethodExpression.CodegenExpression(
                        orderBy[i].ExprNode.Forge,
                        methodNode,
                        classScope);
                    methodNode.Block.AssignArrayElement(
                        "keys",
                        Constant(i),
                        LocalMethod(expression, EnumForgeCodegenNames.REF_EPS, REF_ISNEWDATA, REF_EXPREVALCONTEXT));
                }

                methodNode.Block.MethodReturn(NewInstance(typeof(HashableMultiKey), Ref("keys")));
            };

            return namedMethods.AddMethod(
                typeof(object),
                methodName,
                CodegenNamedParam.From(
                    typeof(EventBean[]),
                    NAME_EPS,
                    typeof(bool),
                    NAME_ISNEWDATA,
                    typeof(ExprEvaluatorContext),
                    NAME_EXPREVALCONTEXT),
                typeof(ResultSetProcessorUtil),
                classScope,
                code);
        }
    }
} // end of namespace