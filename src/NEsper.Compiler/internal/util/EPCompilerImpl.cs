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
using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.client.configuration.common;
using com.espertech.esper.common.client.module;
using com.espertech.esper.common.client.soda;
using com.espertech.esper.common.@internal.compile.stage1;
using com.espertech.esper.common.@internal.compile.stage1.specmapper;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.context.compile;
using com.espertech.esper.common.@internal.epl.classprovided.compiletime;
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

using Module = com.espertech.esper.common.client.module.Module;
using Stream = System.IO.Stream;

namespace com.espertech.esper.compiler.@internal.util
{
    public class EPCompilerImpl : EPCompilerSPI
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public EPCompiled CompileQuery(
            string fireAndForgetEPLQuery,
            CompilerArguments arguments)
        {
            return CompileQueryInternal(new CompilableEPL(fireAndForgetEPLQuery, 1), arguments);
        }

        public EPCompiled CompileQuery(
            EPStatementObjectModel fireAndForgetEPLQueryModel,
            CompilerArguments arguments)
        {
            return CompileQueryInternal(new CompilableSODA(fireAndForgetEPLQueryModel, 1), arguments);
        }

        public EPCompiled Compile(
            string epl,
            CompilerArguments arguments)
        {
            if (arguments == null) {
                arguments = new CompilerArguments(new Configuration());
            }

            try {
                var module = EPLModuleUtil.ParseInternal(epl, null);
                IList<Compilable> compilables = new List<Compilable>();
                foreach (var item in module.Items.Where(m => !m.IsCommentOnly)) {
                    var stmtEpl = item.Expression;
                    compilables.Add(new CompilableEPL(stmtEpl, item.LineNumber));
                }

                // determine module name
                var moduleName = DetermineModuleName(arguments.Options, module);
                var moduleUses = DetermineModuleUses(moduleName, arguments.Options, module);

                // get compile services
                var compileTimeServices = GetCompileTimeServices(arguments, moduleName, moduleUses, false);
                AddModuleImports(module.Imports, compileTimeServices);

                IDictionary<ModuleProperty, object> moduleProperties = EmptyDictionary<ModuleProperty, object>.Instance;
                if (arguments.Configuration.Compiler.ByteCode.IsAttachModuleEPL) {
                    moduleProperties = new Dictionary<ModuleProperty, object>();
                    moduleProperties[ModuleProperty.MODULETEXT] = epl;
                }

                // compile
                return CompilerHelperModuleProvider.Compile(
                    compilables,
                    moduleName,
                    moduleProperties,
                    compileTimeServices,
                    arguments.Options,
                    arguments.Path);
            }
            catch (EPCompileException) {
                throw;
            }
            catch (ParseException t) {
                throw new EPCompileException(
                    "Failed to parse: " + t.Message,
                    t,
                    new EmptyList<EPCompileExceptionItem>());
            }
            catch (Exception ex) {
                throw new EPCompileException(ex.Message, ex, new EmptyList<EPCompileExceptionItem>());
            }
        }

        public EPCompilerSPIExpression ExpressionCompiler(Configuration configuration)
        {
            var arguments = new CompilerArguments(configuration);
            arguments.Configuration = configuration;
            var compileTimeServices = GetCompileTimeServices(arguments, null, null, false);
            return new EPCompilerSPIExpressionImpl(compileTimeServices);
        }

