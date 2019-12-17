///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat.function;

namespace com.espertech.esper.compiler.client.option
{
    /// <summary>
    ///     Provides the environment to <seealso cref="StatementNameOption" />.
    /// </summary>
    public class StatementNameContext : StatementOptionContextBase
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="eplSupplier">epl supplier</param>
        /// <param name="statementName">statement name</param>
        /// <param name="moduleName">module name</param>
        /// <param name="annotations">annotations</param>
        /// <param name="statementNumber">statement number</param>
        public StatementNameContext(
            Supplier<string> eplSupplier,
            string statementName,
            string moduleName,
            Attribute[] annotations,
            int statementNumber)
            : base(eplSupplier, statementName, moduleName, annotations, statementNumber)
        {
        }
    }
} // end of namespace