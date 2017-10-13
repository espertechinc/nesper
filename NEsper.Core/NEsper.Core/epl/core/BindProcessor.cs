///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.spec;
using com.espertech.esper.epl.table.mgmt;

namespace com.espertech.esper.epl.core
{
    /// <summary>
    ///     Works in conjunction with <seealso cref="SelectExprResultProcessor" /> to present
    ///     a result as an object array for 'natural' delivery.
    /// </summary>
    public class BindProcessor
    {
        private readonly string[] _columnNamesAssigned;
        private readonly ExprEvaluator[] _expressionNodes;
        private readonly Type[] _expressionTypes;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="selectionList">the select clause</param>
        /// <param name="typesPerStream">the event types per stream</param>
        /// <param name="streamNames">the stream names</param>
        /// <param name="tableService">table service</param>
        /// <exception cref="ExprValidationException">when the validation of the select clause failed</exception>
        public BindProcessor(
            IEnumerable<SelectClauseElementCompiled> selectionList,
            EventType[] typesPerStream,
            string[] streamNames,
            TableService tableService)
        {
            var expressions = new List<ExprEvaluator>();
            var types = new List<Type>();
            var columnNames = new List<string>();

            foreach (var element in selectionList)
            {
                // handle wildcards by outputting each stream's underlying event
                if (element is SelectClauseElementWildcard)
                {
                    for (var i = 0; i < typesPerStream.Length; i++)
                    {
                        var returnType = typesPerStream[i].UnderlyingType;
                        var tableMetadata = tableService.GetTableMetadataFromEventType(typesPerStream[i]);
                        ExprEvaluator evaluator;
                        if (tableMetadata != null)
                        {
                            evaluator = new BindProcessorEvaluatorStreamTable(i, returnType, tableMetadata);
                        }
                        else
                        {
                            evaluator = new BindProcessorEvaluatorStream(i, returnType);
                        }
                        expressions.Add(evaluator);
                        types.Add(returnType);
                        columnNames.Add(streamNames[i]);
                    }
                }
                else if (element is SelectClauseStreamCompiledSpec)
                {
                    // handle stream wildcards by outputting the stream underlying event
                    var streamSpec = (SelectClauseStreamCompiledSpec) element;
                    var type = typesPerStream[streamSpec.StreamNumber];
                    var returnType = type.UnderlyingType;

                    var tableMetadata = tableService.GetTableMetadataFromEventType(type);
                    ExprEvaluator evaluator;
                    if (tableMetadata != null)
                    {
                        evaluator = new BindProcessorEvaluatorStreamTable(
                            streamSpec.StreamNumber, returnType, tableMetadata);
                    }
                    else
                    {
                        evaluator = new BindProcessorEvaluatorStream(streamSpec.StreamNumber, returnType);
                    }
                    expressions.Add(evaluator);
                    types.Add(returnType);
                    columnNames.Add(streamNames[streamSpec.StreamNumber]);
                }
                else if (element is SelectClauseExprCompiledSpec)
                {
                    // handle expressions
                    var expr = (SelectClauseExprCompiledSpec) element;
                    var evaluator = expr.SelectExpression.ExprEvaluator;
                    expressions.Add(evaluator);
                    types.Add(evaluator.ReturnType);
                    if (expr.AssignedName != null)
                    {
                        columnNames.Add(expr.AssignedName);
                    }
                    else
                    {
                        columnNames.Add(ExprNodeUtility.ToExpressionStringMinPrecedenceSafe(expr.SelectExpression));
                    }
                }
                else
                {
                    throw new IllegalStateException(
                        "Unrecognized select expression element of type " + element.GetType());
                }
            }

            _expressionNodes = expressions.ToArray();
            _expressionTypes = types.ToArray();
            _columnNamesAssigned = columnNames.ToArray();
        }

        /// <summary>
        ///     Process select expressions into columns for native dispatch.
        /// </summary>
        /// <param name="eventsPerStream">each stream's events</param>
        /// <param name="isNewData">true for new events</param>
        /// <param name="exprEvaluatorContext">context for expression evaluatiom</param>
        /// <returns>object array with select-clause results</returns>
        public Object[] Process(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext)
        {
            var parameters = new Object[_expressionNodes.Length];

            var evaluateParams = new EvaluateParams(eventsPerStream, isNewData, exprEvaluatorContext);
            for (var i = 0; i < parameters.Length; i++)
            {
                var result = _expressionNodes[i].Evaluate(evaluateParams);
                parameters[i] = result;
            }

            return parameters;
        }

        /// <summary>
        ///     Returns the expression types generated by the select-clause expressions.
        /// </summary>
        /// <value>types</value>
        public Type[] ExpressionTypes
        {
            get { return _expressionTypes; }
        }

        /// <summary>
        ///     Returns the column names of select-clause expressions.
        /// </summary>
        /// <value>column names</value>
        public string[] ColumnNamesAssigned
        {
            get { return _columnNamesAssigned; }
        }
    }
} // end of namespace