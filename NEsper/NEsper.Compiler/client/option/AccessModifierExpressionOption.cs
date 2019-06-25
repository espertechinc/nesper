///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.util;

namespace com.espertech.esper.compiler.client.option
{
    /// <summary>
    ///     Implement this interface to provide or override the access modifier for a declared expression.
    /// </summary>
    public interface AccessModifierExpressionOption
    {
        /// <summary>
        ///     Returns the access modifier for the expression
        /// </summary>
        /// <param name="env">information about the statement</param>
        /// <returns>modifier</returns>
        NameAccessModifier GetValue(AccessModifierExpressionContext env);
    }
} // end of namespace