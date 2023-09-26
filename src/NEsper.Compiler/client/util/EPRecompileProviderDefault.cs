///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.hook.recompile;
using com.espertech.esper.common.client.module;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.compiler.client.util
{
	public class EPRecompileProviderDefault : EPRecompileProvider
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public EPCompiled Provide(EPRecompileProviderContext env)
		{
			string epl = (string)env.ModuleProperties.Get(ModuleProperty.MODULETEXT);
			if (epl == null) {
				throw new EPRecompileProviderException("EPL not part of module properties");
			}

			Log.Info("Recompiling EPL: " + epl);

			CompilerArguments args = new CompilerArguments(env.Configuration);
			args.Path.AddAll(env.Path);

			try {
				return EPCompilerProvider.Compiler.Compile(epl, args);
			}
			catch (EPCompileException ex) {
				throw new EPRecompileProviderException("Failed to recompile epl '" + epl + "': " + ex.Message, ex);
			}
		}
	}
} // end of namespace
