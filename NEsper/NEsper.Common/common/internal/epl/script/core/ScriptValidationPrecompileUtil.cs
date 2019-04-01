///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.script.core
{
	public class ScriptValidationPrecompileUtil {
	    // All scripts get compiled/verfied - to ensure they are compiled once and not multiple times.
	    public static void ValidateScripts(IList<ExpressionScriptProvided> scripts, ExpressionDeclDesc expressionDeclDesc, StatementCompileTimeServices compileTimeServices) {
	        if (scripts == null) {
	            return;
	        }
	        string defaultDialect = compileTimeServices.Configuration.Compiler.Scripts.DefaultDialect;
	        ISet<NameParameterCountKey> scriptsSet = new HashSet<NameParameterCountKey>();
	        foreach (ExpressionScriptProvided script in scripts) {
	            ValidateScript(script, defaultDialect, compileTimeServices.ImportServiceCompileTime);

	            NameParameterCountKey key = new NameParameterCountKey(script.Name, script.ParameterNames.Length);
	            if (scriptsSet.Contains(key)) {
	                throw new ExprValidationException("Script name '" + script.Name + "' has already been defined with the same number of parameters");
	            }
	            scriptsSet.Add(key);
	        }

	        if (expressionDeclDesc != null) {
	            foreach (ExpressionDeclItem declItem in expressionDeclDesc.Expressions) {
	                if (scriptsSet.Contains(new NameParameterCountKey(declItem.Name, 0))) {
	                    throw new ExprValidationException("Script name '" + declItem.Name + "' overlaps with another expression of the same name");
	                }
	            }
	        }
	    }

	    private static void ValidateScript(ExpressionScriptProvided script, string defaultDialect, ImportServiceCompileTime importService) {
	        string dialect = script.OptionalDialect == null ? defaultDialect : script.OptionalDialect;
	        if (dialect == null) {
	            throw new ExprValidationException("Failed to determine script dialect for script '" + script.Name + "', please configure a default dialect or provide a dialect explicitly");
	        }

	        ExpressionScriptCompiled compiledBuf;
	        if (dialect.Trim().Equals("mvel", StringComparison.InvariantCultureIgnoreCase)) {
	            if (!MVELInvoker.IsMVELInClasspath(importService)) {
	                throw new ExprValidationException("MVEL scripting runtime not found in classpath, script dialect 'mvel' requires mvel in classpath for script '" + script.Name + "'");
	            }
	            MVELHelper.VerifyScript(script, importService);
	            compiledBuf = null;
	        } else {
	            compiledBuf = JSR223Helper.VerifyCompileScript(script.Name, script.Expression, dialect);
	        }
	        script.CompiledBuf = compiledBuf;

	        if (script.ParameterNames.Length != 0) {
	            HashSet<string> parameters = new HashSet<string>();
	            foreach (string param in script.ParameterNames) {
	                if (parameters.Contains(param)) {
	                    throw new ExprValidationException("Invalid script parameters for script '" + script.Name + "', parameter '" + param + "' is defined more then once");
	                }
	                parameters.Add(param);
	            }
	        }
	    }
	}
} // end of namespace