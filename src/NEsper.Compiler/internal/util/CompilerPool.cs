///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.common.client.configuration.compiler;
using com.espertech.esper.common.@internal.compile.compiler;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.compat.concurrency;
using com.espertech.esper.compat.threading;
using com.espertech.esper.compiler.client;
using com.espertech.esper.compiler.client.util;

namespace com.espertech.esper.compiler.@internal.util
{
	class CompilerPool
	{
		private readonly ModuleCompileTimeServices _compileTimeServices;
		private readonly IList<EPCompiled> _path;
		private readonly CompilerAbstraction _compilerAbstraction;
		private readonly CompilerAbstractionArtifactCollection _compilationState;

		private readonly IExecutorService _compilerThreadPool;
		private readonly IFuture<CompilableItemResult>[] _futures;
		private readonly SemaphoreSlim _semaphore;

		internal CompilerPool(
			int size,
			ModuleCompileTimeServices compileTimeServices,
			IList<EPCompiled> path,
			CompilerAbstraction compilerAbstraction,
			CompilerAbstractionArtifactCollection compilationState,
			CompilerThreadPoolFactory compilerThreadPoolFactory)
		{
			_compileTimeServices = compileTimeServices;
			_path = path;
			_compilerAbstraction = compilerAbstraction;
			_compilationState = compilationState;

			ThreadFactory threadFactory = _ => new Thread(_) {
				IsBackground = true,
				Name = "CompilerPool-Thread-" + _
			};

			var config = compileTimeServices.Configuration.Compiler.ByteCode;
			var numThreads = config.ThreadPoolCompilerNumThreads;
			if (numThreads > 0 && size > 1) {
				_compilerThreadPool = compilerThreadPoolFactory != null 
					? compilerThreadPoolFactory.Invoke(config, threadFactory)
					: Executors.NewFixedThreadPool(numThreads, threadFactory);
				_futures = new IFuture<CompilableItemResult>[size];
				var capacity = config.ThreadPoolCompilerCapacity ?? int.MaxValue;
				_semaphore = new SemaphoreSlim(Math.Max(1, capacity));
			}
		}

		public void Submit(
			int statementNumber,
			CompilableItem item)
		{
			// We are adding all class-provided classes to the output.
			// Later we remove the create-class classes.
			_compilationState.Add(item.ArtifactsProvided.OfType<ICompileArtifact>());

			// no thread pool, compile right there
			if (_compilerThreadPool == null) {
				try {
					var context = new CompilerAbstractionCompilationContext(_compileTimeServices, _path);
					var artifact = _compilerAbstraction.CompileClasses(item.Classes, context, _compilationState);
					_compilationState.Artifacts.Add(artifact);
				}
				finally {
					item.PostCompileLatch.Completed(_compilationState.Artifacts);
				}

				return;
			}

			var callable = new CompileCallable(
				item,
				_compileTimeServices,
				_path,
				_semaphore,
				_compilerAbstraction,
				_compilationState);

			_semaphore.Wait();
			//semaphore.Acquire();
			_futures[statementNumber] = _compilerThreadPool.Submit(callable);
		}

		public void ShutdownCollectResults()
		{
			if (_compilerThreadPool == null) {
				return;
			}

			_compilerThreadPool.Shutdown();
			try {
				_compilerThreadPool.AwaitTermination(TimeSpan.FromSeconds(300)); // TimeSpan.FromSeconds(double.MaxValue)
			}
			catch (ThreadInterruptedException e) {
				throw new EPRuntimeException(e);
			}

			foreach (var future in _futures) {
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
			_compilerThreadPool?.Shutdown();
		}
	}
} // end of namespace
