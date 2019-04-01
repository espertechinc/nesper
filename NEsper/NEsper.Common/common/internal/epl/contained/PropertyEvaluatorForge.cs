///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;

namespace com.espertech.esper.common.@internal.epl.contained
{
    /// <summary>
    /// Interface for a function that evaluates the properties of an event and returns event representing the properties.
    /// </summary>
    public interface PropertyEvaluatorForge
    {
        /// <summary>
        /// Returns the result type of the events generated by evaluating a property expression.
        /// </summary>
        /// <value>result event type</value>
        EventType FragmentEventType { get; }

        CodegenExpression Make(CodegenMethodScope parent, SAIFFInitializeSymbol symbols, CodegenClassScope classScope);

        bool CompareTo(PropertyEvaluatorForge other);
    }
} // end of namespace