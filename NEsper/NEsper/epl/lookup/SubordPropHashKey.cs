///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.epl.join.plan;

namespace com.espertech.esper.epl.lookup
{
    /// <summary>Holds property information for joined properties in a lookup. </summary>
    public class SubordPropHashKey
    {
        public SubordPropHashKey(QueryGraphValueEntryHashKeyed hashKey, int? optionalKeyStreamNum, Type coercionType)
        {
            HashKey = hashKey;
            OptionalKeyStreamNum = optionalKeyStreamNum;
            CoercionType = coercionType;
        }

        public int? OptionalKeyStreamNum { get; private set; }

        public QueryGraphValueEntryHashKeyed HashKey { get; private set; }

        public Type CoercionType { get; private set; }
    }
}
