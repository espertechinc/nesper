///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;

namespace com.espertech.esper.common.@internal.epl.expression.core
{
    /// <summary>
    ///     Interface for evaluating of an event re. enumeration.
    /// </summary>
    public interface ExprEnumerationGivenEventForge
    {
        CodegenExpression EvaluateEventGetROCollectionEventsCodegen(
            CodegenMethodScope codegenMethodScope,
            ExprEnumerationGivenEventSymbol symbols,
            CodegenClassScope codegenClassScope);

        CodegenExpression EvaluateEventGetROCollectionScalarCodegen(
            CodegenMethodScope codegenMethodScope,
            ExprEnumerationGivenEventSymbol symbols,
            CodegenClassScope codegenClassScope);

        CodegenExpression EvaluateEventGetEventBeanCodegen(
            CodegenMethodScope codegenMethodScope,
            ExprEnumerationGivenEventSymbol symbols,
            CodegenClassScope codegenClassScope);
    }
} // end of namespace