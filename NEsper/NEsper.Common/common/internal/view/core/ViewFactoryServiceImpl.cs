///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.view.core
{
	public class ViewFactoryServiceImpl : ViewFactoryService {

	    public readonly static ViewFactoryServiceImpl INSTANCE = new ViewFactoryServiceImpl();

	    private ViewFactoryServiceImpl() {
	    }

	    public LengthWindowViewFactory Length() {
	        return new LengthWindowViewFactory();
	    }

	    public PriorEventViewFactory Prior() {
	        return new PriorEventViewFactory();
	    }

	    public TimeWindowViewFactory Time() {
	        return new TimeWindowViewFactory();
	    }

	    public KeepAllViewFactory Keepall() {
	        return new KeepAllViewFactory();
	    }

	    public TimeBatchViewFactory Timebatch() {
	        return new TimeBatchViewFactory();
	    }

	    public TimeLengthBatchViewFactory Timelengthbatch() {
	        return new TimeLengthBatchViewFactory();
	    }

	    public LengthBatchViewFactory Lengthbatch() {
	        return new LengthBatchViewFactory();
	    }

	    public SortWindowViewFactory Sort() {
	        return new SortWindowViewFactory();
	    }

	    public RankWindowViewFactory Rank() {
	        return new RankWindowViewFactory();
	    }

	    public TimeAccumViewFactory Timeaccum() {
	        return new TimeAccumViewFactory();
	    }

	    public UniqueByPropertyViewFactory Unique() {
	        return new UniqueByPropertyViewFactory();
	    }

	    public FirstUniqueByPropertyViewFactory Firstunique() {
	        return new FirstUniqueByPropertyViewFactory();
	    }

	    public FirstTimeViewFactory Firsttime() {
	        return new FirstTimeViewFactory();
	    }

	    public ExternallyTimedBatchViewFactory Exttimebatch() {
	        return new ExternallyTimedBatchViewFactory();
	    }

	    public ExternallyTimedWindowViewFactory Exttime() {
	        return new ExternallyTimedWindowViewFactory();
	    }

	    public TimeOrderViewFactory Timeorder() {
	        return new TimeOrderViewFactory();
	    }

	    public LastEventViewFactory Lastevent() {
	        return new LastEventViewFactory();
	    }

	    public FirstEventViewFactory Firstevent() {
	        return new FirstEventViewFactory();
	    }

	    public FirstLengthWindowViewFactory Firstlength() {
	        return new FirstLengthWindowViewFactory();
	    }

	    public SizeViewFactory Size() {
	        return new SizeViewFactory();
	    }

	    public UnivariateStatisticsViewFactory Uni() {
	        return new UnivariateStatisticsViewFactory();
	    }

	    public WeightedAverageViewFactory Weightedavg() {
	        return new WeightedAverageViewFactory();
	    }

	    public RegressionLinestViewFactory Regression() {
	        return new RegressionLinestViewFactory();
	    }

	    public CorrelationViewFactory Correlation() {
	        return new CorrelationViewFactory();
	    }

	    public GroupByViewFactory Group() {
	        return new GroupByViewFactory();
	    }

	    public IntersectViewFactory Intersect() {
	        return new IntersectViewFactory();
	    }

	    public UnionViewFactory Union() {
	        return new UnionViewFactory();
	    }

	    public ExpressionBatchViewFactory Exprbatch() {
	        return new ExpressionBatchViewFactory();
	    }

	    public ExpressionWindowViewFactory Expr() {
	        return new ExpressionWindowViewFactory();
	    }

	    public RowRecogNFAViewFactory RowRecog() {
	        return new RowRecogNFAViewFactory();
	    }
	}
} // end of namespace