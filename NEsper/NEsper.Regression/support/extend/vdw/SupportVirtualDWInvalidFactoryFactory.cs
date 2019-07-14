///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.hook.vdw;

namespace com.espertech.esper.regressionlib.support.extend.vdw
{
    public class SupportVirtualDWInvalidFactoryFactory : VirtualDataWindowFactoryFactory
    {
        public VirtualDataWindowFactory CreateFactory(VirtualDataWindowFactoryFactoryContext ctx)
        {
            return new SupportVirtualDWInvalidFactory();
        }
    }
} // end of namespace