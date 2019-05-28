///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.resultset.core;
using com.espertech.esper.common.@internal.epl.resultset.grouped;
using com.espertech.esper.compat.function;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.expression.codegen.ExprForgeCodegenNames;
using static com.espertech.esper.common.@internal.epl.resultset.codegen.ResultSetProcessorCodegenNames;
using static com.espertech.esper.common.@internal.epl.resultset.core.ResultSetProcessorUtil;
using static com.espertech.esper.common.@internal.epl.resultset.grouped.ResultSetProcessorGroupedUtil;
using static com.espertech.esper.common.@internal.epl.resultset.rowpergroup.ResultSetProcessorRowPerGroupImpl;

namespace com.espertech.esper.common.@internal.epl.resultset.rowpergroup
{
    public class ResultSetProcessorRowPerGroupUnbound
    {
        public static void ApplyViewResultCodegen(
            ResultSetProcessorRowPerGroupForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            CodegenMethod generateGroupKeyViewSingle = GenerateGroupKeySingleCodegen(
                forge.GroupKeyNodeExpressions, classScope, instance);

            method.Block.DeclareVar(typeof(EventBean[]), NAME_EPS, NewArrayByLength(typeof(EventBean), Constant(1)));

            {
                CodegenBlock ifNew = method.Block.IfCondition(NotEqualsNull(REF_NEWDATA));
                {
                    CodegenBlock newLoop = ifNew.ForEach(typeof(EventBean), "aNewData", REF_NEWDATA);
                    newLoop.AssignArrayElement(NAME_EPS, Constant(0), @Ref("aNewData"))
                        .DeclareVar(
                            typeof(object), "mk", LocalMethod(generateGroupKeyViewSingle, REF_EPS, ConstantTrue()))
                        .ExprDotMethod(@Ref("groupReps"), "put", @Ref("mk"), @Ref("aNewData"))
                        .ExprDotMethod(REF_AGGREGATIONSVC, "applyEnter", REF_EPS, @Ref("mk"), REF_AGENTINSTANCECONTEXT);
                }
            }

            {
                CodegenBlock ifOld = method.Block.IfCondition(NotEqualsNull(REF_OLDDATA));
                {
                    CodegenBlock oldLoop = ifOld.ForEach(typeof(EventBean), "anOldData", REF_OLDDATA);
                    oldLoop.AssignArrayElement(NAME_EPS, Constant(0), @Ref("anOldData"))
                        .DeclareVar(
                            typeof(object), "mk", LocalMethod(generateGroupKeyViewSingle, REF_EPS, ConstantFalse()))
                        .ExprDotMethod(REF_AGGREGATIONSVC, "applyLeave", REF_EPS, @Ref("mk"), REF_AGENTINSTANCECONTEXT);
                }
            }
        }

        protected internal static void ProcessViewResultUnboundCodegen(
            ResultSetProcessorRowPerGroupForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            CodegenMethod generateGroupKeysKeepEvent = GenerateGroupKeysKeepEventCodegen(forge, classScope, instance);
            CodegenMethod generateOutputEventsView = GenerateOutputEventsViewCodegen(forge, classScope, instance);
            CodegenMethod processViewResultNewDepthOneUnbound =
                ProcessViewResultNewDepthOneUnboundCodegen(forge, classScope, instance);

            CodegenBlock ifShortcut = method.Block.IfCondition(
                And(NotEqualsNull(REF_NEWDATA), EqualsIdentity(ArrayLength(REF_NEWDATA), Constant(1))));
            ifShortcut.IfCondition(Or(EqualsNull(REF_OLDDATA), EqualsIdentity(ArrayLength(REF_OLDDATA), Constant(0))))
                .BlockReturn(LocalMethod(processViewResultNewDepthOneUnbound, REF_NEWDATA, REF_ISSYNTHESIZE));

            method.Block.DeclareVar(typeof(IDictionary<object, object>), "keysAndEvents", NewInstance(typeof(Dictionary<object, object>)))
                .DeclareVar(typeof(EventBean[]), NAME_EPS, NewArrayByLength(typeof(EventBean), Constant(1)))
                .DeclareVar(
                    typeof(object[]), "newDataMultiKey",
                    LocalMethod(
                        generateGroupKeysKeepEvent, REF_NEWDATA, @Ref("keysAndEvents"), ConstantTrue(), REF_EPS))
                .DeclareVar(
                    typeof(object[]), "oldDataMultiKey",
                    LocalMethod(
                        generateGroupKeysKeepEvent, REF_OLDDATA, @Ref("keysAndEvents"), ConstantFalse(), REF_EPS))
                .DeclareVar(
                    typeof(EventBean[]), "selectOldEvents",
                    forge.IsSelectRStream
                        ? LocalMethod(
                            generateOutputEventsView, @Ref("keysAndEvents"), ConstantFalse(), REF_ISSYNTHESIZE, REF_EPS)
                        : ConstantNull());

            {
                CodegenBlock ifNew = method.Block.IfCondition(NotEqualsNull(REF_NEWDATA));
                {
                    CodegenBlock newLoop = ifNew.ForLoopIntSimple("i", ArrayLength(REF_NEWDATA));
                    newLoop.AssignArrayElement(NAME_EPS, Constant(0), ArrayAtIndex(REF_NEWDATA, @Ref("i")))
                        .ExprDotMethod(
                            @Ref("groupReps"), "put", ArrayAtIndex(@Ref("newDataMultiKey"), @Ref("i")),
                            ArrayAtIndex(REF_EPS, Constant(0)))
                        .ExprDotMethod(
                            REF_AGGREGATIONSVC, "applyEnter", REF_EPS, ArrayAtIndex(@Ref("newDataMultiKey"), @Ref("i")),
                            REF_AGENTINSTANCECONTEXT);
                }
            }

            {
                CodegenBlock ifOld = method.Block.IfCondition(NotEqualsNull(REF_OLDDATA));
                {
                    CodegenBlock newLoop = ifOld.ForLoopIntSimple("i", ArrayLength(REF_OLDDATA));
                    newLoop.AssignArrayElement(NAME_EPS, Constant(0), ArrayAtIndex(REF_OLDDATA, @Ref("i")))
                        .ExprDotMethod(
                            REF_AGGREGATIONSVC, "applyLeave", REF_EPS, ArrayAtIndex(@Ref("oldDataMultiKey"), @Ref("i")),
                            REF_AGENTINSTANCECONTEXT);
                }
            }

            method.Block.DeclareVar(
                    typeof(EventBean[]), "selectNewEvents",
                    LocalMethod(
                        generateOutputEventsView, @Ref("keysAndEvents"), ConstantTrue(), REF_ISSYNTHESIZE, REF_EPS))
                .MethodReturn(
                    StaticMethod(
                        typeof(ResultSetProcessorUtil), METHOD_TOPAIRNULLIFALLNULL, @Ref("selectNewEvents"),
                        @Ref("selectOldEvents")));
        }

