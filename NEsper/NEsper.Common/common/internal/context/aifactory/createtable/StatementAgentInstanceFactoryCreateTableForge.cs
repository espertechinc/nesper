///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.table.compiletime;
using com.espertech.esper.common.@internal.epl.table.core;
using com.espertech.esper.common.@internal.@event.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.expression.codegen.ExprForgeCodegenNames;

namespace com.espertech.esper.common.@internal.context.aifactory.createtable
{
    public class StatementAgentInstanceFactoryCreateTableForge
    {
        private readonly string className;
        private readonly TableAccessAnalysisResult plan;
        private readonly string tableName;

        public StatementAgentInstanceFactoryCreateTableForge(
            string className,
            string tableName,
            TableAccessAnalysisResult plan)
        {
            this.className = className;
            this.tableName = tableName;
            this.plan = plan;
        }

        public CodegenMethod InitializeCodegen(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            // add aggregation row+factory+serde as inner classes
            var aggregationClassNames = new AggregationClassNames();
            var inners = AggregationServiceFactoryCompiler.MakeTable(
                AggregationCodegenRowLevelDesc.FromTopOnly(plan.AggDesc),
                GetType(),
                classScope,
                aggregationClassNames,
                className);
            classScope.AddInnerClasses(inners);

            var method = parent.MakeChild(typeof(StatementAgentInstanceFactoryCreateTable), GetType(), classScope);

            var primaryKeyGetter = ConstantNull();
            if (plan.PrimaryKeyGetters != null) {
                primaryKeyGetter = EventTypeUtility.CodegenGetterMayMultiKeyWCoerce(
                    plan.InternalEventType,
                    plan.PrimaryKeyGetters,
                    plan.PrimaryKeyTypes,
                    null,
                    method,
                    GetType(),
                    classScope);
            }

            method.Block
                .DeclareVar<StatementAgentInstanceFactoryCreateTable>(
                    "saiff",
                    NewInstance(typeof(StatementAgentInstanceFactoryCreateTable)))
                .SetProperty(Ref("saiff"), "TableName", Constant(tableName))
                .SetProperty(
                    Ref("saiff"),
                    "PublicEventType",
                    EventTypeUtility.ResolveTypeCodegen(plan.PublicEventType, symbols.GetAddInitSvc(method)))
                .SetProperty(Ref("saiff"), "EventToPublic", MakeEventToPublic(method, symbols, classScope))
                .SetProperty(
                    Ref("saiff"),
                    "AggregationRowFactory",
                    NewInstance(aggregationClassNames.RowFactoryTop, Ref("this")))
                .SetProperty(
                    Ref("saiff"),
                    "AggregationSerde",
                    NewInstance(aggregationClassNames.RowSerdeTop, Ref("this")))
                .SetProperty(Ref("saiff"), "PrimaryKeyGetter", primaryKeyGetter)
                .ExprDotMethod(symbols.GetAddInitSvc(method), "AddReadyCallback", Ref("saiff"))
                .MethodReturn(Ref("saiff"));
            return method;
        }

        private CodegenExpression MakeEventToPublic(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(TableMetadataInternalEventToPublic), GetType(), classScope);
            var factory = classScope.AddOrGetDefaultFieldSharable(EventBeanTypedEventFactoryCodegenField.INSTANCE);
            var eventType = classScope.AddDefaultFieldUnshared(
                true,
                typeof(EventType),
                EventTypeUtility.ResolveTypeCodegen(plan.PublicEventType, EPStatementInitServicesConstants.REF));

            CodegenExpressionLambda convertToUnd = new CodegenExpressionLambda(method.Block)
                .WithParams(new CodegenNamedParam(typeof(EventBean), "@event"))
                .WithParams(PARAMS);

            convertToUnd.Block
                .DeclareVar<object[]>(
                    "props",
                    ExprDotName(Cast(typeof(ObjectArrayBackedEventBean), Ref("@event")), "Properties"))
                .DeclareVar<object[]>(
                    "data",
                    NewArrayByLength(typeof(object), Constant(plan.PublicEventType.PropertyNames.Length)));
            foreach (TableMetadataColumnPairPlainCol plain in plan.ColsPlain) {
                convertToUnd.Block.AssignArrayElement(
                    Ref("data"),
                    Constant(plain.Dest),
                    ArrayAtIndex(Ref("props"), Constant(plain.Source)));
            }

            if (plan.ColsAggMethod.Length > 0 || plan.ColsAccess.Length > 0) {
                convertToUnd.Block.DeclareVar<AggregationRow>(
                    "row",
                    Cast(typeof(AggregationRow), ArrayAtIndex(Ref("props"), Constant(0))));
                var count = 0;

                foreach (TableMetadataColumnPairAggMethod aggMethod in plan.ColsAggMethod) {
                    // Code: data[method.getDest()] = row.getMethods()[count++].getValue();
                    convertToUnd.Block.DebugStack();
                    convertToUnd.Block.AssignArrayElement(
                        Ref("data"),
                        Constant(aggMethod.Dest),
                        ExprDotMethod(
                            Ref("row"),
                            "GetValue",
                            Constant(count),
                            REF_EPS,
                            REF_ISNEWDATA,
                            REF_EXPREVALCONTEXT));
                    count++;
                }

                foreach (TableMetadataColumnPairAggAccess aggAccess in plan.ColsAccess) {
                    // Code: data[method.getDest()] = row.getMethods()[count++].getValue();
                    convertToUnd.Block.DebugStack();
                    convertToUnd.Block.AssignArrayElement(
                        Ref("data"),
                        Constant(aggAccess.Dest),
                        ExprDotMethod(
                            Ref("row"),
                            "GetValue",
                            Constant(count),
                            REF_EPS,
                            REF_ISNEWDATA,
                            REF_EXPREVALCONTEXT));
                    count++;
                }
            }

            convertToUnd.Block.BlockReturn(Ref("data"));

            method.Block.DeclareVar<ProxyTableMetadataInternalEventToPublic.ConvertToUndFunc>(
                "convertToUndFunc",
                convertToUnd);

            CodegenExpressionLambda convert = new CodegenExpressionLambda(method.Block)
                .WithParams(new CodegenNamedParam(typeof(EventBean), "@event"))
                .WithParams(PARAMS);

            convert.Block
                .DeclareVar<object[]>(
                    "data",
                    ExprDotMethod(
                        Ref("convertToUndFunc"),
                        "Invoke",
                        Ref("@event"),
                        REF_EPS,
                        REF_ISNEWDATA,
                        REF_EXPREVALCONTEXT))
                .BlockReturn(ExprDotMethod(factory, "AdapterForTypedObjectArray", Ref("data"), eventType));

            method.Block.DeclareVar<ProxyTableMetadataInternalEventToPublic.ConvertFunc>(
                "convertFunc",
                convert);

            method.Block.MethodReturn(
                NewInstance<ProxyTableMetadataInternalEventToPublic>(
                    Ref("convertFunc"),
                    Ref("convertToUndFunc")));

            //method.Block.MethodReturn(clazz);
            return LocalMethod(method);
        }
    }
} // end of namespace