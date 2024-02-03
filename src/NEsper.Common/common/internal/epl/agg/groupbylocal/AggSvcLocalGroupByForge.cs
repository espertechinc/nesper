///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client.annotation;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.compile.multikey;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.fabric;
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionRelational.
    CodegenRelational;
using static com.espertech.esper.common.@internal.epl.agg.core.AggregationServiceCodegenNames;
using static com.espertech.esper.common.@internal.epl.expression.codegen.ExprForgeCodegenNames;
using static com.espertech.esper.common.@internal.epl.util.EPTypeCollectionConst;
using static com.espertech.esper.common.@internal.metrics.instrumentation.InstrumentationCode;

namespace com.espertech.esper.common.@internal.epl.agg.groupbylocal
{
    public class AggSvcLocalGroupByForge : AggregationServiceFactoryForgeWMethodGen
    {
        private static readonly CodegenExpressionMember MEMBER_CURRENTROW = Member("currentRow");
        private static readonly CodegenExpressionMember MEMBER_AGGREGATORSTOPLEVEL = Member("aggregatorsTopLevel");
        private static readonly CodegenExpressionMember MEMBER_AGGREGATORSPERLEVELANDGROUP = Member("aggregatorsPerLevelAndGroup");
        private static readonly CodegenExpressionMember MEMBER_REMOVEDKEYS = Member("removedKeys");
        
        internal readonly bool hasGroupBy;
        internal readonly AggregationLocalGroupByPlanForge localGroupByPlan;
        internal readonly AggregationUseFlags useFlags;
        
        private StateMgmtSetting stateMgmtSetting;

        public AggSvcLocalGroupByForge(
            bool hasGroupBy,
            AggregationLocalGroupByPlanForge localGroupByPlan,
            AggregationUseFlags useFlags)
        {
            this.hasGroupBy = hasGroupBy;
            this.localGroupByPlan = localGroupByPlan;
            this.useFlags = useFlags;
        }

        public AppliesTo? AppliesTo()
        {
            return client.annotation.AppliesTo.AGGREGATION_LOCAL;
        }

        public void RowCtorCodegen(AggregationRowCtorDesc rowCtorDesc)
        {
            AggregationServiceCodegenUtil.GenerateIncidentals(true, false, rowCtorDesc);
        }

        public void ProviderCodegen(
            CodegenMethod method,
            CodegenClassScope classScope,
            AggregationClassNames classNames)
        {
            method.Block.DeclareVar<AggregationLocalGroupByLevel>("optionalTop", ConstantNull());
            if (localGroupByPlan.OptionalLevelTopForge != null) {
                method.Block.AssignRef(
                    "optionalTop",
                    localGroupByPlan.OptionalLevelTopForge.ToExpression(
                        classNames.RowFactoryTop,
                        classNames.RowSerdeTop,
                        ConstantNull(),
                        method,
                        classScope));
            }

            var numLevels = localGroupByPlan.AllLevelsForges.Length;
            method.Block.DeclareVar<AggregationLocalGroupByLevel[]>(
                "levels",
                NewArrayByLength(typeof(AggregationLocalGroupByLevel), Constant(numLevels)));
            for (var i = 0; i < numLevels; i++) {
                var forge = localGroupByPlan.AllLevelsForges[i];
                var eval = MultiKeyCodegen.CodegenExprEvaluatorMayMultikey(
                    forge.PartitionForges,
                    null,
                    forge.PartitionMKClasses,
                    method,
                    classScope);
                method.Block.AssignArrayElement(
                    "levels",
                    Constant(i),
                    localGroupByPlan.AllLevelsForges[i]
                        .ToExpression(
                            classNames.GetRowFactoryPerLevel(i),
                            classNames.GetRowSerdePerLevel(i),
                            eval,
                            method,
                            classScope));
            }

            method.Block.DeclareVar<AggregationLocalGroupByColumn[]>(
                "columns",
                NewArrayByLength(
                    typeof(AggregationLocalGroupByColumn),
                    Constant(localGroupByPlan.ColumnsForges.Length)));
            var rowLevelDesc = RowLevelDesc;
            for (var i = 0; i < localGroupByPlan.ColumnsForges.Length; i++) {
                var col = localGroupByPlan.ColumnsForges[i];
                int fieldNum;
                if (hasGroupBy && col.IsDefaultGroupLevel) {
                    var levelDesc = rowLevelDesc.OptionalAdditionalRows[col.LevelNum];
                    fieldNum = GetRowFieldNum(col, levelDesc);
                }
                else if (col.LevelNum == -1) {
                    var levelDesc = rowLevelDesc.OptionalTopRow;
                    fieldNum = GetRowFieldNum(col, levelDesc);
                }
                else {
                    var levelDesc = rowLevelDesc.OptionalAdditionalRows[col.LevelNum];
                    fieldNum = GetRowFieldNum(col, levelDesc);
                }

                method.Block.AssignArrayElement(
                    "columns",
                    Constant(i),
                    localGroupByPlan.ColumnsForges[i].ToExpression(fieldNum));
            }

            method.Block
                .DeclareVar<AggregationServiceFactory>(
                    "svcFactory",
                    NewInstanceInner(classNames.ServiceFactory, Ref("this")))
                .MethodReturn(
                    ExprDotMethodChain(EPStatementInitServicesConstants.REF)
                        .Get(EPStatementInitServicesConstants.AGGREGATIONSERVICEFACTORYSERVICE)
                        .Add(
                            "GroupLocalGroupBy",
                            Ref("svcFactory"),
                            useFlags.ToExpression(),
                            Constant(hasGroupBy),
                            Ref("optionalTop"),
                            Ref("levels"),
                            Ref("columns"),
                            stateMgmtSetting.ToExpression()));
        }

