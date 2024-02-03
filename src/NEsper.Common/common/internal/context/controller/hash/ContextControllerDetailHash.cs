///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.context.controller.core;

namespace com.espertech.esper.common.@internal.context.controller.hash
{
    public class ContextControllerDetailHash : ContextControllerDetail
    {
        public ContextControllerDetailHashItem[] Items { get; set; }

        public int Granularity { get; set; }

        public bool IsPreallocate { get; set; }
    }
} // end of namespace