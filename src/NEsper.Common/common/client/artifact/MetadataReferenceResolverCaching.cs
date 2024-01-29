///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;

using Microsoft.CodeAnalysis;

namespace com.espertech.esper.common.client.artifact
{
    public class MetadataReferenceResolverCaching : IDisposable {
        private IDictionary<Assembly, MetadataReference> _metadataReferenceCache;
        private MetadataReferenceResolver _parent;

        /// <summary>
        /// Returns the number of assemblies that have been cached.
        /// </summary>
        public int Count => _metadataReferenceCache?.Count ?? 0;

        /// <summary>
        /// Constructs a caching reference resolver.
        /// </summary>
        /// <param name="parent"></param>
        public MetadataReferenceResolverCaching(MetadataReferenceResolver parent)
        {
            _metadataReferenceCache = new Dictionary<Assembly, MetadataReference>();
            _parent = parent;
        }

        /// <summary>
        /// Resolves a MetadataReference given an Assembly.
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        public MetadataReference Resolve(Assembly assembly)
        {
            lock (_metadataReferenceCache) {
                if (!_metadataReferenceCache.TryGetValue(assembly, out var metadataReference)) {
                    metadataReference = _parent.Invoke(assembly);
                    _metadataReferenceCache[assembly] = metadataReference;
                }

                return metadataReference;
            }
        }

        /// <summary>
        /// Disposes of the cache resolver.
        /// </summary>
        public void Dispose()
        {
            _metadataReferenceCache?.Clear();
            _metadataReferenceCache = null;

            _parent = null;
        }
    };
}