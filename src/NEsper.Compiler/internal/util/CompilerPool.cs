///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.artifact;
using com.espertech.esper.common.@internal.compile.compiler;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.compat.concurrency;
using com.espertech.esper.compat.threading;
using com.espertech.esper.compiler.client;

namespace com.espertech.esper.compiler.@internal.util
{
	class CompilerPool
	{
		private readonly ModuleCompileTimeServices compileTimeServices;
		private readonly IList<EPCompiled> path;
		private readonly CompilerAbstraction compilerAbstraction;
		private readonly CompilerAbstractionArtifactCollection compilationState;

		private IExecutorService compilerThreadPool;
		private IFuture<CompilableItemResult>[] futures;
		private SemaphoreSlim semaphore;

		internal CompilerPool(
			int size,
			ModuleCompileTimeServices compileTimeServices,
			IList<EPCompiled> path,
			CompilerAbstraction compilerAbstraction,
			CompilerAbstractionArtifactCollection compilationState)
		{
			this.compileTimeServices = compileTimeServices;
			this.path = path;
			this.compilerAbstraction = compilerAbstraction;
			this.compilationState = compilationState;

			ThreadFactory threadFactory = _ => new Thread(_) {
				IsBackground = true,
				Name = "CompilerPool-Thread-" + _
			};

			var config = compileTimeServices.Configuration.Compiler.ByteCode;
			var numThreads = config.ThreadPoolCompilerNumThreads;
			if (numThreads > 0 && size > 1) {
				compilerThreadPool = Executors.NewFixedThreadPool(numThreads, threadFactory);
				futures = new IFuture<CompilableItemResult>[size];
				var capacity = config.ThreadPoolCompilerCapacity ?? int.MaxValue;
				semaphore = new SemaphoreSlim(Math.Max(1, capacity));
			}
		}

		public void Submit(
			int statementNumber,
			CompilableItem item)
		{
			// We are adding all class-provided classes to the output.
			// Later we remove the create-class classes.
			compilationState.Add(item.ArtifactsProvided.OfType<ICompileArtifact>());

			// no thread pool, compile right there
			if (compilerThreadPool == null) {
				try {
					var context =
						new CompilerAbstractionCompilationContext(compileTimeServices, path);
					compilerAbstraction.CompileClasses(item.Classes, context, compilationState);
				}
				finally {
					item.PostCompileLatch.Completed(compilationState.Artifacts);
				}

				return;
			}

			var callable = new CompileCallable(
				item,
				compileTimeServices,
				path,
				semaphore,
				compilerAbstraction,
				compilationState);

			semaphore.Wait();
			//semaphore.Acquire();
			futures[statementNumber] = compilerThreadPool.Submit(callable);
		}

		public void ShutdownCollectResults()
		{
			if (compilerThreadPool == null) {
				return;
			}

			compilerThreadPool.Shutdown();
			try {
				compilerThreadPool.AwaitTermination(TimeSpan.FromSeconds(300)); // TimeSpan.FromSeconds(double.MaxValue)
			}
			catch (ThreadInterruptedException e) {
				throw new EPRuntimeException(e);
			}

			foreach (var future in futures) {
				if (future == null) {
					continue;
				}

				CompilableItemResult result;
				try {
					result = future.Get();
				}
				catch (ThreadInterruptedException e) {
					throw new EPCompileException(e.Message, e);
				}
				//catch (ExecutionException e) {
				//	throw new EPCompileException(e.Message, e);
				//}

				if (result.Exception != null) {
					throw new EPCompileException(result.Exception.Message, result.Exception);
				}
			}
		}

		public void Shutdown()
		{
			compilerThreadPool?.Shutdown();
		}
	}
} // end of namespace
