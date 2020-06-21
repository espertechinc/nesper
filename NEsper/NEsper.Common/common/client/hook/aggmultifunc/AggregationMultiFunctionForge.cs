///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.configuration.compiler;

namespace com.espertech.esper.common.client.hook.aggmultifunc
{
    /// <summary>
    ///     Entry point for the extension API for aggregation multi-functions.
    ///     <para>
    ///         This API allows adding one or more related aggregation functions that can share state,
    ///         share parameters or exhibit related behavior.
    ///     </para>
    ///     <para>
    ///         Please use <seealso cref="ConfigurationCompilerPlugInAggregationMultiFunction" />to register this factory class in the runtime
    ///         together with one or more function names.
    ///     </para>
    ///     <para>
    ///         The runtime instantiates a single instance of this class at the time it encounters the first
    ///         aggregation multi-function in a given statement at the time of statement parsing or
    ///         compilation from statement object model.
    ///     </para>
    ///     <para>
    ///         At the time of statement parsing, each aggregation multi-function encountered during parsing
    ///         of EPL statement text results in an invocation to {@link #addAggregationFunction(AggregationMultiFunctionDeclarationContext)}}.
    ///         The same upon statement compilation for statement object model.
    ///         For multiple aggregation functions, the order in which such calls occur is not well defined
    ///         and should not be relied on by the implementation.
    ///     </para>
    ///     <para>
    ///         The runtime invokes {@link #validateGetHandler(AggregationMultiFunctionValidationContext)}}
    ///         at the time of expression node validation. Validation occurs after statement parsing
    ///         and when type information is established.
    ///         For multiple aggregation functions, the order in which such calls occur is not well defined
    ///         and should not be relied on by the implementation.
    ///     </para>
    ///     <para>
    ///         Usually a single <seealso cref="AggregationMultiFunctionHandler" /> handler class can handle the needs
    ///         of all related aggregation functions.
    ///         Usually you have a single handler class and return one handler object for each
    ///         aggregation function expression, where the handler object takes the validation context as a parameter.
    ///         Use multiple different handler classes when your aggregation
    ///         functions have sufficiently different execution contexts or behaviors. Your application may want to use the
    ///         expression and type information available in
    ///         <seealso cref="AggregationMultiFunctionValidationContext" /> to decide what behavior to provide.
    ///     </para>
    ///     <para>
    ///         The function class must be Serializable only when used with EsperHA.
    ///     </para>
    /// </summary>
    public interface AggregationMultiFunctionForge
    {
        /// <summary>
        ///     Called for each instance of use of any of the aggregation functions at declaration discovery time
        ///     and before any expression validation takes place.
        /// </summary>
        /// <param name="declarationContext">context</param>
        void AddAggregationFunction(AggregationMultiFunctionDeclarationContext declarationContext);

        /// <summary>
        ///     Called for each instance of use of any of the aggregation functions at validation time
        ///     after all declared aggregation have been added.
        /// </summary>
        /// <param name="validationContext">validationContext</param>
        /// <returns>handler for providing type information, accessor and provider factory</returns>
        AggregationMultiFunctionHandler ValidateGetHandler(AggregationMultiFunctionValidationContext validationContext);
    }
} // end of namespace