        public void MakeServiceCodegen(
            CodegenMethod method,
            CodegenClassScope classScope,
            AggregationClassNames classNames)
        {
            method.Block.MethodReturn(NewInstanceInner(classNames.Service, Ref("o")));
        }

        public void CtorCodegen(
            CodegenCtor ctor,
            IList<CodegenTypedParam> explicitMembers,
            CodegenClassScope classScope,
            AggregationClassNames classNames)
        {
            explicitMembers.Add(
                new CodegenTypedParam(EPTYPE_MAPARRAY_OBJECT_AGGROW, MEMBER_AGGREGATORSPERLEVELANDGROUP.Ref));
            ctor.Block.AssignRef(
                MEMBER_AGGREGATORSPERLEVELANDGROUP,
                NewArrayByLength(typeof(IDictionary<object, AggregationRow>), Constant(localGroupByPlan.AllLevelsForges.Length)));
            for (var i = 0; i < localGroupByPlan.AllLevelsForges.Length; i++) {
                ctor.Block.AssignArrayElement(
                    MEMBER_AGGREGATORSPERLEVELANDGROUP,
                    Constant(i),
                    NewInstance(typeof(Dictionary<object, AggregationRow>)));
            }

            explicitMembers.Add(
                new CodegenTypedParam(typeof(AggregationRow), MEMBER_AGGREGATORSTOPLEVEL.Ref).WithFinal(false));
            if (hasGroupBy) {
                explicitMembers.Add(new CodegenTypedParam(typeof(AggregationRow), MEMBER_CURRENTROW.Ref).WithFinal(false));
            }

            explicitMembers.Add(new CodegenTypedParam(EPTYPE_LIST_AFFLOCALGROUPPAIR, MEMBER_REMOVEDKEYS.Ref));
            ctor.Block.AssignRef(MEMBER_REMOVEDKEYS, NewInstance(typeof(List<AggSvcLocalGroupLevelKeyPair>)));
        }

        public void GetValueCodegen(
            CodegenMethod method,
            CodegenClassScope classScope,
            CodegenNamedMethods namedMethods)
        {
            GetterCodegen("GetValue", method, classScope, namedMethods);
        }

        public void GetCollectionOfEventsCodegen(
            CodegenMethod method,
            CodegenClassScope classScope,
            CodegenNamedMethods namedMethods)
        {
            GetterCodegen("GetCollectionOfEvents", method, classScope, namedMethods);
        }

        public void GetEventBeanCodegen(
            CodegenMethod method,
            CodegenClassScope classScope,
            CodegenNamedMethods namedMethods)
        {
            GetterCodegen("GetEventBean", method, classScope, namedMethods);
        }

