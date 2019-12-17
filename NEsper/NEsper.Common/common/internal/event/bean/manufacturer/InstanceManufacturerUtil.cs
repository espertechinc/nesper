///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Reflection;

using com.espertech.esper.collection;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.bean.manufacturer
{
    public class InstanceManufacturerUtil
    {
        public static Pair<ConstructorInfo, ExprForge[]> GetManufacturer(
            Type targetClass,
            ImportServiceCompileTime importService,
            ExprForge[] exprForges,
            object[] expressionReturnTypes)
        {
            var ctorTypes = new Type[expressionReturnTypes.Length];
            var forges = new ExprForge[exprForges.Length];

            for (var i = 0; i < expressionReturnTypes.Length; i++) {
                var columnType = expressionReturnTypes[i];

                if (columnType is Type || columnType == null) {
                    ctorTypes[i] = (Type) expressionReturnTypes[i];
                    forges[i] = exprForges[i];
                    continue;
                }

                if (columnType is EventType) {
                    var columnEventType = (EventType) columnType;
                    var returnType = columnEventType.UnderlyingType;
                    var inner = exprForges[i];
                    forges[i] = new InstanceManufacturerForgeNonArray(returnType, inner);
                    ctorTypes[i] = returnType;
                    continue;
                }

                // handle case where the select-clause contains an fragment array
                if (columnType is EventType[]) {
                    var columnEventType = ((EventType[]) columnType)[0];
                    var componentReturnType = columnEventType.UnderlyingType;
                    var inner = exprForges[i];
                    forges[i] = new InstanceManufacturerForgeArray(componentReturnType, inner);
                    continue;
                }

                var message = "Invalid assignment of expression " +
                              i +
                              " returning type '" +
                              columnType +
                              "', column and parameter types mismatch";
                throw new ExprValidationException(message);
            }

            try {
                var ctor = importService.ResolveCtor(targetClass, ctorTypes);
                return new Pair<ConstructorInfo, ExprForge[]>(ctor, forges);
            }
            catch (ImportException ex) {
                throw new ExprValidationException(
                    "Failed to find a suitable constructor for class '" + targetClass.Name + "': " + ex.Message,
                    ex);
            }
        }

        public class InstanceManufacturerForgeNonArray : ExprForge
        {
            private readonly ExprForge innerForge;

            internal InstanceManufacturerForgeNonArray(
                Type returnType,
                ExprForge innerForge)
            {
                EvaluationType = returnType;
                this.innerForge = innerForge;
            }

            public ExprEvaluator ExprEvaluator {
                get {
                    var inner = innerForge.ExprEvaluator;
                    return new ProxyExprEvaluator {
                        ProcEvaluate = (
                            eventsPerStream,
                            isNewData,
                            exprEvaluatorContext) => {
                            var @event = (EventBean) inner.Evaluate(
                                eventsPerStream,
                                isNewData,
                                exprEvaluatorContext);
                            if (@event != null) {
                                return @event.Underlying;
                            }

                            return null;
                        }
                    };
                }
            }

            public ExprForgeConstantType ForgeConstantType => ExprForgeConstantType.NONCONST;

            public CodegenExpression EvaluateCodegen(
                Type requiredType,
                CodegenMethodScope codegenMethodScope,
                ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope)
            {
                var methodNode = codegenMethodScope.MakeChild(
                    EvaluationType,
                    typeof(InstanceManufacturerForgeNonArray),
                    codegenClassScope);

                methodNode.Block
                    .DeclareVar<EventBean>(
                        "@event",
                        Cast(
                            typeof(EventBean),
                            innerForge.EvaluateCodegen(requiredType, methodNode, exprSymbol, codegenClassScope)))
                    .IfRefNullReturnNull("@event")
                    .MethodReturn(Cast(EvaluationType, ExprDotUnderlying(Ref("@event"))));
                return LocalMethod(methodNode);
            }

            public Type EvaluationType { get; }

            public ExprNodeRenderable ExprForgeRenderable => innerForge.ExprForgeRenderable;
        }

        public class InstanceManufacturerForgeArray : ExprForge,
            ExprNodeRenderable
        {
            private readonly Type componentReturnType;
            private readonly ExprForge innerForge;

            internal InstanceManufacturerForgeArray(
                Type componentReturnType,
                ExprForge innerForge)
            {
                this.componentReturnType = componentReturnType;
                this.innerForge = innerForge;
            }

            public ExprEvaluator ExprEvaluator {
                get {
                    var inner = innerForge.ExprEvaluator;
                    return new ProxyExprEvaluator {
                        ProcEvaluate = (
                            eventsPerStream,
                            isNewData,
                            exprEvaluatorContext) => {
                            var result = inner.Evaluate(eventsPerStream, isNewData, exprEvaluatorContext);
                            if (!(result is EventBean[])) {
                                return null;
                            }

                            var events = (EventBean[]) result;
                            var values = Array.CreateInstance(componentReturnType, events.Length);
                            for (var i = 0; i < events.Length; i++) {
                                values.SetValue(events[i].Underlying, i);
                            }

                            return values;
                        }
                    };
                }
            }

            public ExprForgeConstantType ForgeConstantType => ExprForgeConstantType.NONCONST;

            public CodegenExpression EvaluateCodegen(
                Type requiredType,
                CodegenMethodScope codegenMethodScope,
                ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope)
            {
                var arrayType = TypeHelper.GetArrayType(componentReturnType);
                var methodNode = codegenMethodScope.MakeChild(
                    arrayType,
                    typeof(InstanceManufacturerForgeArray),
                    codegenClassScope);

                methodNode.Block
                    .DeclareVar<object>(
                        "result",
                        innerForge.EvaluateCodegen(requiredType, methodNode, exprSymbol, codegenClassScope))
                    .IfCondition(Not(InstanceOf(Ref("result"), typeof(EventBean[]))))
                    .BlockReturn(ConstantNull())
                    .DeclareVar<EventBean[]>("events", Cast(typeof(EventBean[]), Ref("result")))
                    .DeclareVar(arrayType, "values", NewArrayByLength(componentReturnType, ArrayLength(Ref("events"))))
                    .ForLoopIntSimple("i", ArrayLength(Ref("events")))
                    .AssignArrayElement(
                        "values",
                        Ref("i"),
                        Cast(
                            componentReturnType,
                            ExprDotName(ArrayAtIndex(Ref("events"), Ref("i")), "Underlying")))
                    .BlockEnd()
                    .MethodReturn(Ref("values"));
                return LocalMethod(methodNode);
            }

            public Type EvaluationType => TypeHelper.GetArrayType(componentReturnType);

            public ExprNodeRenderable ExprForgeRenderable => this;

            public void ToEPL(
                TextWriter writer,
                ExprPrecedenceEnum parentPrecedence)
            {
                writer.Write(GetType().GetSimpleName());
            }
        }
    }
} // end of namespace