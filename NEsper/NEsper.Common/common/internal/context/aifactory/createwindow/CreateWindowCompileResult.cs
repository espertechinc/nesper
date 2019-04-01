///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.context.aifactory.createwindow
{
	public class CreateWindowCompileResult {
	    private readonly FilterSpecCompiled filterSpecCompiled;
	    private readonly SelectClauseSpecRaw selectClauseSpecRaw;
	    private readonly EventType asEventType;

	    public CreateWindowCompileResult(FilterSpecCompiled filterSpecCompiled, SelectClauseSpecRaw selectClauseSpecRaw, EventType asEventType) {
	        this.filterSpecCompiled = filterSpecCompiled;
	        this.selectClauseSpecRaw = selectClauseSpecRaw;
	        this.asEventType = asEventType;
	    }

	    public FilterSpecCompiled FilterSpecCompiled {
	        get => filterSpecCompiled;
	    }

	    public SelectClauseSpecRaw SelectClauseSpecRaw {
	        get => selectClauseSpecRaw;
	    }

	    public EventType AsEventType {
	        get => asEventType;
	    }
	}
} // end of namespace