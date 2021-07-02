///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client.serde;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.util;

using static com.espertech.esper.common.@internal.epl.agg.core.AggregationServiceCodegenNames;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionRelational.CodegenRelational;
using static com.espertech.esper.common.@internal.epl.expression.codegen.ExprForgeCodegenNames;

namespace com.espertech.esper.common.@internal.epl.agg.rollup
{
    /// <summary>
    ///     Implementation for handling aggregation with grouping by group-keys.
    /// </summary>
    public class AggSvcGroupByRollupForge : AggregationServiceFactoryForgeWMethodGen
    {
        private static readonly CodegenExpressionMember MEMBER_AGGREGATORSPERGROUP = Member("aggregatorsPerGroup");
        private static readonly CodegenExpressionMember MEMBER_AGGREGATORTOPGROUP = Member("aggregatorTopGroup");
        private static readonly CodegenExpressionMember MEMBER_CURRENTROW = Member("currentRow");
        private static readonly CodegenExpressionMember MEMBER_CURRENTGROUPKEY = Member("currentGroupKey");
        private static readonly CodegenExpressionMember MEMBER_HASREMOVEDKEY = Member("hasRemovedKey");
        private static readonly CodegenExpressionMember MEMBER_REMOVEDKEYS = Member("removedKeys");

        private readonly ExprNode[] _groupByNodes;
        private readonly AggregationRowStateForgeDesc _rowStateForgeDesc;
        private readonly AggregationGroupByRollupDescForge _rollupDesc;

        private readonly StateMgmtSetting _stateMgmtSettings;
        
        public AggSvcGroupByRollupForge(
            AggregationRowStateForgeDesc rowStateForgeDesc,
            AggregationGroupByRollupDescForge rollupDesc,
            ExprNode[] groupByNodes,
            StateMgmtSetting stateMgmtSettings)
        {
            this._rowStateForgeDesc = rowStateForgeDesc;
            this._rollupDesc = rollupDesc;
            this._groupByNodes = groupByNodes;
            this._stateMgmtSettings = stateMgmtSettings;
        }

        public AggregationCodegenRowLevelDesc RowLevelDesc =>
            AggregationCodegenRowLevelDesc.FromTopOnly(_rowStateForgeDesc);

        public void ProviderCodegen(
            CodegenMethod method,
            CodegenClassScope classScope,
            AggregationClassNames classNames)
        {
            method.Block
                .DeclareVar<AggregationServiceFactory>(
                    "svcFactory",
                    NewInstanceNamed(classNames.ServiceFactory, Ref("this")))
                .DeclareVar<AggregationRowFactory>(
                    "rowFactory",
                    NewInstanceNamed(classNames.RowFactoryTop, Ref("this")))
                .DeclareVar<DataInputOutputSerde<AggregationRow>>(
                    "rowSerde",
                    NewInstanceNamed(classNames.RowSerdeTop, Ref("this")))
                .MethodReturn(
                    ExprDotMethodChain(EPStatementInitServicesConstants.REF)
                        .Get(EPStatementInitServicesConstants.AGGREGATIONSERVICEFACTORYSERVICE)
                        .Add(
                            "GroupByRollup",
                            Ref("svcFactory"),
                            _rollupDesc.Codegen(method, classScope),
                            Ref("rowFactory"),
                            _rowStateForgeDesc.UseFlags.ToExpression(),
                            Ref("rowSerde"),
                            _stateMgmtSettings.ToExpression()));
        }

        public void RowCtorCodegen(AggregationRowCtorDesc rowCtorDesc)
        {
            AggregationServiceCodegenUtil.GenerateIncidentals(true, false, rowCtorDesc);
        }

        public void MakeServiceCodegen(
            CodegenMethod method,
            CodegenClassScope classScope,
            AggregationClassNames classNames)
        {
            method.Block.MethodReturn(NewInstanceNamed(classNames.Service, Ref("o")));
        }

