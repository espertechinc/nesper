///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Concurrent;
using System.Threading;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.configuration.compiler;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.concurrency;
using com.espertech.esper.compiler.client;

namespace com.espertech.esper.compiler.@internal.util
{
	public class CompilerPool
	{
		private readonly ModuleCompileTimeServices compileTimeServices;
		private readonly ConcurrentDictionary<string, byte[]> moduleBytes;

		private IExecutorService compilerThreadPool;
		private IFuture<CompilableItemResult>[] futures;
		private Semaphore semaphore;

		CompilerPool(
			int size,
			ModuleCompileTimeServices compileTimeServices,
			ConcurrentDictionary<string, byte[]> moduleBytes)
		{
			this.compileTimeServices = compileTimeServices;
			this.moduleBytes = moduleBytes;

			ConfigurationCompilerByteCode config = compileTimeServices.Configuration.Compiler.ByteCode;
			int numThreads = config.ThreadPoolCompilerNumThreads;
			if (numThreads > 0 && size > 1) {
				compilerThreadPool = Executors.NewFixedThreadPool(numThreads);
				futures = new IFuture<CompilableItemResult>[size];

				int? capacity = config.ThreadPoolCompilerCapacity;
				semaphore = new Semaphore(capacity == null ? int.MaxValue : Math.Max(1, capacity));
			}
		}

		void Submit(
			int statementNumber,
			CompilableItem item)
		{
			// We are adding all class-provided classes to the output.
			// Later we remove the create-class classes.
			moduleBytes.PutAll(item.ClassesProvided);

			// no thread pool, compile right there
			if (compilerThreadPool == null) {
				try {
					var compiler = new Rosy
					
					foreach (CodegenClass clazz in item.Classes) {
						JaninoCompiler.Compile(clazz, moduleBytes, moduleBytes, compileTimeServices);
					}
				}
				finally {
					item.PostCompileLatch.Completed(moduleBytes);
				}

				return;
			}

			CompileCallable callable = new CompileCallable(item, compileTimeServices, semaphore, moduleBytes);
			semaphore.Acquire();
			futures[statementNumber] = compilerThreadPool.Submit(callable);
		}

		void ShutdownCollectResults()
		{
			if (compilerThreadPool == null) {
				return;
			}

			compilerThreadPool.Shutdown();
			compilerThreadPool.AwaitTermination(TimeSpan.MaxValue);

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

				if (result.Exception != null) {
					throw new EPCompileException(result.Exception.Message, result.Exception);
				}
			}
		}

		public void Shutdown()
		{
			if (compilerThreadPool == null) {
				return;
			}

			compilerThreadPool.Shutdown();
		}
	}
} // end of namespace
