///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Globalization;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.datetime.calop;
using com.espertech.esper.common.@internal.epl.datetime.dtlocal;
using com.espertech.esper.common.@internal.epl.datetime.interval;
using com.espertech.esper.common.@internal.epl.datetime.reformatop;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
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
        private readonly DTLocalForge _forge;
        private readonly EPChainableTypeClass _returnType;

        public ExprDotDTForge(
            IList<CalendarForge> calendarForges,
            TimeAbacus timeAbacus,
            ReformatForge reformatForge,
            IntervalForge intervalForge,
            Type inputType,
            EventType inputEventType)
        {
            if (intervalForge != null) {
                _returnType = EPChainableTypeHelper.SingleValueNonNull(typeof(bool?));
            }
            else if (reformatForge != null) {
                _returnType = EPChainableTypeHelper.SingleValueNonNull(reformatForge.ReturnType);
            }
            else { // only calendar op
                if (inputEventType != null) {
                    _returnType = EPChainableTypeHelper.SingleValueNonNull(
                        inputEventType.GetPropertyType(inputEventType.StartTimestampPropertyName));
                }
                else {
                    _returnType = EPChainableTypeHelper.SingleValueNonNull(inputType);
                }
            }

            _forge = GetForge(calendarForges, timeAbacus, inputType, inputEventType, reformatForge, intervalForge);
        }

        public CodegenExpression Codegen(
            CodegenExpression inner,
            Type innerType,
            CodegenMethodScope parent,
            ExprForgeCodegenSymbol symbols,
            CodegenClassScope classScope)
        {
            var methodNode = parent
                .MakeChild(_returnType.Clazz, typeof(ExprDotDTForge), classScope)
                .AddParam(innerType, "target");

            var targetValue = Unbox(Ref("target"), innerType);
            var block = methodNode.Block;

            if (!innerType.CanBeNull()) {
                block.IfRefNullReturnNull("target");
            }

            block.MethodReturn(_forge.Codegen(targetValue, innerType, methodNode, symbols, classScope));
            return LocalMethod(methodNode, inner);
        }

        public EPChainableType TypeInfo => _returnType;

        public void Visit(ExprDotEvalVisitor visitor)
        {
            visitor.VisitDateTime();
        }

        public DTLocalForge GetForge(
            IList<CalendarForge> calendarForges,
            TimeAbacus timeAbacus,
            Type inputType,
            EventType inputEventType,
            ReformatForge reformatForge,
            IntervalForge intervalForge)
        {
            if (inputEventType == null) {
                var inputTypeBoxed = inputType.GetBoxedType();
                if (reformatForge != null) {
                    if (TypeHelper.IsSubclassOrImplementsInterface(inputType, typeof(DateTimeEx))) {
                        if (calendarForges.IsEmpty()) {
                            return new DTLocalDtxReformatForge(reformatForge);
                        }

                        return new DTLocalDtxOpsReformatForge(calendarForges, reformatForge);
                    }

                    if (inputTypeBoxed == typeof(long?)) {
                        if (calendarForges.IsEmpty()) {
                            return new DTLocalLongReformatForge(reformatForge);
                        }

                        return new DTLocalLongOpsReformatForge(calendarForges, reformatForge, timeAbacus);
                    }

                    if (inputTypeBoxed == typeof(DateTimeOffset?)) {
                        if (calendarForges.IsEmpty()) {
                            return new DTLocalDtoReformatForge(reformatForge);
                        }

                        return new DTLocalDtoOpsReformatForge(calendarForges, reformatForge);
                    }

                    if (inputTypeBoxed == typeof(DateTime?)) {
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

                    if (inputTypeBoxed == typeof(long?)) {
                        if (calendarForges.IsEmpty()) {
                            return new DTLocalLongIntervalForge(intervalForge);
                        }

                        return new DTLocalLongOpsIntervalForge(calendarForges, intervalForge, timeAbacus);
                    }

                    if (inputTypeBoxed == typeof(DateTimeOffset?)) {
                        if (calendarForges.IsEmpty()) {
                            return new DTLocalDtoIntervalForge(intervalForge);
                        }

                        return new DTLocalDtoOpsIntervalForge(calendarForges, intervalForge);
                    }

                    if (inputTypeBoxed == typeof(DateTime?)) {
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

                    if (inputTypeBoxed == typeof(long?)) {
                        return new DTLocalDtxOpsLongForge(calendarForges, timeAbacus);
                    }

                    if (inputTypeBoxed == typeof(DateTimeOffset?)) {
                        return new DTLocalDtxOpsDtoForge(calendarForges);
                    }

                    if (inputTypeBoxed == typeof(DateTime?)) {
                        return new DTLocalDtxOpsDtzForge(calendarForges);
                    }
                }

                throw new ArgumentException("Invalid input type '" + inputTypeBoxed + "'");
            }

            var propertyNameStart = inputEventType.StartTimestampPropertyName;
            var getter = ((EventTypeSPI)inputEventType).GetGetterSPI(propertyNameStart);
            var getterResultType = inputEventType.GetPropertyType(propertyNameStart);
            
            CheckNotNull(getterResultType, propertyNameStart);
            
            if (reformatForge != null) {
                var inner = GetForge(calendarForges, timeAbacus, getterResultType, null, reformatForge, null);
                return new DTLocalBeanReformatForge(getter, getterResultType, inner, reformatForge.ReturnType);
            }

            if (intervalForge == null) { // only calendar op
                var inner = GetForge(calendarForges, timeAbacus, getterResultType, null, null, null);
                return new DTLocalBeanCalOpsForge(
                    getter,
                    getterResultType,
                    inner,
                    _returnType.GetNormalizedType());
            }

            // have interval op but no end timestamp
            if (inputEventType.EndTimestampPropertyName == null) {
                var inner = GetForge(calendarForges, timeAbacus, getterResultType, null, null, intervalForge);
                return new DTLocalBeanIntervalNoEndTSForge(
                    getter,
                    getterResultType,
                    inner,
                    _returnType.GetNormalizedType());
            }

            // interval op and have end timestamp
            var propertyNameEnd = inputEventType.EndTimestampPropertyName;
            var getterEndTimestamp = ((EventTypeSPI)inputEventType).GetGetterSPI(propertyNameEnd);
            var getterEndType = inputEventType.GetPropertyType(propertyNameEnd);
            CheckNotNull(getterEndType, propertyNameEnd);
            var innerX = (DTLocalForgeIntervalComp)GetForge(
                calendarForges,
                timeAbacus,
                getterResultType,
                null,
                null,
                intervalForge);
            return new DTLocalBeanIntervalWithEndForge(
                getter,
                getterResultType,
                getterEndTimestamp,
                getterEndType,
                innerX);
        }

        private void CheckNotNull(
            Type getterResultType,
            string propertyName)
        {
            if (getterResultType == null) {
                throw new ExprValidationException("Invalid null-type input for property '" + propertyName + "'");
            }
        }

        public ExprDotEval DotEvaluator {
            get {
                var evaluator = _forge.DTEvaluator;
                ExprDotForge exprDotForge = this;
                return new ProxyExprDotEval() {
                    ProcEvaluate = (
                        target,
                        eventsPerStream,
                        isNewData,
                        exprEvaluatorContext) => {
                        if (target == null) {
                            return null;
                        }

                        return evaluator.Evaluate(target, eventsPerStream, isNewData, exprEvaluatorContext);
                    },
                    ProcDotForge = () => exprDotForge
                };
            }
        }
    }
} // end of namespace