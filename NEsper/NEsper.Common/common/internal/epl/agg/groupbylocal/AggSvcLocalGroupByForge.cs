///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionRelational.CodegenRelational;
using static com.espertech.esper.common.@internal.epl.expression.codegen.ExprForgeCodegenNames;
using static com.espertech.esper.common.@internal.metrics.instrumentation.InstrumentationCode;
using static com.espertech.esper.common.@internal.context.module.EPStatementInitServicesConstants;
using static com.espertech.esper.common.@internal.epl.agg.core.AggregationServiceCodegenNames;

namespace com.espertech.esper.common.@internal.epl.agg.groupbylocal
{
    public class AggSvcLocalGroupByForge : AggregationServiceFactoryForgeWMethodGen
    {
        private static readonly CodegenExpressionRef REF_CURRENTROW =
            new CodegenExpressionRef("currentRow");
        private static readonly CodegenExpressionRef REF_AGGREGATORSTOPLEVEL =
            new CodegenExpressionRef("aggregatorsTopLevel");
        private static readonly CodegenExpressionRef REF_AGGREGATORSPERLEVELANDGROUP =
            new CodegenExpressionRef("aggregatorsPerLevelAndGroup");

        private static readonly CodegenExpressionRef REF_REMOVEDKEYS = Ref("removedKeys");

        internal readonly bool hasGroupBy;
        internal readonly AggregationLocalGroupByPlanForge localGroupByPlan;
        internal readonly AggregationUseFlags useFlags;

