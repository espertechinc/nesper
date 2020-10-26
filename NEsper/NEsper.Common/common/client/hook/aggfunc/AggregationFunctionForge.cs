///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.client.hook.aggfunc
{
    /// <summary>
    /// Compile-time representation of a plug-in aggregation function.
    /// </summary>
    public interface AggregationFunctionForge
    {
        /// <summary>
        /// Sets the EPL function name assigned to the factory.
        /// </summary>
        /// <value>assigned</value>
        string FunctionName { set; }

        /// <summary>
        /// Implemented by plug-in aggregation functions to allow such functions to validate the
        /// type of values passed to the function at statement compile time and to generally
        /// interrogate parameter expressions.
        /// </summary>
        /// <param name="validationContext">expression information</param>
        /// <throws>ExprValidationException for validation exception</throws>
        void Validate(AggregationFunctionValidationContext validationContext);

        /// <summary>
        /// Returns the type of the current value.
        /// </summary>
        /// <value>type of value returned by the aggregation methods</value>
        Type ValueType { get; }

        /// <summary>
        /// Describes to the compiler how it should manage code for the aggregation function.
        /// </summary>
        /// <value>mode object</value>
        AggregationFunctionMode AggregationFunctionMode { get; }
    }
} // end of namespace