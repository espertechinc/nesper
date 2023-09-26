///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.common.@internal.epl.resultset.core;
using com.espertech.esper.compat.function;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.expression.codegen.ExprForgeCodegenNames;
using static com.espertech.esper.common.@internal.epl.resultset.codegen.ResultSetProcessorCodegenNames;
using static com.espertech.esper.common.@internal.epl.resultset.core.ResultSetProcessorUtil;
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
            method.Block.DeclareVar<EventBean[]>(NAME_EPS, NewArrayByLength(typeof(EventBean), Constant(1)));

            {
                var ifNew = method.Block.IfCondition(NotEqualsNull(REF_NEWDATA));
                {
                    var newLoop = ifNew.ForEach(typeof(EventBean), "aNewData", REF_NEWDATA);
                    newLoop.AssignArrayElement(NAME_EPS, Constant(0), Ref("aNewData"))
                        .DeclareVar<object>("mk", LocalMethod(forge.GenerateGroupKeySingle, REF_EPS, ConstantTrue()))
                        .ExprDotMethod(Ref("groupReps"), "Put", Ref("mk"), Ref("aNewData"))
                        .ExprDotMethod(MEMBER_AGGREGATIONSVC, "ApplyEnter", REF_EPS, Ref("mk"), MEMBER_EXPREVALCONTEXT);
                }
            }

            {
                var ifOld = method.Block.IfCondition(NotEqualsNull(REF_OLDDATA));
                {
                    var oldLoop = ifOld.ForEach(typeof(EventBean), "anOldData", REF_OLDDATA);
                    oldLoop.AssignArrayElement(NAME_EPS, Constant(0), Ref("anOldData"))
                        .DeclareVar<object>("mk", LocalMethod(forge.GenerateGroupKeySingle, REF_EPS, ConstantFalse()))
                        .ExprDotMethod(MEMBER_AGGREGATIONSVC, "ApplyLeave", REF_EPS, Ref("mk"), MEMBER_EXPREVALCONTEXT);
                }
            }
        }

        internal static void ProcessViewResultUnboundCodegen(
            ResultSetProcessorRowPerGroupForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            var generateGroupKeysKeepEvent = GenerateGroupKeysKeepEventCodegen(forge, classScope, instance);
            var generateOutputEventsView = GenerateOutputEventsViewCodegen(forge, classScope, instance);
            var processViewResultNewDepthOneUnbound =
                ProcessViewResultNewDepthOneUnboundCodegen(forge, classScope, instance);

            var ifShortcut = method.Block.IfCondition(
                And(NotEqualsNull(REF_NEWDATA), EqualsIdentity(ArrayLength(REF_NEWDATA), Constant(1))));
            ifShortcut.IfCondition(Or(EqualsNull(REF_OLDDATA), EqualsIdentity(ArrayLength(REF_OLDDATA), Constant(0))))
                .BlockReturn(LocalMethod(processViewResultNewDepthOneUnbound, REF_NEWDATA, REF_ISSYNTHESIZE));

            method.Block.DeclareVar(
                    typeof(IDictionary<object, EventBean>),
                    "keysAndEvents",
                    NewInstance(typeof(Dictionary<object, EventBean>)))
                .DeclareVar<EventBean[]>(NAME_EPS, NewArrayByLength(typeof(EventBean), Constant(1)))
                .DeclareVar<object[]>(
                    "newDataMultiKey",
                    LocalMethod(generateGroupKeysKeepEvent, REF_NEWDATA, Ref("keysAndEvents"), ConstantTrue(), REF_EPS))
                .DeclareVar<object[]>(
                    "oldDataMultiKey",
                    LocalMethod(
                        generateGroupKeysKeepEvent,
                        REF_OLDDATA,
                        Ref("keysAndEvents"),
                        ConstantFalse(),
                        REF_EPS))
                .DeclareVar<EventBean[]>(
                    "selectOldEvents",
                    forge.IsSelectRStream
                        ? LocalMethod(
                            generateOutputEventsView,
                            Ref("keysAndEvents"),
                            ConstantFalse(),
                            REF_ISSYNTHESIZE,
                            REF_EPS)
                        : ConstantNull());

            {
                var ifNew = method.Block.IfCondition(NotEqualsNull(REF_NEWDATA));
                {
                    var newLoop = ifNew.ForLoopIntSimple("i", ArrayLength(REF_NEWDATA));
                    newLoop.AssignArrayElement(NAME_EPS, Constant(0), ArrayAtIndex(REF_NEWDATA, Ref("i")))
                        .ExprDotMethod(
                            Ref("groupReps"),
                            "Put",
                            ArrayAtIndex(Ref("newDataMultiKey"), Ref("i")),
                            ArrayAtIndex(REF_EPS, Constant(0)))
                        .ExprDotMethod(
                            MEMBER_AGGREGATIONSVC,
                            "ApplyEnter",
                            REF_EPS,
                            ArrayAtIndex(Ref("newDataMultiKey"), Ref("i")),
                            MEMBER_EXPREVALCONTEXT);
                }
            }

            {
                var ifOld = method.Block.IfCondition(NotEqualsNull(REF_OLDDATA));
                {
                    var newLoop = ifOld.ForLoopIntSimple("i", ArrayLength(REF_OLDDATA));
                    newLoop.AssignArrayElement(NAME_EPS, Constant(0), ArrayAtIndex(REF_OLDDATA, Ref("i")))
                        .ExprDotMethod(
                            MEMBER_AGGREGATIONSVC,
                            "ApplyLeave",
                            REF_EPS,
                            ArrayAtIndex(Ref("oldDataMultiKey"), Ref("i")),
                            MEMBER_EXPREVALCONTEXT);
                }
            }

            method.Block.DeclareVar<EventBean[]>(
                    "selectNewEvents",
                    LocalMethod(
                        generateOutputEventsView,
                        Ref("keysAndEvents"),
                        ConstantTrue(),
                        REF_ISSYNTHESIZE,
                        REF_EPS))
                .MethodReturn(
                    StaticMethod(
                        typeof(ResultSetProcessorUtil),
                        METHOD_TOPAIRNULLIFALLNULL,
                        Ref("selectNewEvents"),
                        Ref("selectOldEvents")));
        }

        public static void GetIteratorViewUnboundedCodegen(
            ResultSetProcessorRowPerGroupForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            if (!forge.IsSorting) {
                method.Block
                    .DeclareVar(
                        typeof(IEnumerator<EventBean>),
                        "enumerator",
                        ExprDotMethod(Ref("groupReps"), "ValueEnumerator"))
                    .MethodReturn(
                        StaticMethod<ResultSetProcessorRowPerGroupEnumerator>(
                            "For",
                            Ref("enumerator"),
                            Ref("this"),
                            MEMBER_AGGREGATIONSVC,
                            MEMBER_EXPREVALCONTEXT));
            }
            else {
                var getIteratorSorted = GetEnumeratorSortedCodegen(forge, classScope, instance);
                method.Block.MethodReturn(
                    LocalMethod(getIteratorSorted, ExprDotMethod(Ref("groupReps"), "ValueEnumerator")));
            }
        }

        private static CodegenMethod ProcessViewResultNewDepthOneUnboundCodegen(
            ResultSetProcessorRowPerGroupForge forge,
            CodegenClassScope classScope,
            CodegenInstanceAux instance)
        {
            var shortcutEvalGivenKey = ResultSetProcessorRowPerGroupImpl.ShortcutEvalGivenKeyCodegen(
                forge.OptionalHavingNode,
                classScope,
                instance);

            Consumer<CodegenMethod> code = methodNode => {
                methodNode.Block.DeclareVar<object>(
                    "groupKey",
                    LocalMethod(forge.GenerateGroupKeySingle, REF_NEWDATA, ConstantTrue()));
                if (forge.IsSelectRStream) {
                    methodNode.Block.DeclareVar<EventBean>(
                        "rstream",
                        LocalMethod(
                            shortcutEvalGivenKey,
                            REF_NEWDATA,
                            Ref("groupKey"),
                            ConstantFalse(),
                            REF_ISSYNTHESIZE));
                }

                methodNode.Block.ExprDotMethod(
                        MEMBER_AGGREGATIONSVC,
                        "ApplyEnter",
                        REF_NEWDATA,
                        Ref("groupKey"),
                        MEMBER_EXPREVALCONTEXT)
                    .ExprDotMethod(Ref("groupReps"), "Put", Ref("groupKey"), ArrayAtIndex(REF_NEWDATA, Constant(0)))
                    .DeclareVar<EventBean>(
                        "istream",
                        LocalMethod(
                            shortcutEvalGivenKey,
                            REF_NEWDATA,
                            Ref("groupKey"),
                            ConstantTrue(),
                            REF_ISSYNTHESIZE));
                if (forge.IsSelectRStream) {
                    methodNode.Block.MethodReturn(
                        StaticMethod(
                            typeof(ResultSetProcessorUtil),
                            "ToPairNullIfAllNullSingle",
                            Ref("istream"),
                            Ref("rstream")));
                }
                else {
                    methodNode.Block.MethodReturn(
                        StaticMethod(typeof(ResultSetProcessorUtil), "ToPairNullIfNullIStream", Ref("istream")));
                }
            };

            return instance.Methods.AddMethod(
                typeof(UniformPair<EventBean[]>),
                "ProcessViewResultNewDepthOneUnboundCodegen",
                CodegenNamedParam.From(typeof(EventBean[]), NAME_NEWDATA, typeof(bool), NAME_ISSYNTHESIZE),
                typeof(ResultSetProcessorRowPerGroupImpl),
                classScope,
                code);
        }

        public static void StopMethodCodegenUnbound(
            ResultSetProcessorRowPerGroupForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            StopMethodCodegenBound(method, instance);
            method.Block.ExprDotMethod(Ref("groupReps"), "Destroy");
        }
    }
} // end of namespace