///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;

namespace com.espertech.esper.common.@internal.epl.join.exec.composite
{
    //using CompositeDictionary = IDictionary<object, object>;
    using CompositeDictionary = IDictionary<object, CompositeIndexEntry>;

    public interface CompositeIndexEnterRemove
    {
        void Enter(
            EventBean theEvent,
            IDictionary<object, CompositeIndexEntry> parent);

        CompositeIndexEnterRemove Next { set; }

        void Remove(
            EventBean theEvent,
            IDictionary<object, CompositeIndexEntry> parent);

        void GetAll(
            ISet<EventBean> result,
            IDictionary<object, CompositeIndexEntry> parent);
    }
} // end of namespace