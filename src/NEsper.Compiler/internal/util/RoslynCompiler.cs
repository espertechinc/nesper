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

#if NETCORE
using System.Runtime.Loader;
#endif

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.artifact;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

using IContainer = com.espertech.esper.container.IContainer;

namespace com.espertech.esper.compiler.@internal.util
{
    public partial class RoslynCompiler
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly LanguageVersion MaxLanguageVersion = Enum
            .GetValues(typeof(LanguageVersion))
            .Cast<LanguageVersion>()
            .Max();

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
        /// Application container
        /// </summary>
        public IContainer Container { get; }
        
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


        /// <summary>
        /// Gets or sets the metadata references.
        /// </summary>
        public IEnumerable<MetadataReference> MetadataReferences { get; set; }
        
        /// <summary>
        /// Returns true if the compiler should set optimization to debug.
        /// </summary>
        public bool IsDebugOptimization { get; set; }

        /// <summary>
        /// Initializes the assembly resolution mechanism.
        /// </summary>
        private void InitializeAssemblyResolution()
        {
#if FALSE
#if NETCORE
            LoadContext.Resolving += (
                context,
                name) => {
                Log.Info("AssemblyResolve for {0}", name.Name);
                if (_assemblyCacheBindings.TryGetValue(name.Name, out var bindingPair)) {
                    Log.Debug("AssemblyResolve: Located {0}", name.Name);
                    return bindingPair.Assembly;
                }

                Log.Warn("AssemblyResolve: Unable to locate {0}", name.Name);
                return null;
            };
#else
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) => {
                Log.Info("AssemblyResolve for {0}", args.Name);
                if (_assemblyCacheBindings.TryGetValue(args.Name, out var bindingPair)) {
                    Log.Debug("AssemblyResolve: Located {0}", args.Name);
                    return bindingPair.Assembly;
                }

                Log.Warn("AssemblyResolve: Unable to locate {0}", args.Name);
                return null;
            };
#endif
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

        public RoslynCompiler WithMetaDataReferences(IEnumerable<MetadataReference> metadataReferences)
        {
            MetadataReferences = metadataReferences;
            return this;
        }
        
        public RoslynCompiler WithDebugOptimization(bool isDebugOptimization)
        {
            IsDebugOptimization = isDebugOptimization;
            return this;
        }

        private static bool IsGeneratedAssembly(Assembly assembly)
        {
            var generatedAttributesCount = assembly
                .GetCustomAttributes()
                .OfType<EPGeneratedAttribute>()
                .Count();
            return generatedAttributesCount > 0;
        }

        internal IEnumerable<MetadataReference> GetCoreMetadataReferences()
        {
#if NETCORE
            var coreAssemblies = AssemblyLoadContext.Default.Assemblies;
#else
            var coreAssemblies = AppDomain.CurrentDomain.GetAssemblies();
#endif

            return coreAssemblies
                .Distinct()
                .Select(GetMetadataReference)
                .Where(_ => _ != null);
        }
        
        /// <summary>
        /// Compiles a single source into its syntax elements.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private System.Tuple<string, string, SyntaxTree> Compile(Source source)
        {
            var options = new CSharpParseOptions(kind: SourceCodeKind.Regular, languageVersion: MaxLanguageVersion);
            var syntaxTree = CSharpSyntaxTree.ParseText(source.Code, options);
            var @namespace = GetNamespaceForSyntaxTree(syntaxTree);
            
            // Convert the codegen source to syntax tree
            return new System.Tuple<string, string, SyntaxTree>(
                @namespace, 
                source.Name,
                syntaxTree);
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
        public EPCompilationUnit Compile()
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

        public static bool IsDynamicAssembly(Assembly assembly)
        {
            return (
                assembly.IsDynamic ||
                string.IsNullOrEmpty(assembly.Location) ||
                IsGeneratedAssembly(assembly)
            );
        }
        
        public MetadataReference GetMetadataReference(Assembly assembly)
        {
            if (!IsDynamicAssembly(assembly))
                return MetadataReference.CreateFromFile(assembly.Location);
        
            return null;
        }

        /// <summary>
        /// Compiles the specified code generation class into an assembly.
        /// </summary>
        private EPCompilationUnit CompileInternal()
        {
            // Convert the codegen class into it's source representation.
            var syntaxTreePairs = CreateSyntaxTree();
            var syntaxTrees = syntaxTreePairs.Select(_ => _.Item3).ToList();
            syntaxTrees.Insert(0, CompileAssemblyBindings());

            var exportedTypes = GetExportedTypes(syntaxTrees);

            var optimizationLevel = IsDebugOptimization ? OptimizationLevel.Debug : OptimizationLevel.Release;
            
            // Create an in-memory representation of the compiled source.
            var options = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                .WithOptimizationLevel(optimizationLevel)
                .WithAllowUnsafe(true);

            var metadataReferences = Enumerable
                .Concat(GetCoreMetadataReferences(), MetadataReferences);

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

            var assemblyImage = EmitToImage(compilation);

#if DIAGNOSTICS
            Console.WriteLine($"Assembly Pre-Load: {DateTime.Now}");
#endif

#if DEPRECATED
#if NETCORE
            using (var stream = new MemoryStream(AssemblyImage)) {
                Assembly = LoadContext.LoadFromStream(stream);
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
#endif

            return new EPCompilationUnit() {
                Name = assemblyName,
                Image = assemblyImage,
                TypeNames = exportedTypes
            };
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

        private static ICollection<string> GetExportedTypes(IEnumerable<SyntaxTree> syntaxTreeList)
        {
            var exportVisitor = new ExportVisitor();
            foreach (var syntaxTree in syntaxTreeList) {
                exportVisitor.Visit(syntaxTree.GetRoot());
            }
            return exportVisitor.TypeNames;
        }
        
        public class ExportVisitor : CSharpSyntaxWalker
        {
            private string _namespace = null;

            public readonly ISet<string> TypeNames = new HashSet<string>();
            
            public override void VisitNamespaceDeclaration(NamespaceDeclarationSyntax node)
            {
                _namespace = node.Name.ToFullString().Trim();
                base.VisitNamespaceDeclaration(node);
            }

            public override void VisitClassDeclaration(ClassDeclarationSyntax node)
            {
                var typeName = _namespace != null ? _namespace + "." + node.Identifier.ToFullString().Trim() : node.Identifier.ToFullString().Trim();
                TypeNames.Add(typeName);
                base.VisitClassDeclaration(node);
            }
        }
        
        private static string GetNamespaceForSyntaxTree(SyntaxTree syntaxTree)
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
    }
} // end of namespace