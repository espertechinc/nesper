///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.variable.compiletime;
using com.espertech.esper.common.@internal.@event.bean.service;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.view.core
{
	public class ViewForgeEnv {
	    private readonly ViewFactoryForgeArgs args;

	    public ViewForgeEnv(ViewFactoryForgeArgs args) {
	        this.args = args;
	    }

	    public ImportServiceCompileTime ImportServiceCompileTime {
	        get => args.ImportService;
	    }

	    public Configuration Configuration {
	        get => args.Configuration;
	    }

	    public BeanEventTypeFactory BeanEventTypeFactoryProtected {
	        get => args.BeanEventTypeFactoryPrivate;
	    }

	    public EventTypeCompileTimeRegistry EventTypeModuleCompileTimeRegistry {
	        get => args.EventTypeModuleCompileTimeRegistry;
	    }

	    public Attribute[] Annotations {
	        get { return args.Annotations; }
	    }

	    public string OptionalStatementName {
	        get => args.StatementName;
	    }

	    public int StatementNumber {
	        get => args.StatementNumber;
	    }

	    public StatementCompileTimeServices StatementCompileTimeServices {
	        get => args.CompileTimeServices;
	    }

	    public StatementRawInfo StatementRawInfo {
	        get => args.StatementRawInfo;
	    }

	    public VariableCompileTimeResolver VariableCompileTimeResolver {
	        get => args.CompileTimeServices.VariableCompileTimeResolver;
	    }

	    public string ContextName {
	        get => args.StatementRawInfo.ContextName;
	    }

	    public EventTypeCompileTimeResolver EventTypeCompileTimeResolver {
	        get => args.CompileTimeServices.EventTypeCompileTimeResolver;
	    }

	    public string ModuleName {
	        get => args.StatementRawInfo.ModuleName;
	    }
	}
} // end of namespace