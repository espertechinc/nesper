using System;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
namespace com.espertech.esper.common.@internal.epl.rowrecog.expr
{
	///////////////////////////////////////////////////////////////////////////////////////
	// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
	// http://esper.codehaus.org                                                          /
	// ---------------------------------------------------------------------------------- /
	// The software in this package is published under the terms of the GPL license       /
	// a copy of which has been included with this distribution in the license.txt file.  /
	///////////////////////////////////////////////////////////////////////////////////////

	public interface RowRecogExprNodeVisitor {
	    void Visit(RowRecogExprNode node, RowRecogExprNode optionalParent, int level);
	}
} // end of namespace
