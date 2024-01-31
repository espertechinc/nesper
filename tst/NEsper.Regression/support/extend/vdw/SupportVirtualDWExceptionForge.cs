///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.hook.vdw;
using com.espertech.esper.compat;

namespace com.espertech.esper.regressionlib.support.extend.vdw
{
    public class SupportVirtualDWExceptionForge : VirtualDataWindowForge
    {
        public void Initialize(VirtualDataWindowForgeContext initializeContext)
        {
            throw new EPException("This is a test exception");
        }

        public VirtualDataWindowFactoryMode FactoryMode => throw new IllegalStateException();

        public ISet<string> UniqueKeyPropertyNames => null;
    }
} // end of namespace