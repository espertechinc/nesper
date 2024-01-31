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
    public class SupportVirtualDWForge : VirtualDataWindowForge
    {
        private static ISet<string> uniqueKeys;

        public static IList<VirtualDataWindowForgeContext> Initializations { get; } =
            new List<VirtualDataWindowForgeContext>();

        public static ISet<string> UniqueKeys {
            set => uniqueKeys = value;
        }

        public void Initialize(VirtualDataWindowForgeContext initializeContext)
        {
            Initializations.Add(initializeContext);
        }

        public VirtualDataWindowFactoryMode FactoryMode =>
            new VirtualDataWindowFactoryModeManaged()
                .SetInjectionStrategyFactoryFactory(
                    new InjectionStrategyClassNewInstance(typeof(SupportVirtualDWFactoryFactory)));

        public ISet<string> UniqueKeyPropertyNames => uniqueKeys;
    }
} // end of namespace