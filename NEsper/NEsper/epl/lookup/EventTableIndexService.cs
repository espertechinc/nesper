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
    public interface EventTableIndexService
    {
	    bool AllowInitIndex(bool isRecoveringResilient);
	    EventTableFactory CreateUnindexed(int indexedStreamNum, object optionalSerde, bool isFireAndForget);
	    EventTableFactory CreateSingle(int indexedStreamNum, EventType eventType, string indexProp, bool unique, string optionalIndexName, object optionalSerde, bool isFireAndForget);
	    EventTableFactory CreateSingleCoerceAdd(int indexedStreamNum, EventType eventType, string indexProp, Type indexCoercionType, object optionalSerde, bool isFireAndForget);
	    EventTableFactory CreateSingleCoerceAll(int indexedStreamNum, EventType eventType, string indexProp, Type indexCoercionType, object optionalSerde, bool isFireAndForget);
	    EventTableFactory CreateMultiKey(int indexedStreamNum, EventType eventType, IList<string> indexProps, bool unique, string optionalIndexName, object optionalSerde, bool isFireAndForget);
	    EventTableFactory CreateMultiKeyCoerceAdd(int indexedStreamNum, EventType eventType, IList<string> indexProps, IList<Type> indexCoercionTypes, bool isFireAndForget);
	    EventTableFactory CreateMultiKeyCoerceAll(int indexedStreamNum, EventType eventType, IList<string> indexProps, IList<Type> indexCoercionTypes, bool isFireAndForget);
	    EventTableFactory CreateComposite(int indexedStreamNum, EventType eventType, IList<string> indexedKeyProps, IList<Type> coercionKeyTypes, IList<string> indexedRangeProps, IList<Type> coercionRangeTypes, bool isFireAndForget);
	    EventTableFactory CreateSorted(int indexedStreamNum, EventType eventType, string indexedProp, bool isFireAndForget);
	    EventTableFactory CreateSortedCoerce(int indexedStreamNum, EventType eventType, string indexedProp, Type indexCoercionType, bool isFireAndForget);
	    EventTableFactory CreateInArray(int indexedStreamNum, EventType eventType, string[] indexedProp, bool unique);
        EventTableFactory CreateCustom(string indexName, int indexedStreamNum, EventType eventType, bool unique, EventAdvancedIndexProvisionDesc advancedIndexProvisionDesc);
    }
} // end of namespace
