///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.streamtype;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.support
{
	public class SupportExprValidationContextFactory {
	    public static ExprValidationContext MakeEmpty() {
	        return Make(new StreamTypeServiceImpl(false));
	    }

	    public static ExprValidationContext Make(StreamTypeService streamTypeService) {
	        ModuleCompileTimeServices moduleServices = new ModuleCompileTimeServices();
	        moduleServices.Configuration = new Configuration();
	        moduleServices.ImportServiceCompileTime = SupportClasspathImport.INSTANCE;
	        StatementCompileTimeServices services = new StatementCompileTimeServices(1, moduleServices);
	        StatementRawInfo raw = new StatementRawInfo(1, "abc", null, StatementType.SELECT, null, null, null, null);
	        return new ExprValidationContextBuilder(streamTypeService, raw, services).Build();
	    }

	    public static ExprValidationContext MakeEmpty(ThreadingProfile threadingProfile) {
	        return MakeEmpty();
	    }
	}
} // end of namespace