        public void CtorCodegen(
            CodegenCtor ctor,
            IList<CodegenTypedParam> explicitMembers,
            CodegenClassScope classScope,
            AggregationClassNames classNames)
        {
            explicitMembers.Add(
                new CodegenTypedParam(typeof(IDictionary<object, object>[]), MEMBER_AGGREGATORSPERGROUP.Ref));
            explicitMembers.Add(new CodegenTypedParam(typeof(IList<object>[]), MEMBER_REMOVEDKEYS.Ref));
            ctor.Block.AssignRef(
                    MEMBER_AGGREGATORSPERGROUP,
                    NewArrayByLength(typeof(IDictionary<object, object>), Constant(_rollupDesc.NumLevelsAggregation)))
                .AssignRef(
                    MEMBER_REMOVEDKEYS,
                    NewArrayByLength(typeof(IList<object>), Constant(_rollupDesc.NumLevelsAggregation)));
            for (var i = 0; i < _rollupDesc.NumLevelsAggregation; i++) {
                ctor.Block.AssignArrayElement(
                    MEMBER_AGGREGATORSPERGROUP,
                    Constant(i),
                    NewInstance(typeof(Dictionary<object, object>)));
                ctor.Block.AssignArrayElement(MEMBER_REMOVEDKEYS, Constant(i), NewInstance<List<object>>(Constant(4)));
            }

            explicitMembers.Add(new CodegenTypedParam(classNames.RowTop, MEMBER_AGGREGATORTOPGROUP.Ref));
            ctor.Block.AssignRef(MEMBER_AGGREGATORTOPGROUP, NewInstanceNamed(classNames.RowTop, Ref("o")))
                .ExprDotMethod(MEMBER_AGGREGATORTOPGROUP, "DecreaseRefcount");

            explicitMembers.Add(new CodegenTypedParam(typeof(AggregationRow), MEMBER_CURRENTROW.Ref));
            explicitMembers.Add(new CodegenTypedParam(typeof(object), MEMBER_CURRENTGROUPKEY.Ref));
            explicitMembers.Add(new CodegenTypedParam(typeof(bool), MEMBER_HASREMOVEDKEY.Ref));
        }

        public void GetValueCodegen(
            CodegenMethod method,
            CodegenClassScope classScope,
            CodegenNamedMethods namedMethods)
        {
            method.Block.DebugStack();
            method.Block.MethodReturn(
                ExprDotMethod(
                    MEMBER_CURRENTROW,
                    "GetValue",
                    REF_COLUMN,
                    REF_EPS,
                    REF_ISNEWDATA,
                    REF_EXPREVALCONTEXT));
        }

        public void GetCollectionOfEventsCodegen(
            CodegenMethod method,
            CodegenClassScope classScope,
            CodegenNamedMethods namedMethods)
        {
            method.Block.MethodReturn(
                ExprDotMethod(
                    MEMBER_CURRENTROW,
                    "GetCollectionOfEvents",
                    REF_COLUMN,
                    REF_EPS,
                    REF_ISNEWDATA,
                    REF_EXPREVALCONTEXT));
        }

        public void GetEventBeanCodegen(
            CodegenMethod method,
            CodegenClassScope classScope,
            CodegenNamedMethods namedMethods)
        {
            method.Block.MethodReturn(
                ExprDotMethod(MEMBER_CURRENTROW, "GetEventBean", REF_COLUMN, REF_EPS, REF_ISNEWDATA, REF_EXPREVALCONTEXT));
        }

        public void GetCollectionScalarCodegen(
            CodegenMethod method,
            CodegenClassScope classScope,
            CodegenNamedMethods namedMethods)
        {
            method.Block.MethodReturn(
                ExprDotMethod(
                    MEMBER_CURRENTROW,
                    "GetCollectionScalar",
                    REF_COLUMN,
                    REF_EPS,
                    REF_ISNEWDATA,
                    REF_EXPREVALCONTEXT));
        }

        public void ApplyEnterCodegen(
            CodegenMethod method,
            CodegenClassScope classScope,
            CodegenNamedMethods namedMethods,
            AggregationClassNames classNames)
        {
            ApplyCodegen(true, method, classScope, classNames);
        }

        public void ApplyLeaveCodegen(
            CodegenMethod method,
            CodegenClassScope classScope,
            CodegenNamedMethods namedMethods,
            AggregationClassNames classNames)
        {
            ApplyCodegen(false, method, classScope, classNames);
        }

        public void StopMethodCodegen(
            AggregationServiceFactoryForgeWMethodGen forge,
            CodegenMethod method)
        {
            // no action
        }

        public void SetRemovedCallbackCodegen(CodegenMethod method)
        {
            // no action
        }

