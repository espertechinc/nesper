///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client.hook;

namespace com.espertech.esper.supportregression.virtualdw
{
    public class SupportVirtualDWFactory : VirtualDataWindowFactory
    {
        static SupportVirtualDWFactory()
        {
            Initializations = new List<VirtualDataWindowFactoryContext>();
            Windows = new List<SupportVirtualDW>();
        }

        public static IList<SupportVirtualDW> Windows { get; private set; }

        public static bool IsDestroyed { get; set; }

        public static IList<VirtualDataWindowFactoryContext> Initializations { get; private set; }

        public static ICollection<string> UniqueKeys { get; set; }

        public ICollection<string> UniqueKeyPropertyNames
        {
            get { return UniqueKeys; }
        }

        public VirtualDataWindow Create(VirtualDataWindowContext context)
        {
            var vdw = new SupportVirtualDW(context);
            Windows.Add(vdw);
            return vdw;
        }

        public void Initialize(VirtualDataWindowFactoryContext factoryContext)
        {
            Initializations.Add(factoryContext);
        }

        public void DestroyAllContextPartitions()
        {
            IsDestroyed = true;
        }
    }
}
