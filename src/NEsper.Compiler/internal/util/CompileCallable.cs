///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

using com.espertech.esper.common.client.assembly;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.compiler.@internal.util
{
	public class CompileCallable : ICallable<CompilableItemResult>
	{
		private readonly CompilableItem _compilableItem;
		private readonly ModuleCompileTimeServices _compileTimeServices;
		private readonly Semaphore _semaphore;
		private readonly ICollection<Pair<Assembly, byte[]>> _statementAssembliesWithImage;
		private readonly CompilationContext _compilationContext;

		CompileCallable(
			CompilationContext compilationContext,
			CompilableItem compilableItem,
			ModuleCompileTimeServices compileTimeServices,
			Semaphore semaphore,
			ICollection<Pair<Assembly, byte[]>> statementAssembliesWithImage)
		{
			_compilationContext = compilationContext;
			_compilableItem = compilableItem;
			_compileTimeServices = compileTimeServices;
			_semaphore = semaphore;
			_statementAssembliesWithImage = statementAssembliesWithImage;
		}

		public CompilableItemResult Call()
		{
			try {
				var container = _compileTimeServices.Container;
				var compiler = container
					.RoslynCompiler()
					.WithCodeLogging(_compileTimeServices.Configuration.Compiler.Logging.IsEnableCode)
					.WithCodeAuditDirectory(_compileTimeServices.Configuration.Compiler.Logging.AuditDirectory)
					.WithCodegenClasses(_compilableItem.Classes);

				_statementAssembliesWithImage.Add(compiler.Compile(_compilationContext));
			}
			catch (Exception t) {
				return new CompilableItemResult(t);
			}
			finally {
				_semaphore.Release();
				_compilableItem.PostCompileLatch.Completed(_statementAssembliesWithImage);
			}

			return new CompilableItemResult();
		}
	}
} // end of namespace
