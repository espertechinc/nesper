///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.compat.collections;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.resultset.codegen.ResultSetProcessorCodegenNames;
using static com.espertech.esper.common.@internal.epl.resultset.handthru.ResultSetProcessorHandThroughUtil;

namespace com.espertech.esper.common.@internal.epl.resultset.handthru
{
    /// <summary>
    ///     Result set processor for the hand-through case:
    ///     no aggregation functions used in the select clause, and no group-by, no having and ordering.
    /// </summary>
    public class ResultSetProcessorHandThrough
    {
        internal static void ProcessJoinResultCodegen(
            ResultSetProcessorHandThroughFactoryForge prototype,
            CodegenMethod method)
        {
            var oldEvents = ConstantNull();
            if (prototype.IsSelectRStream) {
                oldEvents = StaticMethod(
                    typeof(ResultSetProcessorHandThroughUtil),
                    METHOD_GETSELECTEVENTSNOHAVINGHANDTHRUJOIN,
                    MEMBER_SELECTEXPRPROCESSOR,
                    REF_OLDDATA,
                    Constant(false),
                    REF_ISSYNTHESIZE,
                    MEMBER_AGENTINSTANCECONTEXT);
            }

            var newEvents = StaticMethod(
                typeof(ResultSetProcessorHandThroughUtil),
                METHOD_GETSELECTEVENTSNOHAVINGHANDTHRUJOIN,
                MEMBER_SELECTEXPRPROCESSOR,
                REF_NEWDATA,
                Constant(true),
                REF_ISSYNTHESIZE,
                MEMBER_AGENTINSTANCECONTEXT);

            method.Block
                .DeclareVar<EventBean[]>("selectOldEvents", oldEvents)
                .DeclareVar<EventBean[]>("selectNewEvents", newEvents)
                .MethodReturn(NewInstance<UniformPair<EventBean[]>>(Ref("selectNewEvents"), Ref("selectOldEvents")));
        }

        internal static void ProcessViewResultCodegen(
            ResultSetProcessorHandThroughFactoryForge prototype,
            CodegenMethod method)
        {
            var oldEvents = ConstantNull();
            if (prototype.IsSelectRStream) {
                oldEvents = StaticMethod(
                    typeof(ResultSetProcessorHandThroughUtil),
                    METHOD_GETSELECTEVENTSNOHAVINGHANDTHRUVIEW,
                    MEMBER_SELECTEXPRPROCESSOR,
                    REF_OLDDATA,
                    Constant(false),
                    REF_ISSYNTHESIZE,
                    MEMBER_AGENTINSTANCECONTEXT);
            }

            var newEvents = StaticMethod(
                typeof(ResultSetProcessorHandThroughUtil),
                METHOD_GETSELECTEVENTSNOHAVINGHANDTHRUVIEW,
                MEMBER_SELECTEXPRPROCESSOR,
                REF_NEWDATA,
                Constant(true),
                REF_ISSYNTHESIZE,
                MEMBER_AGENTINSTANCECONTEXT);

            method.Block
                .DeclareVar<EventBean[]>("selectOldEvents", oldEvents)
                .DeclareVar<EventBean[]>("selectNewEvents", newEvents)
                .MethodReturn(NewInstance<UniformPair<EventBean[]>>(Ref("selectNewEvents"), Ref("selectOldEvents")));
        }

        internal static void GetEnumeratorViewCodegen(CodegenMethod methodNode)
        {
            methodNode.Block.MethodReturn(
                NewInstance<TransformEventEnumerator>(
                    ExprDotMethod(REF_VIEWABLE, "GetEnumerator"),
                    NewInstance<ResultSetProcessorHandtruTransform>(Ref("this"))));
        }

        internal static void GetEnumeratorJoinCodegen(CodegenMethod method)
        {
            method.Block
                .DeclareVar<UniformPair<EventBean[]>>(
                    "result",
                    ExprDotMethod(
                        Ref("this"),
                        "ProcessJoinResult",
                        REF_JOINSET,
                        StaticMethod(typeof(Collections), "GetEmptySet", new[] {typeof(MultiKeyArrayOfKeys<EventBean>)}),
                        Constant(true)))
                .MethodReturn(
                    NewInstance<ArrayEventEnumerator>(
                        ExprDotName(Ref("result"), "First")));
        }
    }
} // end of namespace