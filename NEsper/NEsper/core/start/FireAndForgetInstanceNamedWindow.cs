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
using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.join.plan;
using com.espertech.esper.epl.named;
using com.espertech.esper.epl.virtualdw;
using com.espertech.esper.filter;
using com.espertech.esper.view;

namespace com.espertech.esper.core.start
{
    public class FireAndForgetInstanceNamedWindow : FireAndForgetInstance
    {
        private readonly NamedWindowProcessorInstance _processorInstance;
    
        public FireAndForgetInstanceNamedWindow(NamedWindowProcessorInstance processorInstance)
        {
            this._processorInstance = processorInstance;
        }

        public NamedWindowProcessorInstance ProcessorInstance
        {
            get { return _processorInstance; }
        }

        public override EventBean[] ProcessInsert(EPPreparedExecuteIUDSingleStreamExecInsert insert)
        {
            EPPreparedExecuteTableHelper.AssignTableAccessStrategies(insert.Services, insert.OptionalTableNodes, _processorInstance.TailViewInstance.AgentInstanceContext);
            try {
                var @event = insert.InsertHelper.Process(new EventBean[0], true, true, insert.ExprEvaluatorContext);
                var inserted = new EventBean[] {@event};
    
                var ctx = _processorInstance.TailViewInstance.AgentInstanceContext;
                var ailock = ctx.AgentInstanceLock;
                using (ailock.AcquireWriteLock())
                {
                    try
                    {
                        _processorInstance.RootViewInstance.Update(inserted, null);
                    }
                    catch (EPException)
                    {
                        _processorInstance.RootViewInstance.Update(null, inserted);
                    }
                }

                return inserted;
            }
            finally {
                insert.Services.TableService.TableExprEvaluatorContext.ReleaseAcquiredLocks();
            }
        }
    
        public override EventBean[] ProcessDelete(EPPreparedExecuteIUDSingleStreamExecDelete delete) {
            EPPreparedExecuteTableHelper.AssignTableAccessStrategies(delete.Services, delete.OptionalTableNodes, _processorInstance.TailViewInstance.AgentInstanceContext);
            return _processorInstance.TailViewInstance.SnapshotDelete(delete.QueryGraph, delete.OptionalWhereClause, delete.Annotations);
        }
    
        public override EventBean[] ProcessUpdate(EPPreparedExecuteIUDSingleStreamExecUpdate update) {
            EPPreparedExecuteTableHelper.AssignTableAccessStrategies(update.Services, update.OptionalTableNodes, _processorInstance.TailViewInstance.AgentInstanceContext);
            return _processorInstance.TailViewInstance.SnapshotUpdate(update.QueryGraph, update.OptionalWhereClause, update.UpdateHelper, update.Annotations);
        }
    
        public override ICollection<EventBean> SnapshotBestEffort(EPPreparedExecuteMethodQuery query, QueryGraph queryGraph, Attribute[] annotations) {
            EPPreparedExecuteTableHelper.AssignTableAccessStrategies(query.Services, query.TableNodes, _processorInstance.TailViewInstance.AgentInstanceContext);
            return _processorInstance.TailViewInstance.Snapshot(queryGraph, annotations);
        }

        public override AgentInstanceContext AgentInstanceContext
        {
            get { return _processorInstance.TailViewInstance.AgentInstanceContext; }
        }

        public override Viewable TailViewInstance
        {
            get { return _processorInstance.TailViewInstance; }
        }

        public override VirtualDWView VirtualDataWindow
        {
            get { return _processorInstance.RootViewInstance.VirtualDataWindow; }
        }
    }
}
