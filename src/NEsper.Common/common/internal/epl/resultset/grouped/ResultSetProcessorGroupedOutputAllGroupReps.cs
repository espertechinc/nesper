///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.resultset.core;

namespace com.espertech.esper.common.@internal.epl.resultset.grouped
{
    public interface ResultSetProcessorGroupedOutputAllGroupReps : ResultSetProcessorOutputHelper
    {
        object Put(
            object mk,
            EventBean[] array);

        void Remove(object key);

        IEnumerator<KeyValuePair<object, EventBean[]>> EntryEnumerator();

        void Destroy();
    }
} // end of namespace