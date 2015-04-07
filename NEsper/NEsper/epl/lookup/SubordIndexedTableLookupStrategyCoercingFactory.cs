///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.epl.join.table;
using com.espertech.esper.epl.virtualdw;

namespace com.espertech.esper.epl.lookup
{
    /// <summary>
    /// MapIndex lookup strategy that coerces the key values before performing a lookup.
    /// </summary>
    public class SubordIndexedTableLookupStrategyCoercingFactory : SubordIndexedTableLookupStrategyExprFactory
    {
        private readonly Type[] _coercionTypes;
    
        public SubordIndexedTableLookupStrategyCoercingFactory(bool isNWOnTrigger, int numStreamsOuter, IList<SubordPropHashKey> hashKeys, Type[] coercionTypes)
                    : base(isNWOnTrigger, numStreamsOuter, hashKeys)
        {
            _coercionTypes = coercionTypes;
        }
    
        public override SubordTableLookupStrategy MakeStrategy(EventTable[] eventTable, VirtualDWView vdw)
        {
            if (IsNWOnTrigger) {
                return new SubordIndexedTableLookupStrategyCoercingNW(Evaluators, (PropertyIndexedEventTable) eventTable[0], _coercionTypes, StrategyDesc);
            }
            else {
                return new SubordIndexedTableLookupStrategyCoercing(NumStreamsOuter, Evaluators, (PropertyIndexedEventTable)eventTable[0], _coercionTypes, StrategyDesc);
            }
        }
    
    }
}
