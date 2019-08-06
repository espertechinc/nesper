///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.compiler.@internal.util;
using com.espertech.esper.container;
using com.espertech.esper.runtime.client;

namespace com.espertech.esper.regressionrun.runner
{
    public class RegressionSession
    {
        public RegressionSession(Configuration configuration)
        {
            Configuration = configuration;
        }

        public IContainer Container => Configuration.Container;

        public Configuration Configuration { get; }

        public EPRuntime Runtime { get; set; }

        public void Destroy()
        {
            Runtime?.Destroy();
        }
    }
} // end of namespace