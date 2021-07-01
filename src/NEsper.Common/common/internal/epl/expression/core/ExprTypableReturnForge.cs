///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;

namespace com.espertech.esper.common.@internal.epl.expression.core
{
    /// <summary>
    ///     Interface for evaluators that select possible multi-valued results in a single select column,
    ///     such as subqueries and "new" and case+new combined.
    ///     <para />
    ///     When returning non-null results from {#getRowProperties},
    ///     the {@link ExprEvaluator#evaluate(EventBean[], boolean, ExprEvaluatorContext)}
    ///     must return an instance of Map&lt;String, Object&gt; (HashMap is fine).
    ///     <para />
    ///     When returning non-null results, the
    ///     the evaluator must also return either Object[] results or Object[][],
    ///     each object-array following the same exact order as provided by the map,
    ///     matching the multi-row flag.
    /// </summary>
    public interface ExprTypableReturnForge : ExprForge
    {
        /// <summary>
        ///     Return null to indicate no row-type result available,
        ///     or a map of property names and types to indicate a row-type result is available.
        /// </summary>
        /// <value>map of property names and types or null</value>
        /// <throws>ExprValidationException if the expression is invalid</throws>
        IDictionary<string, object> RowProperties { get; }

        /// <summary>
        ///     Return true for multi-row return, return false for return of single row only
        /// </summary>
        /// <value>multi-row flag</value>
        bool? IsMultirow { get; }

        CodegenExpression EvaluateTypableSingleCodegen(
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope);

        CodegenExpression EvaluateTypableMultiCodegen(
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope);
    }
} // end of namespace