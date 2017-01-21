///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.filter
{
    public class EventTypeIndexBuilderIndexLookupablePair
    {
        public EventTypeIndexBuilderIndexLookupablePair(FilterParamIndexBase index, Object lookupable)
        {
            Index = index;
            Lookupable = lookupable;
        }

        public readonly FilterParamIndexBase Index;
        public readonly object Lookupable;
    }
}