        public AggSvcLocalGroupByForge(
            bool hasGroupBy, AggregationLocalGroupByPlanForge localGroupByPlan, AggregationUseFlags useFlags)
        {
            this.hasGroupBy = hasGroupBy;
            this.localGroupByPlan = localGroupByPlan;
            this.useFlags = useFlags;
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
                            false, i, localGroupByPlan.ColumnsForges, localGroupByPlan.AllLevelsForges[i]);
                    }
                }

                return new AggregationCodegenRowLevelDesc(top, additional);
            }
        }

        public void RowCtorCodegen(AggregationRowCtorDesc rowCtorDesc)
        {
            AggregationServiceCodegenUtil.GenerateIncidentals(true, false, rowCtorDesc);
        }

        public void ProviderCodegen(
            CodegenMethod method, CodegenClassScope classScope, AggregationClassNames classNames)
        {
            method.Block.DeclareVar(typeof(AggregationLocalGroupByLevel), "optionalTop", ConstantNull());
            if (localGroupByPlan.OptionalLevelTopForge != null) {
                method.Block.AssignRef(
                    "optionalTop",
                    localGroupByPlan.OptionalLevelTopForge.ToExpression(
                        classNames.RowFactoryTop, classNames.RowSerdeTop, ConstantNull()));
            }

            int numLevels = localGroupByPlan.AllLevelsForges.Length;
            method.Block.DeclareVar(
                typeof(AggregationLocalGroupByLevel[]), "levels",
                NewArrayByLength(typeof(AggregationLocalGroupByLevel), Constant(numLevels)));
            for (var i = 0; i < numLevels; i++) {
                ExprForge[] forges = localGroupByPlan.AllLevelsForges[i].PartitionForges;
                CodegenExpression eval = ExprNodeUtilityCodegen.CodegenEvaluatorMayMultiKeyWCoerce(
                    forges, null, method, GetType(), classScope);
                method.Block.AssignArrayElement(
                    "levels", Constant(i), localGroupByPlan.AllLevelsForges[i].ToExpression(
                        classNames.GetRowFactoryPerLevel(i), classNames.GetRowSerdePerLevel(i), eval));
            }

            method.Block.DeclareVar(
                typeof(AggregationLocalGroupByColumn[]), "columns",
                NewArrayByLength(
                    typeof(AggregationLocalGroupByColumn), Constant(localGroupByPlan.ColumnsForges.Length)));
            var rowLevelDesc = RowLevelDesc;
            for (var i = 0; i < localGroupByPlan.ColumnsForges.Length; i++) {
                AggregationLocalGroupByColumnForge col = localGroupByPlan.ColumnsForges[i];
                int fieldNum;
                if (hasGroupBy && col.IsDefaultGroupLevel) {
                    AggregationCodegenRowDetailDesc levelDesc = rowLevelDesc.OptionalAdditionalRows[col.LevelNum];
                    fieldNum = GetRowFieldNum(col, levelDesc);
                }
                else if (col.LevelNum == -1) {
                    AggregationCodegenRowDetailDesc levelDesc = rowLevelDesc.OptionalTopRow;
                    fieldNum = GetRowFieldNum(col, levelDesc);
                }
                else {
                    AggregationCodegenRowDetailDesc levelDesc = rowLevelDesc.OptionalAdditionalRows[col.LevelNum];
                    fieldNum = GetRowFieldNum(col, levelDesc);
                }

                method.Block.AssignArrayElement(
                    "columns", Constant(i), localGroupByPlan.ColumnsForges[i].ToExpression(fieldNum));
            }

            method.Block
                .DeclareVar(
                    typeof(AggregationServiceFactory), "svcFactory",
                    NewInstance(classNames.ServiceFactory, Ref("this")))
                .MethodReturn(
                    ExprDotMethodChain(EPStatementInitServicesConstants.REF).Add(GETAGGREGATIONSERVICEFACTORYSERVICE).Add(
                        "groupLocalGroupBy",
                        Ref("svcFactory"), useFlags.ToExpression(), Constant(hasGroupBy),
                        Ref("optionalTop"), Ref("levels"), Ref("columns")));
        }

        public void MakeServiceCodegen(
            CodegenMethod method, CodegenClassScope classScope, AggregationClassNames classNames)
        {
            method.Block.MethodReturn(NewInstance(classNames.Service, Ref("o")));
        }

        public void CtorCodegen(
            CodegenCtor ctor, IList<CodegenTypedParam> explicitMembers, CodegenClassScope classScope,
            AggregationClassNames classNames)
        {
            explicitMembers.Add(new CodegenTypedParam(typeof(IDictionary<object, object>[]), REF_AGGREGATORSPERLEVELANDGROUP.Ref));
            ctor.Block.AssignRef(
                REF_AGGREGATORSPERLEVELANDGROUP,
                NewArrayByLength(typeof(IDictionary<object, object>), Constant(localGroupByPlan.AllLevelsForges.Length)));
            for (var i = 0; i < localGroupByPlan.AllLevelsForges.Length; i++) {
                ctor.Block.AssignArrayElement(
                    REF_AGGREGATORSPERLEVELANDGROUP, Constant(i), NewInstance(typeof(Dictionary<object, object>)));
            }

            explicitMembers.Add(new CodegenTypedParam(typeof(AggregationRow), REF_AGGREGATORSTOPLEVEL.Ref));
            if (hasGroupBy) {
                explicitMembers.Add(new CodegenTypedParam(typeof(AggregationRow), REF_CURRENTROW.Ref));
            }

            explicitMembers.Add(new CodegenTypedParam(typeof(IList<object>), REF_REMOVEDKEYS.Ref));
            ctor.Block.AssignRef(REF_REMOVEDKEYS, NewInstance(typeof(List<object>)));
        }

        public void GetValueCodegen(
            CodegenMethod method, CodegenClassScope classScope, CodegenNamedMethods namedMethods)
        {
            GetterCodegen("getValue", method, classScope, namedMethods);
        }

        public void GetCollectionOfEventsCodegen(
            CodegenMethod method, CodegenClassScope classScope, CodegenNamedMethods namedMethods)
        {
            GetterCodegen("getCollectionOfEvents", method, classScope, namedMethods);
        }

        public void GetEventBeanCodegen(
            CodegenMethod method, CodegenClassScope classScope, CodegenNamedMethods namedMethods)
        {
            GetterCodegen("getEventBean", method, classScope, namedMethods);
        }

        public void GetCollectionScalarCodegen(
            CodegenMethod method, CodegenClassScope classScope, CodegenNamedMethods namedMethods)
        {
            GetterCodegen("getCollectionScalar", method, classScope, namedMethods);
        }

        public void ApplyEnterCodegen(
            CodegenMethod method, CodegenClassScope classScope, CodegenNamedMethods namedMethods,
            AggregationClassNames classNames)
        {
            method.Block.Apply(
                Instblock(
                    classScope, "qAggregationGroupedApplyEnterLeave", ConstantTrue(), Constant(-1), Constant(-1),
                    REF_GROUPKEY));
            ApplyCodegen(true, method, classScope, namedMethods, classNames);
            method.Block.Apply(Instblock(classScope, "aAggregationGroupedApplyEnterLeave", ConstantTrue()));
        }

        public void ApplyLeaveCodegen(
            CodegenMethod method, CodegenClassScope classScope, CodegenNamedMethods namedMethods,
            AggregationClassNames classNames)
        {
            method.Block.Apply(
                Instblock(
                    classScope, "qAggregationGroupedApplyEnterLeave", ConstantFalse(), Constant(-1), Constant(-1),
                    REF_GROUPKEY));
            ApplyCodegen(false, method, classScope, namedMethods, classNames);
            method.Block.Apply(Instblock(classScope, "aAggregationGroupedApplyEnterLeave", ConstantFalse()));
        }

        public void StopMethodCodegen(AggregationServiceFactoryForgeWMethodGen forge, CodegenMethod method)
        {
            // no code required
        }

        public void SetRemovedCallbackCodegen(CodegenMethod method)
        {
            // not applicable
        }

        public void RowWriteMethodCodegen(CodegenMethod method, int level)
        {
            if (level != -1) {
                method.Block.ExprDotMethod(Ref("output"), "writeInt", Ref("row.refcount"));
            }
        }

        public void RowReadMethodCodegen(CodegenMethod method, int level)
        {
            if (level != -1) {
                method.Block.AssignRef("row.refcount", ExprDotMethod(Ref("input"), "readInt"));
            }
        }

        public void SetCurrentAccessCodegen(
            CodegenMethod method, CodegenClassScope classScope, AggregationClassNames classNames)
        {
            if (!hasGroupBy) {
                // not applicable
            }
            else {
                if (!localGroupByPlan.AllLevelsForges[0].IsDefaultLevel) {
                    return;
                }

                var indexDefault = -1;
                for (var i = 0; i < localGroupByPlan.AllLevelsForges.Length; i++) {
                    if (localGroupByPlan.AllLevelsForges[i].IsDefaultLevel) {
                        indexDefault = i;
                    }
                }

                method.Block.AssignRef(
                        REF_CURRENTROW,
                        Cast(
                            typeof(AggregationRow),
                            ExprDotMethod(
                                ArrayAtIndex(REF_AGGREGATORSPERLEVELANDGROUP, Constant(0)), "get",
                                AggregationServiceCodegenNames.REF_GROUPKEY)))
                    .IfCondition(EqualsNull(REF_CURRENTROW))
                    .AssignRef(REF_CURRENTROW, NewInstance(classNames.GetRowPerLevel(indexDefault)));
            }
        }

        public void ClearResultsCodegen(CodegenMethod method, CodegenClassScope classScope)
        {
            method.Block.IfCondition(NotEqualsNull(REF_AGGREGATORSTOPLEVEL))
                .ExprDotMethod(REF_AGGREGATORSTOPLEVEL, "clear");
            for (var i = 0; i < localGroupByPlan.AllLevelsForges.Length; i++) {
                method.Block.ExprDotMethod(ArrayAtIndex(REF_AGGREGATORSPERLEVELANDGROUP, Constant(i)), "clear");
            }
        }

        public void AcceptCodegen(CodegenMethod method, CodegenClassScope classScope)
        {
            method.Block.ExprDotMethod(
                REF_AGGVISITOR, "visitAggregations", GetNumGroupsCodegen(method, classScope), REF_AGGREGATORSTOPLEVEL,
                REF_AGGREGATORSPERLEVELANDGROUP);
        }

        public void GetGroupKeysCodegen(CodegenMethod method, CodegenClassScope classScope)
        {
            method.Block.MethodThrowUnsupported();
        }

        public void GetGroupKeyCodegen(CodegenMethod method, CodegenClassScope classScope)
        {
            method.Block.MethodReturn(ConstantNull());
        }

        public void AcceptGroupDetailCodegen(CodegenMethod method, CodegenClassScope classScope)
        {
            method.Block.ExprDotMethod(REF_AGGVISITOR, "visitGrouped", GetNumGroupsCodegen(method, classScope))
                .IfCondition(NotEqualsNull(REF_AGGREGATORSTOPLEVEL))
                .ExprDotMethod(REF_AGGVISITOR, "visitGroup", ConstantNull(), REF_AGGREGATORSTOPLEVEL);

            for (var i = 0; i < localGroupByPlan.AllLevelsForges.Length; i++) {
                method.Block.ForEach(
                        typeof(KeyValuePair<object, object>), "entry",
                        ExprDotMethod(ArrayAtIndex(REF_AGGREGATORSPERLEVELANDGROUP, Constant(i)), "entrySet"))
                    .ExprDotMethod(
                        REF_AGGVISITOR, "visitGroup", ExprDotMethod(Ref("entry"), "getKey"),
                        ExprDotMethod(Ref("entry"), "getValue"));
            }
        }

        public void IsGroupedCodegen(CodegenMethod method, CodegenClassScope classScope)
        {
            method.Block.MethodReturn(ConstantTrue());
        }

        private CodegenExpression GetNumGroupsCodegen(CodegenMethodScope parent, CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(int), GetType(), classScope);
            method.Block.DeclareVar(typeof(int), "size", Constant(0))
                .IfCondition(NotEqualsNull(REF_AGGREGATORSTOPLEVEL)).Increment("size").BlockEnd();
            for (var i = 0; i < localGroupByPlan.AllLevelsForges.Length; i++) {
                method.Block.AssignCompound(
                    "size", "+", ExprDotMethod(ArrayAtIndex(REF_AGGREGATORSPERLEVELANDGROUP, Constant(i)), "size"));
            }

            method.Block.MethodReturn(Ref("size"));
            return LocalMethod(method);
        }

        private void ApplyCodegen(
            bool enter, CodegenMethod method, CodegenClassScope classScope, CodegenNamedMethods namedMethods,
            AggregationClassNames classNames)
        {
            if (enter) {
                method.Block.LocalMethod(HandleRemovedKeysCodegen(method, classScope));
            }

            if (localGroupByPlan.OptionalLevelTopForge != null) {
                method.Block.IfCondition(EqualsNull(REF_AGGREGATORSTOPLEVEL))
                    .AssignRef(REF_AGGREGATORSTOPLEVEL, NewInstance(classNames.RowTop))
                    .BlockEnd()
                    .ExprDotMethod(
                        REF_AGGREGATORSTOPLEVEL, enter ? "applyEnter" : "applyLeave", REF_EPS, REF_EXPREVALCONTEXT);
            }

            for (var levelNum = 0; levelNum < localGroupByPlan.AllLevelsForges.Length; levelNum++) {
                AggregationLocalGroupByLevelForge level = localGroupByPlan.AllLevelsForges[levelNum];
                ExprForge[] partitionForges = level.PartitionForges;

                var groupKeyName = "groupKeyLvl_" + levelNum;
                var rowName = "row_" + levelNum;
                CodegenExpression groupKeyExp;
                if (hasGroupBy && level.IsDefaultLevel) {
                    groupKeyExp = AggregationServiceCodegenNames.REF_GROUPKEY;
                }
                else {
                    groupKeyExp = LocalMethod(
                        AggregationServiceCodegenUtil.ComputeMultiKeyCodegen(
                            levelNum, partitionForges, classScope, namedMethods), REF_EPS, ConstantTrue(),
                        REF_EXPREVALCONTEXT);
                }


                method.Block.DeclareVar(typeof(object), groupKeyName, groupKeyExp)
                    .DeclareVar(
                        typeof(AggregationRow), rowName,
                        Cast(
                            typeof(AggregationRow),
                            ExprDotMethod(
                                ArrayAtIndex(REF_AGGREGATORSPERLEVELANDGROUP, Constant(levelNum)), "get",
                                Ref(groupKeyName))))
                    .IfCondition(EqualsNull(Ref(rowName)))
                    .AssignRef(rowName, NewInstance(classNames.GetRowPerLevel(levelNum)))
                    .ExprDotMethod(
                        ArrayAtIndex(REF_AGGREGATORSPERLEVELANDGROUP, Constant(levelNum)), "put", Ref(groupKeyName),
                        Ref(rowName))
                    .BlockEnd()
                    .ExprDotMethod(Ref(rowName), enter ? "increaseRefcount" : "decreaseRefcount")
                    .ExprDotMethod(Ref(rowName), enter ? "applyEnter" : "applyLeave", REF_EPS, REF_EXPREVALCONTEXT);

                if (!enter) {
                    method.Block.IfCondition(Relational(ExprDotMethod(Ref(rowName), "getRefcount"), LE, Constant(0)))
                        .ExprDotMethod(
                            REF_REMOVEDKEYS, "add",
                            NewInstance(typeof(AggSvcLocalGroupLevelKeyPair), Constant(levelNum), Ref(groupKeyName)));
                }
            }
        }

        private AggregationCodegenRowDetailDesc MapDesc(
            bool top, int levelNum, AggregationLocalGroupByColumnForge[] columns,
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

            AggregationAccessorSlotPairForge[] pairs = accessAccessors.ToArray();
            return new AggregationCodegenRowDetailDesc(
                new AggregationCodegenRowDetailStateDesc(
                    level.MethodForges, level.MethodFactories, level.AccessStateForges), pairs);
        }

        private int AccessorIndex(
            AggregationAccessorSlotPairForge[] accessAccessors, AggregationAccessorSlotPairForge pair)
        {
            for (var i = 0; i < accessAccessors.Length; i++) {
                if (accessAccessors[i] == pair) {
                    return i;
                }
            }

            throw new IllegalStateException();
        }

        private void GetterCodegen(
            string methodName, CodegenMethod method, CodegenClassScope classScope, CodegenNamedMethods namedMethods)
        {
            var rowLevelDesc = RowLevelDesc;

            var blocks = method.Block.SwitchBlockOfLength(
                AggregationServiceCodegenNames.NAME_COLUMN, localGroupByPlan.ColumnsForges.Length, true);
            for (var i = 0; i < blocks.Length; i++) {
                AggregationLocalGroupByColumnForge col = localGroupByPlan.ColumnsForges[i];

                if (hasGroupBy && col.IsDefaultGroupLevel) {
                    AggregationCodegenRowDetailDesc levelDesc = rowLevelDesc.OptionalAdditionalRows[col.LevelNum];
                    var num = GetRowFieldNum(col, levelDesc);
                    blocks[i].BlockReturn(
                        ExprDotMethod(
                            REF_CURRENTROW, methodName, Constant(num), REF_EPS, REF_ISNEWDATA, REF_EXPREVALCONTEXT));
                }
                else if (col.LevelNum == -1) {
                    AggregationCodegenRowDetailDesc levelDesc = rowLevelDesc.OptionalTopRow;
                    var num = GetRowFieldNum(col, levelDesc);
                    blocks[i].BlockReturn(
                        ExprDotMethod(
                            REF_AGGREGATORSTOPLEVEL, methodName, Constant(num), REF_EPS, REF_ISNEWDATA,
                            REF_EXPREVALCONTEXT));
                }
                else {
                    AggregationCodegenRowDetailDesc levelDesc = rowLevelDesc.OptionalAdditionalRows[col.LevelNum];
                    var num = GetRowFieldNum(col, levelDesc);
                    blocks[i].DeclareVar(
                            typeof(object), "groupByKey",
                            LocalMethod(
                                AggregationServiceCodegenUtil.ComputeMultiKeyCodegen(
                                    col.LevelNum, col.PartitionForges, classScope, namedMethods), REF_EPS,
                                REF_ISNEWDATA, REF_EXPREVALCONTEXT))
                        .DeclareVar(
                            typeof(AggregationRow), "row",
                            Cast(
                                typeof(AggregationRow),
                                ExprDotMethod(
                                    ArrayAtIndex(REF_AGGREGATORSPERLEVELANDGROUP, Constant(col.LevelNum)), "get",
                                    Ref("groupByKey"))))
                        .BlockReturn(
                            ExprDotMethod(
                                Ref("row"), methodName, Constant(num), REF_EPS, REF_ISNEWDATA, REF_EXPREVALCONTEXT));
                }
            }
        }

        private int GetRowFieldNum(AggregationLocalGroupByColumnForge col, AggregationCodegenRowDetailDesc levelDesc)
        {
            return col.IsMethodAgg
                ? col.MethodOffset
                : levelDesc.StateDesc.MethodFactories.Length + AccessorIndex(levelDesc.AccessAccessors, col.Pair);
        }

        private CodegenMethod HandleRemovedKeysCodegen(CodegenMethod scope, CodegenClassScope classScope)
        {
            var method = scope.MakeChild(typeof(void), GetType(), classScope);
            method.Block.IfCondition(Not(ExprDotMethod(REF_REMOVEDKEYS, "isEmpty")))
                .ForEach(typeof(AggSvcLocalGroupLevelKeyPair), "removedKey", REF_REMOVEDKEYS)
                .ExprDotMethod(
                    ArrayAtIndex(REF_AGGREGATORSPERLEVELANDGROUP, ExprDotMethod(Ref("removedKey"), "getLevel")),
                    "remove", ExprDotMethod(Ref("removedKey"), "getKey"))
                .BlockEnd()
                .ExprDotMethod(REF_REMOVEDKEYS, "clear");
            return method;
        }
    }
} // end of namespace