///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.common.client;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.compiler.client
{
    /// <summary>
    ///     The compiler uses the path to determine the EPL-objects available to the module.
    ///     <para />
    ///     Visibility can be established by adding a compiled module
    ///     or by adding a <seealso cref="EPCompilerPathable" /> that can be obtained from a runtime.
    /// </summary>
    public class CompilerPath
    {
        /// <summary>
        ///     Returns the compiled modules in path.
        /// </summary>
        /// <returns>compiled modules</returns>
        public IList<EPCompiled> Compileds { get; } = new List<EPCompiled>();

        /// <summary>
        ///     Returns the path information provided by runtimes.
        /// </summary>
        /// <returns>path information provided by runtimes.</returns>
        public IList<EPCompilerPathable> CompilerPathables { get; } = new List<EPCompilerPathable>();

        /// <summary>
        ///     Add a compiled module
        /// </summary>
        /// <param name="compiled">compiled module</param>
        /// <returns>itself</returns>
        public CompilerPath Add(EPCompiled compiled)
        {
            Compileds.Add(compiled);
            return this;
        }

        /// <summary>
        ///     Add all compiled modules
        /// </summary>
        /// <param name="compiledColl">compiled module collection</param>
        /// <returns>tself</returns>
        public CompilerPath AddAll(ICollection<EPCompiled> compiledColl)
        {
            Compileds.AddAll(compiledColl);
            return this;
        }

        /// <summary>
        ///     Adds a path object that can be obtains from a runtime.
        /// </summary>
        /// <param name="pathable">runtime path information</param>
        /// <returns>itself</returns>
        public CompilerPath Add(EPCompilerPathable pathable)
        {
            CompilerPathables.Add(pathable);
            return this;
        }
    }
} // end of namespace