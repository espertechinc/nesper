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
using com.espertech.esper.compat.collections;
using com.espertech.esper.compiler.client.attributes;

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

		/// <summary>
		/// Reads the assembly into an <seealso cref="EPCompiled" /> compiled for deployment
		/// into a runtime.
		/// </summary>
		/// <param name="assemblyName">is the assembly name</param>
		/// <returns>compiled</returns>
		/// <throws>IOException when the read failed</throws>
		public static EPCompiled Read(AssemblyName assemblyName)
		{
			var assembly = AppDomain.CurrentDomain.Load(assemblyName);
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

			var targetHa = false;
			var types = new Dictionary<string, Type>();
			foreach (var type in assembly.GetExportedTypes()) {
				var typeName = type.FullName;
				if (string.IsNullOrWhiteSpace(typeName)) {
					throw new InvalidDataException("type found in assembly without FullName");
				}

				types[typeName] = type;
			}

			return new EPCompiled(
				new [] { assembly },
				new EPCompiledManifest(
					compilerVersion,
					moduleProvider,
					queryProvider,
					targetHa));
		}

#if NOT_SUPPORTED
        /// <summary>
        /// Write the compiled to a jar file. Overwrites the existing jar file.
        /// </summary>
        /// <param name="compiled">compiled</param>
        /// <param name="file">the target file</param>
        /// <throws>IOException when the write failed</throws>
        public static void Write(EPCompiled compiled, FileInfo file) {

	        Manifest manifest = new Manifest();
	        manifest.MainAttributes.Put(Attributes.Name.MANIFEST_VERSION, "1.0");
	        manifest.MainAttributes.Put(new Attributes.Name(MANIFEST_COMPILER_VERSION), compiled.Manifest.CompilerVersion);
	        manifest.MainAttributes.Put(new Attributes.Name(MANIFEST_MODULEPROVIDERCLASSNAME), compiled.Manifest.ModuleProviderClassName);
	        manifest.MainAttributes.Put(new Attributes.Name(MANIFEST_QUERYPROVIDERCLASSNAME), compiled.Manifest.QueryProviderClassName);

	        JarOutputStream target = new JarOutputStream(new FileOutputStream(file), manifest);

	        try {
	            foreach (KeyValuePair<string, byte[]> entry in compiled.Classes) {
	                Write(entry.Key, entry.Value, target);
	            }
	        } finally {
	            target.Close();
	        }
	    }

	    private static void Write(string name, byte[] value, JarOutputStream target) {
	        name = name.Replace(".", "/") + ".class";
	        JarEntry entry = new JarEntry(name);
	        entry.Time = System.CurrentTimeMillis();
	        target.PutNextEntry(entry);
	        target.Write(value, 0, value.Length);
	        target.CloseEntry();
	    }
#endif
	}
} // end of namespace