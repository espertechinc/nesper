///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.compile.util;

namespace com.espertech.esper.common.@internal.compile.stage2
{
    public class FilterSpecTracked
    {
        public FilterSpecTracked(
            CallbackAttribution attribution,
            FilterSpecCompiled filterSpecCompiled)
        {
            Attribution = attribution;
            FilterSpecCompiled = filterSpecCompiled;
        }

        public CallbackAttribution Attribution { get; }

        public FilterSpecCompiled FilterSpecCompiled { get; }
    }
} // end of namespace