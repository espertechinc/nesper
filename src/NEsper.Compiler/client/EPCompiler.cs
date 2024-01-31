///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.client.module;
using com.espertech.esper.common.client.soda;
using com.espertech.esper.compat;

using Stream = System.IO.Stream;

namespace com.espertech.esper.compiler.client
{
    /// <summary>
    ///     The EPL compiler compiles EPL text as well as object models to byte code.
    /// </summary>

    public interface EPCompiler
    {
        /// <summary>
        ///     Compiles EPL and returns the byte code for deployment into a runtime.
        ///     <para />
        ///     Use semicolon(;) to separate multiple statements in a module.
        /// </summary>
        /// <param name="epl">epl to compile</param>
        /// <param name="arguments">compiler arguments</param>
        /// <returns>byte code</returns>
        /// <throws>EPCompileException when the compilation failed</throws>
        EPCompiled Compile(
            string epl,
            CompilerArguments arguments);

        /// <summary>
        ///     Compiles a module object model and returns the byte code for deployment into a runtime.
        /// </summary>
        /// <param name="module">module object to compile</param>
        /// <param name="arguments">compiler arguments</param>
        /// <returns>byte code</returns>
        /// <throws>EPCompileException when the compilation failed</throws>
        EPCompiled Compile(
            Module module,
            CompilerArguments arguments);

        /// <summary>
        ///     Compiles a single fire-and-forget query for execution by the runtime.
        /// </summary>
        /// <param name="fireAndForgetEPLQuery">fire-and-forget query to compile</param>
        /// <param name="arguments">compiler arguments</param>
        /// <returns>byte code</returns>
        /// <throws>EPCompileException when the compilation failed</throws>
        EPCompiled CompileQuery(
            string fireAndForgetEPLQuery,
            CompilerArguments arguments);

        /// <summary>
        ///     Compiles fire-and-forget query object model for execution by the runtime.
        /// </summary>
        /// <param name="fireAndForgetEPLQueryModel">fire-and-forget query to compile</param>
        /// <param name="arguments">compiler arguments</param>
        /// <returns>byte code</returns>
        /// <throws>EPCompileException when the compilation failed</throws>
        EPCompiled CompileQuery(
            EPStatementObjectModel fireAndForgetEPLQueryModel,
            CompilerArguments arguments);

        /// <summary>
        ///     Parse the module text returning the module object model.
        /// </summary>
        /// <param name="eplModuleText">to parse</param>
        /// <returns>module object model</returns>
        /// <throws>IOException    when the parser failed</throws>
        /// <throws>ParseException when parsing of the module failed</throws>
        Module ParseModule(string eplModuleText);

        /// <summary>
        ///     Parse the single-statement EPL and return a statement object model.
        /// </summary>
        /// <param name="epl">to parse</param>
        /// <param name="configuration">a configuration object when available</param>
        /// <returns>statement object model</returns>
        /// <throws>EPCompileException when the EPL could not be parsed</throws>
        EPStatementObjectModel EplToModel(
            string epl,
            Configuration configuration);

        /// <summary>
        ///     Validate the syntax of the module.
        /// </summary>
        /// <param name="module">to validate</param>
        /// <param name="arguments">compiler arguments</param>
        /// <throws>EPCompileException when the EPL could not be parsed</throws>
        void SyntaxValidate(
            Module module,
            CompilerArguments arguments);

        /// <summary>
        ///     Read the input stream and return the module. It is up to the calling method to close the stream when done.
        /// </summary>
        /// <param name="stream">to read</param>
        /// <param name="moduleUri">uri of the module</param>
        /// <returns>module module</returns>
        /// <returns>module module</returns>
        /// <throws>IOException    when the io operation failed</throws>
        /// <throws>ParseException when parsing of the module failed</throws>
        Module ReadModule(
            Stream stream,
            string moduleUri);

        /// <summary>
        ///     Read the resource by opening from resource manager and return the module.
        /// </summary>
        /// <param name="resource">name of the resource</param>
        /// <param name="resourceManager">a resource manager</param>
        /// <returns>module module</returns>
        /// <throws>IOException    when the resource could not be read</throws>
        /// <throws>ParseException when parsing of the module failed</throws>
        Module ReadModule(
            string resource,
            IResourceManager resourceManager);

        /// <summary>
        ///     Read the module by reading the text file and return the module.
        /// </summary>
        /// <param name="file">the file to read</param>
        /// <returns>module</returns>
        /// <throws>IOException    when the file could not be read</throws>
        /// <throws>ParseException when parsing of the module failed</throws>
        Module ReadModule(FileInfo file);

        /// <summary>
        ///     Read the module by reading from the URL provided and return the module.
        /// </summary>
        /// <param name="url">the URL to read</param>
        /// <returns>module</returns>
        /// <throws>IOException    when the url input stream could not be read</throws>
        /// <throws>ParseException when parsing of the module failed</throws>
        Module ReadModule(Uri url);
    }
} // end of namespace