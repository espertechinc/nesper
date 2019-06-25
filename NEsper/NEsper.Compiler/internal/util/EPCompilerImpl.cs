///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using com.espertech.esper.common.client;
using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.client.module;
using com.espertech.esper.common.client.soda;
using com.espertech.esper.common.@internal.compile.stage1;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.stage1.specmapper;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.context.compile;
using com.espertech.esper.common.@internal.epl.expression.declared.compiletime;
using com.espertech.esper.common.@internal.epl.script.compiletime;
using com.espertech.esper.common.@internal.epl.table.compiletime;
using com.espertech.esper.common.@internal.epl.variable.compiletime;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compiler.client;
using com.espertech.esper.compiler.client.option;

using static com.espertech.esper.compiler.@internal.util.CompilerHelperServices;
using static com.espertech.esper.compiler.@internal.util.CompilerHelperSingleEPL;

namespace com.espertech.esper.compiler.@internal.util
{
    using Stream = System.IO.Stream;

    public class EPCompilerImpl : EPCompilerSPI
    {

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public EPCompiled CompileQuery(
            string fireAndForgetEPLQuery,
            CompilerArguments arguments)
        {
            return CompileQueryInternal(new CompilableEPL(fireAndForgetEPLQuery), arguments);
        }

        public EPCompiled CompileQuery(
            EPStatementObjectModel fireAndForgetEPLQueryModel,
            CompilerArguments arguments)
        {
            return CompileQueryInternal(new CompilableSODA(fireAndForgetEPLQueryModel), arguments);
        }

        public EPCompiled Compile(
            string epl,
            CompilerArguments arguments)
        {
            if (arguments == null) {
                arguments = new CompilerArguments(new Configuration());
            }

            try {
                Module module = EPLModuleUtil.ParseInternal(epl, null);
                IList<Compilable> compilables = new List<Compilable>();
                foreach (ModuleItem item in module.Items) {
                    string stmtEpl = item.Expression;
                    compilables.Add(new CompilableEPL(stmtEpl));
                }

                // determine module name
                string moduleName = DetermineModuleName(arguments.Options, module);
                ICollection<string> moduleUses = DetermineModuleUses(moduleName, arguments.Options, module);

                // get compile services
                ModuleCompileTimeServices compileTimeServices = GetCompileTimeServices(arguments, moduleName, moduleUses);

                // compile
                return CompilerHelperModuleProvider.Compile(
                    compilables, moduleName,
                    new EmptyDictionary<ModuleProperty, object>(), compileTimeServices, arguments.Options);
            }
            catch (EPCompileException) {
                throw;
            }
            catch (ParseException t) {
                throw new EPCompileException("Failed to parse: " + t.Message, t, new EmptyList<EPCompileExceptionItem>());
            }
            catch (Exception ex) {
                throw new EPCompileException(ex.Message, ex, new EmptyList<EPCompileExceptionItem>());
            }
        }

        public EPCompilerSPIExpression ExpressionCompiler(Configuration configuration)
        {
            CompilerArguments arguments = new CompilerArguments(configuration);
            arguments.Configuration = configuration;
            ModuleCompileTimeServices compileTimeServices = GetCompileTimeServices(arguments, null, null);
            return new EPCompilerSPIExpressionImpl(compileTimeServices);
        }

        public EPStatementObjectModel EplToModel(
            string stmtText,
            Configuration configuration)
        {
            try {
                StatementSpecMapEnv mapEnv = new StatementSpecMapEnv(
                    MakeImportService(configuration), VariableCompileTimeResolverEmpty.INSTANCE, configuration,
                    ExprDeclaredCompileTimeResolverEmpty.INSTANCE, ContextCompileTimeResolverEmpty.INSTANCE, TableCompileTimeResolverEmpty.INSTANCE,
                    ScriptCompileTimeResolverEmpty.INSTANCE, new CompilerServicesImpl());
                StatementSpecRaw statementSpec = CompilerHelperSingleEPL.ParseWalk(stmtText, mapEnv);
                StatementSpecUnMapResult unmapped = StatementSpecMapper.Unmap(statementSpec);
                return unmapped.ObjectModel;
            }
            catch (StatementSpecCompileException ex) {
                throw new EPCompileException(ex.Message, ex, new EmptyList<EPCompileExceptionItem>());
            }
            catch (Exception t) {
                throw new EPCompileException(t.Message, t, new EmptyList<EPCompileExceptionItem>());
            }
        }

        public Module ParseModule(string eplModuleText)
        {
            return EPLModuleUtil.ParseInternal(eplModuleText, null);
        }

