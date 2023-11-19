///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.bytecodemodel.util;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;

using static com.espertech.esper.common.@internal.bytecodemodel.@base.CodegenMethod;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.agg.core.AggregationServiceCodegenNames;
using static com.espertech.esper.common.@internal.epl.agg.core.AggregationServiceFactoryCompiler;
using static com.espertech.esper.common.@internal.epl.expression.codegen.ExprForgeCodegenNames;
using static com.espertech.esper.common.@internal.metrics.instrumentation.InstrumentationCode;

namespace com.espertech.esper.common.@internal.epl.agg.core
{
    public class AggregationServiceFactoryCompilerRow
    {
        private const string NAME_STATEMENT_FIELDS = ("statementFields");

        private const string NAME_ASSIGNMENT = "ASSIGNMENTS";

        public static AggregationClassAssignmentPerLevel MakeRow(
            bool isGenerateTableEnter,
            AggregationCodegenRowLevelDesc rowLevelDesc,
            Type forgeClass,
            Consumer<AggregationRowCtorDesc> rowCtorConsumer,
            CodegenClassScope classScope,
            IList<CodegenInnerClass> innerClasses,
            AggregationClassNames classNames)
        {
            AggregationClassAssignment[] topAssignments = null;
            if (rowLevelDesc.OptionalTopRow != null) {
                topAssignments = MakeRowForLevel(
                    isGenerateTableEnter,
                    classNames.RowTop,
                    rowLevelDesc.OptionalTopRow,
                    forgeClass,
                    rowCtorConsumer,
                    classScope,
                    innerClasses);
            }

            AggregationClassAssignment[][] leafAssignments = null;
            if (rowLevelDesc.OptionalAdditionalRows != null) {
                leafAssignments = new AggregationClassAssignment[rowLevelDesc.OptionalAdditionalRows.Length][];
                for (var i = 0; i < rowLevelDesc.OptionalAdditionalRows.Length; i++) {
                    var className = classNames.GetRowPerLevel(i);
                    leafAssignments[i] = MakeRowForLevel(
                        isGenerateTableEnter,
                        className,
                        rowLevelDesc.OptionalAdditionalRows[i],
                        forgeClass,
                        rowCtorConsumer,
                        classScope,
                        innerClasses);
                }
            }

            return new AggregationClassAssignmentPerLevel(topAssignments, leafAssignments);
        }

        private static AggregationClassAssignment[] MakeRowForLevel(
            bool table,
            string className,
            AggregationCodegenRowDetailDesc detail,
            Type forgeClass,
            Consumer<AggregationRowCtorDesc> rowCtorConsumer,
            CodegenClassScope classScope,
            IList<CodegenInnerClass> innerClasses)
        {
            // determine column to inner-class assignment to prevent too-many-fields per class
            var currentAssignment = new AggregationClassAssignment(0, forgeClass, classScope);
            var countStates = 0;
            var countVcols = 0;
            var indexAssignment = 0;
            var assignments = new LinkedHashMap<int, AggregationClassAssignment>();
            var slotToAssignment = new Dictionary<int, AggregationClassAssignment>();
            int maxMembersPerClass = classScope.NamespaceScope.Config.InternalUseOnlyMaxMembersPerClass;
            var methodFactoryCount = 0;

            // determine number of fields and field-to-class assignment
            if (detail.StateDesc.MethodFactories != null) {
                methodFactoryCount = detail.StateDesc.MethodFactories.Length;
                for (var methodIndex = 0; methodIndex < detail.StateDesc.MethodFactories.Length; methodIndex++) {
                    if (currentAssignment.MemberSize != 0 && currentAssignment.MemberSize >= maxMembersPerClass) {
                        assignments.Put(indexAssignment++, currentAssignment);
                        currentAssignment = new AggregationClassAssignment(countStates, forgeClass, classScope);
                    }

                    var factory = detail.StateDesc.MethodFactories[methodIndex];
                    currentAssignment.Add(
                        factory,
                        detail.StateDesc.OptionalMethodForges == null
                            ? Array.Empty<ExprForge>()
                            : detail.StateDesc.OptionalMethodForges[methodIndex]);
                    currentAssignment.AddMethod(new AggregationVColMethod(countVcols, factory));
                    factory.Aggregator.InitForge(
                        countStates,
                        currentAssignment.Ctor,
                        currentAssignment.Members,
                        classScope);
                    countStates++;
                    countVcols++;
                }
            }

            if (detail.StateDesc.AccessStateForges != null) {
                for (var accessIndex = 0; accessIndex < detail.StateDesc.AccessStateForges.Length; accessIndex++) {
                    if (currentAssignment.MemberSize != 0 && currentAssignment.MemberSize >= maxMembersPerClass) {
                        assignments.Put(indexAssignment++, currentAssignment);
                        currentAssignment = new AggregationClassAssignment(countStates, forgeClass, classScope);
                    }

                    var factory = detail.StateDesc.AccessStateForges[accessIndex];
                    currentAssignment.Add(factory);
                    factory.Aggregator.InitAccessForge(
                        countStates,
                        currentAssignment.Ctor,
                        currentAssignment.Members,
                        classScope);
                    countStates++;
                    slotToAssignment.Put(accessIndex, currentAssignment);
                }

                // loop through accessors last
                for (var i = 0; i < detail.AccessAccessors.Length; i++) {
                    var slotPair = detail.AccessAccessors[i];
                    var slot = slotPair.Slot;
                    var assignment = slotToAssignment.Get(slot);
                    var stateNumber = methodFactoryCount + slot;
                    assignment.AddAccess(
                        new AggregationVColAccess(
                            countVcols,
                            slotPair.AccessorForge,
                            stateNumber,
                            detail.StateDesc.AccessStateForges[slot]));
                    countVcols++;
                }
            }

            // handle the simpler case of flat-row
            if (assignments.IsEmpty()) {
                currentAssignment.ClassName = className;
                var innerClass = MakeRowForLevelFlat(
                    table,
                    false,
                    currentAssignment,
                    rowCtorConsumer,
                    classScope);
                innerClass.AddInterfaceImplemented(typeof(AggregationRow));
                innerClasses.Add(innerClass);
                return new AggregationClassAssignment[] { currentAssignment };
            }

            // add current
            assignments.Put(indexAssignment, currentAssignment);

            // make leaf-row classes, assign class name and member name
            var assignmentArray = new AggregationClassAssignment[assignments.Count];
            var index = 0;
            foreach (var entry in assignments) {
                var assignment = entry.Value;
                assignment.ClassName = className + "_" + entry.Key;
                assignment.MemberName = "l" + entry.Key;
                var innerClass = MakeRowForLevelFlat(table, true, assignment, ctor => { }, classScope);
                innerClasses.Add(innerClass);
                assignmentArray[index++] = entry.Value;
            }

            var composite = ProduceComposite(
                detail,
                assignmentArray,
                table,
                forgeClass,
                className,
                rowCtorConsumer,
                classScope);
            innerClasses.Add(composite);
            return assignmentArray;
        }

