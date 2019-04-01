///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.io;

namespace com.espertech.esper.common.client
{
	/// <summary>
	/// Manifest is part of the <seealso cref="EPCompiled" /> and provides information for the runtime that
	/// allows it to use the byte code.
	/// </summary>
	[Serializable]
	public class EPCompiledManifest 
	{
	    private readonly string compilerVersion;
	    private readonly string moduleProviderClassName;
	    private readonly string queryProviderClassName;

	    /// <summary>
	    /// Ctor.
	    /// </summary>
	    /// <param name="compilerVersion">compiler version</param>
	    /// <param name="moduleProviderClassName">class name of the class providing the module, or null for fire-and-forget query</param>
	    /// <param name="queryProviderClassName">class name of the class providing the fire-and-forget query, or null when this is a module</param>
	    public EPCompiledManifest(string compilerVersion, string moduleProviderClassName, string queryProviderClassName) {
	        this.compilerVersion = compilerVersion;
	        this.moduleProviderClassName = moduleProviderClassName;
	        this.queryProviderClassName = queryProviderClassName;
	    }

	    /// <summary>
	    /// Returns the compiler version.
	    /// </summary>
	    /// <returns>compiler version</returns>
	    public string CompilerVersion {
	        get => compilerVersion;	    }

	    /// <summary>
	    /// Returns the class name of the class providing the module, or null for fire-and-forget query
	    /// </summary>
	    /// <returns>class name</returns>
	    public string ModuleProviderClassName {
	        get => moduleProviderClassName;	    }

	    /// <summary>
	    /// Returns the class name of the class providing the fire-and-forget query, or null when this is a module
	    /// </summary>
	    /// <returns>class name</returns>
	    public string QueryProviderClassName {
	        get => queryProviderClassName;	    }

	    /// <summary>
	    /// Write the manifest to output.
	    /// </summary>
	    /// <param name="output">output</param>
	    /// <throws>IOException when an IO exception occurs</throws>
	    public void Write(DataOutput output) {
	        output.WriteUTF(compilerVersion);
	        WriteNullableString(moduleProviderClassName, output);
	        WriteNullableString(queryProviderClassName, output);
	    }

	    /// <summary>
	    /// Read the manifest from input.
	    /// </summary>
	    /// <param name="input">input</param>
	    /// <returns>manifest</returns>
	    /// <throws>IOException when an IO exception occurs</throws>
	    public static EPCompiledManifest Read(DataInput input) {
	        string compilerVersion = input.ReadUTF();
	        string moduleClassName = ReadNullableString(input);
	        string queryClassName = ReadNullableString(input);
	        return new EPCompiledManifest(compilerVersion, moduleClassName, queryClassName);
	    }

	    private void WriteNullableString(string value, DataOutput output) {
	        if (value == null) {
	            output.WriteBoolean(false);
	            return;
	        }
	        output.WriteBoolean(true);
	        output.WriteUTF(value);
	    }

	    private static string ReadNullableString(DataInput input) {
	        bool hasValue = input.ReadBoolean();
	        if (!hasValue) {
	            return null;
	        }
	        return input.ReadUTF();
	    }
	}
} // end of namespace