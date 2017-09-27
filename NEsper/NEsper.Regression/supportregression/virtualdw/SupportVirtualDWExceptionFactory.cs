///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client.hook;

namespace com.espertech.esper.supportregression.virtualdw
{
    public class SupportVirtualDWExceptionFactory : VirtualDataWindowFactory
    {
        #region VirtualDataWindowFactory Members

        public void Initialize(VirtualDataWindowFactoryContext factoryContext)
        {
            throw new Exception("This is a test exception");
        }

        public VirtualDataWindow Create(VirtualDataWindowContext context)
        {
            return new SupportVirtualDWInvalid();
        }

        public void DestroyAllContextPartitions()
        {
        }

        public ICollection<string> UniqueKeyPropertyNames
        {
            get { return null; }
        }

        #endregion
    }
}