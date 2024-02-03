///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.compile.stage3;

namespace com.espertech.esper.compiler.client.option
{
    /// <summary>
    ///     Provides the environment to <seealso cref="AccessModifierExpressionOption" />.
    /// </summary>
    public class AccessModifierExpressionContext : StatementOptionContextBase
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="base">statement info</param>
        /// <param name="expressionName">expression name</param>
        public AccessModifierExpressionContext(
            StatementBaseInfo @base,
            string expressionName)
            : base(@base)
        {
            ExpressionName = expressionName;
        }

        /// <summary>
        ///     Returns the expression name
        /// </summary>
        /// <returns>expression name</returns>
        public string ExpressionName { get; }
    }
} // end of namespace