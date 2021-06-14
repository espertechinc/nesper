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
            result.Put(ViewFieldEnum.REGRESSION__SLOPE.GetName(), baseStatisticsBean.Slope);
            result.Put(ViewFieldEnum.REGRESSION__YINTERCEPT.GetName(), baseStatisticsBean.YIntercept);
            result.Put(ViewFieldEnum.REGRESSION__XAVERAGE.GetName(), baseStatisticsBean.XAverage);
            result.Put(
                ViewFieldEnum.REGRESSION__XSTANDARDDEVIATIONPOP.GetName(),
                baseStatisticsBean.XStandardDeviationPop);
            result.Put(
                ViewFieldEnum.REGRESSION__XSTANDARDDEVIATIONSAMPLE.GetName(),
                baseStatisticsBean.XStandardDeviationSample);
            result.Put(ViewFieldEnum.REGRESSION__XSUM.GetName(), baseStatisticsBean.XSum);
            result.Put(ViewFieldEnum.REGRESSION__XVARIANCE.GetName(), baseStatisticsBean.XVariance);
            result.Put(ViewFieldEnum.REGRESSION__YAVERAGE.GetName(), baseStatisticsBean.YAverage);
            result.Put(
                ViewFieldEnum.REGRESSION__YSTANDARDDEVIATIONPOP.GetName(),
                baseStatisticsBean.YStandardDeviationPop);
            result.Put(
                ViewFieldEnum.REGRESSION__YSTANDARDDEVIATIONSAMPLE.GetName(),
                baseStatisticsBean.YStandardDeviationSample);
            result.Put(ViewFieldEnum.REGRESSION__YSUM.GetName(), baseStatisticsBean.YSum);
            result.Put(ViewFieldEnum.REGRESSION__YVARIANCE.GetName(), baseStatisticsBean.YVariance);
            result.Put(ViewFieldEnum.REGRESSION__DATAPOINTS.GetName(), baseStatisticsBean.DataPoints);
            result.Put(ViewFieldEnum.REGRESSION__N.GetName(), baseStatisticsBean.N);
            result.Put(ViewFieldEnum.REGRESSION__SUMX.GetName(), baseStatisticsBean.SumX);
            result.Put(ViewFieldEnum.REGRESSION__SUMXSQ.GetName(), baseStatisticsBean.SumXSq);
            result.Put(ViewFieldEnum.REGRESSION__SUMXY.GetName(), baseStatisticsBean.SumXY);
            result.Put(ViewFieldEnum.REGRESSION__SUMY.GetName(), baseStatisticsBean.SumY);
            result.Put(ViewFieldEnum.REGRESSION__SUMYSQ.GetName(), baseStatisticsBean.SumYSq);
            additionalProps?.AddProperties(result, decoration);

            return eventAdapterService.AdapterForTypedMap(result, eventType);
        }

        protected internal static EventType CreateEventType(
            StatViewAdditionalPropsForge additionalProps,
            ViewForgeEnv env,
            int streamNum)
        {
            LinkedHashMap<string, object> eventTypeMap = new LinkedHashMap<string, object>();
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
            StatViewAdditionalPropsForge.AddCheckDupProperties(
                eventTypeMap,
                additionalProps,
                ViewFieldEnum.REGRESSION__SLOPE,
                ViewFieldEnum.REGRESSION__YINTERCEPT);
            return DerivedViewTypeUtil.NewType("regview", eventTypeMap, env, streamNum);
        }
    }
} // end of namespace