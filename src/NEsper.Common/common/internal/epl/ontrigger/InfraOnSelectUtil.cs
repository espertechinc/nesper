///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.table.core;
using com.espertech.esper.common.@internal.@event.core;

namespace com.espertech.esper.common.@internal.epl.ontrigger
{
    public class InfraOnSelectUtil
    {
        public static EventBean[] HandleDistintAndInsert(
            EventBean[] newData,
            InfraOnSelectViewFactory parent,
            AgentInstanceContext agentInstanceContext,
            TableInstance tableInstanceInsertInto,
            bool audit,
            ExprEvaluator eventPrecedence)
        {
            if (parent.IsDistinct) {
                newData = EventBeanUtility.GetDistinctByProp(newData, parent.DistinctKeyGetter);
            }

            if (tableInstanceInsertInto != null) {
                if (newData != null) {
                    foreach (var aNewData in newData) {
                        tableInstanceInsertInto.AddEventUnadorned(aNewData);
                    }
                }
            }
            else if (parent.IsInsertInto) {
                if (newData != null) {
                    foreach (var aNewData in newData) {
                        if (audit) {
                            agentInstanceContext.AuditProvider.Insert(aNewData, agentInstanceContext);
                        }

                        // Evaluate event precedence
                        var precedence = ExprNodeUtilityEvaluate.EvaluateIntOptional(
                            eventPrecedence,
                            aNewData,
                            0,
                            agentInstanceContext);

                        agentInstanceContext.InternalEventRouter.Route(
                            aNewData,
                            agentInstanceContext,
                            parent.IsAddToFront,
                            precedence);
                    }
                }
            }

            return newData;
        }
    }
} // end of namespace