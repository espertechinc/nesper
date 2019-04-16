///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
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
                AggregationCodegenRowLevelDesc.FromTopOnly(plan.AggDesc), GetType(), classScope, aggregationClassNames,
                className);
            classScope.AddInnerClasses(inners);

            var method = parent.MakeChild(typeof(StatementAgentInstanceFactoryCreateTable), GetType(), classScope);

            var primaryKeyGetter = ConstantNull();
            if (plan.PrimaryKeyGetters != null) {
                primaryKeyGetter = EventTypeUtility.CodegenGetterMayMultiKeyWCoerce(
                    plan.InternalEventType, plan.PrimaryKeyGetters, plan.PrimaryKeyTypes, null, method, GetType(),
                    classScope);
            }

            method.Block
                .DeclareVar(
                    typeof(StatementAgentInstanceFactoryCreateTable), "saiff",
                    NewInstance(typeof(StatementAgentInstanceFactoryCreateTable)))
                .ExprDotMethod(Ref("saiff"), "setTableName", Constant(tableName))
                .ExprDotMethod(
                    Ref("saiff"), "setPublicEventType",
                    EventTypeUtility.ResolveTypeCodegen(plan.PublicEventType, symbols.GetAddInitSvc(method)))
                .ExprDotMethod(Ref("saiff"), "setEventToPublic", MakeEventToPublic(method, symbols, classScope))
                .ExprDotMethod(
                    Ref("saiff"), "setAggregationRowFactory",
                    NewInstance(aggregationClassNames.RowFactoryTop, Ref("this")))
                .ExprDotMethod(
                    Ref("saiff"), "setAggregationSerde", NewInstance(aggregationClassNames.RowSerdeTop, Ref("this")))
                .ExprDotMethod(Ref("saiff"), "setPrimaryKeyGetter", primaryKeyGetter)
                .ExprDotMethod(symbols.GetAddInitSvc(method), "addReadyCallback", Ref("saiff"))
                .MethodReturn(Ref("saiff"));
            return method;
        }

        private CodegenExpression MakeEventToPublic(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(TableMetadataInternalEventToPublic), GetType(), classScope);
            var factory = classScope.AddOrGetFieldSharable(EventBeanTypedEventFactoryCodegenField.INSTANCE);
            var eventType = classScope.AddFieldUnshared(
                true, typeof(EventType),
                EventTypeUtility.ResolveTypeCodegen(plan.PublicEventType, EPStatementInitServicesConstants.REF));

            var clazz = NewAnonymousClass(method.Block, typeof(TableMetadataInternalEventToPublic));

            var convert = CodegenMethod.MakeParentNode(typeof(EventBean), GetType(), classScope)
                .AddParam(typeof(EventBean), "event").AddParam(PARAMS);
            clazz.AddMethod("convert", convert);
            convert.Block
                .DeclareVar(
                    typeof(object[]), "data",
                    ExprDotMethod(
                        Ref("this"), "convertToUnd", Ref("event"), REF_EPS, REF_ISNEWDATA, REF_EXPREVALCONTEXT))
                .MethodReturn(ExprDotMethod(factory, "adapterForTypedObjectArray", Ref("data"), eventType));

            var convertToUnd = CodegenMethod.MakeParentNode(typeof(object[]), GetType(), classScope)
                .AddParam(typeof(EventBean), "event").AddParam(PARAMS);
            clazz.AddMethod("convertToUnd", convertToUnd);
            convertToUnd.Block
                .DeclareVar(
                    typeof(object[]), "props",
                    ExprDotMethod(Cast(typeof(ObjectArrayBackedEventBean), Ref("event")), "getProperties"))
                .DeclareVar(
                    typeof(object[]), "data",
                    NewArrayByLength(typeof(object), Constant(plan.PublicEventType.PropertyNames.Length)));
            foreach (TableMetadataColumnPairPlainCol plain in plan.ColsPlain) {
                convertToUnd.Block.AssignArrayElement(
                    Ref("data"), Constant(plain.Dest), ArrayAtIndex(Ref("props"), Constant(plain.Source)));
            }

            if (plan.ColsAggMethod.Length > 0 || plan.ColsAccess.Length > 0) {
                convertToUnd.Block.DeclareVar(
                    typeof(AggregationRow), "row",
                    Cast(typeof(AggregationRow), ArrayAtIndex(Ref("props"), Constant(0))));
                var count = 0;

                foreach (TableMetadataColumnPairAggMethod aggMethod in plan.ColsAggMethod) {
                    // Code: data[method.getDest()] = row.getMethods()[count++].getValue();
                    convertToUnd.Block.AssignArrayElement(
                        Ref("data"), Constant(aggMethod.Dest),
                        ExprDotMethod(
                            Ref("row"), "getValue", Constant(count), REF_EPS, REF_ISNEWDATA, REF_EXPREVALCONTEXT));
                    count++;
                }

                foreach (TableMetadataColumnPairAggAccess aggAccess in plan.ColsAccess) {
                    // Code: data[method.getDest()] = row.getMethods()[count++].getValue();
                    convertToUnd.Block.AssignArrayElement(
                        Ref("data"), Constant(aggAccess.Dest),
                        ExprDotMethod(
                            Ref("row"), "getValue", Constant(count), REF_EPS, REF_ISNEWDATA, REF_EXPREVALCONTEXT));
                    count++;
                }
            }

            convertToUnd.Block.MethodReturn(Ref("data"));

            method.Block.MethodReturn(clazz);
            return LocalMethod(method);
        }
    }
} // end of namespace