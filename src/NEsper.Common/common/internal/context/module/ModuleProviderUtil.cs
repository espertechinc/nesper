using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.epl.classprovided.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.context.module
{
    public class ModuleProviderUtil
    {
        public static ModuleProviderCLPair Analyze(
            EPCompiled compiled,
            TypeResolver typeResolverParent,
            PathRegistry<string, ClassProvided> classProvidedPathRegistry)
        {
            var classLoader = ClassProvidedImportClassLoaderFactory.GetClassLoader(
                compiled.ArtifactRepository,
                typeResolverParent,
                classProvidedPathRegistry);

            var resourceClassName = compiled.Manifest.ModuleProviderClassName;

            // load module resource class
            Type clazz;
            try {
                clazz = classLoader.ResolveType(resourceClassName, true);
            }
            catch (Exception e) {
                throw new EPException(e);
            }

            // instantiate
            ModuleProvider moduleResource;
            try {
                moduleResource = (ModuleProvider)TypeHelper.Instantiate(clazz);
            }
            catch (EPException) {
                throw;
            }
            catch (Exception e) {
                throw new EPException(e);
            }

            return new ModuleProviderCLPair(classLoader, moduleResource);
        }
    }
}