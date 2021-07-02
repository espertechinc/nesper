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
    /// A view that calculates regression on two fields. The view uses internally a <seealso cref="BaseStatisticsBean" />instance for the calculations, it also returns this bean as the result.
    /// This class accepts most of its behaviour from its parent, <seealso cref="BaseBivariateStatisticsView" />. It adds
    /// the usage of the regression bean and the appropriate schema.
    /// </summary>
    public class RegressionLinestView : BaseBivariateStatisticsView
    {
        public RegressionLinestView(
            ViewFactory viewFactory,
            AgentInstanceContext agentInstanceContext,
            ExprEvaluator xEval,
            ExprEvaluator yEval,
            EventType eventType,
            StatViewAdditionalPropsEval additionalProps)
            : base(viewFactory, agentInstanceContext, xEval, yEval, eventType, additionalProps)
        {
        }

        public override EventType EventType {
            get => eventType;
        }

        protected internal override EventBean PopulateMap(
            BaseStatisticsBean baseStatisticsBean,
            EventBeanTypedEventFactory eventAdapterService,
            EventType eventType,
            StatViewAdditionalPropsEval additionalProps,
            object[] decoration)
        {
            return DoPopulateMap(baseStatisticsBean, eventAdapterService, eventType, additionalProps, decoration);
        }

        public static EventBean DoPopulateMap(
            BaseStatisticsBean baseStatisticsBean,
            EventBeanTypedEventFactory eventAdapterService,
            EventType eventType,
            StatViewAdditionalPropsEval additionalProps,
            object[] decoration)
        {
            IDictionary<string, object> result = new Dictionary<string, object>();
            result.Put(ViewFieldEnum.REGRESSION_SLOPE.GetName(), baseStatisticsBean.Slope);
            result.Put(ViewFieldEnum.REGRESSION_YINTERCEPT.GetName(), baseStatisticsBean.YIntercept);
            result.Put(ViewFieldEnum.REGRESSION_XAVERAGE.GetName(), baseStatisticsBean.XAverage);
            result.Put(
                ViewFieldEnum.REGRESSION_XSTANDARDDEVIATIONPOP.GetName(),
                baseStatisticsBean.XStandardDeviationPop);
            result.Put(
                ViewFieldEnum.REGRESSION_XSTANDARDDEVIATIONSAMPLE.GetName(),
                baseStatisticsBean.XStandardDeviationSample);
            result.Put(ViewFieldEnum.REGRESSION_XSUM.GetName(), baseStatisticsBean.XSum);
            result.Put(ViewFieldEnum.REGRESSION_XVARIANCE.GetName(), baseStatisticsBean.XVariance);
            result.Put(ViewFieldEnum.REGRESSION_YAVERAGE.GetName(), baseStatisticsBean.YAverage);
            result.Put(
                ViewFieldEnum.REGRESSION_YSTANDARDDEVIATIONPOP.GetName(),
                baseStatisticsBean.YStandardDeviationPop);
            result.Put(
                ViewFieldEnum.REGRESSION_YSTANDARDDEVIATIONSAMPLE.GetName(),
                baseStatisticsBean.YStandardDeviationSample);
            result.Put(ViewFieldEnum.REGRESSION_YSUM.GetName(), baseStatisticsBean.YSum);
            result.Put(ViewFieldEnum.REGRESSION_YVARIANCE.GetName(), baseStatisticsBean.YVariance);
            result.Put(ViewFieldEnum.REGRESSION_DATAPOINTS.GetName(), baseStatisticsBean.DataPoints);
            result.Put(ViewFieldEnum.REGRESSION_N.GetName(), baseStatisticsBean.N);
            result.Put(ViewFieldEnum.REGRESSION_SUMX.GetName(), baseStatisticsBean.SumX);
            result.Put(ViewFieldEnum.REGRESSION_SUMXSQ.GetName(), baseStatisticsBean.SumXSq);
            result.Put(ViewFieldEnum.REGRESSION_SUMXY.GetName(), baseStatisticsBean.SumXY);
            result.Put(ViewFieldEnum.REGRESSION_SUMY.GetName(), baseStatisticsBean.SumY);
            result.Put(ViewFieldEnum.REGRESSION_SUMYSQ.GetName(), baseStatisticsBean.SumYSq);
            additionalProps?.AddProperties(result, decoration);

            return eventAdapterService.AdapterForTypedMap(result, eventType);
        }

        protected internal static EventType CreateEventType(
            StatViewAdditionalPropsForge additionalProps,
            ViewForgeEnv env,
            int streamNum)
        {
            LinkedHashMap<string, object> eventTypeMap = new LinkedHashMap<string, object>();
            eventTypeMap.Put(ViewFieldEnum.REGRESSION_SLOPE.GetName(), typeof(double?));
            eventTypeMap.Put(ViewFieldEnum.REGRESSION_YINTERCEPT.GetName(), typeof(double?));
            eventTypeMap.Put(ViewFieldEnum.REGRESSION_XAVERAGE.GetName(), typeof(double?));
            eventTypeMap.Put(ViewFieldEnum.REGRESSION_XSTANDARDDEVIATIONPOP.GetName(), typeof(double?));
            eventTypeMap.Put(ViewFieldEnum.REGRESSION_XSTANDARDDEVIATIONSAMPLE.GetName(), typeof(double?));
            eventTypeMap.Put(ViewFieldEnum.REGRESSION_XSUM.GetName(), typeof(double?));
            eventTypeMap.Put(ViewFieldEnum.REGRESSION_XVARIANCE.GetName(), typeof(double?));
            eventTypeMap.Put(ViewFieldEnum.REGRESSION_YAVERAGE.GetName(), typeof(double?));
            eventTypeMap.Put(ViewFieldEnum.REGRESSION_YSTANDARDDEVIATIONPOP.GetName(), typeof(double?));
            eventTypeMap.Put(ViewFieldEnum.REGRESSION_YSTANDARDDEVIATIONSAMPLE.GetName(), typeof(double?));
            eventTypeMap.Put(ViewFieldEnum.REGRESSION_YSUM.GetName(), typeof(double?));
            eventTypeMap.Put(ViewFieldEnum.REGRESSION_YVARIANCE.GetName(), typeof(double?));
            eventTypeMap.Put(ViewFieldEnum.REGRESSION_DATAPOINTS.GetName(), typeof(long?));
            eventTypeMap.Put(ViewFieldEnum.REGRESSION_N.GetName(), typeof(long?));
            eventTypeMap.Put(ViewFieldEnum.REGRESSION_SUMX.GetName(), typeof(double?));
            eventTypeMap.Put(ViewFieldEnum.REGRESSION_SUMXSQ.GetName(), typeof(double?));
            eventTypeMap.Put(ViewFieldEnum.REGRESSION_SUMXY.GetName(), typeof(double?));
            eventTypeMap.Put(ViewFieldEnum.REGRESSION_SUMY.GetName(), typeof(double?));
            eventTypeMap.Put(ViewFieldEnum.REGRESSION_SUMYSQ.GetName(), typeof(double?));
            StatViewAdditionalPropsForge.AddCheckDupProperties(
                eventTypeMap,
                additionalProps,
                ViewFieldEnum.REGRESSION_SLOPE,
                ViewFieldEnum.REGRESSION_YINTERCEPT);
            return DerivedViewTypeUtil.NewType("regview", eventTypeMap, env, streamNum);
        }
    }
} // end of namespace