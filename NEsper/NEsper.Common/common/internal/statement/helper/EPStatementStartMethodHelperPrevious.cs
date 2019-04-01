///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.common.@internal.view.groupwin;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.statement.helper
{
	public class EPStatementStartMethodHelperPrevious {
	    public static DataWindowViewWithPrevious FindPreviousViewFactory(ViewFactory[] factories) {
	        ViewFactory factoryFound = null;
	        foreach (ViewFactory factory in factories) {
	            if (factory is DataWindowViewWithPrevious) {
	                factoryFound = factory;
	                break;
	            }
	            if (factory is GroupByViewFactory) {
	                GroupByViewFactory grouped = (GroupByViewFactory) factory;
	                return FindPreviousViewFactory(grouped.Groupeds);
	            }
	        }
	        if (factoryFound == null) {
	            throw new RuntimeException("Failed to find 'previous'-handling view factory");  // was verified earlier, should not occur
	        }
	        return (DataWindowViewWithPrevious) factoryFound;
	    }
	}
} // end of namespace