        private static CodegenInnerClass ProduceComposite(
            AggregationCodegenRowDetailDesc detail,
            AggregationClassAssignment[] leafs,
            bool table,
            Type forgeClass,
            string className,
            Consumer<AggregationRowCtorDesc> rowCtorConsumer,
            CodegenClassScope classScope)
        {
            // fill enter and leave
            var applyEnterMethod = MakeMethodReturnVoid(
                AggregationCodegenUpdateType.APPLYENTER.GetParams(),
                classScope);
            var applyLeaveMethod = MakeMethodReturnVoid(
                AggregationCodegenUpdateType.APPLYLEAVE.GetParams(),
                classScope);
            if (!table) {
                foreach (var leaf in leafs) {
                    applyEnterMethod.Block.ExprDotMethod(
                        Ref(leaf.MemberName),
                        "ApplyEnter",
                        REF_EPS,
                        REF_EXPREVALCONTEXT);
                    applyLeaveMethod.Block.ExprDotMethod(
                        Ref(leaf.MemberName),
                        "ApplyLeave",
                        REF_EPS,
                        REF_EXPREVALCONTEXT);
                }
            }

            var clearMethod = MakeMethodReturnVoid(AggregationCodegenUpdateType.CLEAR.GetParams(), classScope);
            foreach (var leaf in leafs) {
                clearMethod.Block.ExprDotMethod(Ref(leaf.MemberName), "Clear");
            }

            // get-access-state
            var getAccessStateMethod = MakeMethodGetAccess(classScope);
            PopulateSwitchByRange(
                leafs,
                getAccessStateMethod,
                true,
                (
                    index,
                    block) => block.BlockReturn(
                    ExprDotMethod(Ref(leafs[index].MemberName), "GetAccessState", REF_SCOL)));

            // make state-update for tables
            var enterAggMethod = MakeMethodTableEnterLeave(classScope);
            var leaveAggMethod = MakeMethodTableEnterLeave(classScope);
            var resetAggMethod = MakeMethodTableReset(classScope);
            var enterAccessMethod = MakeMethodTableAccess(classScope);
            var leaveAccessMethod = MakeMethodTableAccess(classScope);
            if (table) {
                PopulateSwitchByRange(
                    leafs,
                    enterAggMethod,
                    false,
                    (
                        index,
                        block) => block.ExprDotMethod(Ref(leafs[index].MemberName), "EnterAgg", REF_SCOL, REF_VALUE));
                PopulateSwitchByRange(
                    leafs,
                    leaveAggMethod,
                    false,
                    (
                        index,
                        block) => block.ExprDotMethod(Ref(leafs[index].MemberName), "LeaveAgg", REF_SCOL, REF_VALUE));
                PopulateSwitchByRange(
                    leafs,
                    resetAggMethod,
                    false,
                    (
                        index,
                        block) => block.ExprDotMethod(Ref(leafs[index].MemberName), "Reset", REF_SCOL));
                PopulateSwitchByRange(
                    leafs,
                    enterAccessMethod,
                    false,
                    (
                        index,
                        block) => block.ExprDotMethod(
                        Ref(leafs[index].MemberName),
                        "EnterAccess",
                        REF_SCOL,
                        REF_EPS,
                        REF_EXPREVALCONTEXT));
                PopulateSwitchByRange(
                    leafs,
                    leaveAccessMethod,
                    false,
                    (
                        index,
                        block) => block.ExprDotMethod(
                        Ref(leafs[index].MemberName),
                        "LeaveAccess",
                        REF_SCOL,
                        REF_EPS,
                        REF_EXPREVALCONTEXT));
            }

            // make getters
            var vcolIndexes = GetVcolIndexes(detail, leafs);
            var getValueMethod = MakeMethodGet(AggregationCodegenGetType.GETVALUE, classScope);
            PopulateSwitchByIndex(
                leafs,
                getValueMethod,
                true,
                (
                    index,
                    block) => block.BlockReturn(
                    ExprDotMethod(
                        Ref(leafs[index].MemberName),
                        "GetValue",
                        REF_VCOL,
                        REF_EPS,
                        REF_ISNEWDATA,
                        REF_EXPREVALCONTEXT)));
            var getEventBeanMethod = MakeMethodGet(AggregationCodegenGetType.GETEVENTBEAN, classScope);
            PopulateSwitchByIndex(
                leafs,
                getEventBeanMethod,
                true,
                (
                    index,
                    block) => block.BlockReturn(
                    ExprDotMethod(
                        Ref(leafs[index].MemberName),
                        "GetEventBean",
                        REF_VCOL,
                        REF_EPS,
                        REF_ISNEWDATA,
                        REF_EXPREVALCONTEXT)));
            var getCollectionScalarMethod = MakeMethodGet(
                AggregationCodegenGetType.GETCOLLECTIONSCALAR,
                classScope);
            PopulateSwitchByIndex(
                leafs,
                getCollectionScalarMethod,
                true,
                (
                    index,
                    block) => block
                    .CommentFullLine(MethodBase.GetCurrentMethod()!.DeclaringType!.FullName + "." + MethodBase.GetCurrentMethod()!.Name)
                    .BlockReturn(
                        ExprDotMethod(
                            Ref(leafs[index].MemberName),
                            "GetCollectionScalar",
                            REF_VCOL,
                            REF_EPS,
                            REF_ISNEWDATA,
                            REF_EXPREVALCONTEXT)));
            var getCollectionOfEventsMethod = MakeMethodGet(
                AggregationCodegenGetType.GETCOLLECTIONOFEVENTS,
                classScope);
            PopulateSwitchByIndex(
                leafs,
                getCollectionOfEventsMethod,
                true,
                (
                    index,
                    block) => block.BlockReturn(
                    ExprDotMethod(
                        Ref(leafs[index].MemberName),
                        "GetCollectionOfEvents",
                        REF_VCOL,
                        REF_EPS,
                        REF_ISNEWDATA,
                        REF_EXPREVALCONTEXT)));

            var properties = new CodegenClassProperties();
            var methods = new CodegenClassMethods();
            CodegenStackGenerator.RecursiveBuildStack(applyEnterMethod, "ApplyEnter", methods, properties);
            CodegenStackGenerator.RecursiveBuildStack(applyLeaveMethod, "ApplyLeave", methods, properties);
            CodegenStackGenerator.RecursiveBuildStack(clearMethod, "Clear", methods, properties);
            CodegenStackGenerator.RecursiveBuildStack(enterAggMethod, "EnterAgg", methods, properties);
            CodegenStackGenerator.RecursiveBuildStack(leaveAggMethod, "LeaveAgg", methods, properties);
            CodegenStackGenerator.RecursiveBuildStack(resetAggMethod, "Reset", methods, properties);
            CodegenStackGenerator.RecursiveBuildStack(enterAccessMethod, "EnterAccess", methods, properties);
            CodegenStackGenerator.RecursiveBuildStack(leaveAccessMethod, "LeaveAccess", methods, properties);
            CodegenStackGenerator.RecursiveBuildStack(getAccessStateMethod, "GetAccessState", methods, properties);
            CodegenStackGenerator.RecursiveBuildStack(getValueMethod, "GetValue", methods, properties);
            CodegenStackGenerator.RecursiveBuildStack(getEventBeanMethod, "GetEventBean", methods, properties);
            CodegenStackGenerator.RecursiveBuildStack(getCollectionScalarMethod, "GetCollectionScalar", methods, properties);
            CodegenStackGenerator.RecursiveBuildStack(getCollectionOfEventsMethod, "GetCollectionOfEvents", methods, properties);

            // make ctor
            var ctor = new CodegenCtor(forgeClass, classScope, EmptyList<CodegenTypedParam>.Instance);
            foreach (var leaf in leafs) {
                ctor.Block.AssignRef(leaf.MemberName, NewInstanceInner(leaf.ClassName));
            }

            // make members
            IList<CodegenTypedParam> members = new List<CodegenTypedParam>();
            var assignment = new CodegenTypedParam(typeof(int[]), NAME_ASSIGNMENT)
                .WithFinal(true)
                .WithStatic(true)
                .WithInitializer(Constant(vcolIndexes));
            members.Add(assignment);

            // named methods
            var namedMethods = new CodegenNamedMethods();
            rowCtorConsumer.Invoke(new AggregationRowCtorDesc(classScope, ctor, members, namedMethods));

            // add named methods from aggregation desc
            foreach (var methodEntry in namedMethods.Methods) {
                CodegenStackGenerator.RecursiveBuildStack(methodEntry.Value, methodEntry.Key, methods, properties);
            }

            // add row members
            foreach (var leaf in leafs) {
                members.Add(new CodegenTypedParam(leaf.ClassName, leaf.MemberName));
            }

            // add composite class
            return new CodegenInnerClass(className, typeof(AggregationRow), ctor, members, methods, properties);
        }