        public EPCompiled Compile(
            Module module,
            CompilerArguments arguments)
        {
            if (arguments == null) {
                arguments = new CompilerArguments(new Configuration());
            }

            // determine module name
            string moduleName = DetermineModuleName(arguments.Options, module);
            ICollection<string> moduleUses = DetermineModuleUses(moduleName, arguments.Options, module);

            // get compile services
            ModuleCompileTimeServices compileTimeServices = GetCompileTimeServices(arguments, moduleName, moduleUses);

            if (module.Imports != null) {
                foreach (string imported in module.Imports) {
                    try {
                        compileTimeServices.ImportServiceCompileTime.AddImport(imported);
                    }
                    catch (ImportException e) {
                        throw new EPCompileException("Invalid module import: " + e.Message, e);
                    }
                }
            }

            IList<Compilable> compilables = new List<Compilable>();
            foreach (ModuleItem item in module.Items) {
                if (item.IsCommentOnly) {
                    continue;
                }

                if (item.Expression != null && item.Model != null) {
                    throw new EPCompileException("Module item has both an EPL expression and a statement object model");
                }

                if (item.Expression != null) {
                    compilables.Add(new CompilableEPL(item.Expression));
                }
                else if (item.Model != null) {
                    compilables.Add(new CompilableSODA(item.Model));
                }
                else {
                    throw new EPCompileException("Module item has neither an EPL expression nor a statement object model");
                }
            }

            IDictionary<ModuleProperty, object> moduleProperties = new Dictionary<ModuleProperty, object>();
            AddModuleProperty(moduleProperties, ModuleProperty.ARCHIVENAME, module.ArchiveName);
            AddModuleProperty(moduleProperties, ModuleProperty.URI, module.Uri);
            if (arguments.Configuration.Compiler.ByteCode.IsAttachModuleEPL) {
                AddModuleProperty(moduleProperties, ModuleProperty.MODULETEXT, module.ModuleText);
            }

            AddModuleProperty(moduleProperties, ModuleProperty.USEROBJECT, module.UserObjectCompileTime);
            AddModuleProperty(moduleProperties, ModuleProperty.USES, module.Uses == null || module.Uses.IsEmpty() ? null : module.Uses.ToArray());

            // compile
            return CompilerHelperModuleProvider.Compile(compilables, moduleName, moduleProperties, compileTimeServices, arguments.Options);
        }

        public Module ReadModule(
            Stream stream,
            string uri)
        {
            if (Log.IsDebugEnabled) {
                Log.Debug("Reading module from input stream");
            }

            return EPLModuleUtil.ReadInternal(stream, uri, false);
        }

        public Module ReadModule(FileInfo file)
        {
            if (Log.IsDebugEnabled) {
                Log.Debug("Reading resource '" + file.FullName + "'");
            }

            return EPLModuleUtil.ReadFile(file);
        }

        public Module ReadModule(Uri url)
        {
            if (Log.IsDebugEnabled) {
                Log.Debug("Reading resource from url: " + url);
            }

            using (var stream = new WebClient().OpenRead(url)) {
                return EPLModuleUtil.ReadInternal(stream, url.ToString(), false);
            }
        }

        public Module ReadModule(
            string resource,
            IResourceManager resourceManager)
        {
            if (Log.IsDebugEnabled) {
                Log.Debug("Reading resource '" + resource + "'");
            }

            return EPLModuleUtil.ReadResource(resource, resourceManager);
        }

        public void SyntaxValidate(
            Module module,
            CompilerArguments arguments)
        {
            if (arguments == null) {
                arguments = new CompilerArguments(new Configuration());
            }

            // determine module name
            string moduleName = DetermineModuleName(arguments.Options, module);
            ICollection<string> moduleUses = DetermineModuleUses(moduleName, arguments.Options, module);

            ModuleCompileTimeServices moduleCompileTimeServices = GetCompileTimeServices(arguments, moduleName, moduleUses);

            int statementNumber = 0;
            try {
                foreach (ModuleItem item in module.Items) {
                    StatementCompileTimeServices services = new StatementCompileTimeServices(statementNumber, moduleCompileTimeServices);
                    if (item.IsCommentOnly) {
                        continue;
                    }

                    if (item.Expression != null && item.Model != null) {
                        throw new EPCompileException("Module item has both an EPL expression and a statement object model");
                    }

                    if (item.Expression != null) {
                        ParseWalk(new CompilableEPL(item.Expression), services);
                    }
                    else if (item.Model != null) {
                        ParseWalk(new CompilableSODA(item.Model), services);
                        item.Model.ToEPL();
                    }
                    else {
                        throw new EPCompileException("Module item has neither an EPL expression nor a statement object model");
                    }

                    statementNumber++;
                }
            }
            catch (Exception ex) {
                throw new EPCompileException(ex.Message, ex);
            }
        }

        private EPCompiled CompileQueryInternal(
            Compilable compilable,
            CompilerArguments arguments)
        {
            if (arguments == null) {
                arguments = new CompilerArguments(new Configuration());
            }

            // determine module name
            string moduleName = arguments.Options.ModuleName?.GetValue(new ModuleNameContext(null));
            ISet<string> moduleUses = arguments.Options.ModuleUses?.GetValue(new ModuleUsesContext(moduleName, null));

            ModuleCompileTimeServices compileTimeServices = GetCompileTimeServices(arguments, moduleName, moduleUses);
            try {
                return CompilerHelperFAFProvider.Compile(compilable, compileTimeServices, arguments);
            }
            catch (Exception ex) {
                throw new EPCompileException(ex.Message + " [" + compilable.ToEPL() + "]", ex, new EmptyList<EPCompileExceptionItem>());
            }
        }

        private void AddModuleProperty(
            IDictionary<ModuleProperty, object> moduleProperties,
            ModuleProperty key,
            object value)
        {
            if (value == null) {
                return;
            }

            moduleProperties.Put(key, value);
        }

        private string DetermineModuleName(
            CompilerOptions options,
            Module module)
        {
            return options.ModuleName != null ? options.ModuleName.GetValue(new ModuleNameContext(module.Name)) : module.Name;
        }

        private ICollection<string> DetermineModuleUses(
            string moduleName,
            CompilerOptions options,
            Module module)
        {
            return options.ModuleUses != null 
                ? options.ModuleUses.GetValue(new ModuleUsesContext(moduleName, module.Uses))
                : module.Uses;
        }
    }
} // end of namespace