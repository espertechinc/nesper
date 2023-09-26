///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client.artifact;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.type;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.classprovided.core
{
    public class ClassProvidedImportTypeResolver : ArtifactTypeResolver
    {
        private readonly PathRegistry<string, ClassProvided> _pathRegistry;
        private NameAndModule[] _imported;

        public ClassProvidedImportTypeResolver(
            IArtifactRepository artifactRepository,
            TypeResolver parent,
            PathRegistry<string, ClassProvided> pathRegistry)
            : base(artifactRepository, parent)
        {
            _pathRegistry = pathRegistry;
        }

        public NameAndModule[] Imported {
            get => _imported;
            set => _imported = value;
        }

        public override Type ResolveType(
            string typeName,
            bool resolve)
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

            return base.ResolveType(typeName, resolve);
        }
    }
} // end of namespace