        public void GetCollectionScalarCodegen(
            CodegenMethod method,
            CodegenClassScope classScope,
            CodegenNamedMethods namedMethods)
        {
            GetterCodegen("GetCollectionScalar", method, classScope, namedMethods);
        }

        public void ApplyEnterCodegen(
            CodegenMethod method,
            CodegenClassScope classScope,
            CodegenNamedMethods namedMethods,
            AggregationClassNames classNames)
        {
            method.Block.Apply(
                Instblock(
                    classScope,
                    "qAggregationGroupedApplyEnterLeave",
                    ConstantTrue(),
                    Constant(-1),
                    Constant(-1),
                    REF_GROUPKEY));
            ApplyCodegen(true, method, classScope, namedMethods, classNames);
            method.Block.Apply(Instblock(classScope, "aAggregationGroupedApplyEnterLeave", ConstantTrue()));
        }

        public void ApplyLeaveCodegen(
            CodegenMethod method,
            CodegenClassScope classScope,
            CodegenNamedMethods namedMethods,
            AggregationClassNames classNames)
        {
            method.Block.Apply(
                Instblock(
                    classScope,
                    "qAggregationGroupedApplyEnterLeave",
                    ConstantFalse(),
                    Constant(-1),
                    Constant(-1),
                    REF_GROUPKEY));
            ApplyCodegen(false, method, classScope, namedMethods, classNames);
            method.Block.Apply(Instblock(classScope, "aAggregationGroupedApplyEnterLeave", ConstantFalse()));
        }

        public void StopMethodCodegen(
            AggregationServiceFactoryForgeWMethodGen forge,
            CodegenMethod method)
        {
            // no code required
        }

        public void RowWriteMethodCodegen(
            CodegenMethod method,
            int level)
        {
            if (level != -1) {
                method.Block.ExprDotMethod(Ref("output"), "WriteInt", Ref("row.refcount"));
            }
        }

        public void RowReadMethodCodegen(
            CodegenMethod method,
            int level)
        {
            if (level != -1) {
                method.Block.AssignRef("row.refcount", ExprDotMethod(Ref("input"), "ReadInt"));
            }
        }

        public void AppendRowFabricType(FabricTypeCollector fabricTypeCollector)
        {
            throw new IllegalStateException("Not supported for row-specific grouping");
        }

        public void SetCurrentAccessCodegen(
            CodegenMethod method,
            CodegenClassScope classScope,
            AggregationClassNames classNames)
        {
            if (!hasGroupBy) {
                // not applicable
            }
            else {
                if (localGroupByPlan.AllLevelsForges.Length == 0 ||
                    !localGroupByPlan.AllLevelsForges[0].IsDefaultLevel) {
                    return;
                }

                var indexDefault = -1;
                for (var i = 0; i < localGroupByPlan.AllLevelsForges.Length; i++) {
                    if (localGroupByPlan.AllLevelsForges[i].IsDefaultLevel) {
                        indexDefault = i;
                    }
                }

                method.Block
                    .AssignRef(
                        MEMBER_CURRENTROW,
                        Cast(
                            typeof(AggregationRow),
                            ExprDotMethod(
                                ArrayAtIndex(MEMBER_AGGREGATORSPERLEVELANDGROUP, Constant(0)),
                                "Get",
                                REF_GROUPKEY)))
                    .IfCondition(EqualsNull(MEMBER_CURRENTROW))
                    .AssignRef(
                        MEMBER_CURRENTROW,
                        NewInstanceInner(classNames.GetRowPerLevel(indexDefault), Ref("statementFields")));
            }
        }

        public void ClearResultsCodegen(
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            method.Block.IfCondition(NotEqualsNull(MEMBER_AGGREGATORSTOPLEVEL))
                .ExprDotMethod(MEMBER_AGGREGATORSTOPLEVEL, "Clear");
            for (var i = 0; i < localGroupByPlan.AllLevelsForges.Length; i++) {
                method.Block.ExprDotMethod(ArrayAtIndex(MEMBER_AGGREGATORSPERLEVELANDGROUP, Constant(i)), "Clear");
            }
        }

