///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.runtime.@internal.kernel.service
{
    public class RuntimeExtensionServicesNoHA : RuntimeExtensionServicesSPI
    {
        public static readonly RuntimeExtensionServicesNoHA INSTANCE = new RuntimeExtensionServicesNoHA();

        public void Init(
            EPServicesContext servicesContext,
            EPEventServiceSPI runtimeSPI,
            EPDeploymentServiceSPI adminSPI)
        {
        }

        public void Destroy()
        {
        }

        public bool IsHAEnabled()
        {
            return false;
        }
    }
} // end of namespace