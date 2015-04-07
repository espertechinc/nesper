///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.epl.@join.table;
using com.espertech.esper.epl.virtualdw;

namespace com.espertech.esper.epl.lookup
{
    /// <summary>
    /// MapIndex lookup strategy that coerces the key values before performing a lookup.
    /// </summary>
    public class SubordIndexedTableLookupStrategySingleCoercingFactory : SubordIndexedTableLookupStrategySingleExprFactory
    {
        private readonly Type _coercionType;
    
        /// <summary>Ctor. </summary>
        public SubordIndexedTableLookupStrategySingleCoercingFactory(bool isNWOnTrigger, int streamCountOuter, SubordPropHashKey hashKey, Type coercionType) 
            : base(isNWOnTrigger, streamCountOuter, hashKey)
        {
            _coercionType = coercionType;
        }
    
        public override SubordTableLookupStrategy MakeStrategy(EventTable[] eventTable, VirtualDWView vdw) {
            if (IsNWOnTrigger) {
                return new SubordIndexedTableLookupStrategySingleCoercingNW(Evaluator, (PropertyIndexedEventTableSingle) eventTable[0], _coercionType, StrategyDesc);
            }
            else {
                return new SubordIndexedTableLookupStrategySingleCoercing(StreamCountOuter, Evaluator, (PropertyIndexedEventTableSingle)eventTable[0], _coercionType, StrategyDesc);
            }
        }
    }
}