        public void AcceptCodegen(
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            method.Block.ExprDotMethod(
                REF_AGGVISITOR,
                "VisitAggregations",
                GetNumGroupsCodegen(method, classScope),
                MEMBER_AGGREGATORSTOPLEVEL,
                MEMBER_AGGREGATORSPERLEVELANDGROUP);
        }

        public void GetGroupKeysCodegen(
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            method.Block.MethodThrowUnsupported();
        }

        public void GetGroupKeyCodegen(
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            method.Block.MethodReturn(ConstantNull());
        }

        public void AcceptGroupDetailCodegen(
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            method.Block.ExprDotMethod(REF_AGGVISITOR, "VisitGrouped", GetNumGroupsCodegen(method, classScope))
                .IfCondition(NotEqualsNull(MEMBER_AGGREGATORSTOPLEVEL))
                .ExprDotMethod(REF_AGGVISITOR, "VisitGroup", ConstantNull(), MEMBER_AGGREGATORSTOPLEVEL);
            for (var i = 0; i < localGroupByPlan.AllLevelsForges.Length; i++) {
                method.Block
                    .ForEachVar(
                        "entry",
                        ArrayAtIndex(MEMBER_AGGREGATORSPERLEVELANDGROUP, Constant(i)))
                    .ExprDotMethod(
                        REF_AGGVISITOR,
                        "VisitGroup",
                        ExprDotName(Ref("entry"), "Key"),
                        ExprDotName(Ref("entry"), "Value"));
            }
        }

        public void IsGroupedCodegen(
            CodegenProperty property,
            CodegenClassScope classScope)
        {
            property.GetterBlock.BlockReturn(ConstantTrue());
        }

        public void IsGroupedCodegen(
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            method.Block.MethodReturn(ConstantTrue());
        }

        public void GetRowCodegen(
            CodegenMethod method,
            CodegenClassScope classScope,
            CodegenNamedMethods namedMethods)
        {
            method.Block.MethodThrowUnsupported();
        }

        public T Accept<T>(AggregationServiceFactoryForgeVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        private CodegenExpression GetNumGroupsCodegen(
            CodegenMethodScope parent,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(int), GetType(), classScope);
            method.Block.DeclareVar<int>("size", Constant(0))
                .IfCondition(NotEqualsNull(MEMBER_AGGREGATORSTOPLEVEL))
                .IncrementRef("size")
                .BlockEnd();
            for (var i = 0; i < localGroupByPlan.AllLevelsForges.Length; i++) {
                method.Block.AssignCompound(
                    "size",
                    "+",
                    ExprDotMethod(ArrayAtIndex(MEMBER_AGGREGATORSPERLEVELANDGROUP, Constant(i)), "Count"));
            }

            method.Block.MethodReturn(Ref("size"));
            return LocalMethod(method);
        }

