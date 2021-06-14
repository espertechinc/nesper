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
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.fafquery.querymethod;
using com.espertech.esper.common.@internal.epl.join.querygraph;
using com.espertech.esper.common.@internal.view.core;

namespace com.espertech.esper.common.@internal.epl.fafquery.processor
{
    public abstract class FireAndForgetInstance
    {
        public abstract EventBean[] ProcessInsert(FAFQueryMethodIUDInsertInto insert);

        public abstract EventBean[] ProcessDelete(FAFQueryMethodIUDDelete delete);

        public abstract EventBean[] ProcessUpdate(FAFQueryMethodIUDUpdate update);

        public abstract ICollection<EventBean> SnapshotBestEffort(
            QueryGraph queryGraph,
            Attribute[] annotations);

        public abstract AgentInstanceContext AgentInstanceContext { get; }

        public abstract Viewable TailViewInstance { get; }

        //public abstract VirtualDWView getVirtualDataWindow();
    }
} // end of namespace