        private static int[] GetVcolIndexes(
            AggregationCodegenRowDetailDesc detail,
            AggregationClassAssignment[] leafs)
        {
            var size = detail.StateDesc.MethodFactories?.Length ?? 0;
            size += detail.AccessAccessors?.Length ?? 0;
            var vcols = new int[size];
            for (var i = 0; i < leafs.Length; i++) {
                foreach (var method in leafs[i].VcolMethods) {
                    vcols[method.Vcol] = i;
                }

                foreach (var access in leafs[i].VcolAccess) {
                    vcols[access.Vcol] = i;
                }
            }

            return vcols;
        }

        private static void PopulateSwitchByRange(
            AggregationClassAssignment[] leafs,
            CodegenMethod method,
            bool blocksReturnValue,
            BiConsumer<int, CodegenBlock> blockConsumer)
        {
            method.Block.DeclareVar<int>("i", Constant(0));
            CodegenBlock ifBlock = null;
            for (var i = leafs.Length - 1; i > 0; i--) {
                var rangeCheck = Relational(
                    REF_SCOL,
                    CodegenExpressionRelational.CodegenRelational.GE,
                    Constant(leafs[i].Offset));
                if (ifBlock == null) {
                    ifBlock = method.Block.IfCondition(rangeCheck).AssignRef(Ref("i"), Constant(i));
                }
                else {
                    ifBlock.IfElseIf(rangeCheck).AssignRef(Ref("i"), Constant(i));
                }
            }

            var blocks = method.Block.SwitchBlockOfLength(Ref("i"), leafs.Length, blocksReturnValue);
            for (var i = 0; i < leafs.Length; i++) {
                blockConsumer.Invoke(i, blocks[i]);
            }
        }

