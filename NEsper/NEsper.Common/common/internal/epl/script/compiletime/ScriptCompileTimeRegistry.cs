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
using com.espertech.esper.common.@internal.epl.script.core;
using com.espertech.esper.common.@internal.epl.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.script.compiletime
{
	public class ScriptCompileTimeRegistry : CompileTimeRegistry {
	    private readonly IDictionary<NameAndParamNum, ExpressionScriptProvided> expressions = new Dictionary<NameAndParamNum,  ExpressionScriptProvided>();

	    public void NewScript(ExpressionScriptProvided detail) {
	        if (!detail.Visibility.IsModuleProvidedAccessModifier) {
	            throw new IllegalStateException("Invalid visibility for contexts");
	        }
	        NameAndParamNum key = new NameAndParamNum(detail.Name, detail.ParameterNames.Length);
	        ExpressionScriptProvided existing = expressions.Get(key);
	        if (existing != null) {
	            throw new IllegalStateException("Duplicate script has been encountered for name '" + key + "'");
	        }
	        expressions.Put(key, detail);
	    }

	    public IDictionary<NameAndParamNum, ExpressionScriptProvided> GetScripts() {
	        return expressions;
	    }
	}
} // end of namespace