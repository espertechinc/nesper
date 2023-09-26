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
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.expression.codegen.ExprForgeCodegenNames;
using static com.espertech.esper.common.@internal.metrics.instrumentation.InstrumentationCode;

namespace com.espertech.esper.common.@internal.epl.updatehelper
{
    public class EventBeanUpdateHelperForge
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly EventType eventType;
        private readonly EventBeanCopyMethodForge copyMethod;
        private readonly EventBeanUpdateItemForge[] updateItems;

        public EventBeanUpdateHelperForge(
            EventType eventType,
            EventBeanCopyMethodForge copyMethod,
            EventBeanUpdateItemForge[] updateItems)
        {
            this.eventType = eventType;
            this.copyMethod = copyMethod;
            this.updateItems = updateItems;
        }

        public CodegenExpression MakeWCopy(
            CodegenMethodScope scope,
            CodegenClassScope classScope)
        {
            var copyMethodField = classScope.AddDefaultFieldUnshared(
                true,
                typeof(EventBeanCopyMethod),
                copyMethod.MakeCopyMethodClassScoped(classScope));
            var method = scope.MakeChild(typeof(EventBeanUpdateHelperWCopy), GetType(), classScope);
            var updateInternal = MakeUpdateInternal(method, classScope);
            
            // var clazz = NewAnonymousClass(method.Block, typeof(EventBeanUpdateHelperWCopy));
            // var updateWCopy = CodegenMethod.MakeParentNode(typeof(EventBean), GetType(), classScope)
            //     .AddParam<EventBean>("matchingEvent")
            //     .AddParam<EventBean[]>(NAME_EPS)
            //     .AddParam<ExprEvaluatorContext>(NAME_EXPREVALCONTEXT);
            // clazz.AddMethod("updateWCopy", updateWCopy);
            
            var updateWCopy = new CodegenExpressionLambda(method.Block)
                .WithParam(typeof(EventBean), "matchingEvent")
                .WithParam(typeof(EventBean[]), NAME_EPS)
                .WithParam(typeof(ExprEvaluatorContext), NAME_EXPREVALCONTEXT);
            var clazz = NewInstance<EventBeanUpdateHelperWCopy>(updateWCopy);
            
            updateWCopy.Block
                .Apply(
                    Instblock(
                        classScope,
                        "qInfraUpdate",
                        Ref("matchingEvent"),
                        REF_EPS,
                        Constant(updateItems.Length),
                        ConstantTrue()))
                .DeclareVar<EventBean>("copy", ExprDotMethod(copyMethodField, "copy", Ref("matchingEvent")))
                .AssignArrayElement(REF_EPS, Constant(0), Ref("copy"))
                .AssignArrayElement(REF_EPS, Constant(2), Ref("matchingEvent"))
                .LocalMethod(updateInternal, REF_EPS, REF_EXPREVALCONTEXT, Ref("copy"))
                .Apply(Instblock(classScope, "aInfraUpdate", Ref("copy")))
                .ReturnMethodOrBlock(Ref("copy"));
            method.Block.MethodReturn(clazz);
            return LocalMethod(method);
        }

