///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.approx.countminsketch;
using com.espertech.esper.common.@internal.serde.compiletime.resolve;

namespace com.espertech.esper.common.@internal.fabric
{
    public interface FabricTypeCollector
    {
        void Builtin(params Type[] types);
        void RefCountedSetOfDouble();
        void Serde(DataInputOutputSerdeForge serde);
        void SortedDoubleVector();
        void SortedRefCountedSet(DataInputOutputSerdeForge serde);
        void SerdeObjectArrayMayNullNull(DataInputOutputSerdeForge[] criteriaSerdes);
        void SerdeNullableEvent(EventType eventType);

        void TreeMapEventsMayDeque(
            DataInputOutputSerdeForge[] criteriaSerdes,
            EventType eventType);

        void RefCountedSetAtomicInteger(EventType eventType);
        void LinkedHashMapEventsAndInt(EventType eventType);
        void ListEvents(EventType eventType);
        void RefCountedSet(DataInputOutputSerdeForge serde);

        void AggregatorNth(
            short serdeVersion,
            int sizeOfBuf,
            DataInputOutputSerdeForge serde);

        void AggregatorRateEver(short serdeVersion);
        void BigDecimal();
        void BigInteger();
        void CountMinSketch(CountMinSketchSpecForge spec);
        void PlugInAggregation(Type serde);

        static void Collect(
            AggregationForgeFactory[] methodFactories,
            AggregationStateFactoryForge[] accessFactories,
            FabricTypeCollector collector)
        {
            // collect serde information
            if (methodFactories != null) {
                for (var i = 0; i < methodFactories.Length; i++) {
                    methodFactories[i].Aggregator.CollectFabricType(collector);
                }
            }

            if (accessFactories != null) {
                for (var i = 0; i < accessFactories.Length; i++) {
                    accessFactories[i].Aggregator.CollectFabricType(collector);
                }
            }
        }
    }
}