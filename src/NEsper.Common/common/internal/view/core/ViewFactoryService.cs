///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
    public interface ViewFactoryService
    {
        LengthWindowViewFactory Length(StateMgmtSetting stateMgmtSettings);

        PriorEventViewFactory Prior(StateMgmtSetting stateMgmtSettings);

        TimeWindowViewFactory Time(StateMgmtSetting stateMgmtSettings);

        KeepAllViewFactory Keepall(StateMgmtSetting stateMgmtSettings);

        TimeBatchViewFactory Timebatch(StateMgmtSetting stateMgmtSettings);

        TimeLengthBatchViewFactory Timelengthbatch(StateMgmtSetting stateMgmtSettings);

        LengthBatchViewFactory Lengthbatch(StateMgmtSetting stateMgmtSettings);

        SortWindowViewFactory Sort(StateMgmtSetting stateMgmtSettings);

        RankWindowViewFactory Rank(StateMgmtSetting stateMgmtSettings);

        TimeAccumViewFactory Timeaccum(StateMgmtSetting stateMgmtSettings);

        UniqueByPropertyViewFactory Unique(StateMgmtSetting stateMgmtSettings);

        FirstUniqueByPropertyViewFactory Firstunique(StateMgmtSetting stateMgmtSettings);

        FirstTimeViewFactory Firsttime(StateMgmtSetting stateMgmtSettings);

        TimeOrderViewFactory Timeorder(StateMgmtSetting stateMgmtSettings);

        ExternallyTimedBatchViewFactory Exttimebatch(StateMgmtSetting stateMgmtSettings);

        ExternallyTimedWindowViewFactory Exttime(StateMgmtSetting stateMgmtSettings);

        LastEventViewFactory Lastevent(StateMgmtSetting stateMgmtSettings);

        FirstEventViewFactory Firstevent(StateMgmtSetting stateMgmtSettings);

        FirstLengthWindowViewFactory Firstlength(StateMgmtSetting stateMgmtSettings);

        SizeViewFactory Size(StateMgmtSetting stateMgmtSettings);

        UnivariateStatisticsViewFactory Uni(StateMgmtSetting stateMgmtSettings);

        WeightedAverageViewFactory Weightedavg(StateMgmtSetting stateMgmtSettings);

        RegressionLinestViewFactory Regression(StateMgmtSetting stateMgmtSettings);

        CorrelationViewFactory Correlation(StateMgmtSetting stateMgmtSettings);

        GroupByViewFactory Group(StateMgmtSetting stateMgmtSettings);

        IntersectViewFactory Intersect(StateMgmtSetting stateMgmtSettings);

        UnionViewFactory Union(StateMgmtSetting stateMgmtSettings);

        ExpressionBatchViewFactory Exprbatch(StateMgmtSetting stateMgmtSettings);

        ExpressionWindowViewFactory Expr(StateMgmtSetting stateMgmtSettings);

        RowRecogNFAViewFactory RowRecog(StateMgmtSetting stateMgmtSettings);
    }
} // end of namespace