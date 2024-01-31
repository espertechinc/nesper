///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.runtime.@internal.kernel.stage;

namespace com.espertech.esper.runtime.@internal.kernel.service
{
    public class RuntimeExtensionServicesNoHA : RuntimeExtensionServicesSPI
    {
        public static readonly RuntimeExtensionServicesNoHA INSTANCE = new RuntimeExtensionServicesNoHA();

        public void Init(
            EPServicesContext servicesContext,
            EPEventServiceSPI runtimeSPI,
            EPDeploymentServiceSPI adminSPI,
            EPStageServiceSPI stageServiceSPI)
        {
        }

        public void Destroy()
        {
        }

        public bool IsHAEnabled {
            get { return false; }
        }
    }
} // end of namespace