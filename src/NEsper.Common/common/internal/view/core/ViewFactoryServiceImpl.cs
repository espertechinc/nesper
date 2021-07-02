///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.epl.rowrecog.core;
using com.espertech.esper.common.@internal.view.derived;
using com.espertech.esper.common.@internal.view.expression;
using com.espertech.esper.common.@internal.view.exttimedbatch;
using com.espertech.esper.common.@internal.view.exttimedwin;
using com.espertech.esper.common.@internal.view.firstevent;
using com.espertech.esper.common.@internal.view.firstlength;
using com.espertech.esper.common.@internal.view.firsttime;
using com.espertech.esper.common.@internal.view.firstunique;
using com.espertech.esper.common.@internal.view.groupwin;
using com.espertech.esper.common.@internal.view.intersect;
using com.espertech.esper.common.@internal.view.keepall;
using com.espertech.esper.common.@internal.view.lastevent;
using com.espertech.esper.common.@internal.view.length;
using com.espertech.esper.common.@internal.view.lengthbatch;
using com.espertech.esper.common.@internal.view.prior;
using com.espertech.esper.common.@internal.view.rank;
using com.espertech.esper.common.@internal.view.sort;
using com.espertech.esper.common.@internal.view.timebatch;
using com.espertech.esper.common.@internal.view.timelengthbatch;
using com.espertech.esper.common.@internal.view.timetolive;
using com.espertech.esper.common.@internal.view.timewin;
using com.espertech.esper.common.@internal.view.time_accum;
using com.espertech.esper.common.@internal.view.union;
using com.espertech.esper.common.@internal.view.unique;

namespace com.espertech.esper.common.@internal.view.core
{
    public class ViewFactoryServiceImpl : ViewFactoryService
    {
        public static readonly ViewFactoryServiceImpl INSTANCE = new ViewFactoryServiceImpl();

        private ViewFactoryServiceImpl()
        {
        }

        public LengthWindowViewFactory Length(StateMgmtSetting stateMgmtSettings)
        {
            return new LengthWindowViewFactory();
        }

        public PriorEventViewFactory Prior(StateMgmtSetting stateMgmtSettings)
        {
            return new PriorEventViewFactory();
        }

        public TimeWindowViewFactory Time(StateMgmtSetting stateMgmtSettings)
        {
            return new TimeWindowViewFactory();
        }

        public KeepAllViewFactory Keepall(StateMgmtSetting stateMgmtSettings)
        {
            return new KeepAllViewFactory();
        }

        public TimeBatchViewFactory Timebatch(StateMgmtSetting stateMgmtSettings)
        {
            return new TimeBatchViewFactory();
        }

        public TimeLengthBatchViewFactory Timelengthbatch(StateMgmtSetting stateMgmtSettings)
        {
            return new TimeLengthBatchViewFactory();
        }

        public LengthBatchViewFactory Lengthbatch(StateMgmtSetting stateMgmtSettings)
        {
            return new LengthBatchViewFactory();
        }

        public SortWindowViewFactory Sort(StateMgmtSetting stateMgmtSettings)
        {
            return new SortWindowViewFactory();
        }

        public RankWindowViewFactory Rank(StateMgmtSetting stateMgmtSettings)
        {
            return new RankWindowViewFactory();
        }

        public TimeAccumViewFactory Timeaccum(StateMgmtSetting stateMgmtSettings)
        {
            return new TimeAccumViewFactory();
        }

        public UniqueByPropertyViewFactory Unique(StateMgmtSetting stateMgmtSettings)
        {
            return new UniqueByPropertyViewFactory();
        }

        public FirstUniqueByPropertyViewFactory Firstunique(StateMgmtSetting stateMgmtSettings)
        {
            return new FirstUniqueByPropertyViewFactory();
        }

        public FirstTimeViewFactory Firsttime(StateMgmtSetting stateMgmtSettings)
        {
            return new FirstTimeViewFactory();
        }

        public ExternallyTimedBatchViewFactory Exttimebatch(StateMgmtSetting stateMgmtSettings)
        {
            return new ExternallyTimedBatchViewFactory();
        }

        public ExternallyTimedWindowViewFactory Exttime(StateMgmtSetting stateMgmtSettings)
        {
            return new ExternallyTimedWindowViewFactory();
        }

        public TimeOrderViewFactory Timeorder(StateMgmtSetting stateMgmtSettings)
        {
            return new TimeOrderViewFactory();
        }

        public LastEventViewFactory Lastevent(StateMgmtSetting stateMgmtSettings)
        {
            return new LastEventViewFactory();
        }

        public FirstEventViewFactory Firstevent(StateMgmtSetting stateMgmtSettings)
        {
            return new FirstEventViewFactory();
        }

        public FirstLengthWindowViewFactory Firstlength(StateMgmtSetting stateMgmtSettings)
        {
            return new FirstLengthWindowViewFactory();
        }

        public SizeViewFactory Size(StateMgmtSetting stateMgmtSettings)
        {
            return new SizeViewFactory();
        }

        public UnivariateStatisticsViewFactory Uni(StateMgmtSetting stateMgmtSettings)
        {
            return new UnivariateStatisticsViewFactory();
        }

        public WeightedAverageViewFactory Weightedavg(StateMgmtSetting stateMgmtSettings)
        {
            return new WeightedAverageViewFactory();
        }

        public RegressionLinestViewFactory Regression(StateMgmtSetting stateMgmtSettings)
        {
            return new RegressionLinestViewFactory();
        }

        public CorrelationViewFactory Correlation(StateMgmtSetting stateMgmtSettings)
        {
            return new CorrelationViewFactory();
        }

        public GroupByViewFactory Group(StateMgmtSetting stateMgmtSettings)
        {
            return new GroupByViewFactory();
        }

        public IntersectViewFactory Intersect(StateMgmtSetting stateMgmtSettings)
        {
            return new IntersectViewFactory();
        }

        public UnionViewFactory Union(StateMgmtSetting stateMgmtSettings)
        {
            return new UnionViewFactory();
        }

        public ExpressionBatchViewFactory Exprbatch(StateMgmtSetting stateMgmtSettings)
        {
            return new ExpressionBatchViewFactory();
        }

        public ExpressionWindowViewFactory Expr(StateMgmtSetting stateMgmtSettings)
        {
            return new ExpressionWindowViewFactory();
        }

        public RowRecogNFAViewFactory RowRecog(StateMgmtSetting stateMgmtSettings)
        {
            return new RowRecogNFAViewFactory();
        }
    }
} // end of namespace