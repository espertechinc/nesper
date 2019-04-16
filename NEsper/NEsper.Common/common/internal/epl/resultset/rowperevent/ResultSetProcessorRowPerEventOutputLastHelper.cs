///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.epl.resultset.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.resultset.rowperevent
{
    public interface ResultSetProcessorRowPerEventOutputLastHelper : ResultSetProcessorOutputHelper
    {
        void ProcessView(
            EventBean[] newData,
            EventBean[] oldData,
            bool isGenerateSynthetic);

        void ProcessJoin(
            ISet<MultiKey<EventBean>> newEvents,
            ISet<MultiKey<EventBean>> oldEvents,
            bool isGenerateSynthetic);

        UniformPair<EventBean[]> Output();

        void Destroy();
    }
} // end of namespace