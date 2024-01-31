///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.epl.resultset.core;

namespace com.espertech.esper.common.@internal.epl.resultset.rowpergroup
{
    public interface ResultSetProcessorRowPerGroupOutputAllHelper : ResultSetProcessorOutputHelper
    {
        void ProcessView(
            EventBean[] newData,
            EventBean[] oldData,
            bool isGenerateSynthetic);

        void ProcessJoin(
            ISet<MultiKeyArrayOfKeys<EventBean>> newData,
            ISet<MultiKeyArrayOfKeys<EventBean>> oldData,
            bool isGenerateSynthetic);

        UniformPair<EventBean[]> OutputView(bool isSynthesize);

        UniformPair<EventBean[]> OutputJoin(bool isSynthesize);

        void Destroy();
    }
} // end of namespace