///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.epl.agg.aggregator;
using com.espertech.esper.epl.agg.service;

namespace com.espertech.esper.client.hook
{
    /// <summary>
    /// Interface to implement for factories of aggregation functions.
    /// </summary>
    public interface AggregationFunctionFactory
    {
        /// <summary>
        /// Sets the EPL function name assigned to the factory.
        /// </summary>
        /// <value>Name of the function.</value>
        string FunctionName { set; }

        /// <summary>
        /// Implemented by plug-in aggregation functions to allow such functions to validate the type of values passed to the function at statement compile time and to generally interrogate parameter expressions.
        /// </summary>
        /// <param name="validationContext">expression information</param>
        void Validate(AggregationValidationContext validationContext);

        /// <summary>Make a new, initalized aggregation state. </summary>
        /// <returns>initialized aggregator</returns>
        AggregationMethod NewAggregator();

        /// <summary>Returns the type of the current value. </summary>
        /// <value>type of value returned by the aggregation methods</value>
        Type ValueType { get; }
    }
}