        private static void PopulateSwitchByIndex(
            AggregationClassAssignment[] leafs,
            CodegenMethod method,
            bool blocksReturnValue,
            BiConsumer<int, CodegenBlock> blockConsumer)
        {
            method.Block.DeclareVar<int>("i", ArrayAtIndex(Ref(NAME_ASSIGNMENT), Ref(NAME_VCOL)));
            var blocks = method.Block.SwitchBlockOfLength(Ref("i"), leafs.Length, blocksReturnValue);
            for (var i = 0; i < leafs.Length; i++) {
                blockConsumer.Invoke(i, blocks[i]);
            }
        }

        private static CodegenMethod MakeMethodReturnVoid(
            IList<CodegenNamedParam> @params,
            CodegenClassScope classScope)
        {
            return MakeParentNode(typeof(void), typeof(AggregationServiceFactoryCompilerRow), classScope)
                .AddParam(@params);
        }

        private static CodegenInnerClass MakeRowForLevelFlat(
            bool table,
            bool standalone,
            AggregationClassAssignment assignment,
            Consumer<AggregationRowCtorDesc> rowCtorConsumer,
            CodegenClassScope classScope)
        {
            var numMethodFactories = assignment.MethodFactories?.Length ?? 0;
            var offset = assignment.Offset;

            // make member+ctor
            var namedMethods = new CodegenNamedMethods();
            
            // row members
            var rowMembers = new List<CodegenTypedParam>();

            foreach (var entry in assignment.Members.Members) {
                rowMembers.Add(new CodegenTypedParam(entry.Value, entry.Key.Ref, false, true));
            }

            rowCtorConsumer.Invoke(new AggregationRowCtorDesc(classScope, assignment.Ctor, rowMembers, namedMethods));

            // make state-update
            var applyEnterMethod = ProduceStateUpdate(
                !table,
                AggregationCodegenUpdateType.APPLYENTER,
                assignment,
                classScope,
                namedMethods);
            var applyLeaveMethod = ProduceStateUpdate(
                !table,
                AggregationCodegenUpdateType.APPLYLEAVE,
                assignment,
                classScope,
                namedMethods);
            var clearMethod = ProduceStateUpdate(
                true,
                AggregationCodegenUpdateType.CLEAR,
                assignment,
                classScope,
                namedMethods);

            // get-access-state
            var getAccessStateMethod = ProduceGetAccessState(
                assignment.Offset + numMethodFactories,
                assignment.AccessStateFactories,
                classScope);

            // make state-update for tables
            var enterAggMethod = ProduceTableMethod(
                offset,
                table,
                AggregationCodegenTableUpdateType.ENTER,
                assignment.MethodFactories,
                classScope);
            var leaveAggMethod = ProduceTableMethod(
                offset,
                table,
                AggregationCodegenTableUpdateType.LEAVE,
                assignment.MethodFactories,
                classScope);
            var resetAggMethod = ProduceTableResetMethod(
                offset,
                table,
                assignment.MethodFactories,
                assignment.AccessStateFactories,
                classScope);
            var enterAccessMethod = ProduceTableAccess(
                offset + numMethodFactories,
                table,
                AggregationCodegenTableUpdateType.ENTER,
                assignment.AccessStateFactories,
                classScope,
                namedMethods);
            var leaveAccessMethod = ProduceTableAccess(
                offset + numMethodFactories,
                table,
                AggregationCodegenTableUpdateType.LEAVE,
                assignment.AccessStateFactories,
                classScope,
                namedMethods);

            // make getters
            var getValueMethod = ProduceGet(
                AggregationCodegenGetType.GETVALUE,
                assignment,
                classScope,
                namedMethods);
            var getEventBeanMethod = ProduceGet(
                AggregationCodegenGetType.GETEVENTBEAN,
                assignment,
                classScope,
                namedMethods);
            var getCollectionScalarMethod = ProduceGet(
                AggregationCodegenGetType.GETCOLLECTIONSCALAR,
                assignment,
                classScope,
                namedMethods);
            var getCollectionOfEventsMethod = ProduceGet(
                AggregationCodegenGetType.GETCOLLECTIONOFEVENTS,
                assignment,
                classScope,
                namedMethods);

            var innerProperties = new CodegenClassProperties();
            var innerMethods = new CodegenClassMethods();
            if (!standalone || !table) {
                CodegenStackGenerator.RecursiveBuildStack(applyEnterMethod, "ApplyEnter", innerMethods, innerProperties);
                CodegenStackGenerator.RecursiveBuildStack(applyLeaveMethod, "ApplyLeave", innerMethods, innerProperties);
            }

            CodegenStackGenerator.RecursiveBuildStack(clearMethod, "Clear", innerMethods, innerProperties);
            if (table || !standalone) {
                CodegenStackGenerator.RecursiveBuildStack(enterAggMethod, "EnterAgg", innerMethods, innerProperties);
                CodegenStackGenerator.RecursiveBuildStack(leaveAggMethod, "LeaveAgg", innerMethods, innerProperties);
                CodegenStackGenerator.RecursiveBuildStack(resetAggMethod, "Reset", innerMethods, innerProperties);
                CodegenStackGenerator.RecursiveBuildStack(enterAccessMethod, "EnterAccess", innerMethods, innerProperties);
                CodegenStackGenerator.RecursiveBuildStack(leaveAccessMethod, "LeaveAccess", innerMethods, innerProperties);
            }

            CodegenStackGenerator.RecursiveBuildStack(getAccessStateMethod, "GetAccessState", innerMethods, innerProperties);
            CodegenStackGenerator.RecursiveBuildStack(getValueMethod, "GetValue", innerMethods, innerProperties);
            CodegenStackGenerator.RecursiveBuildStack(getEventBeanMethod, "GetEventBean", innerMethods, innerProperties);
            CodegenStackGenerator.RecursiveBuildStack(getCollectionScalarMethod, "GetCollectionScalar", innerMethods, innerProperties);
            CodegenStackGenerator.RecursiveBuildStack(
                getCollectionOfEventsMethod,
                "GetCollectionOfEvents",
                innerMethods,
                innerProperties);
            foreach (var methodEntry in namedMethods.Methods) {
                CodegenStackGenerator.RecursiveBuildStack(methodEntry.Value, methodEntry.Key, innerMethods, innerProperties);
            }

            return new CodegenInnerClass(assignment.ClassName, null, assignment.Ctor, rowMembers, innerMethods, innerProperties);
        }

