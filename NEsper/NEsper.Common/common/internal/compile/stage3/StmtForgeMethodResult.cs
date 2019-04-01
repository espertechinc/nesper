///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.compile.stage3
{
	public class StmtForgeMethodResult {

	    private readonly IList<StmtClassForgable> forgables;
	    private readonly IList<FilterSpecCompiled> filtereds;
	    private readonly IList<ScheduleHandleCallbackProvider> scheduleds;
	    private readonly IList<NamedWindowConsumerStreamSpec> namedWindowConsumers;
	    private readonly IList<FilterSpecParamExprNodeForge> filterBooleanExpressions;

	    public StmtForgeMethodResult(IList<StmtClassForgable> forgables, IList<FilterSpecCompiled> filtereds, IList<ScheduleHandleCallbackProvider> scheduleds, IList<NamedWindowConsumerStreamSpec> namedWindowConsumers, IList<FilterSpecParamExprNodeForge> filterBooleanExpressions) {
	        this.forgables = forgables;
	        this.filtereds = filtereds;
	        this.scheduleds = scheduleds;
	        this.namedWindowConsumers = namedWindowConsumers;
	        this.filterBooleanExpressions = filterBooleanExpressions;
	    }

	    public IList<StmtClassForgable> GetForgables() {
	        return forgables;
	    }

	    public IList<ScheduleHandleCallbackProvider> GetScheduleds() {
	        return scheduleds;
	    }

	    public IList<FilterSpecCompiled> GetFiltereds() {
	        return filtereds;
	    }

	    public IList<NamedWindowConsumerStreamSpec> GetNamedWindowConsumers() {
	        return namedWindowConsumers;
	    }

	    public IList<FilterSpecParamExprNodeForge> GetFilterBooleanExpressions() {
	        return filterBooleanExpressions;
	    }
	}
} // end of namespace