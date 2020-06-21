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

using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.type;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
namespace com.espertech.esper.common.@internal.epl.classprovided.core
{
	public class ClassProvidedImportClassLoader : ByteArrayProvidingClassLoader {
	    private readonly PathRegistry<string, ClassProvided> pathRegistry;
	    private NameAndModule[] imported;

	    public ClassProvidedImportClassLoader(IDictionary<string, byte[]> classes, ClassLoader parent, PathRegistry<string, ClassProvided> pathRegistry)
	    : base(classes, parent)
	    {
	        this.pathRegistry = pathRegistry;
	    }

	    public void SetImported(NameAndModule[] imported) {
	        this.imported = imported;
	    }

	    public Type FindClass(string name) {
		    if (name == null) {
			    throw new ArgumentNullException(nameof(name));
		    }

	        if (imported == null || imported.Length == 0) {
	            return base.FindClass(name);
	        }
	        foreach (NameAndModule nameAndModule in imported) {
	            PathDeploymentEntry<ClassProvided> entry = pathRegistry.GetEntryWithModule(nameAndModule.Name, nameAndModule.ModuleName);
	            foreach (Type clazz in entry.Entity.ClassesMayNull) {
	                if (clazz.FullName == name) {
	                    return clazz;
	                }
	            }
	        }
	        return base.FindClass(name);
	    }
	}
} // end of namespace
