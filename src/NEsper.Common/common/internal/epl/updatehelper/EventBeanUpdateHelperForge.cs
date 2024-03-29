///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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

        private readonly EventType _eventType;
        private readonly EventBeanCopyMethodForge _copyMethod;
        private readonly EventBeanUpdateItemForge[] _updateItems;

        public EventBeanUpdateHelperForge(
            EventType eventType,
            EventBeanCopyMethodForge copyMethod,
            EventBeanUpdateItemForge[] updateItems)
        {
            _eventType = eventType;
            _copyMethod = copyMethod;
            _updateItems = updateItems;
        }

        public CodegenExpression MakeWCopy(
            CodegenMethodScope scope,
            CodegenClassScope classScope)
        {
            var copyMethodField = classScope.AddDefaultFieldUnshared(
                true,
                typeof(EventBeanCopyMethod),
                _copyMethod.MakeCopyMethodClassScoped(classScope));
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
                        Constant(_updateItems.Length),
                        ConstantTrue()))
                .DeclareVar<EventBean>("copy", ExprDotMethod(copyMethodField, "Copy", Ref("matchingEvent")))
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
            method.Block.DeclareVar<ProxyEventBeanUpdateHelperNoCopy>(
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
            
            var types = new Type[_updateItems.Length];
            for (var i = 0; i < _updateItems.Length; i++) {
                types[i] = _updateItems[i].Expression.EvaluationType;
            }

            var forgeExpressions = new EventBeanUpdateItemForgeWExpressions[_updateItems.Length];
            for (var i = 0; i < _updateItems.Length; i++) {
                var nullableType = types[i] == null || types[i] == null ? null : types[i];
                var targetType = _updateItems[i].IsUseUntypedAssignment ? typeof(object) : nullableType;
                forgeExpressions[i] = _updateItems[i].ToExpression(targetType, exprMethod, exprSymbol, classScope);
            }

            exprSymbol.DerivedSymbolsCodegen(method, method.Block, classScope);
            method.Block.DeclareVar(
                _eventType.UnderlyingType,
                "und",
                Cast(_eventType.UnderlyingType, ExprDotUnderlying(Ref("target"))));
            
            for (var i = 0; i < _updateItems.Length; i++) {
                var updateItem = _updateItems[i];
                var rhs = forgeExpressions[i].RhsExpression;
                if (_updateItems[i].IsUseTriggeringEvent) {
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
                if (!_updateItems[i].IsUseUntypedAssignment) {
                    targetType = type;
                }

                var @ref = Ref("r" + i);
                method.Block.DeclareVar(targetType, @ref.Ref, rhs);
                
                CodegenExpression assigned = @ref;
                var assignedType = types[i];
                if (updateItem.OptionalWidener != null) {
                    assigned = updateItem.OptionalWidener.WidenCodegen(@ref, method, classScope);
                    assignedType = updateItem.OptionalWidener.WidenResultType;
                }

                if (updateItem.OptionalArray != null) {
                    // handle array value with array index expression
                    var index = Ref("i" + i);
                    var array = Ref("a" + i);
                    var arraySet = updateItem.OptionalArray;
                    CodegenBlock arrayBlock;

                    var elementType = arraySet.ArrayType.GetComponentType();
                    var arrayOfPrimitiveNullRHS = elementType.IsPrimitive && (assignedType == null || assignedType.CanBeNull());
                    if (arrayOfPrimitiveNullRHS) {
                        assigned = Unbox(assigned, assignedType);
                        arrayBlock = method.Block.IfNull(@ref)
                            .StaticMethod(
                                typeof(EventBeanUpdateHelperForge),
                                "LogWarnWhenNullAndNotNullable",
                                Constant(updateItem.OptionalPropertyName))
                            .IfElse();
                    }
                    else {
                        arrayBlock = method.Block;
                    }

                    arrayBlock
                        .DeclareVar<int?>(index.Ref, forgeExpressions[i].OptionalArrayExpressions.Index)
                        .IfRefNotNull(index.Ref)
                        .DeclareVar(arraySet.ArrayType, array.Ref, forgeExpressions[i].OptionalArrayExpressions.ArrayGet)
                        .IfRefNotNull(array.Ref)
                        .IfCondition(Relational(index, CodegenExpressionRelational.CodegenRelational.LT, ArrayLength(array)))
                        .CommentFullLine("MakeUpdateInternal//AssignArrayElement")
                        .AssignArrayElement(array, Unbox(index), assigned)
                        .IfElse()
                        .BlockThrow(
                            NewInstance<EPException>(
                                Concat(
                                    Constant("Array length "),
                                    ArrayLength(array),
                                    Constant(" less than index "),
                                    index,
                                    Constant(" for property '" + _updateItems[i].OptionalArray.PropertyName + "'"))))
                        .BlockEnd()
                        .BlockEnd();
                    if (arrayOfPrimitiveNullRHS) {
                        arrayBlock.BlockEnd();
                    }
                }
                else {
                    // handle regular values
                    if (type.CanBeNull() && updateItem.IsNotNullableField) {
                        method.Block.IfNull(@ref)
                            .StaticMethod(
                                typeof(EventBeanUpdateHelperForge),
                                "LogWarnWhenNullAndNotNullable",
                                Constant(updateItem.OptionalPropertyName))
                            .IfElse()
                            .Expression(
                                updateItem.OptionalWriter.WriteCodegen(
                                    Unbox(assigned, assignedType),
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

        public bool IsRequiresStream2InitialValueEvent => _copyMethod != null;

        public EventBeanUpdateItemForge[] UpdateItems => _updateItems;

        public string[] UpdateItemsPropertyNames {
            get {
                IList<string> properties = new List<string>();
                foreach (var item in _updateItems) {
                    if (item.OptionalPropertyName != null) {
                        properties.Add(item.OptionalPropertyName);
                    }
                }

                return properties.ToArray();
            }
        }
    }
} // end of namespace