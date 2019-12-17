using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.common.@internal.util;

namespace com.espertech.esper.common.@internal.context.module
{
    public class ModuleProviderUtil
    {
        public static ModuleProviderResult Analyze(
            EPCompiled compiled,
            ImportService importService)
        {
            var resourceClassName = compiled.Manifest.ModuleProviderClassName;

            // load module resource class
            Type clazz;
            try {
                clazz = TypeHelper.ResolveType(resourceClassName, true);
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

            return new ModuleProviderResult(moduleResource);
        }
    }
}