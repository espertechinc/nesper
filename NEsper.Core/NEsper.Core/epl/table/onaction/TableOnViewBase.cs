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
using com.espertech.esper.compat.logging;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.lookup;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.util;
using com.espertech.esper.view;

namespace com.espertech.esper.epl.table.onaction
{
    public abstract class TableOnViewBase : ViewSupport
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        protected readonly SubordWMatchExprLookupStrategy LookupStrategy;
        protected readonly TableStateInstance TableStateInstance;
        protected readonly ExprEvaluatorContext exprEvaluatorContext;
        protected readonly TableMetadata Metadata;
        protected readonly bool AcquireWriteLock;

        protected TableOnViewBase(SubordWMatchExprLookupStrategy lookupStrategy, TableStateInstance tableStateInstance, ExprEvaluatorContext exprEvaluatorContext, TableMetadata metadata, bool acquireWriteLock)
        {
            this.LookupStrategy = lookupStrategy;
            this.TableStateInstance = tableStateInstance;
            this.exprEvaluatorContext = exprEvaluatorContext;
            this.Metadata = metadata;
            this.AcquireWriteLock = acquireWriteLock;
        }

        public abstract void HandleMatching(EventBean[] triggerEvents, EventBean[] matchingEvents);

        public void Stop()
        {
            Log.Debug(".stop");
        }

        public override void Update(EventBean[] newData, EventBean[] oldData)
        {
            if (newData == null)
            {
                return;
            }

            if (AcquireWriteLock)
            {
                using (TableStateInstance.TableLevelRWLock.WriteLock.Acquire())
                {
                    EventBean[] eventsFound = LookupStrategy.Lookup(newData, exprEvaluatorContext);
                    HandleMatching(newData, eventsFound);
                }
            }
            else
            {
                using (TableStateInstance.TableLevelRWLock.ReadLock.Acquire())
                {
                    EventBean[] eventsFound = LookupStrategy.Lookup(newData, exprEvaluatorContext);
                    HandleMatching(newData, eventsFound);
                }
            }
        }

        /// <summary>
        /// returns expr context.
        /// </summary>
        /// <value>context</value>
        public ExprEvaluatorContext ExprEvaluatorContext
        {
            get { return exprEvaluatorContext; }
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            return CollectionUtil.NULL_EVENT_ITERATOR;
        }

        public override EventType EventType
        {
            get { return Metadata.PublicEventType; }
        }
    }
}
