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
using com.espertech.esper.common.@internal.epl.resultset.core;

namespace com.espertech.esper.common.@internal.epl.resultset.simple
{
    public interface ResultSetProcessorSimpleOutputLastHelper : ResultSetProcessorOutputHelper
    {
        void ProcessView(
            EventBean[] newData,
            EventBean[] oldData);

        void ProcessJoin(
            ISet<MultiKey<EventBean>> newEvents,
            ISet<MultiKey<EventBean>> oldEvents);

        UniformPair<EventBean[]> OutputView(bool isSynthesize);

        UniformPair<EventBean[]> OutputJoin(bool isSynthesize);

        void Destroy();
    }
} // end of namespace