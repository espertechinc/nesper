///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.table.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.ontrigger
{
	public class InfraOnSelectUtil {
	    public static EventBean[] HandleDistintAndInsert(EventBean[] newData, InfraOnSelectViewFactory parent, AgentInstanceContext agentInstanceContext, TableInstance tableInstanceInsertInto, bool audit) {
	        if (parent.IsDistinct) {
	            newData = EventBeanUtility.GetDistinctByProp(newData, parent.EventBeanReader);
	        }

	        if (tableInstanceInsertInto != null) {
	            if (newData != null) {
	                foreach (EventBean aNewData in newData) {
	                    tableInstanceInsertInto.AddEventUnadorned(aNewData);
	                }
	            }
	        } else if (parent.IsInsertInto) {
	            if (newData != null) {
	                foreach (EventBean aNewData in newData) {
	                    if (audit) {
	                        agentInstanceContext.AuditProvider.Insert(aNewData, agentInstanceContext);
	                    }
	                    agentInstanceContext.InternalEventRouter.Route(aNewData, agentInstanceContext, parent.IsAddToFront);
	                }
	            }
	        }

	        return newData;
	    }
	}
} // end of namespace