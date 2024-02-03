///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Reflection;

using com.espertech.esper.container;

using Microsoft.CodeAnalysis;

namespace com.espertech.esper.common.client.artifact
{
    public static class MetadataReferenceResolverExtensions
    {
        /// <summary>
        /// Gets a metadata reference from an assembly.  This method provides no caching of the metadata
        /// reference.
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns>a metadata reference for the requested assembly</returns>
        public static MetadataReference GetMetadataReference(Assembly assembly)
        {
            return string.IsNullOrEmpty(assembly.Location) ? null : MetadataReference.CreateFromFile(assembly.Location);
        }

        /// <summary>
        /// Retrieves an instance of the MetadataReferenceResolver.  If none has been registered, this method
        /// will return the default resolver.
        /// </summary>
        /// <param name="container"></param>
        /// <returns></returns>
        public static MetadataReferenceResolver MetadataReferenceResolver(this IContainer container)
        {
            container.CheckContainer();

            lock (container) {
                if (container.DoesNotHave<MetadataReferenceResolver>()) {
                    return GetMetadataReference;
                }
            }

            return container.Resolve<MetadataReferenceResolver>();
        }

        public static bool RegisterMetadataReferenceResolver(
            this IContainer container,
            MetadataReferenceResolver resolver)
        {
            container.CheckContainer();

            lock (container) {
                if (container.DoesNotHave<MetadataReferenceResolver>()) {
                    container.Register(resolver, Lifespan.Singleton);
                    return true;
                }
            }

            return false;
        }
    }
}