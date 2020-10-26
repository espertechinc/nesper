///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

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
        LengthWindowViewFactory Length();

        PriorEventViewFactory Prior();

        TimeWindowViewFactory Time();

        KeepAllViewFactory Keepall();

        TimeBatchViewFactory Timebatch();

        TimeLengthBatchViewFactory Timelengthbatch();

        LengthBatchViewFactory Lengthbatch();

        SortWindowViewFactory Sort();

        RankWindowViewFactory Rank();

        TimeAccumViewFactory Timeaccum();

        UniqueByPropertyViewFactory Unique();

        FirstUniqueByPropertyViewFactory Firstunique();

        FirstTimeViewFactory Firsttime();

        TimeOrderViewFactory Timeorder();

        ExternallyTimedBatchViewFactory Exttimebatch();

        ExternallyTimedWindowViewFactory Exttime();

        LastEventViewFactory Lastevent();

        FirstEventViewFactory Firstevent();

        FirstLengthWindowViewFactory Firstlength();

        SizeViewFactory Size();

        UnivariateStatisticsViewFactory Uni();

        WeightedAverageViewFactory Weightedavg();

        RegressionLinestViewFactory Regression();

        CorrelationViewFactory Correlation();

        GroupByViewFactory Group();

        IntersectViewFactory Intersect();

        UnionViewFactory Union();

        ExpressionBatchViewFactory Exprbatch();

        ExpressionWindowViewFactory Expr();

        RowRecogNFAViewFactory RowRecog();
    }
} // end of namespace