///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client.artifact;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.compile.compiler;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compiler.@internal.util;

namespace com.espertech.esper.compiler.@internal.compiler.abstraction
{
	public class CompilerAbstractionRoslyn : CompilerAbstraction
	{
		public static readonly CompilerAbstractionRoslyn INSTANCE = new CompilerAbstractionRoslyn();

		private CompilerAbstractionRoslyn()
		{
		}

		public CompilerAbstractionArtifactCollection NewArtifactCollection()
		{
			return new CompilerAbstractionArtifactCollectionImpl();
		}

		public ICompileArtifact CompileClasses(
			IList<CodegenClass> classes,
			CompilerAbstractionCompilationContext context,
			CompilerAbstractionArtifactCollection state)
		{
			var sourceList = classes
				.Select(clazz => new RoslynCompiler.SourceCodegen(clazz))
				.Cast<RoslynCompiler.Source>()
				.ToList();

			var container = context.Container;
			var configuration = context.Services.Configuration.Compiler;
			var repository = container.ArtifactRepositoryManager().DefaultRepository;
			var compiler = container
				.RoslynCompiler()
				.WithMetaDataReferences(repository.AllMetadataReferences)
				.WithMetaDataReferences(container.MetadataReferenceProvider()?.Invoke())
				.WithDebugOptimization(configuration.IsDebugOptimization)
				.WithCodeLogging(configuration.Logging.IsEnableCode)
				.WithCodeAuditDirectory(configuration.Logging.AuditDirectory)
				.WithSources(sourceList);

			return repository.Register(compiler.Compile());
		}

		public CompilerAbstractionCompileSourcesResult CompileSources(
			IList<string> sources,
			CompilerAbstractionCompilationContext context,
			CompilerAbstractionArtifactCollection state)
		{
			string Filename(int ii)
			{
				return "provided_" + ii + "_" + CodeGenerationIDGenerator.GenerateClassNameUUID();
			}

			var names = new LinkedHashMap<string, IList<string>>();
			var sourceList = sources
				.Select((_, index) => new RoslynCompiler.SourceBasic(Filename(index), _))
				.Cast<RoslynCompiler.Source>()
				.ToList();
			
			var container = context.Container;
			var configuration = context.Services.Configuration.Compiler;
			var repository = container.ArtifactRepositoryManager().DefaultRepository;
			var compiler = container
				.RoslynCompiler()
				.WithMetaDataReferences(repository.AllMetadataReferences)
				.WithMetaDataReferences(container.MetadataReferenceProvider()?.Invoke())
				.WithDebugOptimization(configuration.IsDebugOptimization)
				.WithCodeLogging(configuration.Logging.IsEnableCode)
				.WithCodeAuditDirectory(configuration.Logging.AuditDirectory)
				.WithSources(sourceList);

			var artifact = repository.Register(compiler.Compile());
			state.Add(new IArtifact[] { artifact });

			// invoke the compile result consumer if one has been provided
			context.CompileResultConsumer?.Invoke(artifact);

			// JaninoCompiler.Compile(
			// 	classText,
			// 	filename,
			// 	state.Classes,
			// 	output,
			// 	context.CompileResultConsumer,
			// 	context.Services);

			return new CompilerAbstractionCompileSourcesResult(names, artifact);
		}
	}
} // end of namespace
