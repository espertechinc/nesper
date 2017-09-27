///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.metric;
using com.espertech.esper.epl.spec;
using com.espertech.esper.epl.table.merge;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.epl.table.upd;
using com.espertech.esper.epl.updatehelper;
using com.espertech.esper.events;

namespace com.espertech.esper.epl.table.onaction
{
    /// <summary>
    /// View for the on-delete statement that handles removing events from a named window.
    /// </summary>
    public class TableOnViewFactoryFactory {
        public static TableOnViewFactory Make(TableMetadata tableMetadata,
                                              OnTriggerDesc onTriggerDesc,
                                              EventType filterEventType,
                                              string filterStreamName,
                                              StatementContext statementContext,
                                              StatementMetricHandle metricsHandle,
                                              bool isDistinct,
                                              InternalEventRouter internalEventRouter
        )
                {
            if (onTriggerDesc.OnTriggerType == OnTriggerType.ON_DELETE) {
                return new TableOnDeleteViewFactory(statementContext.StatementResultService, tableMetadata);
            } else if (onTriggerDesc.OnTriggerType == OnTriggerType.ON_SELECT) {
                EventBeanReader eventBeanReader = null;
                if (isDistinct) {
                    eventBeanReader = tableMetadata.InternalEventType.Reader;
                }
                OnTriggerWindowDesc windowDesc = (OnTriggerWindowDesc) onTriggerDesc;
                return new TableOnSelectViewFactory(tableMetadata, internalEventRouter, statementContext.EpStatementHandle,
                        eventBeanReader, isDistinct, statementContext.StatementResultService, statementContext.InternalEventEngineRouteDest, windowDesc.IsDeleteAndSelect);
            } else if (onTriggerDesc.OnTriggerType == OnTriggerType.ON_UPDATE) {
                OnTriggerWindowUpdateDesc updateDesc = (OnTriggerWindowUpdateDesc) onTriggerDesc;
                EventBeanUpdateHelper updateHelper = EventBeanUpdateHelperFactory.Make(tableMetadata.TableName, (EventTypeSPI) tableMetadata.InternalEventType, updateDesc.Assignments, updateDesc.OptionalAsName, filterEventType, false, statementContext.StatementName, statementContext.EngineURI, statementContext.EventAdapterService);
                TableUpdateStrategy updateStrategy = statementContext.TableService.GetTableUpdateStrategy(tableMetadata, updateHelper, false);
                var onUpdateViewFactory = new TableOnUpdateViewFactory(statementContext.StatementResultService, tableMetadata, updateHelper, updateStrategy);
                statementContext.TableService.AddTableUpdateStrategyReceiver(tableMetadata, statementContext.StatementName, onUpdateViewFactory, updateHelper, false);
                return onUpdateViewFactory;
            } else if (onTriggerDesc.OnTriggerType == OnTriggerType.ON_MERGE) {
                OnTriggerMergeDesc onMergeTriggerDesc = (OnTriggerMergeDesc) onTriggerDesc;
                var onMergeHelper = new TableOnMergeHelper(statementContext, onMergeTriggerDesc, filterEventType, filterStreamName, internalEventRouter, tableMetadata);
                return new TableOnMergeViewFactory(tableMetadata, onMergeHelper, statementContext.StatementResultService, metricsHandle, statementContext.MetricReportingService);
            } else {
                throw new IllegalStateException("Unknown trigger type " + onTriggerDesc.OnTriggerType);
            }
        }
    }
} // end of namespace