        private static CodegenMethod ProduceTableMethod(
            int offset,
            bool isGenerateTableEnter,
            AggregationCodegenTableUpdateType type,
            AggregationForgeFactory[] methodFactories,
            CodegenClassScope classScope)
        {
            var method = MakeMethodTableEnterLeave(classScope);
            if (!isGenerateTableEnter) {
                method.Block.MethodThrowUnsupported();
                return method;
            }

            var blocks = method.Block.SwitchBlockOfLength(REF_SCOL, methodFactories.Length, true, offset);
            for (var i = 0; i < methodFactories.Length; i++) {
                var factory = methodFactories[i];
                var evaluationTypes =
                    ExprNodeUtilityQuery.GetExprResultTypes(factory.AggregationExpression.PositionalParams);
                var updateMethod = method.MakeChild(typeof(void), factory.Aggregator.GetType(), classScope)
                    .AddParam<object>("value");
                if (type == AggregationCodegenTableUpdateType.ENTER) {
                    factory.Aggregator.ApplyTableEnterCodegen(REF_VALUE, evaluationTypes, updateMethod, classScope);
                }
                else {
                    factory.Aggregator.ApplyTableLeaveCodegen(REF_VALUE, evaluationTypes, updateMethod, classScope);
                }

                blocks[i].LocalMethod(updateMethod, REF_VALUE).BlockReturnNoValue();
            }

            return method;
        }

