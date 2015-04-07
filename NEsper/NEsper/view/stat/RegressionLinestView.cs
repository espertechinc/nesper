///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
    /// A view that calculates regression on two fields. The view uses internally a <seealso cref="BaseStatisticsBean" /> 
    /// instance for the calculations, it also returns this bean as the result. This class accepts most of its 
    /// behaviour from its parent, <seealso cref="com.espertech.esper.view.stat.BaseBivariateStatisticsView" />. 
    /// It adds the usage of the regression bean and the appropriate schema. 
    /// </summary>
    public sealed class RegressionLinestView : BaseBivariateStatisticsView, CloneableView
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="viewFactory"></param>
        /// <param name="agentInstanceContext">contains required view services</param>
        /// <param name="xFieldName">is the field name of the field providing X data points</param>
        /// <param name="yFieldName">is the field name of the field providing X data points</param>
        /// <param name="eventType">Type of the event.</param>
        /// <param name="additionalProps">The additional props.</param>
        public RegressionLinestView(ViewFactory viewFactory, AgentInstanceContext agentInstanceContext, ExprNode xFieldName, ExprNode yFieldName, EventType eventType, StatViewAdditionalProps additionalProps)
            : base(viewFactory, agentInstanceContext, xFieldName, yFieldName, eventType, additionalProps)
        {
        }

        public View CloneView()
        {
            return new RegressionLinestView(ViewFactory, AgentInstanceContext, ExpressionX, ExpressionY, EventType, AdditionalProps);
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

        protected override EventBean PopulateMap(BaseStatisticsBean baseStatisticsBean,
                                             EventAdapterService eventAdapterService,
                                             EventType eventType,
                                             StatViewAdditionalProps additionalProps,
                                             Object[] decoration)
        {
            return DoPopulateMap(baseStatisticsBean, eventAdapterService, eventType, additionalProps, decoration);
        }

        public static EventBean DoPopulateMap(BaseStatisticsBean baseStatisticsBean,
                                             EventAdapterService eventAdapterService,
                                             EventType eventType,
                                             StatViewAdditionalProps additionalProps,
                                             Object[] decoration)
        {
            IDictionary<String, Object> result = new Dictionary<String, Object>();
            result.Put(ViewFieldEnum.REGRESSION__SLOPE.GetName(), baseStatisticsBean.Slope);
            result.Put(ViewFieldEnum.REGRESSION__YINTERCEPT.GetName(), baseStatisticsBean.YIntercept);
            result.Put(ViewFieldEnum.REGRESSION__XAVERAGE.GetName(), baseStatisticsBean.XAverage);
            result.Put(ViewFieldEnum.REGRESSION__XSTANDARDDEVIATIONPOP.GetName(), baseStatisticsBean.XStandardDeviationPop);
            result.Put(ViewFieldEnum.REGRESSION__XSTANDARDDEVIATIONSAMPLE.GetName(), baseStatisticsBean.XStandardDeviationSample);
            result.Put(ViewFieldEnum.REGRESSION__XSUM.GetName(), baseStatisticsBean.XSum);
            result.Put(ViewFieldEnum.REGRESSION__XVARIANCE.GetName(), baseStatisticsBean.XVariance);
            result.Put(ViewFieldEnum.REGRESSION__YAVERAGE.GetName(), baseStatisticsBean.YAverage);
            result.Put(ViewFieldEnum.REGRESSION__YSTANDARDDEVIATIONPOP.GetName(), baseStatisticsBean.YStandardDeviationPop);
            result.Put(ViewFieldEnum.REGRESSION__YSTANDARDDEVIATIONSAMPLE.GetName(), baseStatisticsBean.YStandardDeviationSample);
            result.Put(ViewFieldEnum.REGRESSION__YSUM.GetName(), baseStatisticsBean.YSum);
            result.Put(ViewFieldEnum.REGRESSION__YVARIANCE.GetName(), baseStatisticsBean.YVariance);
            result.Put(ViewFieldEnum.REGRESSION__DATAPOINTS.GetName(), baseStatisticsBean.DataPoints);
            result.Put(ViewFieldEnum.REGRESSION__N.GetName(), baseStatisticsBean.N);
            result.Put(ViewFieldEnum.REGRESSION__SUMX.GetName(), baseStatisticsBean.SumX);
            result.Put(ViewFieldEnum.REGRESSION__SUMXSQ.GetName(), baseStatisticsBean.SumXSq);
            result.Put(ViewFieldEnum.REGRESSION__SUMXY.GetName(), baseStatisticsBean.SumXY);
            result.Put(ViewFieldEnum.REGRESSION__SUMY.GetName(), baseStatisticsBean.SumY);
            result.Put(ViewFieldEnum.REGRESSION__SUMYSQ.GetName(), baseStatisticsBean.SumYSq);
            if (additionalProps != null)
            {
                additionalProps.AddProperties(result, decoration);
            }
            return eventAdapterService.AdapterForTypedMap(result, eventType);
        }

        /// <summary>
        /// Creates the event type for this view.
        /// </summary>
        /// <param name="statementContext">is the event adapter service</param>
        /// <param name="additionalProps">The additional props.</param>
        /// <param name="streamNum">The stream num.</param>
        /// <returns>event type of view</returns>
        internal static EventType CreateEventType(StatementContext statementContext, StatViewAdditionalProps additionalProps, int streamNum)
        {
            IDictionary<String, Object> eventTypeMap = new Dictionary<String, Object>();
            eventTypeMap.Put(ViewFieldEnum.REGRESSION__SLOPE.GetName(), typeof(double?));
            eventTypeMap.Put(ViewFieldEnum.REGRESSION__YINTERCEPT.GetName(), typeof(double?));
            eventTypeMap.Put(ViewFieldEnum.REGRESSION__XAVERAGE.GetName(), typeof(double?));
            eventTypeMap.Put(ViewFieldEnum.REGRESSION__XSTANDARDDEVIATIONPOP.GetName(), typeof(double?));
            eventTypeMap.Put(ViewFieldEnum.REGRESSION__XSTANDARDDEVIATIONSAMPLE.GetName(), typeof(double?));
            eventTypeMap.Put(ViewFieldEnum.REGRESSION__XSUM.GetName(), typeof(double?));
            eventTypeMap.Put(ViewFieldEnum.REGRESSION__XVARIANCE.GetName(), typeof(double?));
            eventTypeMap.Put(ViewFieldEnum.REGRESSION__YAVERAGE.GetName(), typeof(double?));
            eventTypeMap.Put(ViewFieldEnum.REGRESSION__YSTANDARDDEVIATIONPOP.GetName(), typeof(double?));
            eventTypeMap.Put(ViewFieldEnum.REGRESSION__YSTANDARDDEVIATIONSAMPLE.GetName(), typeof(double?));
            eventTypeMap.Put(ViewFieldEnum.REGRESSION__YSUM.GetName(), typeof(double?));
            eventTypeMap.Put(ViewFieldEnum.REGRESSION__YVARIANCE.GetName(), typeof(double?));
            eventTypeMap.Put(ViewFieldEnum.REGRESSION__DATAPOINTS.GetName(), typeof(long?));
            eventTypeMap.Put(ViewFieldEnum.REGRESSION__N.GetName(), typeof(long?));
            eventTypeMap.Put(ViewFieldEnum.REGRESSION__SUMX.GetName(), typeof(double?));
            eventTypeMap.Put(ViewFieldEnum.REGRESSION__SUMXSQ.GetName(), typeof(double?));
            eventTypeMap.Put(ViewFieldEnum.REGRESSION__SUMXY.GetName(), typeof(double?));
            eventTypeMap.Put(ViewFieldEnum.REGRESSION__SUMY.GetName(), typeof(double?));
            eventTypeMap.Put(ViewFieldEnum.REGRESSION__SUMYSQ.GetName(), typeof(double?));
            StatViewAdditionalProps.AddCheckDupProperties(eventTypeMap, additionalProps,
                    ViewFieldEnum.REGRESSION__SLOPE, ViewFieldEnum.REGRESSION__YINTERCEPT);
            String outputEventTypeName = statementContext.StatementId + "_regview_" + streamNum;
            return statementContext.EventAdapterService.CreateAnonymousMapType(outputEventTypeName, eventTypeMap);
        }
    }
}
