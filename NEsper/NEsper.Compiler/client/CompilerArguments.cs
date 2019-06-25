///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.configuration;

namespace com.espertech.esper.compiler.client
{
    /// <summary>
    ///     Arguments holder for use with <seealso cref="EPCompiler.Compile" />.
    ///     <para>
    ///         The compiler arguments always contain a configuration. When there is no configuration provided the compiler
    ///         uses the default
    ///         (empty) configuration.
    ///     </para>
    ///     <para>
    ///         The compiler path provides information on the EPL-objects that are visible at compilation time.
    ///         Add compiled modules and path information from runtimes to the path for modules to gain access to existing EPL
    ///         objects.
    ///     </para>
    ///     <para>
    ///         Compiler options are callbacks as well as optional values for the compiler.
    ///     </para>
    /// </summary>
    public class CompilerArguments
    {
        /// <summary>
        ///     Empty constructor uses an empty <seealso cref="Configuration" />
        /// </summary>
        public CompilerArguments()
            : this(new Configuration())
        {
        }

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="configuration">the compiler configuration</param>
        public CompilerArguments(Configuration configuration)
        {
            Configuration = configuration;
            Path = new CompilerPath();
            Options = new CompilerOptions();
        }

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="compilerPathable">a compiler pathable provide path information</param>
        public CompilerArguments(EPCompilerPathable compilerPathable)
            : this()
        {
            Path.Add(compilerPathable);
        }

        /// <summary>
        ///     Returns the path.
        /// </summary>
        /// <returns>path</returns>
        public CompilerPath Path { get; set; }

        /// <summary>
        ///     Returns the configuration
        /// </summary>
        /// <returns>configuration</returns>
        public Configuration Configuration { get; set; }

        /// <summary>
        ///     Returns the compiler options
        /// </summary>
        /// <returns>options</returns>
        public CompilerOptions Options { get; set; }
    }
} // end of namespace