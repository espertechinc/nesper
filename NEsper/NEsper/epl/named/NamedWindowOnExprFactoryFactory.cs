///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.client.soda;
using com.espertech.esper.compat;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.metric;
using com.espertech.esper.epl.spec;
using com.espertech.esper.epl.updatehelper;
using com.espertech.esper.events;

namespace com.espertech.esper.epl.named
{
    /// <summary>
    /// View for the on-delete statement that handles removing events from a named window.
    /// </summary>
    public class NamedWindowOnExprFactoryFactory
    {
        public static NamedWindowOnExprFactory Make(
            EventType namedWindowEventType,
            string namedWindowName,
            string namedWindowAlias,
            OnTriggerDesc onTriggerDesc,
            EventType filterEventType,
            string filterStreamName,
            bool addToFront,
            InternalEventRouter internalEventRouter,
            EventType outputEventType,
            StatementContext statementContext,
            StatementMetricHandle createNamedWindowMetricsHandle,
            bool isDistinct,
            StreamSelector? optionalStreamSelector,
            string optionalInsertIntoTableName)
        {
            if (onTriggerDesc.OnTriggerType == OnTriggerType.ON_DELETE)
            {
                return new NamedWindowOnDeleteViewFactory(namedWindowEventType, statementContext.StatementResultService);
            }
            else if (onTriggerDesc.OnTriggerType == OnTriggerType.ON_SELECT)
            {
                EventBeanReader eventBeanReader = null;
                if (isDistinct)
                {
                    if (outputEventType is EventTypeSPI)
                    {
                        eventBeanReader = ((EventTypeSPI) outputEventType).Reader;
                    }
                    if (eventBeanReader == null)
                    {
                        eventBeanReader = new EventBeanReaderDefaultImpl(outputEventType);
                    }
                }
                var windowDesc = (OnTriggerWindowDesc) onTriggerDesc;
                return new NamedWindowOnSelectViewFactory(
                    namedWindowEventType, internalEventRouter, addToFront,
                    statementContext.EpStatementHandle, eventBeanReader, isDistinct,
                    statementContext.StatementResultService, statementContext.InternalEventEngineRouteDest,
                    windowDesc.IsDeleteAndSelect, optionalStreamSelector, optionalInsertIntoTableName);
            }
            else if (onTriggerDesc.OnTriggerType == OnTriggerType.ON_UPDATE)
            {
                var updateDesc = (OnTriggerWindowUpdateDesc) onTriggerDesc;
                var updateHelper = EventBeanUpdateHelperFactory.Make(
                    namedWindowName, (EventTypeSPI) namedWindowEventType, updateDesc.Assignments, namedWindowAlias,
                    filterEventType, true, statementContext.StatementName, statementContext.EngineURI,
                    statementContext.EventAdapterService);
                return new NamedWindowOnUpdateViewFactory(
                    namedWindowEventType, statementContext.StatementResultService, updateHelper);
            }
            else if (onTriggerDesc.OnTriggerType == OnTriggerType.ON_MERGE)
            {
                var onMergeTriggerDesc = (OnTriggerMergeDesc) onTriggerDesc;
                var onMergeHelper = new NamedWindowOnMergeHelper(
                    statementContext, onMergeTriggerDesc, filterEventType, filterStreamName, internalEventRouter,
                    namedWindowName, (EventTypeSPI) namedWindowEventType);
                return new NamedWindowOnMergeViewFactory(
                    namedWindowEventType, onMergeHelper, statementContext.StatementResultService,
                    createNamedWindowMetricsHandle, statementContext.MetricReportingService);
            }
            else
            {
                throw new IllegalStateException("Unknown trigger type " + onTriggerDesc.OnTriggerType);
            }
        }
    }
} // end of namespace
