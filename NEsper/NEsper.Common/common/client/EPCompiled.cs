///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.client
{
	/// <summary>
	/// The byte code and manifest of a compile EPL module or EPL fire-and-forget query.
	/// </summary>
	[Serializable]
	public class EPCompiled 
	{
	    private readonly IDictionary<string, byte[]> classes;
	    private readonly EPCompiledManifest manifest;

	    /// <summary>
	    /// Ctor.
	    /// </summary>
	    /// <param name="classes">map of class name and byte code for a classloader</param>
	    /// <param name="manifest">the manifest</param>
	    public EPCompiled(IDictionary<string, byte[]> classes, EPCompiledManifest manifest) {
	        this.classes = classes;
	        this.manifest = manifest;
	    }

	    /// <summary>
	    /// Returns a map of class name and byte code for a classloader
	    /// </summary>
	    /// <returns>classes</returns>
	    public IDictionary<string, byte[]> GetClasses() {
	        return classes;
	    }

	    /// <summary>
	    /// Returns a manifest object
	    /// </summary>
	    /// <returns>manifest</returns>
	    public EPCompiledManifest Manifest {
	        get => manifest;	    }
	}
} // end of namespace