///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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

#if NETCOREAPP3_0_OR_GREATER
using System.Runtime.Loader;
#endif

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.artifact;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compiler.client.attributes;
using com.espertech.esper.container;

namespace com.espertech.esper.compiler.client.util
{
	/// <summary>
	/// IO-related utilized for <seealso cref="EPCompiled" /></summary>
	public class EPCompiledIOUtil
	{
		/// <summary>
		/// Name of the attribute providing the compiler version.
		/// </summary>
		public const string MANIFEST_COMPILER_VERSION = "Esper-CompilerVersion";

		/// <summary>
		/// Name of the attribute providing the module provider class name.
		/// </summary>
		public const string MANIFEST_MODULEPROVIDERCLASSNAME = "Esper-ModuleProvider";

		/// <summary>
		/// Name of the attribute providing the fire-and-forget query provider class name.
		/// </summary>
		public const string MANIFEST_QUERYPROVIDERCLASSNAME = "Esper-QueryProvider";

		/// <summary>
		/// Name of the attribute providing the flag whether the compiler targets high-availability.
		/// </summary>
		public const string MANIFEST_TARGETHA = "Esper-TargetHA";

		private static Assembly LoadAssembly(byte[] image)
		{
#if NETCOREAPP3_0_OR_GREATER
			var assemblyLoadContext = AssemblyLoadContext.CurrentContextualReflectionContext;
			if (assemblyLoadContext == null) {
				assemblyLoadContext = AssemblyLoadContext.Default;
			}

			using var stream = new MemoryStream(image);
			return assemblyLoadContext.LoadFromStream(stream);
#else
			return AppDomain.CurrentDomain.Load(image);
#endif
		}
		
		/// <summary>
		/// Reads the assembly into an <seealso cref="EPCompiled" /> compiled for deployment
		/// into a runtime.
		/// </summary>
		/// <param name="container">The container</param>
		/// <param name="fileInfo">The file containing the assembly</param>
		/// <returns>compiled</returns>
		/// <throws>IOException when the read failed</throws>
		public static EPCompiled Read(IContainer container, FileInfo fileInfo)
		{
			var image = File.ReadAllBytes(fileInfo.FullName);
			var unit = new EPCompilationUnit() {
				Image = image,
				TypeNames = new EmptySet<string>()
			};
			var repository = container.ArtifactRepositoryManager().DefaultRepository;
			var artifact = repository.Register(unit);
			var assembly = artifact.Runtime.Assembly;
			var attributes = assembly
				.GetCustomAttributes()
				.OfType<ManifestPropertyAttribute>()
				.ToDictionary(_ => _.Name, _ => _.Value);

			var compilerVersion = attributes.Get(MANIFEST_COMPILER_VERSION);
			if (compilerVersion == null) {
				throw new IOException("Manifest is missing " + MANIFEST_COMPILER_VERSION);
			}

			var moduleProvider = attributes.Get(MANIFEST_MODULEPROVIDERCLASSNAME);
			var queryProvider = attributes.Get(MANIFEST_QUERYPROVIDERCLASSNAME);
			if (moduleProvider == null && queryProvider == null) {
				throw new IOException("Manifest is missing both " + MANIFEST_MODULEPROVIDERCLASSNAME + " and " + MANIFEST_QUERYPROVIDERCLASSNAME);
			}

			var targetHA = false;
			var types = new Dictionary<string, Type>();
			foreach (var type in assembly.GetExportedTypes()) {
				var typeName = type.FullName;
				if (string.IsNullOrWhiteSpace(typeName)) {
					throw new InvalidDataException("type found in assembly without FullName");
				}

				types[typeName] = type;
			}

			return new EPCompiled(
				repository,
				new [] { artifact },
				new EPCompiledManifest(
					compilerVersion,
					moduleProvider,
					queryProvider,
					targetHA));
		}

		/// <summary>
        /// Write the compiled to a assembly file. Overwrites the existing assembly file.
        /// </summary>
        /// <param name="compiled">compiled</param>
        /// <param name="file">the target file</param>
        /// <throws>IOException when the write failed</throws>
        public static void Write(EPCompiled compiled, FileInfo file)
        {
	        foreach (var assembly in compiled.Artifacts) {
	        }
        }
	}
} // end of namespace