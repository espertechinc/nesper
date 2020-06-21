///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.collection;

namespace com.espertech.esper.common.@internal.epl.output.view
{
    public interface OutputProcessViewConditionDeltaSet
    {
        int NumChangesetRows { get; }

        IList<UniformPair<ISet<MultiKeyArrayOfKeys<EventBean>>>> JoinEventsSet { get; }

        IList<UniformPair<EventBean[]>> ViewEventsSet { get; }

        void AddView(UniformPair<EventBean[]> events);

        void AddJoin(UniformPair<ISet<MultiKeyArrayOfKeys<EventBean>>> events);

        void Clear();

        void Destroy();
    }
} // end of namespace