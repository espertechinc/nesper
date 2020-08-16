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

using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.type;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
namespace com.espertech.esper.common.@internal.epl.classprovided.core
{
	public class ClassProvidedImportClassLoader : ClassLoader
	{
		private readonly ClassLoader _parent;
		private readonly PathRegistry<string, ClassProvided> _pathRegistry;
		private NameAndModule[] _imported;

		public ClassProvidedImportClassLoader(
			ClassLoader parent,
			PathRegistry<string, ClassProvided> pathRegistry)
		{
			_parent = parent;
			_pathRegistry = pathRegistry;
		}

		public NameAndModule[] Imported {
			get => _imported;
			set => _imported = value;
		}

		public Type GetClass(string typeName)
		{
			if (typeName == null) {
				throw new ArgumentNullException(nameof(typeName));
			}

			if (_imported != null && _imported.Length != 0) {
				foreach (var nameAndModule in _imported) {
					var entry = _pathRegistry.GetEntryWithModule(nameAndModule.Name, nameAndModule.ModuleName);
					foreach (var clazz in entry.Entity.ClassesMayNull) {
						if (clazz.FullName == typeName) {
							return clazz;
						}
					}
				}
			}

			return _parent.GetClass(typeName);
		}
	}
} // end of namespace
