///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.hook.vdw;

namespace NEsper.Examples.VirtualDW
{
    public class SampleVirtualDataWindowFactory : VirtualDataWindowFactory
    {
        public void Initialize(VirtualDataWindowFactoryContext factoryContext)
        {
        }

        public VirtualDataWindow Create(VirtualDataWindowContext context)
        {
            return new SampleVirtualDataWindow(context);
        }

        public void Destroy()
        {
            // cleanup can be performed here
        }

        public ICollection<string> UniqueKeyPropertyNames
        {
            get
            {
                // lets assume there is no unique key property names
                return null;
            }
        }
    }
}