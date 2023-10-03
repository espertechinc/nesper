///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.common.client.artifact;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.compiler.@internal.util
{
	public class CompilableItemPostCompileLatchDefault : CompilableItemPostCompileLatch
	{
		public static readonly CompilableItemPostCompileLatchDefault INSTANCE = new CompilableItemPostCompileLatchDefault();

		private CompilableItemPostCompileLatchDefault()
		{
		}

		public void AwaitAndRun()
		{
		}

		public void Completed(IEnumerable<IArtifact> artifacts)
		{
		}
	}
} // end of namespace