        private static CodegenMethod MakeMethodTableEnterLeave(CodegenClassScope classScope)
        {
            return MakeParentNode(
                    typeof(void),
                    typeof(AggregationServiceFactoryCompilerRow),
                    CodegenSymbolProviderEmpty.INSTANCE,
                    classScope)
                .AddParam<int>(NAME_SCOL)
                .AddParam<object>(NAME_VALUE);
        }

        private static CodegenMethod ProduceTableResetMethod(
            int offset,
            bool isGenerateTableEnter,
            AggregationForgeFactory[] methodFactories,
            AggregationStateFactoryForge[] accessFactories,
            CodegenClassScope classScope)
        {
            var method = MakeMethodTableReset(classScope);
            if (!isGenerateTableEnter) {
                method.Block.MethodThrowUnsupported();
                return method;
            }

            IList<CodegenMethod> methods = new List<CodegenMethod>();

            if (methodFactories != null) {
                foreach (var factory in methodFactories) {
                    var resetMethod = method.MakeChild(
                        typeof(void),
                        factory.Aggregator.GetType(),
                        classScope);
                    factory.Aggregator.ClearCodegen(resetMethod, classScope);
                    methods.Add(resetMethod);
                }
            }

            if (accessFactories != null) {
                foreach (var accessFactory in accessFactories) {
                    var resetMethod = method.MakeChild(
                        typeof(void),
                        accessFactory.Aggregator.GetType(),
                        classScope);
                    accessFactory.Aggregator.ClearCodegen(resetMethod, classScope);
                    methods.Add(resetMethod);
                }
            }

            var blocks = method.Block.SwitchBlockOfLength(REF_SCOL, methods.Count, false, offset);
            var count = 0;
            foreach (var getValue in methods) {
                blocks[count++].Expression(LocalMethod(getValue));
            }

            return method;
        }

        private static CodegenMethod MakeMethodTableReset(CodegenClassScope classScope)
        {
            return MakeParentNode(
                    typeof(void),
                    typeof(AggregationServiceFactoryCompilerRow),
                    CodegenSymbolProviderEmpty.INSTANCE,
                    classScope)
                .AddParam<int>(NAME_SCOL);
        }

        private static CodegenMethod ProduceTableAccess(
            int offset,
            bool isGenerateTableEnter,
            AggregationCodegenTableUpdateType type,
            AggregationStateFactoryForge[] accessStateFactories,
            CodegenClassScope classScope,
            CodegenNamedMethods namedMethods)
        {
            var method = MakeMethodTableAccess(classScope);
            if (!isGenerateTableEnter) {
                method.Block.MethodThrowUnsupported();
                return method;
            }

            var colums = new int[accessStateFactories.Length];
            for (var i = 0; i < accessStateFactories.Length; i++) {
                colums[i] = offset + i;
            }

            var blocks = method.Block.SwitchBlockOptions(REF_SCOL, colums, true);
            for (var i = 0; i < accessStateFactories.Length; i++) {
                var stateFactoryForge = accessStateFactories[i];
                var aggregator = stateFactoryForge.Aggregator;

                var symbols = new ExprForgeCodegenSymbol(false, null);
                var updateMethod = method
                    .MakeChildWithScope(typeof(void), stateFactoryForge.GetType(), symbols, classScope)
                    .AddParam(PARAMS);
                if (type == AggregationCodegenTableUpdateType.ENTER) {
                    aggregator.ApplyEnterCodegen(updateMethod, symbols, classScope, namedMethods);
                    blocks[i].LocalMethod(updateMethod, REF_EPS, ConstantTrue(), REF_EXPREVALCONTEXT);
                }
                else {
                    aggregator.ApplyLeaveCodegen(updateMethod, symbols, classScope, namedMethods);
                    blocks[i].LocalMethod(updateMethod, REF_EPS, ConstantFalse(), REF_EXPREVALCONTEXT);
                }

                blocks[i].BlockReturnNoValue();
            }

            return method;
        }

