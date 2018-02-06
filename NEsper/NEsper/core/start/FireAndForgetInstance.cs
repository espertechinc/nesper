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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.join.plan;
using com.espertech.esper.epl.virtualdw;
using com.espertech.esper.filter;
using com.espertech.esper.view;

namespace com.espertech.esper.core.start
{
    public abstract class FireAndForgetInstance
    {
        public abstract EventBean[] ProcessInsert(EPPreparedExecuteIUDSingleStreamExecInsert insert);
        public abstract EventBean[] ProcessDelete(EPPreparedExecuteIUDSingleStreamExecDelete delete);
        public abstract EventBean[] ProcessUpdate(EPPreparedExecuteIUDSingleStreamExecUpdate update);
        public abstract ICollection<EventBean> SnapshotBestEffort(EPPreparedExecuteMethodQuery epPreparedExecuteMethodQuery, QueryGraph queryGraph, Attribute[] annotations);
        public abstract AgentInstanceContext AgentInstanceContext { get; }
        public abstract Viewable TailViewInstance { get; }
        public abstract VirtualDWView VirtualDataWindow { get; }
    }
}
