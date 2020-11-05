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
using com.espertech.esper.common.@internal.epl.namedwindow.core;
using com.espertech.esper.common.@internal.view.core;

namespace com.espertech.esper.common.@internal.epl.fafquery.processor
{
    public class FireAndForgetInstanceNamedWindow : FireAndForgetInstance
    {
        public FireAndForgetInstanceNamedWindow(NamedWindowInstance processorInstance)
        {
            ProcessorInstance = processorInstance;
        }

        public NamedWindowInstance ProcessorInstance { get; }

        public override AgentInstanceContext AgentInstanceContext =>
            ProcessorInstance.TailViewInstance.AgentInstanceContext;

        public override Viewable TailViewInstance => ProcessorInstance.TailViewInstance;

        public override ICollection<EventBean> SnapshotBestEffort(
            QueryGraph queryGraph,
            Attribute[] annotations)
        {
            return ProcessorInstance.TailViewInstance.Snapshot(queryGraph, annotations);
        }

        public override EventBean[] ProcessInsert(FAFQueryMethodIUDInsertInto insert)
        {
            AgentInstanceContext ctx = ProcessorInstance.TailViewInstance.AgentInstanceContext;
            try {
                var @event = insert.InsertHelper.Process(new EventBean[0], true, true, ctx);
                EventBean[] inserted = {@event};

                using (ctx.AgentInstanceLock.AcquireWriteLock()) {
                    try {
                        ProcessorInstance.RootViewInstance.Update(inserted, null);
                    }
                    catch (EPException) {
                        ProcessorInstance.RootViewInstance.Update(null, inserted);
                    }
                }

                return inserted;
            }
            finally {
                ctx.TableExprEvaluatorContext.ReleaseAcquiredLocks();
            }
        }

        public override EventBean[] ProcessDelete(FAFQueryMethodIUDDelete delete)
        {
            return ProcessorInstance.TailViewInstance.SnapshotDelete(
                delete.QueryGraph,
                delete.OptionalWhereClause,
                delete.Annotations);
        }

        public override EventBean[] ProcessUpdate(FAFQueryMethodIUDUpdate update)
        {
            return ProcessorInstance.TailViewInstance.SnapshotUpdate(
                update.QueryGraph,
                update.OptionalWhereClause,
                update.UpdateHelperNamedWindow,
                update.Annotations);
        }
    }
} // end of namespace