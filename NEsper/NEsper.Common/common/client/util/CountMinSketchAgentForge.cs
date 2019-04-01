///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;

namespace com.espertech.esper.common.client.util
{
    /// <summary>
    ///     For use with Count-min sketch aggregation functions:
    ///     The agent implementation encapsulates transformation of value objects to byte-array and back (when needed),
    ///     and may override or provide custom behavior.
    ///     <para />
    ///     This is an extension API and may use internal classes. As such the interface may change between versions.
    /// </summary>
    public interface CountMinSketchAgentForge
    {
        /// <summary>
        ///     Returns an array of types that the agent can handle, for validation purposes.
        ///     For example, an agent that accepts byte-array type values should return "new Class[] {String.class}".
        ///     Interfaces and supertype classes can also be part of the class array.
        /// </summary>
        /// <value>class array of acceptable type</value>
        Type[] AcceptableValueTypes { get; }

        /// <summary>
        ///     Provides the code for the agent.
        /// </summary>
        /// <param name="parent">parent methods</param>
        /// <param name="classScope">class scope</param>
        /// <returns>expression</returns>
        CodegenExpression CodegenMake(CodegenMethod parent, CodegenClassScope classScope);
    }
} // end of namespace