        public static void GetIteratorViewUnboundedCodegen(
            ResultSetProcessorRowPerGroupForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            if (!forge.IsSorting) {
                method.Block.DeclareVar(typeof(IEnumerator<EventBean>), "it", ExprDotMethod(@Ref("groupReps"), "valueIterator"))
                    .MethodReturn(
                        NewInstance<ResultSetProcessorRowPerGroupEnumerator>(
                            @Ref("it"), @Ref("this"), REF_AGGREGATIONSVC,
                            REF_AGENTINSTANCECONTEXT));
            }
            else {
                CodegenMethod getIteratorSorted = GetIteratorSortedCodegen(forge, classScope, instance);
                method.Block.MethodReturn(
                    LocalMethod(getIteratorSorted, ExprDotMethod(@Ref("groupReps"), "valueIterator")));
            }
        }

        protected internal static CodegenMethod ProcessViewResultNewDepthOneUnboundCodegen(
            ResultSetProcessorRowPerGroupForge forge,
            CodegenClassScope classScope,
            CodegenInstanceAux instance)
        {
            CodegenMethod shortcutEvalGivenKey =
                ResultSetProcessorRowPerGroupImpl.ShortcutEvalGivenKeyCodegen(
                    forge.OptionalHavingNode, classScope, instance);
            CodegenMethod generateGroupKeySingle =
                ResultSetProcessorGroupedUtil.GenerateGroupKeySingleCodegen(
                    forge.GroupKeyNodeExpressions, classScope, instance);

            Consumer<CodegenMethod> code = methodNode => {
                methodNode.Block.DeclareVar(
                    typeof(object), "groupKey", LocalMethod(generateGroupKeySingle, REF_NEWDATA, ConstantTrue()));
                if (forge.IsSelectRStream) {
                    methodNode.Block.DeclareVar(
                        typeof(EventBean), "rstream",
                        LocalMethod(
                            shortcutEvalGivenKey, REF_NEWDATA, @Ref("groupKey"), ConstantFalse(), REF_ISSYNTHESIZE));
                }

                methodNode.Block.ExprDotMethod(
                        REF_AGGREGATIONSVC, "applyEnter", REF_NEWDATA, @Ref("groupKey"), REF_AGENTINSTANCECONTEXT)
                    .ExprDotMethod(@Ref("groupReps"), "put", @Ref("groupKey"), ArrayAtIndex(REF_NEWDATA, Constant(0)))
                    .DeclareVar(
                        typeof(EventBean), "istream",
                        LocalMethod(
                            shortcutEvalGivenKey, REF_NEWDATA, @Ref("groupKey"), ConstantTrue(), REF_ISSYNTHESIZE));
                if (forge.IsSelectRStream) {
                    methodNode.Block.MethodReturn(
                        StaticMethod(
                            typeof(ResultSetProcessorUtil), "toPairNullIfAllNullSingle", @Ref("istream"),
                            @Ref("rstream")));
                }
                else {
                    methodNode.Block.MethodReturn(
                        StaticMethod(typeof(ResultSetProcessorUtil), "toPairNullIfNullIStream", @Ref("istream")));
                }
            };

            return instance.Methods.AddMethod(
                typeof(UniformPair<>), "processViewResultNewDepthOneUnboundCodegen",
                CodegenNamedParam.From(typeof(EventBean[]), NAME_NEWDATA, typeof(bool), NAME_ISSYNTHESIZE),
                typeof(ResultSetProcessorRowPerGroupImpl), classScope, code);
        }

        public static void StopMethodCodegenUnbound(
            ResultSetProcessorRowPerGroupForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            ResultSetProcessorRowPerGroupImpl.StopMethodCodegenBound(method, instance);
            method.Block.ExprDotMethod(@Ref("groupReps"), "destroy");
        }
    }
} // end of namespace