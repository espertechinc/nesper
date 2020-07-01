///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;

namespace com.espertech.esper.compiler.client.util
{
	/// <summary>
	/// IO-related utilized for <seealso cref="EPCompiled" /></summary>
	public class EPCompiledIOUtil {
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

	    /// <summary>
	    /// Reads the jar file into an <seealso cref="EPCompiled" /> compiled for deployment into a runtime.
	    /// </summary>
	    /// <param name="file">is the source jar file</param>
	    /// <returns>compiled</returns>
	    /// <throws>IOException when the read failed</throws>
	    public static EPCompiled Read(FileInfo file) {
	        JarFile jarFile = new JarFile(file);

	        Attributes attributes = jarFile.Manifest.MainAttributes;
	        string compilerVersion = GetAttribute(attributes, MANIFEST_COMPILER_VERSION);
	        if (compilerVersion == null) {
	            throw new IOException("Manifest is missing " + MANIFEST_COMPILER_VERSION);
	        }
	        string moduleProvider = GetAttribute(attributes, MANIFEST_MODULEPROVIDERCLASSNAME);
	        string queryProvider = GetAttribute(attributes, MANIFEST_QUERYPROVIDERCLASSNAME);
	        if (moduleProvider == null && queryProvider == null) {
	            throw new IOException("Manifest is missing both " + MANIFEST_MODULEPROVIDERCLASSNAME + " and " + MANIFEST_QUERYPROVIDERCLASSNAME);
	        }

	        IDictionary<string, byte[]> classes = new Dictionary<string, byte[]>();
	        try {
	            Enumeration<JarEntry> entries = jarFile.Entries();
	            while (entries.HasMoreElements) {
	                Read(jarFile, entries.NextElement(), classes);
	            }

	        } finally {
	            jarFile.Close();
	        }

	        return new EPCompiled(classes, new EPCompiledManifest(compilerVersion, moduleProvider, queryProvider));
	    }

	    private static string GetAttribute(Attributes attributes, string name) {
	        Attributes.Name attr = new Attributes.Name(name);
	        string value = attributes.GetValue(attr);
	        if (value == null || value.Equals("null")) {
	            return null;
	        }
	        return value;
	    }

	    private static void Write(string name, byte[] value, JarOutputStream target) {
	        name = name.Replace(".", "/") + ".class";
	        JarEntry entry = new JarEntry(name);
	        entry.Time = System.CurrentTimeMillis();
	        target.PutNextEntry(entry);
	        target.Write(value, 0, value.Length);
	        target.CloseEntry();
	    }

	    private static void Read(JarFile jarFile, JarEntry jarEntry, IDictionary<string, byte[]> classes) {
	        if (jarEntry.IsDirectory || jarEntry.Name.Equals("META-INF/MANIFEST.MF")) {
	            return;
	        }

	        long size = jarEntry.Size;
	        if (size > Int32.MaxValue - 1) {
	            throw new IOException("Encountered jar entry with size " + size + " greater than max integer size");
	        }

	        Stream @in = jarFile.GetInputStream(jarEntry);
	        byte[] bytes;
	        try {
	            ByteArrayOutputStream os = new ByteArrayOutputStream();
	            byte[] buffer = new byte[1024];
	            int len;
	            while ((len = @in.Read(buffer)) != -1) {
	                os.Write(buffer, 0, len);
	            }
	            bytes = os.ToByteArray();
	        } finally {
	            @in.Close();
	        }

	        string className = jarEntry.Name.Replace("/", ".");
	        if (className.EndsWith(".class")) {
	            className = className.Substring(0, className.Length - 6);
	        }
	        classes.Put(className, bytes);
	    }
#endif
	}
} // end of namespace