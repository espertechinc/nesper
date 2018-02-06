///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.client;
using com.espertech.esper.compat.logging;
using com.espertech.esper.epl.metric;
using com.espertech.esper.events.vaevent;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.named
{
    /// <summary>
    /// The root window in a named window plays multiple roles: It holds the indexes for 
    /// deleting rows, if any on-delete statement requires such indexes. Such indexes are 
    /// updated when events arrive, or remove from when a data window or on-delete statement 
    /// expires events. The view keeps track of on-delete statements their indexes used.
    /// </summary>
    public class NamedWindowRootView
    {
        public static readonly ILog QueryPlanLogInstance = LogManager.GetLogger(AuditPath.QUERYPLAN_LOG);

        public static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public NamedWindowRootView(ValueAddEventProcessor revisionProcessor,
                                   bool queryPlanLogging,
                                   MetricReportingService metricReportingService,
                                   EventType eventType,
                                   bool childBatching,
                                   bool isEnableIndexShare,
                                   ICollection<string> optionalUniqueKeyProps)
        {
            RevisionProcessor = revisionProcessor;
            IsQueryPlanLogging = queryPlanLogging;
            EventType = eventType;
            IsChildBatching = childBatching;
            IsEnableIndexShare = isEnableIndexShare;
            OptionalUniqueKeyProps = optionalUniqueKeyProps;
        }

        public static ILog QueryPlanLog => QueryPlanLogInstance;

        public ValueAddEventProcessor RevisionProcessor { get; private set; }

        public bool IsChildBatching { get; private set; }

        public bool IsQueryPlanLogging { get; private set; }

        public EventType EventType { get; private set; }

        public bool IsEnableIndexShare { get; private set; }

        public ICollection<string> OptionalUniqueKeyProps { get; private set; }
    }
}
