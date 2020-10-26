///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Reflection;

namespace com.espertech.esper.compiler.@internal.util
{
	public class CompilableItemPostCompileLatchDefault : CompilableItemPostCompileLatch
	{
		public readonly static CompilableItemPostCompileLatchDefault INSTANCE = new CompilableItemPostCompileLatchDefault();

		private CompilableItemPostCompileLatchDefault()
		{
		}

		public void AwaitAndRun()
		{
		}

		public void Completed(ICollection<Assembly> assemblies)
		{
		}
	}
} // end of namespace
