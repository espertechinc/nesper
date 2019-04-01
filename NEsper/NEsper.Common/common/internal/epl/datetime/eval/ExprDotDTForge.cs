///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.datetime.calop;
using com.espertech.esper.common.@internal.epl.datetime.dtlocal;
using com.espertech.esper.common.@internal.epl.datetime.interval;
using com.espertech.esper.common.@internal.epl.datetime.reformatop;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.dot.core;
using com.espertech.esper.common.@internal.epl.expression.time.abacus;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.rettype;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.datetime.eval
{
    public class ExprDotDTForge : ExprDotForge
    {
        private readonly DTLocalForge forge;

        public ExprDotDTForge(
            IList<CalendarForge> calendarForges, TimeAbacus timeAbacus, ReformatForge reformatForge,
            IntervalForge intervalForge, Type inputType, EventType inputEventType)
        {
            if (intervalForge != null) {
                TypeInfo = EPTypeHelper.SingleValue(typeof(bool?));
            }
            else if (reformatForge != null) {
                TypeInfo = EPTypeHelper.SingleValue(reformatForge.ReturnType);
            }
            else { // only calendar op
                if (inputEventType != null) {
                    TypeInfo = EPTypeHelper.SingleValue(
                        inputEventType.GetPropertyType(inputEventType.StartTimestampPropertyName));
                }
                else {
                    TypeInfo = EPTypeHelper.SingleValue(inputType);
                }
            }

            forge = GetForge(calendarForges, timeAbacus, inputType, inputEventType, reformatForge, intervalForge);
        }

        public ExprDotEval DotEvaluator {
            get {
                var evaluator = forge.DTEvaluator;
                ExprDotForge exprDotForge = this;
                return new ProxyExprDotEval {
                    ProcEvaluate = (target, eventsPerStream, isNewData, exprEvaluatorContext) => {
                        if (target == null) {
                            return null;
                        }

                        return evaluator.Evaluate(target, eventsPerStream, isNewData, exprEvaluatorContext);
                    },

                    ProcDotForge = () => exprDotForge
                };
            }
        }

        public CodegenExpression Codegen(
            CodegenExpression inner, Type innerType, CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol, CodegenClassScope codegenClassScope)
        {
            var methodNode = codegenMethodScope.MakeChild(
                    ((ClassEPType) TypeInfo).Clazz, typeof(ExprDotDTForge), codegenClassScope)
                .AddParam(innerType, "target");

            var block = methodNode.Block;
            if (!innerType.IsPrimitive) {
                block.IfRefNullReturnNull("target");
            }

            block.MethodReturn(forge.Codegen(Ref("target"), innerType, methodNode, exprSymbol, codegenClassScope));
            return LocalMethod(methodNode, inner);
        }

        public EPType TypeInfo { get; }

        public void Visit(ExprDotEvalVisitor visitor)
        {
            visitor.VisitDateTime();
        }

        public DTLocalForge GetForge(
            IList<CalendarForge> calendarForges, TimeAbacus timeAbacus, Type inputType, EventType inputEventType,
            ReformatForge reformatForge, IntervalForge intervalForge)
        {
            if (inputEventType == null) {
                if (reformatForge != null) {
                    if (TypeHelper.IsSubclassOrImplementsInterface(inputType, typeof(DateTimeEx))) {
                        if (calendarForges.IsEmpty()) {
                            return new DTLocalDtxReformatForge(reformatForge);
                        }

                        return new DTLocalDtxOpsReformatForge(calendarForges, reformatForge);
                    }

                    if (inputType.GetBoxedType() == typeof(long?)) {
                        if (calendarForges.IsEmpty()) {
                            return new DTLocalLongReformatForge(reformatForge);
                        }

                        return new DTLocalLongOpsReformatForge(calendarForges, reformatForge, timeAbacus);
                    }

                    if (TypeHelper.IsSubclassOrImplementsInterface(inputType, typeof(DateTimeOffset))) {
                        if (calendarForges.IsEmpty()) {
                            return new DTLocalDtoReformatForge(reformatForge);
                        }

                        return new DTLocalDtoOpsReformatForge(calendarForges, reformatForge);
                    }

                    if (TypeHelper.IsSubclassOrImplementsInterface(inputType, typeof(DateTime))) {
                        if (calendarForges.IsEmpty()) {
                            return new DTLocalDateTimeReformatForge(reformatForge);
                        }

                        return new DTLocalDateTimeOpsReformatForge(calendarForges, reformatForge);
                    }
                }
                else if (intervalForge != null) {
                    if (TypeHelper.IsSubclassOrImplementsInterface(inputType, typeof(DateTimeEx))) {
                        if (calendarForges.IsEmpty()) {
                            return new DTLocalDtxIntervalForge(intervalForge);
                        }

                        return new DTLocalDtxOpsIntervalForge(calendarForges, intervalForge);
                    }

                    if (inputType.GetBoxedType() == typeof(long?)) {
                        if (calendarForges.IsEmpty()) {
                            return new DTLocalLongIntervalForge(intervalForge);
                        }

                        return new DTLocalLongOpsIntervalForge(calendarForges, intervalForge, timeAbacus);
                    }

                    if (TypeHelper.IsSubclassOrImplementsInterface(inputType, typeof(DateTimeOffset))) {
                        if (calendarForges.IsEmpty()) {
                            return new DTLocalDtoIntervalForge(intervalForge);
                        }

                        return new DTLocalDtoOpsIntervalForge(calendarForges, intervalForge);
                    }

                    if (TypeHelper.IsSubclassOrImplementsInterface(inputType, typeof(DateTime))) {
                        if (calendarForges.IsEmpty()) {
                            return new DTLocalDateTimeIntervalForge(intervalForge);
                        }

                        return new DTLocalDateTimeOpsIntervalForge(calendarForges, intervalForge);
                    }
                }
                else { // only calendar op, nothing else
                    if (TypeHelper.IsSubclassOrImplementsInterface(inputType, typeof(DateTimeEx))) {
                        return new DTLocalDtxOpsDtxForge(calendarForges);
                    }

                    if (inputType.GetBoxedType() == typeof(long?)) {
                        return new DTLocalDtxOpsLongForge(calendarForges, timeAbacus);
                    }

                    if (TypeHelper.IsSubclassOrImplementsInterface(inputType, typeof(DateTimeOffset))) {
                        return new DTLocalDtxOpsDtoForge(calendarForges);
                    }

                    if (TypeHelper.IsSubclassOrImplementsInterface(inputType, typeof(DateTime))) {
                        return new DTLocalDtxOpsDtzForge(calendarForges);
                    }
                }

                throw new ArgumentException("Invalid input type '" + inputType + "'");
            }

            var getter = ((EventTypeSPI) inputEventType).GetGetterSPI(inputEventType.StartTimestampPropertyName);
            var getterResultType = inputEventType.GetPropertyType(inputEventType.StartTimestampPropertyName);

            if (reformatForge != null) {
                var inner = GetForge(calendarForges, timeAbacus, getterResultType, null, reformatForge, null);
                return new DTLocalBeanReformatForge(getter, getterResultType, inner, reformatForge.ReturnType);
            }

            if (intervalForge == null) { // only calendar op
                var inner = GetForge(calendarForges, timeAbacus, getterResultType, null, null, null);
                return new DTLocalBeanCalOpsForge(getter, getterResultType, inner, TypeInfo.GetNormalizedClass());
            }

            // have interval op but no end timestamp
            if (inputEventType.EndTimestampPropertyName == null) {
                var inner = GetForge(calendarForges, timeAbacus, getterResultType, null, null, intervalForge);
                return new DTLocalBeanIntervalNoEndTSForge(
                    getter, getterResultType, inner, TypeInfo.GetNormalizedClass());
            }

            // interval op and have end timestamp
            var getterEndTimestamp =
                ((EventTypeSPI) inputEventType).GetGetterSPI(inputEventType.EndTimestampPropertyName);
            var getterEndType = inputEventType.GetPropertyType(inputEventType.EndTimestampPropertyName);
            var innerX = (DTLocalForgeIntervalComp) GetForge(
                calendarForges, timeAbacus, getterResultType, null, null, intervalForge);
            return new DTLocalBeanIntervalWithEndForge(
                getter, getterResultType, getterEndTimestamp, getterEndType, innerX);
        }
    }
} // end of namespace