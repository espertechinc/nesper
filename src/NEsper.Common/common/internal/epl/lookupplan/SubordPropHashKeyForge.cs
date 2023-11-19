///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.epl.join.querygraph;

namespace com.espertech.esper.common.@internal.epl.lookupplan
{
    /// <summary>
    ///     Holds property information for joined properties in a lookup.
    /// </summary>
    public class SubordPropHashKeyForge
    {
        public SubordPropHashKeyForge(
            QueryGraphValueEntryHashKeyedForge hashKey,
            int? optionalKeyStreamNum,
            Type coercionType)
        {
            HashKey = hashKey;
            OptionalKeyStreamNum = optionalKeyStreamNum;
            CoercionType = coercionType;
        }

        public QueryGraphValueEntryHashKeyedForge HashKey { get; }

        public Type CoercionType { get; }

        public int? OptionalKeyStreamNum { get; }
    }
} // end of namespace