using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.classprovided.core;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.context.module
{
    public class ModuleProviderUtil
    {
        public static ModuleProviderCLPair Analyze(
            EPCompiled compiled,
            ClassLoader classLoaderParent,
            PathRegistry<String, ClassProvided> classProvidedPathRegistry)
        {
            var classLoader = new PriorityClassLoader(classLoaderParent, compiled.Assemblies);
            var resourceClassName = compiled.Manifest.ModuleProviderClassName;

            // load module resource class
            Type clazz;
            try {
                clazz = classLoader.GetClass(resourceClassName);
            }
            catch (Exception e) {
                throw new EPException(e);
            }

            // instantiate
            ModuleProvider moduleResource;
            try {
                moduleResource = (ModuleProvider) TypeHelper.Instantiate(clazz);
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