        public void SetCurrentAccessCodegen(
            CodegenMethod method,
            CodegenClassScope classScope,
            AggregationClassNames classNames)
        {
            method.Block.IfCondition(ExprDotName(REF_ROLLUPLEVEL, "IsAggregationTop"))
                .AssignRef(MEMBER_CURRENTROW, MEMBER_AGGREGATORTOPGROUP)
                .IfElse()
                .AssignRef(
                    MEMBER_CURRENTROW,
                    Cast(
                        typeof(AggregationRow),
                        ExprDotMethod(
                            ArrayAtIndex(
                                MEMBER_AGGREGATORSPERGROUP,
                                ExprDotName(REF_ROLLUPLEVEL, "AggregationOffset")),
                            "Get",
                            REF_GROUPKEY)))
                .IfCondition(EqualsNull(MEMBER_CURRENTROW))
                .AssignRef(MEMBER_CURRENTROW, NewInstanceNamed(classNames.RowTop, Ref("o")))
                .BlockEnd()
                .BlockEnd()
                .AssignRef(MEMBER_CURRENTGROUPKEY, REF_GROUPKEY);
        }

        public void ClearResultsCodegen(
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            method.Block.ExprDotMethod(MEMBER_AGGREGATORTOPGROUP, "Clear");
            for (var i = 0; i < _rollupDesc.NumLevelsAggregation; i++) {
                method.Block.ExprDotMethod(ArrayAtIndex(MEMBER_AGGREGATORSPERGROUP, Constant(i)), "Clear");
            }
        }

        public void AcceptCodegen(
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            method.Block.ExprDotMethod(
                REF_AGGVISITOR,
                "VisitAggregations",
                GetGroupKeyCountCodegen(method, classScope),
                MEMBER_AGGREGATORSPERGROUP,
                MEMBER_AGGREGATORTOPGROUP);
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
            method.Block.MethodReturn(MEMBER_CURRENTGROUPKEY);
        }

        public void AcceptGroupDetailCodegen(
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            method.Block.ExprDotMethod(REF_AGGVISITOR, "VisitGrouped", GetGroupKeyCountCodegen(method, classScope))
                .ForEach(typeof(IDictionary<object, object>), "anAggregatorsPerGroup", MEMBER_AGGREGATORSPERGROUP)
                .ForEach(
                    typeof(KeyValuePair<object, object>),
                    "entry",
                    Ref("anAggregatorsPerGroup"))
                .ExprDotMethod(
                    REF_AGGVISITOR,
                    "VisitGroup",
                    ExprDotName(Ref("entry"), "Key"),
                    ExprDotName(Ref("entry"), "Value"))
                .BlockEnd()
                .BlockEnd()
                .ExprDotMethod(
                    REF_AGGVISITOR,
                    "VisitGroup",
                    PublicConstValue(typeof(CollectionUtil), "OBJECTARRAY_EMPTY"),
                    MEMBER_AGGREGATORTOPGROUP);
        }

        public void IsGroupedCodegen(
            CodegenProperty property,
            CodegenClassScope classScope)
        {
            property.GetterBlock.BlockReturn(ConstantTrue());
        }

        public void RowWriteMethodCodegen(
            CodegenMethod method,
            int level)
        {
            method.Block.ExprDotMethod(Ref("output"), "WriteInt", Ref("row.refcount"));
        }

        public void RowReadMethodCodegen(
            CodegenMethod method,
            int level)
        {
            method.Block.AssignRef("row.refcount", ExprDotMethod(Ref("input"), "ReadInt"));
        }

        public void GetRowCodegen(CodegenMethod method, CodegenClassScope classScope, CodegenNamedMethods namedMethods)
        {
            method.Block.MethodReturn(MEMBER_CURRENTROW);
        }
        
        private CodegenExpression GetGroupKeyCountCodegen(
            CodegenMethodScope parent,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(int), typeof(AggSvcGroupByRollupForge), classScope);
            method.Block.DeclareVar<int>("size", Constant(1));
            for (var i = 0; i < _rollupDesc.NumLevelsAggregation; i++) {
                method.Block.AssignCompound(
                    "size",
                    "+",
                    ExprDotName(ArrayAtIndex(MEMBER_AGGREGATORSPERGROUP, Constant(i)), "Count"));
            }

            method.Block.MethodReturn(Ref("size"));
            return LocalMethod(method);
        }

