///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.script.core
{
	public class ScriptDescriptorRuntime {
	    private string optionalDialect;
	    private string scriptName;
	    private string expression;
	    private string[] parameterNames;
	    private ExprEvaluator[] parameters;
	    private Type[] evaluationTypes;
	    private SimpleNumberCoercer coercer;

	    private string defaultDialect;
	    private ImportService _importService;

	    public void SetOptionalDialect(string optionalDialect) {
	        this.optionalDialect = optionalDialect;
	    }

	    public void SetScriptName(string scriptName) {
	        this.scriptName = scriptName;
	    }

	    public void SetExpression(string expression) {
	        this.expression = expression;
	    }

	    public void SetParameterNames(string[] parameterNames) {
	        this.parameterNames = parameterNames;
	    }

	    public void SetEvaluationTypes(Type[] evaluationTypes) {
	        this.evaluationTypes = evaluationTypes;
	    }

	    public string OptionalDialect {
	        get => optionalDialect;
	    }

	    public string ScriptName {
	        get => scriptName;
	    }

	    public string Expression {
	        get => expression;
	    }

	    public string[] GetParameterNames() {
	        return parameterNames;
	    }

	    public Type[] GetEvaluationTypes() {
	        return evaluationTypes;
	    }

	    public string DefaultDialect {
	        get => defaultDialect;
	    }

	    public void SetDefaultDialect(string defaultDialect) {
	        this.defaultDialect = defaultDialect;
	    }

	    public ImportService ImportService {
	        get => _importService;
	    }

	    public void SetClasspathImportService(ImportService importService) {
	        this._importService = importService;
	    }

	    public ExprEvaluator[] GetParameters() {
	        return parameters;
	    }

	    public void SetParameters(ExprEvaluator[] parameters) {
	        this.parameters = parameters;
	    }

	    public SimpleNumberCoercer SimpleNumberCoercer {
	        get => coercer;
	    }

	    public void SetCoercer(SimpleNumberCoercer coercer) {
	        this.coercer = coercer;
	    }
	}
} // end of namespace