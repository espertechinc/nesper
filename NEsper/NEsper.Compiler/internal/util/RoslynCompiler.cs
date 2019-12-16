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
using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace com.espertech.esper.compiler.@internal.util
{
    public class RoslynCompiler
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly LanguageVersion MaxLanguageVersion = Enum
            .GetValues(typeof(LanguageVersion))
            .Cast<LanguageVersion>()
            .Max();

        private static readonly IDictionary<Assembly, PortableExecutableReference> PortableExecutionReferenceCache =
            new Dictionary<Assembly, PortableExecutableReference>();
        private static readonly IDictionary<string, CacheBinding> AssemblyCacheBindings = 
            new Dictionary<string, CacheBinding>();

        private IReadOnlyCollection<MetadataReference> _metadataReferences = null;

        static RoslynCompiler()
        {
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) => {
                Log.Info("AssemblyResolve for {0}", args.Name);
                if (AssemblyCacheBindings.TryGetValue(args.Name, out var bindingPair)) {
                    Log.Debug("AssemblyResolve: Located {0}", args.Name);
                    return bindingPair.Assembly;
                }

                Log.Warn("AssemblyResolve: Unable to locate {0}", args.Name);
                return null;
            };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RoslynCompiler"/> class.
        /// </summary>
        public RoslynCompiler()
        {
            CodegenClasses = new List<CodegenClass>();
        }

        /// <summary>
        /// Gets the assembly.
        /// </summary>
        public Assembly Assembly { get; private set; }

        /// <summary>
        /// Gets or sets the codegen class.
        /// </summary>
        public IList<CodegenClass> CodegenClasses { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether we include code logging.
        /// </summary>
        public bool IsCodeLogging { get; set; }

        /// <summary>
        /// Gets or sets the location for code source to be written.
        /// </summary>
        public string CodeAuditDirectory { get; set; }
        
        public RoslynCompiler WithCodegenClasses(IEnumerable<CodegenClass> codegenClasses)
        {
            CodegenClasses = new List<CodegenClass>(codegenClasses);
            return this;
        }

        public RoslynCompiler WithCodegenClass(CodegenClass codegenClass)
        {
            CodegenClasses.Add(codegenClass);
            return this;
        }

        public RoslynCompiler WithCodeLogging(bool isCodeLogging)
        {
            IsCodeLogging = isCodeLogging;
            return this;
        }

        public RoslynCompiler WithCodeAuditDirectory(string targetDirectory)
        {
            CodeAuditDirectory = targetDirectory;
            return this;
        }
        
        internal bool IsGeneratedAssembly(Assembly assembly)
        {
            var generatedAttributesCount = assembly
                .GetCustomAttributes()
                .OfType<EPGeneratedAttribute>()
                .Count();
            return generatedAttributesCount > 0;
        }

        /// <summary>
        /// Gets the current metadata references.  Metadata references are specific to the AppDomain.
        /// </summary>
        internal IReadOnlyCollection<MetadataReference> GetCurrentMetadataReferences()
        {
            if (_metadataReferences == null) {
                var metadataReferences = new List<MetadataReference>();

                lock (AssemblyCacheBindings) {
                    foreach (var assemblyBinding in AssemblyCacheBindings) {
                        //Console.WriteLine("metadataReferences[0]: {0}", assemblyBinding.Value.Assembly.FullName);
                        metadataReferences.Add(assemblyBinding.Value.MetadataReference);
                    }
                }

                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                    if (!IsGeneratedAssembly(assembly) &&
                        !assembly.IsDynamic && 
                        !string.IsNullOrEmpty(assembly.Location)) {
                        lock (PortableExecutionReferenceCache) {
                            if (!PortableExecutionReferenceCache.TryGetValue(
                                assembly,
                                out var portableExecutableReference)) {
                                portableExecutableReference = MetadataReference.CreateFromFile(assembly.Location);
                                PortableExecutionReferenceCache[assembly] = portableExecutableReference;
                            }

                            //Console.WriteLine("metadataReferences[1]: {0}", assembly.FullName);
                            metadataReferences.Add(portableExecutableReference);
                        }
                    }
                }

                _metadataReferences = metadataReferences;
            }

            return _metadataReferences;
        }

        private Pair<CodegenClass, SyntaxTree> Compile(CodegenClass codegenClass)
        {
            var options = new CSharpParseOptions(kind: SourceCodeKind.Regular, languageVersion: MaxLanguageVersion);
            // Convert the codegen to source
            var source = CodegenSyntaxGenerator.Compile(codegenClass);
            // Convert the codegen source to syntax tree
            return new Pair<CodegenClass, SyntaxTree>(
                codegenClass,
                CSharpSyntaxTree.ParseText(source, options));
        }

        private SyntaxTree CompileAssemblyBindings()
        {
            //Console.WriteLine("Creating assembly bindings");

            CompilationUnitSyntax assemblyBindingsCompilationUnit = CompilationUnit()
                .WithUsings(
                    SingletonList<UsingDirectiveSyntax>(
                        UsingDirective(
                            QualifiedName(
                                QualifiedName(
                                    QualifiedName(
                                        QualifiedName(
                                            IdentifierName("com"),
                                            IdentifierName("espertech")),
                                        IdentifierName("esper")),
                                    IdentifierName("common")),
                                IdentifierName("client")))))
                .WithAttributeLists(
                    SingletonList<AttributeListSyntax>(
                        AttributeList(
                                SingletonSeparatedList<AttributeSyntax>(
                                    Attribute(
                                        IdentifierName("EPGenerated"))))
                            .WithTarget(
                                AttributeTargetSpecifier(
                                    Token(SyntaxKind.AssemblyKeyword)))))
                .NormalizeWhitespace();

            return SyntaxTree(assemblyBindingsCompilationUnit);
        }

#if COMPILATION_DIAGNOSTICS
        private static long totalMicroTime = 0L;
        private static long totalInvocations = 0L;
        private static long minMicroTime = long.MaxValue;
        private static long maxMicroTime = 0L;
#endif
        
        /// <summary>
        /// Compiles the specified code generation class into an assembly.
        /// </summary>
        public Assembly Compile()
        {
#if COMPILATION_DIAGNOSTICS
            var startMicro = PerformanceObserver.MicroTime;
            try {
#endif
                return CompileInternal();
#if COMPILATION_DIAGNOSTICS
            }
            finally {
                var deltaMicro = PerformanceObserver.MicroTime - startMicro;
                totalMicroTime += deltaMicro;
                totalInvocations++;
                if (deltaMicro > maxMicroTime) maxMicroTime = deltaMicro;
                if (deltaMicro < minMicroTime) minMicroTime = deltaMicro;
                
                var averageMicroTime = totalMicroTime / totalInvocations;
                Console.WriteLine(
                    "Invocations: {0}, Time: {1}, Average: {2}, Min: {3}, Max: {4}",
                    totalInvocations,
                    totalMicroTime / 1000,
                    averageMicroTime / 1000,
                    minMicroTime / 1000,
                    maxMicroTime / 1000);
            }
#endif
        }

        /// <summary>
        /// Compiles the specified code generation class into an assembly.
        /// </summary>
        private Assembly CompileInternal()
        {
            // Convert the codegen class into it's source representation.
            var syntaxTreePairs = CodegenClasses
                .Select(Compile)
                .ToList();
            var syntaxTrees = syntaxTreePairs
                .Select(p => p.Second)
                .ToList();

            syntaxTrees.Insert(0, CompileAssemblyBindings());
            
            // Create an in-memory representation of the compiled source.
            var options = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                .WithOptimizationLevel(OptimizationLevel.Debug)
                .WithAllowUnsafe(true);

            var metadataReferences = GetCurrentMetadataReferences();

            var assemblyId = Guid.NewGuid().ToString().Replace("-", "");
            var assemblyName = $"NEsper_{assemblyId}";
            var compilation = CSharpCompilation
                .Create(assemblyName, options: options)
                .AddReferences(metadataReferences)
                .AddSyntaxTrees(syntaxTrees);

            if (CodeAuditDirectory != null) {
                foreach (var syntaxTreePair in syntaxTreePairs) {
                    string tempClassName = syntaxTreePair.First.ClassName;
                    string tempClassPath = Path.Combine(CodeAuditDirectory, $"{tempClassName}.cs");
                    try {
                        File.WriteAllText(tempClassPath, syntaxTreePair.Second.ToString());
                    }
                    catch (Exception) {
                        // Not fatal, but we need to log the failure
                        Log.Warn($"Unable to write audit file for {tempClassName} to \"{tempClassPath}\"");
                    }
                }
            }

#if DIAGNOSTICS
            Console.WriteLine("EmitToImage: {0}", assemblyName);
            foreach (var syntaxTreePair in syntaxTreePairs) {
                Console.WriteLine("\t- {0}", syntaxTreePair.First.ClassName);
            }
#endif

            var assemblyData = EmitToImage(compilation);

#if DIAGNOSTICS
            Console.WriteLine($"Assembly Pre-Load: {DateTime.Now}");
#endif
            Assembly = Assembly.Load(assemblyData);

#if DIAGNOSTICS
            Console.WriteLine($"Assembly Loaded (Image): {DateTime.Now}");
            Console.WriteLine($"\tFullName:{_assembly.FullName}");
            Console.WriteLine($"\tName:{_assembly.GetName()}");
#endif

            lock (AssemblyCacheBindings) {
                var metadataReference = MetadataReference.CreateFromImage(assemblyData);

#if DIAGNOSTICS
                Console.WriteLine($"MetaDataReference: {DateTime.Now}");
                Console.WriteLine($"\tFullType: {metadataReference.GetType().FullName}");
                Console.WriteLine($"\tDisplay: {metadataReference.Display}");
                Console.WriteLine($"\tProperties: {metadataReference.Properties}");
#endif
                AssemblyCacheBindings[Assembly.FullName] = new CacheBinding(Assembly, metadataReference);
            }

            return Assembly;
        }

        private static byte[] EmitToImage(CSharpCompilation compilation)
        {
            using (var stream = new MemoryStream()) {
                var result = compilation.Emit(stream);
                if (!result.Success) {
                    foreach (var error in result.Diagnostics) {
                        Console.WriteLine(error);
                    }

                    throw new RoslynCompilationException(
                        "failure during module compilation",
                        result.Diagnostics);
                }

                return stream.ToArray();
            }
        }

        struct CacheBinding
        {
            internal readonly Assembly Assembly;
            internal readonly MetadataReference MetadataReference;

            public CacheBinding(
                Assembly assembly,
                MetadataReference metadataReference)
            {
                Assembly = assembly;
                MetadataReference = metadataReference;
            }
        }
    }
} // end of namespace