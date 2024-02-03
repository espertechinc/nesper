///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.resultset.core;
using com.espertech.esper.common.@internal.epl.resultset.grouped;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.resultset.codegen.ResultSetProcessorCodegenNames;
using static com.espertech.esper.common.@internal.epl.resultset.core.ResultSetProcessorUtil;
using static com.espertech.esper.common.@internal.epl.resultset.grouped.ResultSetProcessorGroupedUtil;
using static com.espertech.esper.common.@internal.epl.resultset.rowpergrouprollup.
    ResultSetProcessorRowPerGroupRollupImpl;

namespace com.espertech.esper.common.@internal.epl.resultset.rowpergrouprollup
{
    public class ResultSetProcessorRowPerGroupRollupUnbound
    {
        private const string NAME_UNBOUNDHELPER = "unboundHelper";

        internal static void StopMethodUnboundCodegen(
            ResultSetProcessorRowPerGroupRollupForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            StopMethodCodegenBound(method, instance);
            method.Block.ExprDotMethod(Ref(NAME_UNBOUNDHELPER), "Destroy");
        }

        internal static void ApplyViewResultUnboundCodegen(
            ResultSetProcessorRowPerGroupRollupForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            var generateGroupKeysView = GenerateGroupKeysViewCodegen(forge, classScope, instance);

            method.Block
                .DeclareVar<object[][]>(
                    "newDataMultiKey",
                    LocalMethod(
                        generateGroupKeysView,
                        REF_NEWDATA,
                        ExprDotName(Ref(NAME_UNBOUNDHELPER), "Buffer"),
                        ConstantTrue()))
                .DeclareVar<object[][]>(
                    "oldDataMultiKey",
                    LocalMethod(
                        generateGroupKeysView,
                        REF_OLDDATA,
                        ExprDotName(Ref(NAME_UNBOUNDHELPER), "Buffer"),
                        ConstantFalse()))
                .DeclareVar<EventBean[]>("eventsPerStream", NewArrayByLength(typeof(EventBean), Constant(1)))
                .StaticMethod(
                    typeof(ResultSetProcessorGroupedUtil),
                    METHOD_APPLYAGGVIEWRESULTKEYEDVIEW,
                    MEMBER_AGGREGATIONSVC,
                    MEMBER_EXPREVALCONTEXT,
                    REF_NEWDATA,
                    Ref("newDataMultiKey"),
                    REF_OLDDATA,
                    Ref("oldDataMultiKey"),
                    Ref("eventsPerStream"));
        }

        internal static void ProcessViewResultUnboundCodegen(
            ResultSetProcessorRowPerGroupRollupForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            var factory =
                classScope.AddOrGetDefaultFieldSharable(ResultSetProcessorHelperFactoryField.INSTANCE);
            var eventTypes = classScope.AddDefaultFieldUnshared(
                true,
                typeof(EventType[]),
                EventTypeUtility.ResolveTypeArrayCodegen(forge.EventTypes, EPStatementInitServicesConstants.REF));
            instance.AddMember(NAME_UNBOUNDHELPER, typeof(ResultSetProcessorRowPerGroupRollupUnboundHelper));
            instance.ServiceCtor.Block.AssignRef(
                NAME_UNBOUNDHELPER,
                ExprDotMethod(
                    factory,
                    "MakeRSRowPerGroupRollupSnapshotUnbound",
                    MEMBER_EXPREVALCONTEXT,
                    Ref("this"),
                    Constant(forge.GroupKeyTypes),
                    Constant(forge.NumStreams),
                    eventTypes,
                    forge.OutputSnapshotSettings.ToExpression()));

            var generateGroupKeysView = GenerateGroupKeysViewCodegen(forge, classScope, instance);
            var generateOutputEventsView = GenerateOutputEventsViewCodegen(forge, classScope, instance);

            method.Block
                .DeclareVar<object[][]>(
                    "newDataMultiKey",
                    LocalMethod(
                        generateGroupKeysView,
                        REF_NEWDATA,
                        ExprDotName(Ref(NAME_UNBOUNDHELPER), "Buffer"),
                        ConstantTrue()))
                .DeclareVar<object[][]>(
                    "oldDataMultiKey",
                    LocalMethod(
                        generateGroupKeysView,
                        REF_OLDDATA,
                        ExprDotName(Ref(NAME_UNBOUNDHELPER), "Buffer"),
                        ConstantFalse()))
                .DeclareVar<EventBean[]>(
                    "selectOldEvents",
                    forge.IsSelectRStream
                        ? LocalMethod(
                            generateOutputEventsView,
                            ExprDotName(Ref(NAME_UNBOUNDHELPER), "Buffer"),
                            ConstantFalse(),
                            REF_ISSYNTHESIZE)
                        : ConstantNull())
                .DeclareVar<EventBean[]>("eventsPerStream", NewArrayByLength(typeof(EventBean), Constant(1)))
                .StaticMethod(
                    typeof(ResultSetProcessorGroupedUtil),
                    METHOD_APPLYAGGVIEWRESULTKEYEDVIEW,
                    MEMBER_AGGREGATIONSVC,
                    MEMBER_EXPREVALCONTEXT,
                    REF_NEWDATA,
                    Ref("newDataMultiKey"),
                    REF_OLDDATA,
                    Ref("oldDataMultiKey"),
                    Ref("eventsPerStream"))
                .DeclareVar<EventBean[]>(
                    "selectNewEvents",
                    LocalMethod(
                        generateOutputEventsView,
                        ExprDotName(Ref(NAME_UNBOUNDHELPER), "Buffer"),
                        ConstantTrue(),
                        REF_ISSYNTHESIZE))
                .MethodReturn(
                    StaticMethod(
                        typeof(ResultSetProcessorUtil),
                        METHOD_TOPAIRNULLIFALLNULL,
                        Ref("selectNewEvents"),
                        Ref("selectOldEvents")));
        }

        internal static void GetEnumeratorViewUnboundCodegen(
            ResultSetProcessorRowPerGroupRollupForge forge,
            CodegenClassScope classScope,
            CodegenMethod method,
            CodegenInstanceAux instance)
        {
            var generateOutputEventsView = GenerateOutputEventsViewCodegen(forge, classScope, instance);

            method.Block.DeclareVar<EventBean[]>(
                    "output",
                    LocalMethod(
                        generateOutputEventsView,
                        ExprDotName(Ref(NAME_UNBOUNDHELPER), "Buffer"),
                        ConstantTrue(),
                        ConstantTrue()))
                .MethodReturn(StaticMethod(typeof(Arrays), "GetEnumerator", Ref("output")));
        }
    }
} // end of namespace