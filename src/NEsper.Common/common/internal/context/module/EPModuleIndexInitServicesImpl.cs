///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.index.compile;

namespace com.espertech.esper.common.@internal.context.module
{
    public class EPModuleIndexInitServicesImpl : EPModuleIndexInitServices
    {
        public EPModuleIndexInitServicesImpl(IndexCollector indexCollector)
        {
            IndexCollector = indexCollector;
        }

        public IndexCollector IndexCollector { get; }
    }
} // end of namespace