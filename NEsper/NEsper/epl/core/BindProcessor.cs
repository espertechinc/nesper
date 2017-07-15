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
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.spec;
using com.espertech.esper.epl.table.mgmt;

namespace com.espertech.esper.epl.core
{
    /// <summary>
    /// Works in conjunction with <seealso cref="SelectExprResultProcessor" /> to present
    /// a result as an object array for 'natural' delivery.
    /// </summary>
    public class BindProcessor {
        private ExprEvaluator[] expressionNodes;
        private Type[] expressionTypes;
        private string[] columnNamesAssigned;
    
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="selectionList">the select clause</param>
        /// <param name="typesPerStream">the event types per stream</param>
        /// <param name="streamNames">the stream names</param>
        /// <param name="tableService">table service</param>
        /// <exception cref="ExprValidationException">when the validation of the select clause failed</exception>
        public BindProcessor(SelectClauseElementCompiled[] selectionList,
                             EventType[] typesPerStream,
                             string[] streamNames,
                             TableService tableService)
                {
            var expressions = new List<ExprEvaluator>();
            var types = new List<Type>();
            var columnNames = new List<string>();
    
            foreach (SelectClauseElementCompiled element in selectionList) {
                // handle wildcards by outputting each stream's underlying event
                if (element is SelectClauseElementWildcard) {
                    for (int i = 0; i < typesPerStream.Length; i++) {
                        Type returnType = typesPerStream[i].UnderlyingType;
                        TableMetadata tableMetadata = tableService.GetTableMetadataFromEventType(typesPerStream[i]);
                        ExprEvaluator evaluator;
                        if (tableMetadata != null) {
                            evaluator = new BindProcessorEvaluatorStreamTable(i, returnType, tableMetadata);
                        } else {
                            evaluator = new BindProcessorEvaluatorStream(i, returnType);
                        }
                        expressions.Add(evaluator);
                        types.Add(returnType);
                        columnNames.Add(streamNames[i]);
                    }
                } else if (element is SelectClauseStreamCompiledSpec) {
                    // handle stream wildcards by outputting the stream underlying event
                    SelectClauseStreamCompiledSpec streamSpec = (SelectClauseStreamCompiledSpec) element;
                    EventType type = typesPerStream[streamSpec.StreamNumber];
                    Type returnType = type.UnderlyingType;
    
                    TableMetadata tableMetadata = tableService.GetTableMetadataFromEventType(type);
                    ExprEvaluator evaluator;
                    if (tableMetadata != null) {
                        evaluator = new BindProcessorEvaluatorStreamTable(streamSpec.StreamNumber, returnType, tableMetadata);
                    } else {
                        evaluator = new BindProcessorEvaluatorStream(streamSpec.StreamNumber, returnType);
                    }
                    expressions.Add(evaluator);
                    types.Add(returnType);
                    columnNames.Add(streamNames[streamSpec.StreamNumber]);
                } else if (element is SelectClauseExprCompiledSpec) {
                    // handle expressions
                    SelectClauseExprCompiledSpec expr = (SelectClauseExprCompiledSpec) element;
                    ExprEvaluator evaluator = expr.SelectExpression.ExprEvaluator;
                    expressions.Add(evaluator);
                    types.Add(evaluator.Type);
                    if (expr.AssignedName != null) {
                        columnNames.Add(expr.AssignedName);
                    } else {
                        columnNames.Add(ExprNodeUtility.ToExpressionStringMinPrecedenceSafe(expr.SelectExpression));
                    }
                } else {
                    throw new IllegalStateException("Unrecognized select expression element of type " + element.Class);
                }
            }
    
            expressionNodes = expressions.ToArray(new ExprEvaluator[expressions.Count]);
            expressionTypes = types.ToArray(new Type[types.Count]);
            columnNamesAssigned = columnNames.ToArray(new string[columnNames.Count]);
        }
    
        /// <summary>
        /// Process select expressions into columns for native dispatch.
        /// </summary>
        /// <param name="eventsPerStream">each stream's events</param>
        /// <param name="isNewData">true for new events</param>
        /// <param name="exprEvaluatorContext">context for expression evaluatiom</param>
        /// <returns>object array with select-clause results</returns>
        public Object[] Process(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext) {
            var parameters = new Object[expressionNodes.Length];
    
            for (int i = 0; i < parameters.Length; i++) {
                Object result = expressionNodes[i].Evaluate(eventsPerStream, isNewData, exprEvaluatorContext);
                parameters[i] = result;
            }
    
            return parameters;
        }
    
        /// <summary>
        /// Returns the expression types generated by the select-clause expressions.
        /// </summary>
        /// <returns>types</returns>
        public Type[] GetExpressionTypes() {
            return expressionTypes;
        }
    
        /// <summary>
        /// Returns the column names of select-clause expressions.
        /// </summary>
        /// <returns>column names</returns>
        public string[] GetColumnNamesAssigned() {
            return columnNamesAssigned;
        }
    }
} // end of namespace
