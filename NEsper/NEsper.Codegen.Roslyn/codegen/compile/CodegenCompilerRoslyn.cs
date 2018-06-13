///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using com.espertech.esper.client;
using com.espertech.esper.client.util;
using com.espertech.esper.codegen.core;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.util;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace com.espertech.esper.codegen.compile
{
    public class CodegenCompilerRoslyn : ICodegenCompiler
    {
        public const string DEFAULT_ASSEMBLY_NAME = "NEsper.CodeGen";

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly ICollection<MetadataReference> _references;
        private Compilation _compilation;

        /// <summary>
        /// Gets or sets a value indicating whether this instance is debug enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is debug enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsDebugEnabled { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CodegenCompilerRoslyn" /> class.
        /// </summary>
        /// <param name="assemblyName">Name of the assembly.</param>
        /// <param name="isOptimizationEnabled">if set to <c>true</c> [is optimization enabled].</param>
        public CodegenCompilerRoslyn(string assemblyName = null, bool isOptimizationEnabled = true)
        {
            if (assemblyName == null)
            {
                assemblyName = DEFAULT_ASSEMBLY_NAME;
            }

            _references = AppDomain.CurrentDomain.GetAssemblies()
                .Select(assembly => MetadataReference.CreateFromFile(assembly.Location))
                .ToArray();
            _compilation = CreateCompilation(assemblyName, isOptimizationEnabled);
        }

        /// <summary>
        /// Creates the compilation.  This routine should only be called once per engine.
        /// </summary>
        /// <param name="assemblyName">Name of the assembly.</param>
        /// <param name="enableOptimizations">if set to <c>true</c> [enable optimizations].</param>
        /// <returns></returns>
        private Compilation CreateCompilation(string assemblyName, bool enableOptimizations)
        {
            var optimizationLevel = enableOptimizations
                ? OptimizationLevel.Release
                : OptimizationLevel.Debug;
            var options = new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary,
                optimizationLevel: optimizationLevel
            );

            return CSharpCompilation.Create(assemblyName, null, _references, options);
        }

        public EventPropertyGetter Compile(
            ICodegenClass clazz, 
            ClassLoaderProvider classLoaderProvider, 
            Type interfaceClass,
            string classLevelComment)
        {
            // build members and namespaces
            var memberSet = new LinkedHashSet<ICodegenMember>(clazz.Members);
            var classes = clazz.GetReferencedClasses();
            var imports = CompileImports(classes);
    
            // generate code
            var code = GenerateCode(imports, clazz, memberSet, classLevelComment);
    
            var version = LanguageVersion.Latest;
            var options = new CSharpParseOptions(
                languageVersion: version,
                documentationMode: DocumentationMode.None,
                kind: SourceCodeKind.Regular,
                preprocessorSymbols: null);

            var syntaxTree = CSharpSyntaxTree.ParseText(code, options);

            _compilation = _compilation.AddSyntaxTrees(syntaxTree);

            using (var stream = new MemoryStream())
            {
                var emitResult = _compilation.Emit(stream);
                if (emitResult.Success)
                {
                    stream.Seek(0, SeekOrigin.Begin);
                    var assembly = Assembly.Load(stream.ToArray());
                }
                else
                {
                    var failures = emitResult.Diagnostics.Where(diagnostic =>
                        diagnostic.IsWarningAsError ||
                        diagnostic.Severity == DiagnosticSeverity.Error);
                }
            }
            throw new NotImplementedException();
        }

        private IDictionary<Type, string> CompileImports(ICollection<Type> classes)
        {
            var imports = new Dictionary<Type, string>();
            var assignments = new Dictionary<string, Type>();
            foreach (var clazz in classes) {
                if (clazz == null) {
                    continue;
                }
                if (clazz.IsArray) {
                    CompileImports(TypeHelper.GetComponentTypeOutermost(clazz), imports, assignments);
                } else {
                    CompileImports(clazz, imports, assignments);
                }
            }
            return imports;
        }
    
        private void CompileImports(
            Type clazz, 
            IDictionary<Type, string> imports,
            IDictionary<string, Type> assignments)
        {
            if (clazz == null || clazz.IsPrimitive) {
                return;
            }
    
            if (clazz.Namespace == "System") {
                imports.Put(clazz, clazz.Name);
                return;
            }
    
            if (assignments.ContainsKey(clazz.Name)) {
                return;
            }
            imports.Put(clazz, clazz.Name);
            assignments.Put(clazz.Name, clazz);
        }

        private void WriteUsingDeclarations(TextWriter textWriter, IEnumerable<string> namespaces)
        {
            foreach (var @namespace in namespaces)
            {
                textWriter.WriteLine("using {0};", @namespace);
            }
        }

        private void WriteNamespace(TextWriter textWriter, string @namespace, Action textAction)
        {
            textWriter.WriteLine("namespace {0} {{", @namespace);
            textAction.Invoke();
            textWriter.WriteLine("}}");
        }

        private void WriteClass(
            TextWriter textWriter, 
            ICodegenClass clazz,
            Action textAction)
        {
            textWriter.Write("    public class {0}", clazz.ClassName);
            if (clazz.InterfaceImplemented != null)
            {
                textWriter.Write(" : {0}", clazz.InterfaceImplemented.FullName);
            }

            textWriter.WriteLine();
            textWriter.WriteLine("    {{");
            textWriter.WriteLine("    }}");
        }

        private void WriteFields(
            TextWriter textWriter,
            ICodegenClass clazz)
        {
            if (clazz.Members.HasFirst())
            {
                foreach (var member in clazz.Members)
                {
                    string modifiers = "private";
                    string typeName = CodeGenerationHelper.CompliantName(member.MemberType);
                    textWriter.WriteLine("{0} {1} {2};",modifiers, typeName, member.MemberName);
                }

                textWriter.WriteLine();
            }
        }

        private void WriteMethods(
            TextWriter textWriter,
            ICodegenClass clazz)
        {
            foreach (var method in clazz.PublicMethods)
                method.Render(textWriter, true);
            foreach (var method in clazz.PrivateMethods)
                method.Render(textWriter, false);
        }

        private void WriteConstructor(
            TextWriter textWriter,
            ICodegenClass clazz)
        {
            textWriter.Write("public {0}", clazz.ClassName);
            // start constructor arguments
            textWriter.Write("(");
            textWriter.Write(string.Join(",", clazz.Members.Select(CompliantTypeAndName)));
            textWriter.Write(")");
            textWriter.WriteLine();
            // start constructor body
            textWriter.WriteLine("{{");
            // start parameter assignments
            foreach (var member in clazz.Members)
                textWriter.WriteLine("this.{0} = {0};", member.MemberName);
            // end constructor body
            textWriter.WriteLine("}}");
            textWriter.WriteLine("");
        }

        private string GenerateCode(
            IDictionary<Type, string> namespaces, 
            ICodegenClass clazz, 
            ICollection<ICodegenMember> memberSet, 
            string classLevelComment)
        {
            var writer = new StringWriter();

            WriteUsingDeclarations(writer, namespaces.Values);
            WriteNamespace(writer, clazz.Namespace, () =>
                WriteClass(writer, clazz, () =>
                {
                    WriteFields(writer, clazz);
                    WriteConstructor(writer, clazz);
                    WriteMethods(writer, clazz);
                }));

            return writer.ToString();
        }
        
        private static string CompliantTypeAndName(ICodegenMember member)
        {
            return CodeGenerationHelper.CompliantName(member.MemberType) + " " + member.MemberName;
        }
    }
} // end of namespace
