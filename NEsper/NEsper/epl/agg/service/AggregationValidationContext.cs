///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.agg.service
{
    /// <summary>
    /// Context for use with plug-in custom aggregation functions that implement
    /// <seealso cref="com.espertech.esper.client.hook.AggregationFunctionFactory" />.
    /// <para />
    /// This context object provides access to the parameter expressions themselves as 
    /// well as information compiled from the parameter expressions for your convenience.
    /// </summary>
    public class AggregationValidationContext
    {
        /// <summary>Ctor. </summary>
        /// <param name="parameterTypes">the type of each parameter expression.</param>
        /// <param name="constantValue">for each parameter expression an indicator whether the expression returns a constant result</param>
        /// <param name="constantValues">for each parameter expression that returns a constant result this array contains the constant value</param>
        /// <param name="distinct">true if 'distinct' keyword was provided</param>
        /// <param name="windowed">true if all event properties references by parameter expressions are from streams that have data windows declared onto the stream or are from named windows</param>
        /// <param name="expressions">the parameter expressions themselves</param>
        /// <param name="namedParameters"></param>
        public AggregationValidationContext(
            Type[] parameterTypes,
            bool[] constantValue, 
            object[] constantValues,
            bool distinct,
            bool windowed,
            ExprNode[] expressions, 
            IDictionary<string, IList<ExprNode>> namedParameters)
        {
            ParameterTypes = parameterTypes;
            IsConstantValue = constantValue;
            ConstantValues = constantValues;
            IsDistinct = distinct;
            IsWindowed = windowed;
            Expressions = expressions;
            NamedParameters = namedParameters;
        }

        /// <summary>The return type of each parameter expression. <para />This information can also be obtained by calling getType on each parameter expression. </summary>
        /// <value>array providing result type of each parameter expression</value>
        public Type[] ParameterTypes { get; private set; }

        /// <summary>A bool indicator for each parameter expression that is true if the expression returns a constant result or false if the expression result is not a constant value. <para />This information can also be obtained by calling isConstantResult on each parameter expression. </summary>
        /// <value>array providing an indicator per parameter expression that the result is a constant value</value>
        public bool[] IsConstantValue { get; private set; }

        /// <summary>If a parameter expression returns a constant value, the value of the constant it returns is provided in this array. <para />This information can also be obtained by calling evaluate on each parameter expression providing a constant value. </summary>
        /// <value>array providing the constant return value per parameter expression that has constant result value, or nullif a parameter expression is deemded to not provide a constant result value </value>
        public object[] ConstantValues { get; private set; }

        /// <summary>Returns true to indicate that the 'distinct' keyword was specified for this aggregation function. </summary>
        /// <value>distinct value indicator</value>
        public bool IsDistinct { get; private set; }

        /// <summary>Returns true to indicate that all parameter expressions return event properties that originate from a stream that provides a remove stream. </summary>
        /// <value>windowed indicator</value>
        public bool IsWindowed { get; private set; }

        /// <summary>Returns the parameter expressions themselves for interrogation. </summary>
        /// <value>parameter expressions</value>
        public ExprNode[] Expressions { get; private set; }

        /// <summary>
        /// Returns any named parameters or null if there are no named parameters
        /// </summary>
        /// <value></value>
        public IDictionary<string, IList<ExprNode>> NamedParameters { get; private set; }
    }
}