        private void ApplyCodegen(
            bool enter,
            CodegenMethod method,
            CodegenClassScope classScope,
            CodegenNamedMethods namedMethods,
            AggregationClassNames classNames)
        {
            if (enter) {
                method.Block.LocalMethod(HandleRemovedKeysCodegen(method, classScope));
            }

            if (localGroupByPlan.OptionalLevelTopForge != null) {
                method.Block.IfCondition(EqualsNull(MEMBER_AGGREGATORSTOPLEVEL))
                    .AssignRef(MEMBER_AGGREGATORSTOPLEVEL, NewInstanceInner(classNames.RowTop, Ref("statementFields")))
                    .BlockEnd()
                    .ExprDotMethod(
                        MEMBER_AGGREGATORSTOPLEVEL,
                        enter ? "ApplyEnter" : "ApplyLeave",
                        REF_EPS,
                        REF_EXPREVALCONTEXT);
            }

            for (var levelNum = 0; levelNum < localGroupByPlan.AllLevelsForges.Length; levelNum++) {
                var level = localGroupByPlan.AllLevelsForges[levelNum];
                var partitionForges = level.PartitionForges;
                var groupKeyName = "groupKeyLvl_" + levelNum;
                var rowName = "row_" + levelNum;
                CodegenExpression groupKeyExp;
                if (hasGroupBy && level.IsDefaultLevel) {
                    groupKeyExp = REF_GROUPKEY;
                }
                else {
                    groupKeyExp = LocalMethod(
                        AggregationServiceCodegenUtil.ComputeMultiKeyCodegen(
                            levelNum,
                            partitionForges,
                            level.PartitionMKClasses,
                            classScope,
                            namedMethods),
                        REF_EPS,
                        ConstantTrue(),
                        REF_EXPREVALCONTEXT);
                }

                method.Block
                    .DeclareVar<object>(groupKeyName, groupKeyExp)
                    .DeclareVar<
                        AggregationRow>(
                        rowName,
                        Cast(
                            typeof(AggregationRow),
                            ExprDotMethod(
                                ArrayAtIndex(MEMBER_AGGREGATORSPERLEVELANDGROUP, Constant(levelNum)),
                                "Get",
                                Ref(groupKeyName))))
                    .IfCondition(EqualsNull(Ref(rowName)))
                    .AssignRef(rowName, NewInstanceInner(classNames.GetRowPerLevel(levelNum), Ref("statementFields"))) // Ref("o") <== previously
                    .ExprDotMethod(
                        ArrayAtIndex(MEMBER_AGGREGATORSPERLEVELANDGROUP, Constant(levelNum)),
                        "Put",
                        Ref(groupKeyName),
                        Ref(rowName))
                    .BlockEnd()
                    .ExprDotMethod(Ref(rowName), enter ? "IncreaseRefcount" : "DecreaseRefcount")
                    .ExprDotMethod(Ref(rowName), enter ? "ApplyEnter" : "ApplyLeave", REF_EPS, REF_EXPREVALCONTEXT);
                if (!enter) {
                    method.Block.IfCondition(Relational(ExprDotMethod(Ref(rowName), "GetRefcount"), LE, Constant(0)))
                        .ExprDotMethod(
                            MEMBER_REMOVEDKEYS,
                            "Add",
                            NewInstance(typeof(AggSvcLocalGroupLevelKeyPair), Constant(levelNum), Ref(groupKeyName)));
                }
            }
        }

        private AggregationCodegenRowDetailDesc MapDesc(
            bool top,
            int levelNum,
            AggregationLocalGroupByColumnForge[] columns,
            AggregationLocalGroupByLevelForge level)
        {
            IList<AggregationAccessorSlotPairForge> accessAccessors = new List<AggregationAccessorSlotPairForge>(4);
            for (var i = 0; i < columns.Length; i++) {
                var column = columns[i];
                if (column.Pair != null) {
                    if (top && column.IsDefaultGroupLevel) {
                        accessAccessors.Add(column.Pair);
                    }
                    else if (column.LevelNum == levelNum) {
                        accessAccessors.Add(column.Pair);
                    }
                }
            }

            var pairs = accessAccessors.ToArray();
            return new AggregationCodegenRowDetailDesc(
                new AggregationCodegenRowDetailStateDesc(
                    level.MethodForges,
                    level.MethodFactories,
                    level.AccessStateForges),
                pairs,
                level.PartitionMKClasses);
        }

        private int AccessorIndex(
            AggregationAccessorSlotPairForge[] accessAccessors,
            AggregationAccessorSlotPairForge pair)
        {
            for (var i = 0; i < accessAccessors.Length; i++) {
                if (accessAccessors[i] == pair) {
                    return i;
                }
            }

            throw new IllegalStateException();
        }