        private static CodegenMethod MakeMethodTableAccess(CodegenClassScope classScope)
        {
            return MakeParentNode(
                    typeof(void),
                    typeof(AggregationServiceFactoryCompilerRow),
                    CodegenSymbolProviderEmpty.INSTANCE,
                    classScope)
                .AddParam<int>(NAME_SCOL)
                .AddParam<EventBean[]>(NAME_EPS)
                .AddParam<ExprEvaluatorContext>(NAME_EXPREVALCONTEXT);
        }

        private static CodegenMethod ProduceGetAccessState(
            int offset,
            AggregationStateFactoryForge[] accessStateFactories,
            CodegenClassScope classScope)
        {
            var method = MakeMethodGetAccess(classScope);

            var colums = new int[accessStateFactories?.Length ?? 0];
            for (var i = 0; i < colums.Length; i++) {
                colums[i] = offset + i;
            }

            var blocks = method.Block.SwitchBlockOptions(REF_SCOL, colums, true);
            for (var i = 0; i < colums.Length; i++) {
                var stateFactoryForge = accessStateFactories[i];
                var expr = stateFactoryForge.CodegenGetAccessTableState(i + offset, method, classScope);
                blocks[i].BlockReturn(expr);
            }

            return method;
        }

        private static CodegenMethod MakeMethodGetAccess(CodegenClassScope classScope)
        {
            return MakeParentNode(
                    typeof(object),
                    typeof(AggregationServiceFactoryCompilerRow),
                    CodegenSymbolProviderEmpty.INSTANCE,
                    classScope)
                .AddParam<int>(NAME_SCOL);
        }

        private static CodegenMethod ProduceGet(
            AggregationCodegenGetType getType,
            AggregationClassAssignment assignment,
            CodegenClassScope classScope,
            CodegenNamedMethods namedMethods)
        {
            var parent = MakeMethodGet(getType, classScope);

            // for non-get-value we can simply return null if this has no access aggs
            if (getType != AggregationCodegenGetType.GETVALUE &&
                (assignment.AccessStateFactories == null || assignment.AccessStateFactories.Length == 0)) {
                parent.Block.MethodReturn(ConstantNull());
                return parent;
            }

            IList<CodegenMethod> methods = new List<CodegenMethod>();
            IList<int> vcols = new List<int>(8);
            foreach (var vcolMethod in assignment.VcolMethods) {
                var method = parent.MakeChild(getType.GetReturnType(), vcolMethod.Forge.GetType(), classScope)
                    .AddParam(
                        CodegenNamedParam.From(
                            typeof(EventBean[]),
                            NAME_EPS,
                            typeof(bool),
                            NAME_ISNEWDATA,
                            typeof(ExprEvaluatorContext),
                            NAME_EXPREVALCONTEXT));
                methods.Add(method);

                if (getType == AggregationCodegenGetType.GETVALUE) {
                    vcolMethod.Forge.Aggregator.GetValueCodegen(method, classScope);
                }
                else {
                    method.Block.MethodReturn(ConstantNull()); // method aggs don't do others
                }

                vcols.Add(vcolMethod.Vcol);
            }

            foreach (var vcolAccess in assignment.VcolAccess) {
                var method = parent
                    .MakeChild(getType.GetReturnType(), vcolAccess.AccessorForge.GetType(), classScope)
                    .AddParam(
                        CodegenNamedParam.From(
                            typeof(EventBean[]),
                            NAME_EPS,
                            typeof(bool),
                            NAME_ISNEWDATA,
                            typeof(ExprEvaluatorContext),
                            NAME_EXPREVALCONTEXT));

                var ctx = new AggregationAccessorForgeGetCodegenContext(
                    vcolAccess.StateNumber,
                    classScope,
                    vcolAccess.StateForge,
                    method,
                    namedMethods);
                switch (getType) {
                    case AggregationCodegenGetType.GETVALUE:
                        vcolAccess.AccessorForge.GetValueCodegen(ctx);
                        break;

                    case AggregationCodegenGetType.GETEVENTBEAN:
                        vcolAccess.AccessorForge.GetEnumerableEventCodegen(ctx);
                        break;

                    case AggregationCodegenGetType.GETCOLLECTIONSCALAR:
                        vcolAccess.AccessorForge.GetEnumerableScalarCodegen(ctx);
                        break;

                    case AggregationCodegenGetType.GETCOLLECTIONOFEVENTS:
                        vcolAccess.AccessorForge.GetEnumerableEventsCodegen(ctx);
                        break;
                }

                methods.Add(method);
                vcols.Add(vcolAccess.Vcol);
            }

            var options = vcols.ToArray();
            var blocks = parent.Block.SwitchBlockOptions(REF_VCOL, options, true);
            var count = 0;
            foreach (var getValue in methods) {
                blocks[count++].BlockReturn(LocalMethod(getValue, REF_EPS, REF_ISNEWDATA, REF_EXPREVALCONTEXT));
            }

            return parent;
        }

