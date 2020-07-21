///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Threading;

using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.compat;

namespace com.espertech.esper.compiler.@internal.util
{
	public class CompileCallable : ICallable<CompilableItemResult>
	{
		private readonly CompilableItem _compilableItem;
		private readonly ModuleCompileTimeServices _compileTimeServices;
		private readonly Semaphore _semaphore;
		private readonly IDictionary<string, byte[]> _statementBytes;

		CompileCallable(
			CompilableItem compilableItem,
			ModuleCompileTimeServices compileTimeServices,
			Semaphore semaphore,
			IDictionary<string, byte[]> statementBytes)
		{
			_compilableItem = compilableItem;
			_compileTimeServices = compileTimeServices;
			_semaphore = semaphore;
			_statementBytes = statementBytes;
		}

		public CompilableItemResult Call()
		{
			try {
				var compiler = new RoslynCompiler()
					.WithCodeLogging(_compileTimeServices.Configuration.Compiler.Logging.IsEnableCode)
					.WithCodeAuditDirectory(_compileTimeServices.Configuration.Compiler.Logging.AuditDirectory)
					.WithCodegenClasses(_compilableItem.Classes);

				var assembly = compiler.Compile();
			}
			catch (Exception t) {
				return new CompilableItemResult(t);
			}
			finally {
				_semaphore.Release();
				_compilableItem.PostCompileLatch.Completed(_statementBytes);
			}

			return new CompilableItemResult();
		}
	}
} // end of namespace