        private void GetterCodegen(
            string methodName,
            CodegenMethod method,
            CodegenClassScope classScope,
            CodegenNamedMethods namedMethods)
        {
            var rowLevelDesc = RowLevelDesc;
            var blocks = method.Block.SwitchBlockOfLength(REF_VCOL, localGroupByPlan.ColumnsForges.Length, true);
            for (var i = 0; i < blocks.Length; i++) {
                var col = localGroupByPlan.ColumnsForges[i];
                if (hasGroupBy && col.IsDefaultGroupLevel) {
                    var levelDesc = rowLevelDesc.OptionalAdditionalRows[col.LevelNum];
                    var num = GetRowFieldNum(col, levelDesc);
                    blocks[i]
                        .BlockReturn(
                            ExprDotMethod(
                                MEMBER_CURRENTROW,
                                methodName,
                                Constant(num),
                                REF_EPS,
                                REF_ISNEWDATA,
                                REF_EXPREVALCONTEXT));
                }
                else if (col.LevelNum == -1) {
                    var levelDesc = rowLevelDesc.OptionalTopRow;
                    var num = GetRowFieldNum(col, levelDesc);
                    blocks[i]
                        .BlockReturn(
                            ExprDotMethod(
                                MEMBER_AGGREGATORSTOPLEVEL,
                                methodName,
                                Constant(num),
                                REF_EPS,
                                REF_ISNEWDATA,
                                REF_EXPREVALCONTEXT));
                }
                else {
                    var levelDesc = rowLevelDesc.OptionalAdditionalRows[col.LevelNum];
                    var num = GetRowFieldNum(col, levelDesc);
                    blocks[i]
                        .DeclareVar<object>(
                            "groupByKey",
                            LocalMethod(
                                AggregationServiceCodegenUtil.ComputeMultiKeyCodegen(
                                    col.LevelNum,
                                    col.PartitionForges,
                                    levelDesc.MultiKeyClassRef,
                                    classScope,
                                    namedMethods),
                                REF_EPS,
                                REF_ISNEWDATA,
                                REF_EXPREVALCONTEXT))
                        .DeclareVar<AggregationRow>(
                            "row",
                            Cast(
                                typeof(AggregationRow),
                                ExprDotMethod(
                                    ArrayAtIndex(MEMBER_AGGREGATORSPERLEVELANDGROUP, Constant(col.LevelNum)),
                                    "Get",
                                    Ref("groupByKey"))))
                        .BlockReturn(
                            ExprDotMethod(
                                Ref("row"),
                                methodName,
                                Constant(num),
                                REF_EPS,
                                REF_ISNEWDATA,
                                REF_EXPREVALCONTEXT));
                }
            }
        }

        private int GetRowFieldNum(
            AggregationLocalGroupByColumnForge col,
            AggregationCodegenRowDetailDesc levelDesc)
        {
            return col.IsMethodAgg
                ? col.MethodOffset
                : levelDesc.StateDesc.MethodFactories.Length + AccessorIndex(levelDesc.AccessAccessors, col.Pair);
        }

        private CodegenMethod HandleRemovedKeysCodegen(
            CodegenMethod scope,
            CodegenClassScope classScope)
        {
            var method = scope.MakeChild(typeof(void), GetType(), classScope);
            method.Block
                .IfCondition(Not(ExprDotMethod(MEMBER_REMOVEDKEYS, "IsEmpty")))
                .ForEach<AggSvcLocalGroupLevelKeyPair>("removedKey", MEMBER_REMOVEDKEYS)
                .ExprDotMethod(
                    ArrayAtIndex(MEMBER_AGGREGATORSPERLEVELANDGROUP, ExprDotName(Ref("removedKey"), "Level")),
                    "Remove",
                    ExprDotName(Ref("removedKey"), "Key"))
                .BlockEnd()
                .ExprDotMethod(MEMBER_REMOVEDKEYS, "Clear");
            return method;
        }

        public AggregationLocalGroupByPlanForge LocalGroupByPlan => localGroupByPlan;

        public StateMgmtSetting StateMgmtSetting {
            set => stateMgmtSetting = value;
        }

        public AggregationCodegenRowLevelDesc RowLevelDesc {
            get {
                AggregationCodegenRowDetailDesc top = null;
                if (localGroupByPlan.OptionalLevelTopForge != null) {
                    top = MapDesc(true, -1, localGroupByPlan.ColumnsForges, localGroupByPlan.OptionalLevelTopForge);
                }

                AggregationCodegenRowDetailDesc[] additional = null;
                if (localGroupByPlan.AllLevelsForges != null) {
                    additional = new AggregationCodegenRowDetailDesc[localGroupByPlan.AllLevelsForges.Length];
                    for (var i = 0; i < localGroupByPlan.AllLevelsForges.Length; i++) {
                        additional[i] = MapDesc(
                            false,
                            i,
                            localGroupByPlan.ColumnsForges,
                            localGroupByPlan.AllLevelsForges[i]);
                    }
                }

                return new AggregationCodegenRowLevelDesc(top, additional);
            }
        }

        public void SetRemovedCallbackCodegen(CodegenMethod method)
        {
            // not applicable
        }
    }
} // end of namespace