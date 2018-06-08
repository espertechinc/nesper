///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.epl.index.service;
using com.espertech.esper.epl.join.table;

namespace com.espertech.esper.epl.lookup
{
	public class EventTableIndexServiceImpl : EventTableIndexService {
	    public bool AllowInitIndex(bool isRecoveringResilient) {
	        return true;
	    }

	    public EventTableFactory CreateSingleCoerceAll(int indexedStreamNum, EventType eventType, string indexProp, Type indexCoercionType, object optionalSerde, bool isFireAndForget) {
	        return new PropertyIndexedEventTableSingleCoerceAllFactory(indexedStreamNum, eventType, indexProp, indexCoercionType);
	    }

	    public EventTableFactory CreateSingleCoerceAdd(int indexedStreamNum, EventType eventType, string indexProp, Type indexCoercionType, object optionalSerde, bool isFireAndForget) {
	        return new PropertyIndexedEventTableSingleCoerceAddFactory(indexedStreamNum, eventType, indexProp, indexCoercionType);
	    }

	    public EventTableFactory CreateSingle(int indexedStreamNum, EventType eventType, string propertyName, bool unique, string optionalIndexName, object optionalSerde, bool isFireAndForget) {
	        return new PropertyIndexedEventTableSingleFactory(indexedStreamNum, eventType, propertyName, unique, optionalIndexName);
	    }

	    public EventTableFactory CreateUnindexed(int indexedStreamNum, object optionalSerde, bool isFireAndForget) {
	        return new UnindexedEventTableFactory(indexedStreamNum);
	    }

	    public EventTableFactory CreateMultiKey(int indexedStreamNum, EventType eventType, IList<string> indexProps, bool unique, string optionalIndexName, object optionalSerde, bool isFireAndForget) {
	        return new PropertyIndexedEventTableFactory(indexedStreamNum, eventType, indexProps, unique, optionalIndexName);
	    }

	    public EventTableFactory CreateMultiKeyCoerceAdd(int indexedStreamNum, EventType eventType, IList<string> indexProps, IList<Type> indexCoercionTypes, bool isFireAndForget) {
	        return new PropertyIndexedEventTableCoerceAddFactory(indexedStreamNum, eventType, indexProps, indexCoercionTypes);
	    }

	    public EventTableFactory CreateMultiKeyCoerceAll(int indexedStreamNum, EventType eventType, IList<string> indexProps, IList<Type> indexCoercionTypes, bool isFireAndForget) {
	        return new PropertyIndexedEventTableCoerceAllFactory(indexedStreamNum, eventType, indexProps, indexCoercionTypes);
	    }

	    public EventTableFactory CreateComposite(int indexedStreamNum, EventType eventType, IList<string> indexedKeyProps, IList<Type> coercionKeyTypes, IList<string> indexedRangeProps, IList<Type> coercionRangeTypes, bool isFireAndForget) {
	        return new PropertyCompositeEventTableFactory(indexedStreamNum, eventType, indexedKeyProps, coercionKeyTypes, indexedRangeProps, coercionRangeTypes);
	    }

	    public EventTableFactory CreateSorted(int indexedStreamNum, EventType eventType, string indexedProp, bool isFireAndForget) {
	        return new PropertySortedEventTableFactory(indexedStreamNum, eventType, indexedProp);
	    }

	    public EventTableFactory CreateSortedCoerce(int indexedStreamNum, EventType eventType, string indexedProp, Type indexCoercionType, bool isFireAndForget) {
	        return new PropertySortedEventTableCoercedFactory(indexedStreamNum, eventType, indexedProp, indexCoercionType);
	    }

	    public EventTableFactory CreateInArray(int indexedStreamNum, EventType eventType, string[] indexedProp, bool unique) {
	        return new PropertyIndexedEventTableSingleArrayFactory(0, eventType, indexedProp, unique, null);
	    }

        public EventTableFactory CreateCustom(string indexName, int indexedStreamNum, EventType eventType, bool unique, EventAdvancedIndexProvisionDesc advancedIndexProvisionDesc) {
            return new EventTableFactoryCustomIndex(indexName, indexedStreamNum, eventType, unique, advancedIndexProvisionDesc);
        }
    }
} // end of namespace
