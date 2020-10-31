///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.runtime.@internal.kernel.stage;

namespace com.espertech.esper.runtime.@internal.kernel.service
{
    internal interface RuntimeExtensionServicesSPI : RuntimeExtensionServices
    {
        /// <summary>
        ///     Invoked to initialize extension services after runtime services initialization.
        /// </summary>
        /// <param name="servicesContext">the runtime</param>
        /// <param name="runtimeSPI">runtime SPI</param>
        /// <param name="adminSPI">admin SPI</param>
        /// <param name="stageServiceSPI"></param>
        void Init(
            EPServicesContext servicesContext,
            EPEventServiceSPI runtimeSPI,
            EPDeploymentServiceSPI adminSPI,
            EPStageServiceSPI stageServiceSPI);

        /// <summary>
        ///     Invoked to destroy the extension services, when an existing runtime is initialized.
        /// </summary>
        void Destroy();

#if INHERITED_HIDE
        bool IsHAEnabled { get; }
#endif
    }
} // end of namespace