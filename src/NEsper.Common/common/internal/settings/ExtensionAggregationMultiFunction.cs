///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.settings
{
    public interface ExtensionAggregationMultiFunction
    {
        Pair<Type, string[]> ResolveAggregationMultiFunction(string name);
    }
} // end of namespace