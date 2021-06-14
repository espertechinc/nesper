///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;

namespace com.espertech.esper.common.client.hook.forgeinject
{
    /// <summary>
    /// Interface for providing the compiler with code that allocates and initializes an instance of some class
    /// </summary>
    public interface InjectionStrategy
    {
        /// <summary>
        /// Returns the initialization expression
        /// </summary>
        /// <param name="classScope">the class scope</param>
        /// <returns>class scope</returns>
        CodegenExpression GetInitializationExpression(CodegenClassScope classScope);
    }
} // end of namespace