        public CodegenExpression MakeNoCopy(
            CodegenMethodScope scope,
            CodegenClassScope classScope)
        {
            var method = scope.MakeChild(typeof(EventBeanUpdateHelperNoCopy), GetType(), classScope);
            var updateInternal = MakeUpdateInternal(method, classScope);
            
            // var clazz = NewAnonymousClass(method.Block, typeof(EventBeanUpdateHelperNoCopy));
            // var updateNoCopy = CodegenMethod.MakeParentNode(typeof(void), GetType(), classScope)
            //     .AddParam<EventBean>("matchingEvent")
            //     .AddParam<EventBean[]>(NAME_EPS)
            //     .AddParam<ExprEvaluatorContext>(NAME_EXPREVALCONTEXT);
            // clazz.AddMethod("updateNoCopy", updateNoCopy);
            
            var eventBeanUpdateHelper = Ref("eventBeanUpdateHelper");
            method.Block.DeclareVar(
                typeof(ProxyEventBeanUpdateHelperNoCopy),
                eventBeanUpdateHelper.Ref,
                NewInstance<ProxyEventBeanUpdateHelperNoCopy>());
            
            method.Block
                .SetProperty(
                    eventBeanUpdateHelper,
                    "ProcIsRequiresStream2InitialValueEvent",
                    new CodegenExpressionLambda(method.Block)
                        .WithBody(block => block.BlockReturn(Constant(IsRequiresStream2InitialValueEvent))))
                .SetProperty(
                    eventBeanUpdateHelper,
                    "ProcUpdatedProperties",
                    new CodegenExpressionLambda(method.Block)
                        .WithBody(block => block.BlockReturn(Constant(UpdateItemsPropertyNames))))
                .SetProperty(
                    eventBeanUpdateHelper,
                    "ProcUpdateNoCopy",
                    new CodegenExpressionLambda(method.Block)
                        .WithParam(typeof(EventBean), "matchingEvent")
                        .WithParam(typeof(EventBean[]), NAME_EPS)
                        .WithParam(typeof(ExprEvaluatorContext), NAME_EXPREVALCONTEXT)
                        .WithBody(
                            block => block
                                .Apply(
                                    Instblock(
                                        classScope,
                                        "qInfraUpdate",
                                        Ref("matchingEvent"),
                                        REF_EPS,
                                        Constant(UpdateItems.Length),
                                        ConstantFalse()))
                                .LocalMethod(updateInternal, REF_EPS, REF_EXPREVALCONTEXT, Ref("matchingEvent"))
                                .Apply(Instblock(classScope, "aInfraUpdate", Ref("matchingEvent")))));

            method.Block.MethodReturn(eventBeanUpdateHelper);
            return LocalMethod(method);
        }

