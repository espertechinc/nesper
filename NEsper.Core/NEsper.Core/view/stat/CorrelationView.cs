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
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.events;

namespace com.espertech.esper.view.stat
{
    /// <summary>
    /// A view that calculates correlation on two fields. The view uses
    /// internally a <seealso cref="BaseStatisticsBean"/> instance for the calculations, 
    /// it also returns this bean as the result. This class accepts most of its behaviour 
    /// from its parent, <seealso cref="com.espertech.esper.view.stat.BaseBivariateStatisticsView"/>. 
    /// It adds the usage of the correlation bean and the appropriate schema.
    /// </summary>
    public sealed class CorrelationView
        : BaseBivariateStatisticsView
        , CloneableView
    {
        /// <summary>Constructor. </summary>
        /// <param name="viewFactory"></param>
        /// <param name="agentInstanceContext">contains required view services</param>
        /// <param name="xExpression">is the expression providing X data points</param>
        /// <param name="yExpression">is the expression providing X data points</param>
        /// <param name="eventType">event type</param>
        /// <param name="additionalProps">additional properties</param>
        public CorrelationView(ViewFactory viewFactory, AgentInstanceContext agentInstanceContext, ExprNode xExpression, ExprNode yExpression, EventType eventType, StatViewAdditionalProps additionalProps)
            : base(viewFactory, agentInstanceContext, xExpression, yExpression, eventType, additionalProps)
        {
        }

        public View CloneView()
        {
            return new CorrelationView(ViewFactory, AgentInstanceContext, ExpressionX, ExpressionY, ((ViewSupport)this).EventType, AdditionalProps);
        }

        protected override EventBean PopulateMap(BaseStatisticsBean baseStatisticsBean,
                                                 EventAdapterService eventAdapterService,
                                                 EventType eventType,
                                                 StatViewAdditionalProps additionalProps,
                                                 Object[] decoration)
        {
            return DoPopulateMap(baseStatisticsBean, eventAdapterService, eventType, additionalProps, decoration);
        }

        /// <summary>Populate bean. </summary>
        /// <param name="baseStatisticsBean">results</param>
        /// <param name="eventAdapterService">event wrapping</param>
        /// <param name="eventType">type to produce</param>
        /// <param name="additionalProps">addition properties</param>
        /// <param name="decoration">decoration values</param>
        /// <returns>bean</returns>
        public static EventBean DoPopulateMap(BaseStatisticsBean baseStatisticsBean,
                                             EventAdapterService eventAdapterService,
                                             EventType eventType,
                                             StatViewAdditionalProps additionalProps,
                                             Object[] decoration)
        {
            IDictionary<String, Object> result = new Dictionary<String, Object>();
            result.Put(ViewFieldEnum.CORRELATION__CORRELATION.GetName(), baseStatisticsBean.Correlation);
            if (additionalProps != null)
            {
                additionalProps.AddProperties(result, decoration);
            }

            return eventAdapterService.AdapterForTypedMap(result, eventType);
        }

        public override EventType EventType
        {
            get { return _eventType; }
        }

        public override String ToString()
        {
            return GetType().FullName +
                    " fieldX=" + ExpressionX +
                    " fieldY=" + ExpressionY;
        }

        /// <summary>
        /// Creates the event type for this view.
        /// </summary>
        /// <param name="statementContext">is the event adapter service</param>
        /// <param name="additionalProps">additional props</param>
        /// <param name="streamNum">The stream num.</param>
        /// <returns>event type of view</returns>
        public static EventType CreateEventType(StatementContext statementContext, StatViewAdditionalProps additionalProps, int streamNum)
        {
            IDictionary<String, Object> eventTypeMap = new Dictionary<String, Object>();
            eventTypeMap.Put(ViewFieldEnum.CORRELATION__CORRELATION.GetName(), typeof(double?));
            StatViewAdditionalProps.AddCheckDupProperties(eventTypeMap, additionalProps,
                    ViewFieldEnum.CORRELATION__CORRELATION);
            String outputEventTypeName = statementContext.StatementId + "_correlview_" + streamNum;
            return statementContext.EventAdapterService.CreateAnonymousMapType(outputEventTypeName, eventTypeMap, false);
        }
    }
}
