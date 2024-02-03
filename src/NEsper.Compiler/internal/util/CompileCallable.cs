///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Threading;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.compile.compiler;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.compat;

namespace com.espertech.esper.compiler.@internal.util
{
	public class CompileCallable : ICallable<CompilableItemResult>
	{
		private readonly CompilableItem compilableItem;
		private readonly ModuleCompileTimeServices compileTimeServices;
		private readonly IList<EPCompiled> path;
		private readonly SemaphoreSlim semaphore;
		private readonly CompilerAbstraction compilerAbstraction;
		private readonly CompilerAbstractionArtifactCollection compilationState;

		public CompileCallable(
			CompilableItem compilableItem,
			ModuleCompileTimeServices compileTimeServices,
			IList<EPCompiled> path,
			SemaphoreSlim semaphore,
			CompilerAbstraction compilerAbstraction,
			CompilerAbstractionArtifactCollection compilationState)
		{
			this.compilableItem = compilableItem;
			this.compileTimeServices = compileTimeServices;
			this.path = path;
			this.semaphore = semaphore;
			this.compilerAbstraction = compilerAbstraction;
			this.compilationState = compilationState;
		}

		public CompilableItemResult Call()
		{
			try {
				CompilerAbstractionCompilationContext context =
					new CompilerAbstractionCompilationContext(compileTimeServices, path);
				compilerAbstraction.CompileClasses(compilableItem.Classes, context, compilationState);
			}
			catch (Exception e) {
				return new CompilableItemResult(e);
			}
			finally {
				semaphore.Release();
				compilableItem.PostCompileLatch.Completed(compilationState.Artifacts);
			}

			return new CompilableItemResult();
		}
	}
} // end of namespace
