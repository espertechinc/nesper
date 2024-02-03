///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.client.hook.aggmultifunc
{
    /// <summary>
    ///     Context for use with plug-in aggregation multi-functions aggregation methods.
    /// </summary>
    public class AggregationMultiFunctionAggregationMethodContext
    {
        private readonly string _aggregationMethodName;
        private readonly ExprNode[] _parameters;
        private readonly ExprValidationContext _validationContext;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="aggregationMethodName">name of aggregation method</param>
        /// <param name="parameters">parameter expressions</param>
        /// <param name="validationContext">validation context</param>
        public AggregationMultiFunctionAggregationMethodContext(
            string aggregationMethodName,
            ExprNode[] parameters,
            ExprValidationContext validationContext)
        {
            _aggregationMethodName = aggregationMethodName;
            _parameters = parameters;
            _validationContext = validationContext;
        }

        /// <summary>
        ///     Returns the aggregation method name.
        /// </summary>
        /// <value>name</value>
        public string AggregationMethodName => _aggregationMethodName;

        /// <summary>
        ///     Returns the parameter expressions
        /// </summary>
        /// <value>params</value>
        public ExprNode[] Parameters => _parameters;

        /// <summary>
        ///     Returns the validation context
        /// </summary>
        /// <value>validation context</value>
        public ExprValidationContext ValidationContext => _validationContext;
    }
} // end of namespace