        private void ApplyCodegen(
            bool enter,
            CodegenMethod method,
            CodegenClassScope classScope,
            AggregationClassNames classNames)
        {
            if (enter) {
                method.Block.LocalMethod(HandleRemovedKeysCodegen(method, classScope));
            }

            method.Block.DeclareVar<object[]>(
                "groupKeyPerLevel",
                Cast(typeof(object[]), REF_GROUPKEY));
            for (var i = 0; i < _rollupDesc.NumLevels; i++) {
                var level = _rollupDesc.Levels[i];
                var groupKeyName = "groupKey_" + i;
                method.Block.DeclareVar<object>(
                    groupKeyName,
                    ArrayAtIndex(Ref("groupKeyPerLevel"), Constant(i)));

                if (level.IsAggregationTop) {
                    method.Block.AssignRef(MEMBER_CURRENTROW, MEMBER_AGGREGATORTOPGROUP)
                        .ExprDotMethod(MEMBER_CURRENTROW, enter ? "IncreaseRefcount" : "DecreaseRefcount");
                }
                else {
                    if (enter) {
                        method.Block.AssignRef(
                                MEMBER_CURRENTROW,
                                Cast(
                                    typeof(AggregationRow),
                                    ExprDotMethod(
                                        ArrayAtIndex(MEMBER_AGGREGATORSPERGROUP, Constant(level.AggregationOffset)),
                                        "Get",
                                        Ref(groupKeyName))))
                            .IfCondition(EqualsNull(MEMBER_CURRENTROW))
                            .AssignRef(MEMBER_CURRENTROW, NewInstanceNamed(classNames.RowTop, Ref("o")))
                            .ExprDotMethod(
                                ArrayAtIndex(MEMBER_AGGREGATORSPERGROUP, Constant(level.AggregationOffset)),
                                "Put",
                                Ref(groupKeyName),
                                MEMBER_CURRENTROW)
                            .BlockEnd()
                            .ExprDotMethod(MEMBER_CURRENTROW, "IncreaseRefcount");
                    }
                    else {
                        method.Block.AssignRef(
                                MEMBER_CURRENTROW,
                                Cast(
                                    typeof(AggregationRow),
                                    ExprDotMethod(
                                        ArrayAtIndex(MEMBER_AGGREGATORSPERGROUP, Constant(level.AggregationOffset)),
                                        "Get",
                                        Ref(groupKeyName))))
                            .IfCondition(EqualsNull(MEMBER_CURRENTROW))
                            .AssignRef(MEMBER_CURRENTROW, NewInstanceNamed(classNames.RowTop, Ref("o")))
                            .ExprDotMethod(
                                ArrayAtIndex(MEMBER_AGGREGATORSPERGROUP, Constant(level.AggregationOffset)),
                                "Put",
                                Ref(groupKeyName),
                                MEMBER_CURRENTROW)
                            .BlockEnd()
                            .ExprDotMethod(MEMBER_CURRENTROW, "DecreaseRefcount");
                    }
                }

                method.Block.ExprDotMethod(
                    MEMBER_CURRENTROW,
                    enter ? "ApplyEnter" : "ApplyLeave",
                    REF_EPS,
                    REF_EXPREVALCONTEXT);

                if (!enter && !level.IsAggregationTop) {
                    var ifCanDelete = method.Block.IfCondition(
                        Relational(ExprDotMethod(MEMBER_CURRENTROW, "GetRefcount"), LE, Constant(0)));
                    ifCanDelete.AssignRef(MEMBER_HASREMOVEDKEY, ConstantTrue());
                    if (!level.IsAggregationTop) {
                        var removedKeyForLevel = ArrayAtIndex(MEMBER_REMOVEDKEYS, Constant(level.AggregationOffset));
                        ifCanDelete.ExprDotMethod(removedKeyForLevel, "Add", Ref(groupKeyName));
                    }
                }
            }
        }

        private CodegenMethod HandleRemovedKeysCodegen(
            CodegenMethod scope,
            CodegenClassScope classScope)
        {
            var method = scope.MakeChild(typeof(void), GetType(), classScope);
            method.Block.IfCondition(Not(MEMBER_HASREMOVEDKEY))
                .BlockReturnNoValue()
                .AssignRef(MEMBER_HASREMOVEDKEY, ConstantFalse())
                .ForLoopIntSimple("i", ArrayLength(MEMBER_REMOVEDKEYS))
                .IfCondition(ExprDotMethod(ArrayAtIndex(MEMBER_REMOVEDKEYS, Ref("i")), "IsEmpty"))
                .BlockContinue()
                .ForEach(typeof(object), "removedKey", ArrayAtIndex(MEMBER_REMOVEDKEYS, Ref("i")))
                .ExprDotMethod(ArrayAtIndex(MEMBER_AGGREGATORSPERGROUP, Ref("i")), "Remove", Ref("removedKey"))
                .BlockEnd()
                .ExprDotMethod(ArrayAtIndex(MEMBER_REMOVEDKEYS, Ref("i")), "Clear");
            return method;
        }
    }
} // end of namespace