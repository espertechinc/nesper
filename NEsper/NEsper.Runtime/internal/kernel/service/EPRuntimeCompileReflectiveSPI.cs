///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.module;
using com.espertech.esper.common.client.soda;
using com.espertech.esper.common.@internal.compile.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.runtime.client;

namespace com.espertech.esper.runtime.@internal.kernel.service
{
	public class EPRuntimeCompileReflectiveSPI : CompileExpressionSPI
	{
		private readonly EPRuntimeCompileReflectiveService _provider;
		private readonly EPRuntime _runtime;

		public EPRuntimeCompileReflectiveSPI(
			EPRuntimeCompileReflectiveService provider,
			EPRuntime runtime)
		{
			_provider = provider;
			_runtime = runtime;
		}

		public bool IsCompilerAvailable => _provider.IsCompilerAvailable;

		public EPCompiled ReflectiveCompile(string epl)
		{
			return _provider.ReflectiveCompile(epl, _runtime.ConfigurationDeepCopy, _runtime.RuntimePath);
		}

		public EPCompiled ReflectiveCompile(Module module)
		{
			return _provider.ReflectiveCompile(module, _runtime.ConfigurationDeepCopy, _runtime.RuntimePath);
		}

		public EPCompiled ReflectiveCompileFireAndForget(string epl)
		{
			return _provider.ReflectiveCompileFireAndForget(epl, _runtime.ConfigurationDeepCopy, _runtime.RuntimePath);
		}

		public ExprNode ReflectiveCompileExpression(
			string epl,
			EventType[] eventTypes,
			string[] streamNames)
		{
			return _provider.ReflectiveCompileExpression(epl, eventTypes, streamNames, _runtime.ConfigurationDeepCopy);
		}

		public EPStatementObjectModel ReflectiveEPLToModel(string epl)
		{
			return _provider.ReflectiveEPLToModel(epl, _runtime.ConfigurationDeepCopy);
		}

		public ExprNode CompileExpression(
			string epl,
			EventType[] eventTypes,
			string[] streamNames)
		{
			return ReflectiveCompileExpression(epl, eventTypes, streamNames);
		}

		public Module ReflectiveParseModule(string epl)
		{
			return _provider.ReflectiveParseModule(epl);
		}
	}
} // end of namespace
