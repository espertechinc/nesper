///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.hook.forgeinject;
using com.espertech.esper.common.client.hook.vdw;

namespace com.espertech.esper.regressionlib.support.extend.vdw
{
    public class SupportVirtualDWInvalidForge : VirtualDataWindowForge
    {
        public void Initialize(VirtualDataWindowForgeContext initializeContext)
        {
        }

        public VirtualDataWindowFactoryMode FactoryMode =>
            new VirtualDataWindowFactoryModeManaged().SetInjectionStrategyFactoryFactory(
                new InjectionStrategyClassNewInstance(typeof(SupportVirtualDWInvalidFactoryFactory)));

        public ISet<string> UniqueKeyPropertyNames => null;
    }
} // end of namespace