        public EPStatementObjectModel EplToModel(
            string stmtText,
            Configuration configuration)
        {
            try {
                var mapEnv = new StatementSpecMapEnv(
                    configuration.Container,
                    MakeImportService(configuration),
                    VariableCompileTimeResolverEmpty.INSTANCE,
                    configuration,
                    ExprDeclaredCompileTimeResolverEmpty.INSTANCE,
                    ContextCompileTimeResolverEmpty.INSTANCE,
                    TableCompileTimeResolverEmpty.INSTANCE,
                    ScriptCompileTimeResolverEmpty.INSTANCE,
                    new CompilerServicesImpl(),
                    new ClassProvidedExtensionImpl(ClassProvidedCompileTimeResolverEmpty.INSTANCE));
                var statementSpec = ParseWalk(stmtText, mapEnv);
                var unmapped = StatementSpecMapper.Unmap(statementSpec);
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
            var moduleName = DetermineModuleName(arguments.Options, module);
            var moduleUses = DetermineModuleUses(moduleName, arguments.Options, module);

            // get compile services
            var compileTimeServices = GetCompileTimeServices(arguments, moduleName, moduleUses, false);
            AddModuleImports(module.Imports, compileTimeServices);

            IList<Compilable> compilables = new List<Compilable>();
            foreach (var item in module.Items) {
                if (item.IsCommentOnly) {
                    continue;
                }

                if (item.Expression != null && item.Model != null) {
                    throw new EPCompileException("Module item has both an EPL expression and a statement object model");
                }

                if (item.Expression != null) {
                    compilables.Add(new CompilableEPL(item.Expression, item.LineNumber));
                }
                else if (item.Model != null) {
                    compilables.Add(new CompilableSODA(item.Model, item.LineNumber));
                }
                else {
                    throw new EPCompileException(
                        "Module item has neither an EPL expression nor a statement object model");
                }
            }

            IDictionary<ModuleProperty, object> moduleProperties = new Dictionary<ModuleProperty, object>();
            AddModuleProperty(moduleProperties, ModuleProperty.ARCHIVENAME, module.ArchiveName);
            AddModuleProperty(moduleProperties, ModuleProperty.URI, module.Uri);
            if (arguments.Configuration.Compiler.ByteCode.IsAttachModuleEPL) {
                AddModuleProperty(moduleProperties, ModuleProperty.MODULETEXT, module.ModuleText);
            }

            AddModuleProperty(moduleProperties, ModuleProperty.USEROBJECT, module.UserObjectCompileTime);
            AddModuleProperty(moduleProperties, ModuleProperty.USES, module.Uses.ToArrayOrNull());
            AddModuleProperty(moduleProperties, ModuleProperty.IMPORTS, module.Imports.ToArrayOrNull());

            // compile
            return CompilerHelperModuleProvider.Compile(
                compilables,
                moduleName,
                moduleProperties,
                compileTimeServices,
                arguments.Options,
                arguments.Path);
        }

        public Module ReadModule(
            Stream stream,
            string uri)
        {
            if (Log.IsDebugEnabled) {
                Log.Debug("Reading module from input stream");
            }

            return EPLModuleUtil.ReadInternal(stream, uri);
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
                return EPLModuleUtil.ReadInternal(stream, url.ToString());
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
            var moduleName = DetermineModuleName(arguments.Options, module);
            var moduleUses = DetermineModuleUses(moduleName, arguments.Options, module);

            var moduleCompileTimeServices = GetCompileTimeServices(arguments, moduleName, moduleUses, false);

            var statementNumber = 0;
            try {
                foreach (var item in module.Items) {
                    var services = new StatementCompileTimeServices(statementNumber, moduleCompileTimeServices);
                    if (item.IsCommentOnly) {
                        continue;
                    }

                    if (item.Expression != null && item.Model != null) {
                        throw new EPCompileException(
                            "Module item has both an EPL expression and a statement object model");
                    }
                    
                    var inlinedClassInspection = arguments.Options?.InlinedClassInspection;
                    if (item.Expression != null) {
                        CompilerHelperSingleEPL.ParseCompileInlinedClassesWalk(
                            new CompilableEPL(item.Expression, item.LineNumber),
                            inlinedClassInspection,
                            services);
                    }
                    else if (item.Model != null) {
                        CompilerHelperSingleEPL.ParseCompileInlinedClassesWalk(
                            new CompilableSODA(item.Model, item.LineNumber),
                            inlinedClassInspection,
                            services);
                        item.Model.ToEPL();
                    }
                    else {
                        throw new EPCompileException(
                            "Module item has neither an EPL expression nor a statement object model");
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
            var moduleName = arguments.Options.ModuleName?.Invoke(new ModuleNameContext(null));
            var moduleUses = arguments.Options.ModuleUses?.Invoke(new ModuleUsesContext(moduleName, null));

            var compileTimeServices = GetCompileTimeServices(arguments, moduleName, moduleUses, true);
            try {
                return CompilerHelperFAFProvider.Compile(compilable, compileTimeServices, arguments);
            }
            catch (Exception ex) {
                throw new EPCompileException(
                    ex.Message + " [" + compilable.ToEPL() + "]",
                    ex,
                    new EmptyList<EPCompileExceptionItem>());
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
            return options.ModuleName != null
                ? options.ModuleName.Invoke(new ModuleNameContext(module.Name))
                : module.Name;
        }

        private ICollection<string> DetermineModuleUses(
            string moduleName,
            CompilerOptions options,
            Module module)
        {
            return options.ModuleUses != null
                ? options.ModuleUses.Invoke(new ModuleUsesContext(moduleName, module.Uses))
                : module.Uses;
        }

        private void AddModuleImports(
            ICollection<Import> imports,
            ModuleCompileTimeServices compileTimeServices)
        {
            if (imports != null) {
                foreach (var imported in imports) {
                    try {
                        compileTimeServices.ImportServiceCompileTime.AddImport(imported);
                    }
                    catch (ImportException e) {
                        throw new EPCompileException("Invalid module import: " + e.Message, e);
                    }
                }
            }
        }
    }
} // end of namespace