        private static CodegenMethod MakeMethodGet(
            AggregationCodegenGetType getType,
            CodegenClassScope classScope)
        {
            return MakeParentNode(
                    getType.GetReturnType(),
                    typeof(AggregationServiceFactoryCompilerRow),
                    CodegenSymbolProviderEmpty.INSTANCE,
                    classScope)
                .AddParam(GETPARAMS);
        }

        private static CodegenMethod ProduceStateUpdate(
            bool isGenerate,
            AggregationCodegenUpdateType updateType,
            AggregationClassAssignment assignment,
            CodegenClassScope classScope,
            CodegenNamedMethods namedMethods)
        {
            var symbols = new ExprForgeCodegenSymbol(
                true,
                updateType == AggregationCodegenUpdateType.APPLYENTER);
            var parent = MakeParentNode(
                    typeof(void),
                    typeof(AggregationServiceFactoryCompilerRow),
                    symbols,
                    classScope)
                .AddParam(updateType.GetParams());

            var count = 0;
            IList<CodegenMethod> methods = new List<CodegenMethod>();

            if (assignment.MethodFactories != null && isGenerate) {
                foreach (var factory in assignment.MethodFactories) {
                    string exprText = null;
                    CodegenExpression getValue = null;
                    if (classScope.IsInstrumented) {
                        exprText = ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(
                            factory.AggregationExpression);
                        getValue = ExprDotMethod(
                            Ref("this"),
                            "GetValue",
                            Constant(count),
                            ConstantNull(),
                            ConstantTrue(),
                            ConstantNull());
                    }

                    var method = parent.MakeChild(typeof(void), factory.GetType(), classScope);
                    methods.Add(method);
                    switch (updateType) {
                        case AggregationCodegenUpdateType.APPLYENTER:
                            method.Block.Apply(
                                Instblock(
                                    classScope,
                                    "qAggNoAccessEnterLeave",
                                    ConstantTrue(),
                                    Constant(count),
                                    getValue,
                                    Constant(exprText)));
                            factory.Aggregator.ApplyEvalEnterCodegen(
                                method,
                                symbols,
                                assignment.MethodForges[count],
                                classScope);
                            method.Block.Apply(
                                Instblock(
                                    classScope,
                                    "aAggNoAccessEnterLeave",
                                    ConstantTrue(),
                                    Constant(count),
                                    getValue));
                            break;

                        case AggregationCodegenUpdateType.APPLYLEAVE:
                            method.Block.Apply(
                                Instblock(
                                    classScope,
                                    "qAggNoAccessEnterLeave",
                                    ConstantFalse(),
                                    Constant(count),
                                    getValue,
                                    Constant(exprText)));
                            factory.Aggregator.ApplyEvalLeaveCodegen(
                                method,
                                symbols,
                                assignment.MethodForges[count],
                                classScope);
                            method.Block.Apply(
                                Instblock(
                                    classScope,
                                    "aAggNoAccessEnterLeave",
                                    ConstantFalse(),
                                    Constant(count),
                                    getValue));
                            break;

                        case AggregationCodegenUpdateType.CLEAR:
                            factory.Aggregator.ClearCodegen(method, classScope);
                            break;
                    }

                    count++;
                }
            }

            if (assignment.AccessStateFactories != null && isGenerate) {
                foreach (var factory in assignment.AccessStateFactories) {
                    string exprText = null;
                    if (classScope.IsInstrumented) {
                        exprText = ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(factory.Expression);
                    }

                    var method = parent.MakeChild(typeof(void), factory.GetType(), classScope);
                    methods.Add(method);
                    switch (updateType) {
                        case AggregationCodegenUpdateType.APPLYENTER:
                            method.Block.Apply(
                                Instblock(
                                    classScope,
                                    "qAggAccessEnterLeave",
                                    ConstantTrue(),
                                    Constant(count),
                                    Constant(exprText)));
                            factory.Aggregator.ApplyEnterCodegen(method, symbols, classScope, namedMethods);
                            method.Block.Apply(
                                Instblock(classScope, "aAggAccessEnterLeave", ConstantTrue(), Constant(count)));
                            break;

                        case AggregationCodegenUpdateType.APPLYLEAVE:
                            method.Block.Apply(
                                Instblock(
                                    classScope,
                                    "qAggAccessEnterLeave",
                                    ConstantFalse(),
                                    Constant(count),
                                    Constant(exprText)));
                            factory.Aggregator.ApplyLeaveCodegen(method, symbols, classScope, namedMethods);
                            method.Block.Apply(
                                Instblock(classScope, "aAggAccessEnterLeave", ConstantFalse(), Constant(count)));
                            break;

                        case AggregationCodegenUpdateType.CLEAR:
                            factory.Aggregator.ClearCodegen(method, classScope);
                            break;
                    }

                    count++;
                }
            }

            // code for enter
            symbols.DerivedSymbolsCodegen(parent, parent.Block, classScope);
            foreach (var method in methods) {
                parent.Block.LocalMethod(method);
            }

            return parent;
        }
    }
} // end of namespace