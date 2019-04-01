///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.context.aifactory.select
{
	public class StmtForgeMethodSelect : StmtForgeMethod {

	    private readonly StatementBaseInfo @base;

	    public StmtForgeMethodSelect(StatementBaseInfo @base) {
	        this.@base = @base;
	    }

	    public StmtForgeMethodResult Make(
	        string packageName,
	        string classPostfix,
	        StatementCompileTimeServices services) {
	        StmtForgeMethodSelectResult forgablesResult = StmtForgeMethodSelectUtil.Make(
	            false, packageName, classPostfix, @base, services);
	        return forgablesResult.ForgeResult;
	    }
	}
} // end of namespace