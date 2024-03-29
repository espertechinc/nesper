///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.hook.vdw;

namespace com.espertech.esper.regressionlib.support.extend.vdw
{
    public class SupportVirtualDWInvalidFactory : VirtualDataWindowFactory
    {
        public void Initialize(VirtualDataWindowFactoryContext initializeContext)
        {
        }

        public VirtualDataWindow Create(VirtualDataWindowContext context)
        {
            return new SupportVirtualDWInvalid();
        }

        public void Destroy()
        {
        }
    }
} // end of namespace