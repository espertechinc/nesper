///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.hook.vdw;

namespace com.espertech.esper.regressionlib.support.extend.vdw
{
    public class SupportVirtualDWFactory : VirtualDataWindowFactory
    {
        public static bool IsDestroyed { get; set; }

        public static IList<SupportVirtualDW> Windows { get; } = new List<SupportVirtualDW>();

        public VirtualDataWindowFactoryContext InitializeContext { get; private set; }

        public void Initialize(VirtualDataWindowFactoryContext initializeContext)
        {
            InitializeContext = initializeContext;
        }

        public VirtualDataWindow Create(VirtualDataWindowContext context)
        {
            var vdw = new SupportVirtualDW(context);
            Windows.Add(vdw);
            return vdw;
        }

        public void Destroy()
        {
            IsDestroyed = true;
        }
    }
} // end of namespace