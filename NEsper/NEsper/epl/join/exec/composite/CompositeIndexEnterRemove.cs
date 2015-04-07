///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;

namespace com.espertech.esper.epl.join.exec.composite
{
    using DataMap = IDictionary<string, object>;
    using AnyMap = IDictionary<object, object>;

    public interface CompositeIndexEnterRemove
    {
        void Enter(EventBean theEvent, AnyMap parent);
        void SetNext(CompositeIndexEnterRemove next);
        void Remove(EventBean theEvent, AnyMap parent);
        void GetAll(ICollection<EventBean> result, AnyMap parent);
    }
}