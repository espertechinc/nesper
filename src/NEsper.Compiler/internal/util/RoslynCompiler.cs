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

#if NETSTANDARD
using System.Runtime.Loader;
#endif

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.assembly;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.container;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace com.espertech.esper.compiler.@internal.util
{
    public partial class RoslynCompiler
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        internal static readonly LanguageVersion MaxLanguageVersion = Enum
            .GetValues(typeof(LanguageVersion))
            .Cast<LanguageVersion>()
            .Max();

        private readonly IDictionary<Assembly, PortableExecutableReference> _portableExecutionReferenceCache =
            new Dictionary<Assembly, PortableExecutableReference>();
        private readonly IDictionary<string, CacheBinding> _assemblyCacheBindings = 
            new Dictionary<string, CacheBinding>();

        private IList<MetadataReference> _metadataReferences = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="RoslynCompiler"/> class.
        /// </summary>
        public RoslynCompiler(IContainer container)
        {
            Container = container;
            Sources = new List<Source>();
            InitializeAssemblyResolution();
        }

        /// <summary>
        /// Gets the container.
        /// </summary>
        public IContainer Container { get; set; }
        
        /// <summary>
        /// Gets the assembly.
        /// </summary>
        public Assembly Assembly { get; private set; }

        /// <summary>
        /// Gets the assembly image.
        /// </summary>
        public byte[] AssemblyImage { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether we include code logging.
        /// </summary>
        public bool IsCodeLogging { get; set; }

        /// <summary>
        /// Gets or sets the location for code source to be written.
        /// </summary>
        public string CodeAuditDirectory { get; set; }
        
        /// <summary>
        /// Gets or sets the codegen class.
        /// </summary>
        public IList<Source> Sources { get; set; }
        
#if NETSTANDARD
        Assembly OnLoadContextOnResolving(
            AssemblyLoadContext context,
            AssemblyName name)
        {
            Log.Info("AssemblyResolve for {0}", name.Name);
            if (_assemblyCacheBindings.TryGetValue(name.Name, out var bindingPair)) {
                Log.Debug("AssemblyResolve: Located {0}", name.Name);
                return bindingPair.Assembly;
            }

            Log.Warn("AssemblyResolve: Unable to locate {0}", name.Name);
            return null;
        }
#endif

        private Assembly OnCurrentDomainOnAssemblyResolve(
            object sender,
            ResolveEventArgs args)
        {
            Log.Info("AssemblyResolve for {0}", args.Name);
            if (_assemblyCacheBindings.TryGetValue(args.Name, out var bindingPair)) {
                Log.Debug("AssemblyResolve: Located {0}", args.Name);
                return bindingPair.Assembly;
            }

            Log.Warn("AssemblyResolve: Unable to locate {0}", args.Name);
            return null;
        }

        /// <summary>
        /// Initializes the assembly resolution mechanism.
        /// </summary>
        private void InitializeAssemblyResolution()
        {
#if !NETSTANDARD
            AppDomain.CurrentDomain.AssemblyResolve += OnCurrentDomainOnAssemblyResolve;
#endif
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


        public RoslynCompiler WithCodegenClasses(IList<CodegenClass> sorted)
        {
            Sources = sorted.Select(_ => new SourceCodegen(_)).ToList<Source>();
            return this;
        }
        
        public RoslynCompiler WithSources(IList<Source> sources)
        {
            Sources = sources;
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
#if NETSTANDARD
        internal ICollection<MetadataReference> GetCurrentMetadataReferences(AssemblyLoadContext loadContext)
#else
        internal ICollection<MetadataReference> GetCurrentMetadataReferences(AppDomain currentDomain)
#endif
        {
            if (_metadataReferences == null) {
                var metadataReferences = new List<MetadataReference>();

                lock (_assemblyCacheBindings) {
                    foreach (var assemblyBinding in _assemblyCacheBindings) {
                        metadataReferences.Add(assemblyBinding.Value.MetadataReference);
                    }
                }

#if NETSTANDARD
                var assemblies = Enumerable.Concat(
                    AssemblyLoadContext.Default.Assemblies,
                    loadContext.Assemblies);
#else
                var assemblies = currentDomain.GetAssemblies();
#endif

                foreach (var assembly in assemblies) {
                    if (!IsGeneratedAssembly(assembly) &&
                        !assembly.IsDynamic && 
                        !string.IsNullOrEmpty(assembly.Location)) {
                        lock (_portableExecutionReferenceCache) {
                            if (!_portableExecutionReferenceCache.TryGetValue(
                                assembly,
                                out var portableExecutableReference)) {
                                portableExecutableReference = MetadataReference.CreateFromFile(assembly.Location);
                                _portableExecutionReferenceCache[assembly] = portableExecutableReference;
                            }

                            metadataReferences.Add(portableExecutableReference);
                        }
                    }
                }

                _metadataReferences = metadataReferences;
            }

            return _metadataReferences;
        }

        /// <summary>
        /// Compiles a single source into its syntax elements.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private System.Tuple<string, string, SyntaxTree> Compile(Source source)
        {
            try {
                var options = new CSharpParseOptions(kind: SourceCodeKind.Regular, languageVersion: MaxLanguageVersion);
                var syntaxTree = CSharpSyntaxTree.ParseText(source.Code, options);
                var @namespace = GetNamespaceForSyntaxTree(syntaxTree);

                // Convert the codegen source to syntax tree
                return new System.Tuple<string, string, SyntaxTree>(
                    @namespace,
                    source.Name,
                    syntaxTree);
            }
            finally {
            }
        }
        
        /// <summary>
        /// Creates a syntax-tree list.
        /// </summary>
        /// <returns></returns>
        private IList<System.Tuple<string, string, SyntaxTree>> CreateSyntaxTree()
        {
            return Sources.Select(Compile).ToList();
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
        public Pair<Assembly, byte[]> Compile(CompilationContext compilationContext)
        {
#if COMPILATION_DIAGNOSTICS
            var startMicro = PerformanceObserver.MicroTime;
            try {
#endif
                return CompileInternal(compilationContext);
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
        private Pair<Assembly, byte[]> CompileInternal(CompilationContext compilationContext)
        {
#if NETSTANDARD
            var loadContext = Container.GetLoadContext(compilationContext);
#else
            var currentDomain = AppDomain.CurrentDomain;
#endif
            
            try {
#if NETSTANDARD
                loadContext.Resolving += OnLoadContextOnResolving;
#else
                currentDomain.AssemblyResolve += OnCurrentDomainOnAssemblyResolve;
#endif

                // Convert the codegen class into it's source representation.
                var syntaxTreePairs = CreateSyntaxTree();
                var syntaxTrees = syntaxTreePairs.Select(_ => _.Item3).ToList();
                syntaxTrees.Insert(0, CompileAssemblyBindings());

                // Create an in-memory representation of the compiled source.
                var options = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                    .WithOptimizationLevel(OptimizationLevel.Debug)
                    .WithAllowUnsafe(true);

#if NETSTANDARD
                var metadataReferences = GetCurrentMetadataReferences(loadContext);
#else
                var metadataReferences = GetCurrentMetadataReferences(currentDomain);
#endif

                var assemblyId = Guid.NewGuid().ToString().Replace("-", "");
                var assemblyName = $"NEsper_{assemblyId}";
                var compilation = CSharpCompilation
                    .Create(assemblyName, options: options)
                    .AddReferences(metadataReferences)
                    .AddSyntaxTrees(syntaxTrees);

                if (CodeAuditDirectory != null) {
                    WriteCodeAudit(syntaxTreePairs, CodeAuditDirectory);
                }

#if DIAGNOSTICS
                Console.WriteLine("EmitToImage: {0}", assemblyName);
                foreach (var syntaxTreePair in syntaxTreePairs) {
                    Console.WriteLine("\t- {0}", syntaxTreePair.First.ClassName);
                }
#endif

                AssemblyImage = EmitToImage(compilation);

#if DIAGNOSTICS
                Console.WriteLine($"Assembly Pre-Load: {DateTime.Now}");
#endif

#if NETSTANDARD
                using (var stream = new MemoryStream(AssemblyImage)) {
                    Assembly = loadContext.LoadFromStream(stream);
                }
#else
                Assembly = AppDomain.CurrentDomain.Load(AssemblyImage);
#endif

#if DIAGNOSTICS
                Console.WriteLine($"Assembly Loaded (Image): {DateTime.Now}");
                Console.WriteLine($"\tFullName:{_assembly.FullName}");
                Console.WriteLine($"\tName:{_assembly.GetName()}");
#endif

                lock (_assemblyCacheBindings) {
                    var metadataReference = MetadataReference.CreateFromImage(AssemblyImage);

#if DIAGNOSTICS
                Console.WriteLine($"MetaDataReference: {DateTime.Now}");
                Console.WriteLine($"\tFullType: {metadataReference.GetType().FullName}");
                Console.WriteLine($"\tDisplay: {metadataReference.Display}");
                Console.WriteLine($"\tProperties: {metadataReference.Properties}");
#endif
                    _metadataReferences.Add(metadataReference);
                    _assemblyCacheBindings[Assembly.FullName] = new CacheBinding(Assembly, metadataReference);
                }

                return new Pair<Assembly, byte[]>(Assembly, AssemblyImage);
            }
            finally {
#if NETSTANDARD
                loadContext.Resolving -= OnLoadContextOnResolving;
#else
                currentDomain.AssemblyResolve -= OnCurrentDomainOnAssemblyResolve;
#endif
            }
        }

        private void WriteCodeAudit(IList<System.Tuple<string, string, SyntaxTree>> syntaxTreePairs, string targetDirectory)
        {
            foreach (var syntaxTreePair in syntaxTreePairs) {
                string tempNamespace = syntaxTreePair.Item1;
                string tempClassName = syntaxTreePair.Item2;
                string tempClassPath = Path.Combine(targetDirectory, tempNamespace);

                try {
                    if (!Directory.Exists(tempClassPath)) {
                        Directory.CreateDirectory(tempClassPath);
                    }

                    tempClassPath = Path.Combine(tempClassPath, $"{tempClassName}.cs");
                    File.WriteAllText(tempClassPath, syntaxTreePair.Item3.ToString());
                }
                catch (Exception) {
                    // Not fatal, but we need to log the failure
                    Log.Warn($"Unable to write audit file for {tempClassName} to \"{tempClassPath}\"");
                }
            }
        }

        private static byte[] EmitToImage(CSharpCompilation compilation)
        {
            using (var stream = new MemoryStream()) {
                var result = compilation.Emit(stream);
                if (!result.Success) {
                    var diagnosticsMessage = result.Diagnostics.RenderAny();
                    throw new RoslynCompilationException(
                        "Failure during module compilation: " + diagnosticsMessage,
                        result.Diagnostics);
                }

                return stream.ToArray();
            }
        }

        private string GetNamespaceForSyntaxTree(SyntaxTree syntaxTree)
        {
            var namespaceVisitor = new NamespaceVisitor();
            namespaceVisitor.Visit(syntaxTree.GetRoot());
            return namespaceVisitor.Namespace;
        }

        public class NamespaceVisitor : CSharpSyntaxWalker
        {
            private string _namespace = "generated";

            public string Namespace => _namespace;

            public override void VisitNamespaceDeclaration(NamespaceDeclarationSyntax node)
            {
                _namespace = node.Name.ToFullString().Trim();
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