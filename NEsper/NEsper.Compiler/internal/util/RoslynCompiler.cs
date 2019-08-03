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

using com.espertech.esper.common.@internal.bytecodemodel.core;
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

        private static readonly ISet<MetadataReference> MetadataCacheBindings = 
            new HashSet<MetadataReference>();

        private IReadOnlyCollection<MetadataReference> _metadataReferences = null;

        private Guid _assemblyId;
        private String _assemblyName;
        private IList<CodegenClass> _codegenClasses;
        private bool _isCodeLogging;
        private Assembly _assembly;

        /// <summary>
        /// Initializes a new instance of the <see cref="RoslynCompiler"/> class.
        /// </summary>
        public RoslynCompiler()
        {
            _assemblyId = Guid.NewGuid();
            _assemblyName = $"NEsper_{_assemblyId}";
            _codegenClasses = new List<CodegenClass>();
        }

        /// <summary>
        /// Gets or sets the assembly identifier.
        /// </summary>
        public Guid AssemblyId {
            get => _assemblyId;
        }

        /// <summary>
        /// Gets or sets the name of the assembly.
        /// </summary>
        public string AssemblyName {
            get => _assemblyName;
            set => _assemblyName = value;
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
                    metadataReferences.AddRange(MetadataCacheBindings);
                }

                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                    if (!assembly.IsDynamic && !string.IsNullOrEmpty(assembly.Location)) {
                        metadataReferences.Add(MetadataReference.CreateFromFile(assembly.Location));
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
            var source = CodegenClassGenerator.Compile(codegenClass);
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

            var compilation = CSharpCompilation
                .Create(_assemblyName, options: options)
                .AddReferences(GetCurrentMetadataReferences())
                .AddSyntaxTrees(syntaxTrees);

            foreach (var syntaxTree in syntaxTrees) {
                string tempClassPath = $@"C:\Src\Espertech\NEsper-master\NEsper\NEsper.Runtime.Tests\foobar\Class{DebugSequence}.cs";
                DebugSequence++;
                File.WriteAllText(tempClassPath, syntaxTree.ToString());
            }

            using (var stream = new MemoryStream()) {
                var result = compilation.Emit(stream);
                if (result.Success) {
                    stream.Seek(0, SeekOrigin.Begin);
                    // When rewriting this for .NET Core, replace this with System.Runtime.Loader.AssemblyLoadContext
                    _assembly = Assembly.Load(stream.ToArray());
                }
                else {
                    foreach (var error in result.Diagnostics) {
                        Console.WriteLine(error);
                    }

                    throw new RoslynCompilationException(
                        "failure during module compilation",
                        result.Diagnostics);
                }
            }

            lock (MetadataCacheBindings) {
                MetadataCacheBindings.Add(compilation.ToMetadataReference());
            }
            
            return _assembly;

#if false
            try
            {
                string optionalFileName = null;
                if (Boolean.GetBoolean(ICookable.SYSTEM_PROPERTY_SOURCE_DEBUGGING_ENABLE))
                {
                    string dirName = System.GetProperty(ICookable.SYSTEM_PROPERTY_SOURCE_DEBUGGING_DIR);
                    if (dirName == null)
                    {
                        dirName = System.GetProperty("java.io.tmpdir");
                    }
                    var file = new FileInfo(dirName, clazz.ClassName + ".java");
                    if (!file.Exists())
                    {
                        bool created = file.CreateNewFile();
                        if (!created)
                        {
                            throw new RuntimeException("Failed to created file '" + file + "'");
                        }
                    }

                    FileWriter writer = null;
                    try
                    {
                        writer = new FileWriter(file);
                        PrintWriter print = new PrintWriter(writer);
                        print.Write(code);
                        print.Close();
                    }
                    catch (IOException ex)
                    {
                        throw new RuntimeException("Failed to write to file '" + file + "'");
                    }
                    finally
                    {
                        if (writer != null)
                        {
                            writer.Close();
                        }
                    }

                    file.DeleteOnExit();
                    optionalFileName = file.AbsolutePath;
                }

                org.codehaus.janino.Scanner scanner = new Scanner(optionalFileName, new ByteArrayInputStream(
                        code.GetBytes("UTF-8")), "UTF-8");

                ByteArrayProvidingClassLoader cl = new ByteArrayProvidingClassLoader(classes);
                UnitCompiler unitCompiler = new UnitCompiler(
                        new Parser(scanner).ParseCompilationUnit(),
                        new ClassLoaderClassLoader(cl));
                ClassFile[] classFiles = unitCompiler.CompileUnit(true, true, true);
                for (int i = 0; i < classFiles.Length; i++)
                {
                    classes.Put(classFiles[i].ThisClassName, classFiles[i].ToByteArray());
                }

                if (withCodeLogging)
                {
                    Log.Info("Code:\n" + CodeWithLineNum(code));
                }
            }
            catch (Exception ex)
            {
                Log.Error("Failed to compile: " + ex.Message + "\ncode:" + CodeWithLineNum(code));
                throw new RuntimeException(ex);
            }
#endif
        }
    }
} // end of namespace