///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;

using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

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
        private static readonly IDictionary<Assembly, MetadataReference> MetadataCacheBindings = 
            new Dictionary<Assembly, MetadataReference>();

        private IReadOnlyCollection<MetadataReference> _metadataReferences = null;

        private IncrementalHash _codegenHash;
        private IList<CodegenClass> _codegenClasses;
        private bool _isCodeLogging;
        private Assembly _assembly;
        private String _assemblyCachePath;

        /// <summary>
        /// Initializes a new instance of the <see cref="RoslynCompiler"/> class.
        /// </summary>
        public RoslynCompiler()
        {
            _codegenClasses = new List<CodegenClass>();
            _codegenHash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
            if (AppDomain.CurrentDomain.DynamicDirectory != null) {
                _assemblyCachePath = AppDomain.CurrentDomain.DynamicDirectory;
            } else if (AppDomain.CurrentDomain.RelativeSearchPath != null) {
                _assemblyCachePath = AppDomain.CurrentDomain.RelativeSearchPath;
            }
            else {
                _assemblyCachePath = AppDomain.CurrentDomain.BaseDirectory;
            }

            //Directory.CreateDirectory(_assemblyCachePath);
        }

        /// <summary>
        /// Gets the assembly.
        /// </summary>
        public Assembly Assembly {
            get => _assembly;
        }

        /// <summary>
        /// Gets or sets the codegen class.
        /// </summary>
        public IList<CodegenClass> CodegenClasses {
            get => _codegenClasses;
            set => _codegenClasses = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether we include code logging.
        /// </summary>
        public bool IsCodeLogging {
            get => _isCodeLogging;
            set => _isCodeLogging = value;
        }

        public RoslynCompiler WithCodegenClasses(IEnumerable<CodegenClass> codegenClasses)
        {
            this.CodegenClasses = new List<CodegenClass>(codegenClasses);
            return this;
        }

        public RoslynCompiler WithCodegenClass(CodegenClass codegenClass)
        {
            this.CodegenClasses.Add(codegenClass);
            return this;
        }

        public RoslynCompiler WithCodeLogging(bool isCodeLogging)
        {
            this.IsCodeLogging = isCodeLogging;
            return this;
        }

        /// <summary>
        /// Gets the current metadata references.  Metadata references are specific to the AppDomain.
        /// </summary>
        internal IReadOnlyCollection<MetadataReference> GetCurrentMetadataReferences()
        {
            if (_metadataReferences == null) {
                var metadataReferences = new List<MetadataReference>();
                lock (MetadataCacheBindings) {
                    metadataReferences.AddRange(MetadataCacheBindings.Values);
                }

                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                    if (!assembly.IsDynamic && !string.IsNullOrEmpty(assembly.Location)) {
                        lock (PortableExecutionReferenceCache) {
                            if (!PortableExecutionReferenceCache.TryGetValue(
                                assembly,
                                out var portableExecutableReference)) {
                                portableExecutableReference = MetadataReference.CreateFromFile(assembly.Location);
                                PortableExecutionReferenceCache[assembly] = portableExecutableReference;
                            }

                            metadataReferences.Add(portableExecutableReference);
                        }
                    }
                }

                _metadataReferences = metadataReferences;
            }

            return _metadataReferences;
        }

        private SyntaxTree Compile(CodegenClass codegenClass)
        {
            var options = new CSharpParseOptions(kind: SourceCodeKind.Regular, languageVersion: MaxLanguageVersion);
            // Convert the codegen to source
            var source = CodegenSyntaxGenerator.Compile(codegenClass);
            // Update the codegen hash
            _codegenHash.AppendData(source.GetUTF8Bytes());
            // Convert the codegen source to syntax tree
            return CSharpSyntaxTree.ParseText(source, options);
        }

        private static int DebugSequence = 1;

        /// <summary>
        /// Compiles the specified code generation class into an assembly.
        /// </summary>
        public Assembly Compile()
        {
            // Convert the codegen class into it's source representation.
            var syntaxTrees = _codegenClasses
                .Select(Compile)
                .ToList();
            
            // Create an in-memory representation of the compiled source.
            var options = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                .WithOptimizationLevel(OptimizationLevel.Debug)
                .WithAllowUnsafe(true);

            var assemblyHash = _codegenHash.GetHashAndReset().ToHexString();
            var assemblyName = $"NEsper_{assemblyHash}";
            var assemblyPath = Path.Combine(_assemblyCachePath, assemblyName + ".dll");
            if (!File.Exists(assemblyPath)) {
                var compilation = CSharpCompilation
                    .Create(assemblyName, options: options)
                    .AddReferences(GetCurrentMetadataReferences())
                    .AddSyntaxTrees(syntaxTrees);

                foreach (var syntaxTree in syntaxTrees) {
                    string tempClassPath =
                        $@"C:\Src\Espertech\NEsper-master\NEsper\NEsper.Regression.Review\Class{DebugSequence}.cs";
                    DebugSequence++;
                    File.WriteAllText(tempClassPath, syntaxTree.ToString());
                }

                using (var stream = File.Create(assemblyPath)) {
                    var result = compilation.Emit(stream);
                    if (!result.Success) {
                        foreach (var error in result.Diagnostics) {
                            Console.WriteLine(error);
                        }

                        throw new RoslynCompilationException(
                            "failure during module compilation",
                            result.Diagnostics);
                    }
                }
            }

#if DIAGNOSTICS
            Console.WriteLine($"Assembly Pre-Load: {DateTime.Now}");
#endif
            _assembly = Assembly.LoadFile(assemblyPath);

#if DIAGNOSTICS
            Console.WriteLine($"Assembly Loaded (Image): {DateTime.Now}");
            Console.WriteLine($"\tFullName:{_assembly.FullName}");
            Console.WriteLine($"\tName:{_assembly.GetName()}");
#endif

            lock (MetadataCacheBindings) {
                var metadataReference = MetadataReference.CreateFromFile(assemblyPath);
#if DIAGNOSTICS
                Console.WriteLine($"MetaDataReference: {DateTime.Now}");
                Console.WriteLine($"\tFullType: {metadataReference.GetType().FullName}");
                Console.WriteLine($"\tDisplay: {metadataReference.Display}");
                Console.WriteLine($"\tProperties: {metadataReference.Properties}");
#endif
                MetadataCacheBindings[_assembly] = metadataReference;
            }

            return _assembly;
        }

        public static void DeleteAll()
        {
            lock (MetadataCacheBindings) {
                foreach (var metadataAssembly in MetadataCacheBindings.Keys) {
                    try {
                        File.Delete(metadataAssembly.Location);
                    }
                    catch {
                    }
                }
            }
        }
    }
} // end of namespace