        private CodegenMethod MakeUpdateInternal(
            CodegenMethodScope scope,
            CodegenClassScope classScope)
        {
            var method = scope
                .MakeChildWithScope(typeof(void), GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope)
                .AddParam<EventBean[]>(NAME_EPS)
                .AddParam<ExprEvaluatorContext>(NAME_EXPREVALCONTEXT)
                .AddParam<EventBean>("target");
            var exprSymbol = new ExprForgeCodegenSymbol(true, true);
            var exprMethod = method.MakeChildWithScope(
                    typeof(void),
                    typeof(CodegenLegoMethodExpression),
                    exprSymbol,
                    classScope)
                .AddParam(PARAMS);
            var types = new Type[updateItems.Length];
            for (var i = 0; i < updateItems.Length; i++) {
                types[i] = updateItems[i].Expression.EvaluationType;
            }

            var forgeExpressions = new EventBeanUpdateItemForgeWExpressions[updateItems.Length];
            for (var i = 0; i < updateItems.Length; i++) {
                var nullableType = types[i] == null || types[i] == null ? null : types[i];
                var targetType = updateItems[i].IsUseUntypedAssignment ? typeof(object) : nullableType;
                forgeExpressions[i] = updateItems[i].ToExpression(targetType, exprMethod, exprSymbol, classScope);
            }

            exprSymbol.DerivedSymbolsCodegen(method, method.Block, classScope);
            method.Block.DeclareVar(
                eventType.UnderlyingType,
                "und",
                Cast(eventType.UnderlyingType, ExprDotUnderlying(Ref("target"))));
            for (var i = 0; i < updateItems.Length; i++) {
                var updateItem = updateItems[i];
                var rhs = forgeExpressions[i].RhsExpression;
                if (updateItems[i].IsUseTriggeringEvent) {
                    rhs = ArrayAtIndex(Ref(NAME_EPS), Constant(1));
                }

                method.Block.Apply(Instblock(classScope, "qInfraUpdateRHSExpr", Constant(i)));
                var type = types[i];
                if ((type == null) && updateItem.OptionalWriter != null) {
                    method.Block.Expression(
                        updateItem.OptionalWriter.WriteCodegen(
                            ConstantNull(),
                            Ref("und"),
                            Ref("target"),
                            method,
                            classScope));
                    continue;
                }

                if (type != null &&
                    (type.IsTypeVoid() ||
                     (updateItem.OptionalWriter == null && updateItem.OptionalArray == null))) {
                    method.Block.Expression(rhs).Apply(Instblock(classScope, "aInfraUpdateRHSExpr", ConstantNull()));
                    continue;
                }

                var targetType = typeof(object);
                if (!updateItems[i].IsUseUntypedAssignment && type is Type type1) {
                    targetType = type1;
                }

                var @ref = Ref("r" + i);
                method.Block.DeclareVar(targetType, @ref.Ref, rhs);
                CodegenExpression assigned = @ref;
                if (updateItem.OptionalWidener != null) {
                    assigned = updateItem.OptionalWidener.WidenCodegen(@ref, method, classScope);
                }

                if (updateItem.OptionalArray != null) {
                    // handle array value with array index expression
                    var index = Ref("i" + i);
                    var array = Ref("a" + i);
                    var arraySet = updateItem.OptionalArray;
                    CodegenBlock arrayBlock;
                    var arrayOfPrimitiveNullRHS =
                        arraySet.ArrayType.GetComponentType().IsPrimitive && (type == null || !type.IsPrimitive);
                    if (arrayOfPrimitiveNullRHS) {
                        arrayBlock = method.Block.IfNull(@ref)
                            .StaticMethod(
                                typeof(EventBeanUpdateHelperForge),
                                "logWarnWhenNullAndNotNullable",
                                Constant(updateItem.OptionalPropertyName))
                            .IfElse();
                    }
                    else {
                        arrayBlock = method.Block;
                    }

                    arrayBlock.DeclareVar(typeof(int?), index.Ref, forgeExpressions[i].OptionalArrayExpressions.Index)
                        .IfRefNotNull(index.Ref)
                        .DeclareVar(
                            arraySet.ArrayType,
                            array.Ref,
                            forgeExpressions[i].OptionalArrayExpressions.ArrayGet)
                        .IfRefNotNull(array.Ref)
                        .IfCondition(
                            Relational(index, CodegenExpressionRelational.CodegenRelational.LT, ArrayLength(array)))
                        .AssignArrayElement(array, Cast(typeof(int), index), assigned)
                        .IfElse()
                        .BlockThrow(
                            NewInstance(
                                typeof(EPException),
                                Concat(
                                    Constant("Array length "),
                                    ArrayLength(array),
                                    Constant(" less than index "),
                                    index,
                                    Constant(" for property '" + updateItems[i].OptionalArray.PropertyName + "'"))))
                        .BlockEnd()
                        .BlockEnd();
                    if (arrayOfPrimitiveNullRHS) {
                        arrayBlock.BlockEnd();
                    }
                }
                else {
                    // handle regular values
                    if (!type.IsPrimitive && updateItem.IsNotNullableField) {
                        method.Block.IfNull(@ref)
                            .StaticMethod(
                                typeof(EventBeanUpdateHelperForge),
                                "logWarnWhenNullAndNotNullable",
                                Constant(updateItem.OptionalPropertyName))
                            .IfElse()
                            .Expression(
                                updateItem.OptionalWriter.WriteCodegen(
                                    assigned,
                                    Ref("und"),
                                    Ref("target"),
                                    method,
                                    classScope))
                            .BlockEnd();
                    }
                    else {
                        method.Block.Expression(
                            updateItem.OptionalWriter.WriteCodegen(
                                assigned,
                                Ref("und"),
                                Ref("target"),
                                method,
                                classScope));
                    }
                }

                method.Block.Apply(Instblock(classScope, "aInfraUpdateRHSExpr", assigned));
            }

            return method;
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name = "propertyName">name</param>
        public static void LogWarnWhenNullAndNotNullable(string propertyName)
        {
            Log.Warn(
                "Null value returned by expression for assignment to property '" +
                propertyName +
                "' is ignored as the property type is not nullable for expression");
        }

        public bool IsRequiresStream2InitialValueEvent => copyMethod != null;

        public EventBeanUpdateItemForge[] UpdateItems => updateItems;

        public string[] UpdateItemsPropertyNames {
            get {
                IList<string> properties = new List<string>();
                foreach (var item in updateItems) {
                    if (item.OptionalPropertyName != null) {
                        properties.Add(item.OptionalPropertyName);
                    }
                }

                return properties.ToArray();
            }
        }
    }
} // end of namespace