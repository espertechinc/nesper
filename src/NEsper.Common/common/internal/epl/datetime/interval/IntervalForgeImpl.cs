///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.common.@internal.epl.datetime.eval;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.dot.core;
using com.espertech.esper.common.@internal.epl.expression.time.abacus;
using com.espertech.esper.common.@internal.epl.streamtype;
using com.espertech.esper.common.@internal.epl.table.compiletime;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.datetime.interval
{
    public partial class IntervalForgeImpl : IntervalForge
    {
        private readonly ExprForge _forgeTimestamp;

        private readonly int? _parameterStreamNum;
        private readonly string _parameterPropertyStart;
        private readonly string _parameterPropertyEnd;

        internal readonly IntervalOpForge _intervalOpForge;

        public IntervalForgeImpl(
            DatetimeMethodDesc method,
            string methodNameUse,
            StreamTypeService streamTypeService,
            IList<ExprNode> expressions,
            TimeAbacus timeAbacus,
            TableCompileTimeResolver tableCompileTimeResolver)
        {
            ExprForge forgeEndTimestamp = null;
            Type timestampType;

            if (expressions[0] is ExprStreamUnderlyingNode) {
                var und = (ExprStreamUnderlyingNode)expressions[0];
                _parameterStreamNum = und.StreamId;
                var type = streamTypeService.EventTypes[_parameterStreamNum.Value];
                _parameterPropertyStart = type.StartTimestampPropertyName;
                if (_parameterPropertyStart == null) {
                    throw new ExprValidationException(
                        "For date-time method '" +
                        methodNameUse +
                        "' the first parameter is event type '" +
                        type.Name +
                        "', however no timestamp property has been defined for this event type");
                }

                var getter = ((EventTypeSPI)type).GetGetterSPI(_parameterPropertyStart);
                var getterReturnType = type.GetPropertyType(_parameterPropertyStart);
                if (getterReturnType == null) {
                    throw new ExprValidationException(
                        "Property '" + _parameterPropertyStart + "' does not exist or returns a null-type value");
                }

                timestampType = type.GetPropertyType(_parameterPropertyStart);
                var getterReturnTypeBoxed = getterReturnType.GetBoxedType();
                _forgeTimestamp = new ExprEvaluatorStreamDTProp(_parameterStreamNum.Value, getter, getterReturnTypeBoxed);

                if (type.EndTimestampPropertyName != null) {
                    _parameterPropertyEnd = type.EndTimestampPropertyName;
                    var getterEndTimestamp =
                        ((EventTypeSPI)type).GetGetterSPI(type.EndTimestampPropertyName);
                    forgeEndTimestamp = new ExprEvaluatorStreamDTProp(
                        _parameterStreamNum.Value,
                        getterEndTimestamp,
                        getterReturnTypeBoxed);
                }
                else {
                    _parameterPropertyEnd = _parameterPropertyStart;
                }
            }
            else {
                _forgeTimestamp = expressions[0].Forge;
                var forgeType = _forgeTimestamp.EvaluationType;
                timestampType = forgeType;

                string unresolvedPropertyName = null;
                if (expressions[0] is ExprIdentNode) {
                    var identNode = (ExprIdentNode)expressions[0];
                    _parameterStreamNum = identNode.StreamId;
                    _parameterPropertyStart = identNode.ResolvedPropertyName;
                    _parameterPropertyEnd = _parameterPropertyStart;
                    unresolvedPropertyName = identNode.UnresolvedPropertyName;
                }

                if (!TypeHelper.IsDateTime(ForgeTimestamp.EvaluationType)) {
                    // ident node may represent a fragment
                    if (unresolvedPropertyName != null) {
                        var propertyDesc = ExprIdentNodeUtil.GetTypeFromStream(
                            streamTypeService,
                            unresolvedPropertyName,
                            false,
                            true,
                            tableCompileTimeResolver);
                        if (propertyDesc.First.FragmentEventType != null) {
                            var type = propertyDesc.First.FragmentEventType.FragmentType;
                            _parameterPropertyStart = type.StartTimestampPropertyName;
                            if (_parameterPropertyStart == null) {
                                throw new ExprValidationException(
                                    "For date-time method '" +
                                    methodNameUse +
                                    "' the first parameter is event type '" +
                                    type.Name +
                                    "', however no timestamp property has been defined for this event type");
                            }

                            timestampType = type.GetPropertyType(_parameterPropertyStart);
                            var getterFragment =
                                ((EventTypeSPI)streamTypeService.EventTypes[_parameterStreamNum.Value]).GetGetterSPI(
                                    unresolvedPropertyName);
                            var getterStartTimestamp =
                                ((EventTypeSPI)type).GetGetterSPI(_parameterPropertyStart);
                            _forgeTimestamp = new ExprEvaluatorStreamDTPropFragment(
                                _parameterStreamNum.Value,
                                getterFragment,
                                getterStartTimestamp);

                            if (type.EndTimestampPropertyName != null) {
                                _parameterPropertyEnd = type.EndTimestampPropertyName;
                                var getterEndTimestamp =
                                    ((EventTypeSPI)type).GetGetterSPI(type.EndTimestampPropertyName);
                                forgeEndTimestamp = new ExprEvaluatorStreamDTPropFragment(
                                    _parameterStreamNum.Value,
                                    getterFragment,
                                    getterEndTimestamp);
                            }
                            else {
                                _parameterPropertyEnd = _parameterPropertyStart;
                            }
                        }
                    }
                    else {
                        throw new ExprValidationException(
                            "For date-time method '" +
                            methodNameUse +
                            "' the first parameter expression returns '" +
                            _forgeTimestamp.EvaluationType +
                            "', however requires a Date, DateTimeEx, Long-type return value or event (with timestamp)");
                    }
                }
            }

            var intervalComputerForge =
                IntervalComputerForgeFactory.Make(method, expressions, timeAbacus);

            // evaluation without end timestamp
            var timestampTypeBoxed = timestampType.GetBoxedType();
            if (forgeEndTimestamp == null) {
                if (TypeHelper.IsSubclassOrImplementsInterface(timestampType, typeof(DateTimeEx))) {
                    _intervalOpForge = new IntervalOpDateTimeExForge(intervalComputerForge);
                }
                else if (timestampTypeBoxed == typeof(long?)) {
                    _intervalOpForge = new IntervalOpForgeLong(intervalComputerForge);
                }
                else if (timestampTypeBoxed == typeof(DateTimeOffset?)) {
                    _intervalOpForge = new IntervalOpDateTimeOffsetForge(intervalComputerForge);
                }
                else if (timestampTypeBoxed == typeof(DateTime?)) {
                    _intervalOpForge = new IntervalOpDateTimeForge(intervalComputerForge);
                }
                else {
                    throw new ArgumentException("Invalid interval first parameter type '" + timestampType + "'");
                }
            }
            else {
                if (TypeHelper.IsSubclassOrImplementsInterface(timestampType, typeof(DateTimeEx))) {
                    _intervalOpForge = new IntervalOpDateTimeExWithEndForge(intervalComputerForge, forgeEndTimestamp);
                }
                else if (timestampTypeBoxed == typeof(long?)) {
                    _intervalOpForge = new IntervalOpLongWithEndForge(intervalComputerForge, forgeEndTimestamp);
                }
                else if (timestampTypeBoxed == typeof(DateTimeOffset?)) {
                    _intervalOpForge = new IntervalOpDateTimeOffsetWithEndForge(
                        intervalComputerForge,
                        forgeEndTimestamp);
                }
                else if (timestampTypeBoxed == typeof(DateTime?)) {
                    _intervalOpForge = new IntervalOpDateTimeWithEndForge(intervalComputerForge, forgeEndTimestamp);
                }
                else {
                    throw new ArgumentException("Invalid interval first parameter type '" + timestampType + "'");
                }
            }
        }

        public IntervalOp Op => new IntervalForgeOp(_forgeTimestamp.ExprEvaluator, _intervalOpForge.MakeEval());

        public CodegenExpression Codegen(
            CodegenExpression start,
            CodegenExpression end,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return IntervalForgeOp.Codegen(this, start, end, codegenMethodScope, exprSymbol, codegenClassScope);
        }

        public ExprForge ForgeTimestamp => _forgeTimestamp;

        /// <summary>
        /// Obtain information used by filter analyzer to handle this dot-method invocation as part of query planning/indexing.
        /// </summary>
        /// <param name="typesPerStream">event types</param>
        /// <param name="currentMethod">current method</param>
        /// <param name="currentParameters">current params</param>
        /// <param name="inputDesc">descriptor of what the input to this interval method is</param>
        public FilterExprAnalyzerDTIntervalAffector GetFilterDesc(
            EventType[] typesPerStream,
            DatetimeMethodDesc currentMethod,
            IList<ExprNode> currentParameters,
            ExprDotNodeFilterAnalyzerInput inputDesc)
        {
            // with intervals is not currently query planned
            if (currentParameters.Count > 1) {
                return null;
            }

            // Get input (target)
            int targetStreamNum;
            string targetPropertyStart;
            string targetPropertyEnd;
            if (inputDesc is ExprDotNodeFilterAnalyzerInputStream desc) {
                targetStreamNum = desc.StreamNum;
                var targetType = typesPerStream[targetStreamNum];
                targetPropertyStart = targetType.StartTimestampPropertyName;
                targetPropertyEnd = targetType.EndTimestampPropertyName ?? targetPropertyStart;
            }
            else if (inputDesc is ExprDotNodeFilterAnalyzerInputProp targetStream) {
                targetStreamNum = targetStream.StreamNum;
                targetPropertyStart = targetStream.PropertyName;
                targetPropertyEnd = targetStream.PropertyName;
            }
            else {
                return null;
            }

            // check parameter info
            if (_parameterPropertyStart == null) {
                return null;
            }

            return new FilterExprAnalyzerDTIntervalAffector(
                currentMethod,
                typesPerStream,
                targetStreamNum,
                targetPropertyStart,
                targetPropertyEnd,
                _parameterStreamNum.Value,
                _parameterPropertyStart,
                _parameterPropertyEnd);
        }
    }
} // end of namespace