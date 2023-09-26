///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.compile.stage3;

namespace com.espertech.esper.common.@internal.epl.resultset.select.core
{
    public class SelectExprProcessorDescriptor
    {
        public SelectExprProcessorDescriptor(
            SelectSubscriberDescriptor subscriberDescriptor,
            SelectExprProcessorForge forge,
            IList<StmtClassForgeableFactory> additionalForgeables)
        {
            SubscriberDescriptor = subscriberDescriptor;
            Forge = forge;
            AdditionalForgeables = additionalForgeables;
        }

        public SelectSubscriberDescriptor SubscriberDescriptor { get; }

        public SelectExprProcessorForge Forge { get; }

        public IList<StmtClassForgeableFactory> AdditionalForgeables { get; }
    }
} // end of namespace