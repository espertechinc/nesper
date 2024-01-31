///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.rowrecog.core;
using com.espertech.esper.common.@internal.epl.virtualdw;
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
    public interface ViewFactoryForgeVisitor<T>
    {
        T Visit(LengthWindowViewForge forge);
        T Visit(SortWindowViewForge forge);
        T Visit(TimeLengthBatchViewForge forge);
        T Visit(SizeViewForge forge);
        T Visit(UnivariateStatisticsViewForge forge);
        T Visit(MergeViewFactoryForge forge);
        T Visit(UniqueByPropertyViewForge forge);
        T Visit(TimeAccumViewForge forge);
        T Visit(ExternallyTimedBatchViewForge forge);
        T Visit(IntersectViewFactoryForge forge);
        T Visit(PriorEventViewForge forge);
        T Visit(FirstLengthWindowViewForge forge);
        T Visit(TimeOrderViewForge forge);
        T Visit(ExpressionBatchViewForge forge);
        T Visit(LastEventViewForge forge);
        T Visit(TimeToLiveViewForge forge);
        T Visit(FirstTimeViewForge forge);
        T Visit(TimeWindowViewForge forge);
        T Visit(RowRecogNFAViewFactoryForge forge);
        T Visit(RankWindowViewForge forge);
        T Visit(ExternallyTimedWindowViewForge forge);
        T Visit(WeightedAverageViewForge forge);
        T Visit(LengthBatchViewForge forge);
        T Visit(RegressionLinestViewForge forge);
        T Visit(CorrelationViewForge forge);
        T Visit(KeepAllViewForge forge);
        T Visit(ExpressionWindowViewForge forge);
        T Visit(UnionViewFactoryForge forge);
        T Visit(TimeBatchViewForge forge);
        T Visit(FirstUniqueByPropertyViewForge forge);
        T Visit(FirstEventViewForge forge);
        T Visit(GroupByViewFactoryForge forge);
        T Visit(VirtualDWViewFactoryForge forge);
        T VisitExtension(ViewFactoryForge extension);
    }
} // end of namespace