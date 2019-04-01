///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.@event.bean.service;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.view.core
{
	public class ViewFactoryForgeArgs {
	    private readonly StatementRawInfo statementRawInfo;
	    private readonly int streamNum;
	    private readonly StreamSpecOptions options;
	    private readonly bool isSubquery;
	    private readonly int subqueryNumber;
	    private readonly string optionalCreateNamedWindowName;
	    private readonly StatementCompileTimeServices compileTimeServices;

	    public ViewFactoryForgeArgs(int streamNum, bool isSubquery, int subqueryNumber, StreamSpecOptions options, string optionalCreateNamedWindowName, StatementRawInfo statementRawInfo, StatementCompileTimeServices compileTimeServices) {
	        this.statementRawInfo = statementRawInfo;
	        this.streamNum = streamNum;
	        this.options = options;
	        this.isSubquery = isSubquery;
	        this.subqueryNumber = subqueryNumber;
	        this.optionalCreateNamedWindowName = optionalCreateNamedWindowName;
	        this.compileTimeServices = compileTimeServices;
	    }

	    public int StreamNum {
	        get => streamNum;
	    }

	    public StreamSpecOptions Options {
	        get => options;
	    }

	    public bool IsSubquery() {
	        return isSubquery;
	    }

	    public int SubqueryNumber {
	        get => subqueryNumber;
	    }

	    public ImportServiceCompileTime ImportService {
	        get => compileTimeServices.ImportServiceCompileTime;
	    }

	    public Configuration Configuration {
	        get => compileTimeServices.Configuration;
	    }

	    public ViewResolutionService ViewResolutionService {
	        get => compileTimeServices.ViewResolutionService;
	    }

	    public BeanEventTypeFactory BeanEventTypeFactoryPrivate {
	        get => compileTimeServices.BeanEventTypeFactoryPrivate;
	    }

	    public EventTypeCompileTimeRegistry EventTypeModuleCompileTimeRegistry {
	        get => compileTimeServices.EventTypeCompileTimeRegistry;
	    }

	    public Attribute[] GetAnnotations() {
	        return statementRawInfo.Annotations;
	    }

	    public string StatementName {
	        get => statementRawInfo.StatementName;
	    }

	    public int StatementNumber {
	        get => statementRawInfo.StatementNumber;
	    }

	    public StatementCompileTimeServices CompileTimeServices {
	        get => compileTimeServices;
	    }

	    public StatementRawInfo StatementRawInfo {
	        get => statementRawInfo;
	    }

	    public string OptionalCreateNamedWindowName {
	        get => optionalCreateNamedWindowName;
	    }
	}
} // end of namespace