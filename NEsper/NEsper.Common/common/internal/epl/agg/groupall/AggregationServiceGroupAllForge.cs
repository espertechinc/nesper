///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.serde;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.serde;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.agg.core.AggregationServiceCodegenNames;
using static com.espertech.esper.common.@internal.epl.expression.codegen.ExprForgeCodegenNames;
using static com.espertech.esper.common.@internal.metrics.instrumentation.InstrumentationCode;

namespace com.espertech.esper.common.@internal.epl.agg.groupall
{
    /// <summary>
    ///     Aggregation service for use when only first/last/window aggregation functions are used an none other.
    /// </summary>
    public class AggregationServiceGroupAllForge : AggregationServiceFactoryForgeWMethodGen
    {
        private static readonly CodegenExpressionMember MEMBER_ROW = Member("row");

        internal readonly AggregationRowStateForgeDesc rowStateDesc;

        public AggregationServiceGroupAllForge(AggregationRowStateForgeDesc rowStateDesc)
        {
            this.rowStateDesc = rowStateDesc;
        }

        public AggregationCodegenRowLevelDesc RowLevelDesc => AggregationCodegenRowLevelDesc.FromTopOnly(rowStateDesc);

        public void ProviderCodegen(
            CodegenMethod method,
            CodegenClassScope classScope,
            AggregationClassNames classNames)
        {
            method.Block
                .DeclareVar<AggregationRowFactory>(
                    "rowFactory",
                    NewInstance(classNames.RowFactoryTop, Ref("this")))
                .DeclareVar<DataInputOutputSerde<AggregationRow>>(
                    "rowSerde",
                    NewInstance(classNames.RowSerdeTop, Ref("this")))
                .DeclareVar<AggregationServiceFactory>(
                    "svcFactory",
                    NewInstance(classNames.ServiceFactory, Ref("this")))
                .MethodReturn(
                    ExprDotMethodChain(EPStatementInitServicesConstants.REF)
                        .Get(EPStatementInitServicesConstants.AGGREGATIONSERVICEFACTORYSERVICE)
                        .Add(
                            "GroupAll",
                            Ref("svcFactory"),
                            Ref("rowFactory"),
                            rowStateDesc.UseFlags.ToExpression(),
                            Ref("rowSerde")));
        }

        public void MakeServiceCodegen(
            CodegenMethod method,
            CodegenClassScope classScope,
            AggregationClassNames classNames)
        {
            method.Block.MethodReturn(NewInstance(classNames.Service, Ref("o")));
        }

        public void RowCtorCodegen(AggregationRowCtorDesc rowCtorDesc)
        {
            AggregationServiceCodegenUtil.GenerateIncidentals(false, false, rowCtorDesc);
        }

        public void CtorCodegen(
            CodegenCtor ctor,
            IList<CodegenTypedParam> explicitMembers,
            CodegenClassScope classScope,
            AggregationClassNames classNames)
        {
            explicitMembers.Add(new CodegenTypedParam(classNames.RowTop, MEMBER_ROW.Ref));
            ctor.Block.AssignRef(MEMBER_ROW, NewInstance(classNames.RowTop, Ref("o")));
        }

        public void GetValueCodegen(
            CodegenMethod method,
            CodegenClassScope classScope,
            CodegenNamedMethods namedMethods)
        {
            method.Block.DebugStack();
            method.Block.MethodReturn(
                ExprDotMethod(MEMBER_ROW, "GetValue", REF_COLUMN, REF_EPS, REF_ISNEWDATA, REF_EXPREVALCONTEXT));
        }

        public void GetEventBeanCodegen(
            CodegenMethod method,
            CodegenClassScope classScope,
            CodegenNamedMethods namedMethods)
        {
            method.Block.MethodReturn(
                ExprDotMethod(MEMBER_ROW, "GetEventBean", REF_COLUMN, REF_EPS, REF_ISNEWDATA, REF_EXPREVALCONTEXT));
        }

        public void ApplyEnterCodegen(
            CodegenMethod method,
            CodegenClassScope classScope,
            CodegenNamedMethods namedMethods,
            AggregationClassNames classNames)
        {
            method.Block
                .Apply(
                    Instblock(
                        classScope,
                        "qAggregationUngroupedApplyEnterLeave",
                        ConstantTrue(),
                        Constant(rowStateDesc.NumMethods),
                        Constant(rowStateDesc.NumAccess)))
                .ExprDotMethod(MEMBER_ROW, "ApplyEnter", REF_EPS, REF_EXPREVALCONTEXT)
                .Apply(Instblock(classScope, "aAggregationUngroupedApplyEnterLeave", ConstantTrue()));
        }

        public void ApplyLeaveCodegen(
            CodegenMethod method,
            CodegenClassScope classScope,
            CodegenNamedMethods namedMethods,
            AggregationClassNames classNames)
        {
            method.Block
                .Apply(
                    Instblock(
                        classScope,
                        "qAggregationUngroupedApplyEnterLeave",
                        ConstantFalse(),
                        Constant(rowStateDesc.NumMethods),
                        Constant(rowStateDesc.NumAccess)))
                .ExprDotMethod(MEMBER_ROW, "ApplyLeave", REF_EPS, REF_EXPREVALCONTEXT)
                .Apply(Instblock(classScope, "aAggregationUngroupedApplyEnterLeave", ConstantFalse()));
        }

        public void StopMethodCodegen(
            AggregationServiceFactoryForgeWMethodGen forge,
            CodegenMethod method)
        {
            // no code
        }

        public void SetRemovedCallbackCodegen(CodegenMethod method)
        {
            // no code
        }

        public void SetCurrentAccessCodegen(
            CodegenMethod method,
            CodegenClassScope classScope,
            AggregationClassNames classNames)
        {
            // no code
        }

        public void ClearResultsCodegen(
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            method.Block.ExprDotMethod(MEMBER_ROW, "Clear");
        }

        public void GetCollectionScalarCodegen(
            CodegenMethod method,
            CodegenClassScope classScope,
            CodegenNamedMethods namedMethods)
        {
            method.Block.MethodReturn(
                ExprDotMethod(
                    MEMBER_ROW,
                    "GetCollectionScalar",
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
                    MEMBER_ROW,
                    "GetCollectionOfEvents",
                    REF_COLUMN,
                    REF_EPS,
                    REF_ISNEWDATA,
                    REF_EXPREVALCONTEXT));
        }

        public void AcceptCodegen(
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            method.Block.ExprDotMethod(REF_AGGVISITOR, "VisitAggregations", Constant(1), MEMBER_ROW);
        }

        public void GetGroupKeysCodegen(
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            method.Block.MethodReturn(ConstantNull());
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
            // not implemented
        }

        public void IsGroupedCodegen(
            CodegenProperty property,
            CodegenClassScope classScope)
        {
            property.GetterBlock.BlockReturn(ConstantFalse());
        }

        public void RowWriteMethodCodegen(
            CodegenMethod method,
            int level)
        {
        }

        public void RowReadMethodCodegen(
            CodegenMethod method,
            int level)
        {
        }

        public void GetRowCodegen(
            CodegenMethod method,
            CodegenClassScope classScope,
            CodegenNamedMethods namedMethods)
        {
            method.Block.MethodReturn(MEMBER_ROW);
        }
    }
} // end of namespace