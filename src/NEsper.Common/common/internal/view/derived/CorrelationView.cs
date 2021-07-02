///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.view.derived
{
    /// <summary>
    ///     A view that calculates correlation on two fields. The view uses internally a <seealso cref="BaseStatisticsBean" />
    ///     instance for the calculations, it also returns this bean as the result.
    ///     This class accepts most of its behaviour from its parent, <seealso cref="BaseBivariateStatisticsView" />. It adds
    ///     the usage of the correlation bean and the appropriate schema.
    /// </summary>
    public class CorrelationView : BaseBivariateStatisticsView
    {
        public CorrelationView(
            ViewFactory viewFactory,
            AgentInstanceContext agentInstanceContext,
            ExprEvaluator xExpressionEval,
            ExprEvaluator yExpressionEval,
            EventType eventType,
            StatViewAdditionalPropsEval additionalProps)
            : base(
                viewFactory,
                agentInstanceContext,
                xExpressionEval,
                yExpressionEval,
                eventType,
                additionalProps)
        {
        }

        public override EventType EventType => eventType;

        protected internal override EventBean PopulateMap(
            BaseStatisticsBean baseStatisticsBean,
            EventBeanTypedEventFactory eventAdapterService,
            EventType eventType,
            StatViewAdditionalPropsEval additionalProps,
            object[] decoration)
        {
            return DoPopulateMap(baseStatisticsBean, eventAdapterService, eventType, additionalProps, decoration);
        }

        /// <summary>
        ///     Populate bean.
        /// </summary>
        /// <param name="baseStatisticsBean">results</param>
        /// <param name="eventAdapterService">event wrapping</param>
        /// <param name="eventType">type to produce</param>
        /// <param name="additionalProps">addition properties</param>
        /// <param name="decoration">decoration values</param>
        /// <returns>bean</returns>
        public static EventBean DoPopulateMap(
            BaseStatisticsBean baseStatisticsBean,
            EventBeanTypedEventFactory eventAdapterService,
            EventType eventType,
            StatViewAdditionalPropsEval additionalProps,
            object[] decoration)
        {
            IDictionary<string, object> result = new Dictionary<string, object>();
            result.Put(ViewFieldEnum.CORRELATION_CORRELATION.GetName(), baseStatisticsBean.Correlation);
            additionalProps?.AddProperties(result, decoration);

            return eventAdapterService.AdapterForTypedMap(result, eventType);
        }

        protected internal static EventType CreateEventType(
            StatViewAdditionalPropsForge additionalProps,
            ViewForgeEnv viewForgeEnv,
            int streamNum)
        {
            var eventTypeMap = new LinkedHashMap<string, object>();
            eventTypeMap.Put(ViewFieldEnum.CORRELATION_CORRELATION.GetName(), typeof(double?));
            StatViewAdditionalPropsForge.AddCheckDupProperties(
                eventTypeMap,
                additionalProps,
                ViewFieldEnum.CORRELATION_CORRELATION);
            return DerivedViewTypeUtil.NewType("correlview", eventTypeMap, viewForgeEnv, streamNum);
        }
    }
} // end of namespace