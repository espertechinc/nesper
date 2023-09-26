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
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.compile.multikey;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.table.compiletime;
using com.espertech.esper.common.@internal.epl.table.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.serde.compiletime.resolve;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.expression.codegen.ExprForgeCodegenNames;

namespace com.espertech.esper.common.@internal.context.aifactory.createtable
{
    public class StatementAgentInstanceFactoryCreateTableForge
    {
        private readonly string className;
        private readonly string tableName;
        private readonly TableAccessAnalysisResult _plan;
        private readonly bool isTargetHA;

        public StatementAgentInstanceFactoryCreateTableForge(
            string className,
            string tableName,
            TableAccessAnalysisResult plan,
            bool isTargetHA)
        {
            this.className = className;
            this.tableName = tableName;
            this._plan = plan;
            this.isTargetHA = isTargetHA;
        }

        public CodegenMethod InitializeCodegen(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            // add aggregation row+factory+serde as inner classes
            var aggregationClassNames = new AggregationClassNames();
            var inners = AggregationServiceFactoryCompiler.MakeTable(
                AggregationCodegenRowLevelDesc.FromTopOnly(_plan.AggDesc),
                GetType(),
                classScope,
                aggregationClassNames,
                className,
                isTargetHA);
            classScope.AddInnerClasses(inners);

            var method = parent.MakeChild(typeof(StatementAgentInstanceFactoryCreateTable), GetType(), classScope);

            var primaryKeyGetter = MultiKeyCodegen.CodegenGetterMayMultiKey(
                _plan.InternalEventType,
                _plan.PrimaryKeyGetters,
                _plan.PrimaryKeyTypes,
                null,
                _plan.PrimaryKeyMultikeyClasses,
                method,
                classScope);
            var fafTransform = MultiKeyCodegen.CodegenMultiKeyFromArrayTransform(
                _plan.PrimaryKeyMultikeyClasses,
                method,
                classScope);
            var intoTableTransform = MultiKeyCodegen.CodegenMultiKeyFromMultiKeyTransform(
                _plan.PrimaryKeyMultikeyClasses,
                method,
                classScope);

            method.Block
                .DeclareVarNewInstance(typeof(StatementAgentInstanceFactoryCreateTable), "saiff")
                .ExprDotMethod(Ref("saiff"), "setTableName", Constant(tableName))
                .ExprDotMethod(
                    Ref("saiff"),
                    "setPublicEventType",
                    EventTypeUtility.ResolveTypeCodegen(_plan.PublicEventType, symbols.GetAddInitSvc(method)))
                .ExprDotMethod(Ref("saiff"), "setEventToPublic", MakeEventToPublic(method, symbols, classScope))
                .ExprDotMethod(
                    Ref("saiff"),
                    "setAggregationRowFactory",
                    CodegenExpressionBuilder.NewInstanceInner(aggregationClassNames.RowFactoryTop, Ref("this")))
                .ExprDotMethod(
                    Ref("saiff"),
                    "setAggregationSerde",
                    CodegenExpressionBuilder.NewInstanceInner(aggregationClassNames.RowSerdeTop, Ref("this")))
                .ExprDotMethod(Ref("saiff"), "setPrimaryKeyGetter", primaryKeyGetter)
                .ExprDotMethod(
                    Ref("saiff"),
                    "setPrimaryKeySerde",
                    _plan.PrimaryKeyMultikeyClasses.GetExprMKSerde(method, classScope))
                .ExprDotMethod(
                    Ref("saiff"),
                    "setPropertyForges",
                    DataInputOutputSerdeForgeExtensions.CodegenArray(
                        _plan.InternalEventTypePropertySerdes,
                        method,
                        classScope,
                        ExprDotName(symbols.GetAddInitSvc(method), EPStatementInitServicesConstants.EVENTTYPERESOLVER)))
                .ExprDotMethod(Ref("saiff"), "setPrimaryKeyObjectArrayTransform", fafTransform)
                .ExprDotMethod(Ref("saiff"), "setPrimaryKeyIntoTableTransform", intoTableTransform)
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
            var factory =
                classScope.AddOrGetDefaultFieldSharable(EventBeanTypedEventFactoryCodegenField.INSTANCE);
            var eventType = classScope.AddDefaultFieldUnshared(
                true,
                typeof(EventType),
                EventTypeUtility.ResolveTypeCodegen(_plan.PublicEventType, EPStatementInitServicesConstants.REF));

            CodegenExpressionLambda convertToUnd = new CodegenExpressionLambda(method.Block)
                .WithParams(new CodegenNamedParam(typeof(EventBean), "@event"))
                .WithParams(PARAMS);

            // CodegenExpressionNewAnonymousClass clazz = NewAnonymousClass(
            //     method.Block,
            //     typeof(TableMetadataInternalEventToPublic));

            convertToUnd.Block
                .DeclareVar<object[]>(
                    "props",
                    ExprDotName(Cast(typeof(ObjectArrayBackedEventBean), Ref("@event")), "Properties"))
                .DeclareVar<object[]>(
                    "data",
                    NewArrayByLength(typeof(object), Constant(_plan.PublicEventType.PropertyNames.Length)));
            
            // var convert = CodegenMethod.MakeParentNode(typeof(EventBean), GetType(), classScope)
            //     .AddParam<EventBean>("event")
            //     .AddParam(PARAMS);
            // clazz.AddMethod("convert", convert);
            // convert.Block
            //     .DeclareVar(
            //         typeof(object[]),
            //         "data",
            //         ExprDotMethod(
            //             Ref("this"),
            //             "convertToUnd",
            //             Ref("event"),
            //             REF_EPS,
            //             REF_ISNEWDATA,
            //             REF_EXPREVALCONTEXT))
            //     .MethodReturn(ExprDotMethod(factory, "adapterForTypedObjectArray", Ref("data"), eventType));

            // var convertToUnd = CodegenMethod.MakeParentNode(typeof(object[]), GetType(), classScope)
            //     .AddParam<EventBean>("event")
            //     .AddParam(PARAMS);
            // clazz.AddMethod("convertToUnd", convertToUnd);
            // convertToUnd.Block
            //     .DeclareVar(
            //         typeof(object[]),
            //         "props",
            //         ExprDotMethod(Cast(typeof(ObjectArrayBackedEventBean), Ref("event")), "getProperties"))
            //     .DeclareVar(
            //         typeof(object[]),
            //         "data",
            //         NewArrayByLength(typeof(object), Constant(plan.PublicEventType.PropertyNames.Length)));
            
            foreach (var plain in _plan.ColsPlain) {
                convertToUnd.Block.AssignArrayElement(
                    Ref("data"),
                    Constant(plain.Dest),
                    ArrayAtIndex(Ref("props"), Constant(plain.Source)));
            }

            if (_plan.ColsAggMethod.Length > 0 || _plan.ColsAccess.Length > 0) {
                convertToUnd.Block.DeclareVar<AggregationRow>("row",
                    Cast(typeof(AggregationRow), ArrayAtIndex(Ref("props"), Constant(0))));
                var count = 0;

                foreach (var aggMethod in _plan.ColsAggMethod) {
                    // Code: data[method.getDest()] = row.getMethods()[count++].getValue();
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

                foreach (var aggAccess in _plan.ColsAccess) {
                    // Code: data[method.getDest()] = row.getMethods()[count++].getValue();
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