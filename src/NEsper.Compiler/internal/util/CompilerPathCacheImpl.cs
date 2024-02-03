///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Concurrent;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.util;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compiler.client.option;

namespace com.espertech.esper.compiler.@internal.util
{
	public class CompilerPathCacheImpl : CompilerPathCache
	{
		private readonly IDictionary<EPCompiled, EPCompilerPathableImpl> pathables =
			new ConcurrentDictionary<EPCompiled, EPCompilerPathableImpl>();

		public void Put(
			EPCompiled compiled,
			EPCompilerPathableImpl pathable)
		{
			pathables.Put(compiled, pathable);
		}

		public EPCompilerPathableImpl Get(EPCompiled unit)
		{
			return pathables.Get(unit);
		}
	}
} // end of namespace
