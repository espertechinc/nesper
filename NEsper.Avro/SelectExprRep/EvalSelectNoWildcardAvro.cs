///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using Avro.Generic;

using com.espertech.esper.client;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.core.eval;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.util;

using NEsper.Avro.Core;

namespace NEsper.Avro.SelectExprRep
{
    public class EvalSelectNoWildcardAvro : SelectExprProcessor
    {
        private readonly ExprEvaluator[] _evaluator;
        private readonly AvroEventType _resultEventType;
        private readonly SelectExprContext _selectExprContext;

        public EvalSelectNoWildcardAvro(
            SelectExprContext selectExprContext,
            EventType resultEventType,
            string statementName,
            string engineURI)
        {
            _selectExprContext = selectExprContext;
            _resultEventType = (AvroEventType) resultEventType;

            _evaluator = new ExprEvaluator[selectExprContext.ExpressionNodes.Length];
            var typeWidenerCustomizer =
                selectExprContext.EventAdapterService.GetTypeWidenerCustomizer(resultEventType);
            for (var i = 0; i < _evaluator.Length; i++)
            {
                var eval = selectExprContext.ExpressionNodes[i];
                _evaluator[i] = eval;

                if (eval is SelectExprProcessorEvalByGetterFragment)
                {
                    _evaluator[i] = HandleFragment((SelectExprProcessorEvalByGetterFragment) eval);
                }
                else if (eval is SelectExprProcessorEvalStreamInsertUnd)
                {
                    var und = (SelectExprProcessorEvalStreamInsertUnd) eval;
                    _evaluator[i] = new ProxyExprEvaluator
                    {
                        ProcEvaluate = (evaluateParams) =>
                        {
                            var @event = evaluateParams.EventsPerStream[und.StreamNum];
                            if (@event == null)
                            {
                                return null;
                            }
                            return @event.Underlying;
                        },
                        ProcReturnType = () => { return typeof (GenericRecord); }
                    };
                }
                else if (eval is SelectExprProcessorEvalTypableMap)
                {
                    var typableMap = (SelectExprProcessorEvalTypableMap) eval;
                    _evaluator[i] = new SelectExprProcessorEvalAvroMapToAvro(
                        typableMap.InnerEvaluator, ((AvroEventType) resultEventType).SchemaAvro,
                        selectExprContext.ColumnNames[i]);
                }
                else if (eval is SelectExprProcessorEvalStreamInsertNamedWindow)
                {
                    var nw = (SelectExprProcessorEvalStreamInsertNamedWindow) eval;
                    _evaluator[i] = new ProxyExprEvaluator
                    {
                        ProcEvaluate = (evaluateParams) =>
                        {
                            var @event = evaluateParams.EventsPerStream[nw.StreamNum];
                            if (@event == null)
                            {
                                return null;
                            }
                            return @event.Underlying;
                        },
                        ProcReturnType = () => { return typeof (GenericRecord); }
                    };
                }
                else if (eval.ReturnType != null && eval.ReturnType.IsArray)
                {
                    var widener = TypeWidenerFactory.GetArrayToCollectionCoercer(eval.ReturnType.GetElementType());
                    //if (eval.ReturnType == typeof (byte[]))
                    //{
                    //    widener = TypeWidenerFactory.BYTE_ARRAY_TO_BYTE_BUFFER_COERCER;
                    //}
                    _evaluator[i] = new SelectExprProcessorEvalAvroArrayCoercer(eval, widener);
                }
                else
                {
                    var propertyName = selectExprContext.ColumnNames[i];
                    var propertyType = resultEventType.GetPropertyType(propertyName);
                    var widener = TypeWidenerFactory.GetCheckPropertyAssignType(
                        propertyName, eval.ReturnType, propertyType, propertyName, true, typeWidenerCustomizer,
                        statementName, engineURI);
                    if (widener != null)
                    {
                        _evaluator[i] = new SelectExprProcessorEvalAvroArrayCoercer(eval, widener);
                    }
                }
            }
        }

        public EventBean Process(
            EventBean[] eventsPerStream,
            bool isNewData,
            bool isSynthesize,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var evaluateParams = new EvaluateParams(eventsPerStream, isNewData, exprEvaluatorContext);
            var columnNames = _selectExprContext.ColumnNames;

            var record = new GenericRecord(_resultEventType.SchemaAvro);

            // Evaluate all expressions and build a map of name-value pairs
            for (var i = 0; i < _evaluator.Length; i++)
            {
                var evalResult = _evaluator[i].Evaluate(evaluateParams);
                record.Add(columnNames[i], evalResult);
            }

            return _selectExprContext.EventAdapterService.AdapterForTypedAvro(record, _resultEventType);
        }

        public EventType ResultEventType
        {
            get { return _resultEventType; }
        }

        private ExprEvaluator HandleFragment(SelectExprProcessorEvalByGetterFragment eval)
        {
            if (eval.ReturnType == typeof (GenericRecord[]))
            {
                return new SelectExprProcessorEvalByGetterFragmentAvroArray(
                    eval.StreamNum, eval.Getter, typeof (ICollection<object>));
            }
            if (eval.ReturnType == typeof (GenericRecord))
            {
                return new SelectExprProcessorEvalByGetterFragmentAvro(
                    eval.StreamNum, eval.Getter, typeof (GenericRecord));
            }
            throw new EPException("Unrecognized return type " + eval.ReturnType + " for use with Avro");
        }
    }
} // end of namespace