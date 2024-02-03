///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
namespace com.espertech.esper.compiler.client.option
{
	/// <summary>
	/// Provides the environment to <seealso cref="CompilerHookOption" />.
	/// </summary>
	public class CompilerHookContext
	{
		private readonly string moduleName;

		/// <summary>
		/// Ctor.
		/// </summary>
		/// <param name="moduleName">module name or null when none provided</param>
		public CompilerHookContext(string moduleName)
		{
			this.moduleName = moduleName;
		}

		/// <summary>
		/// Returns the module name or null when none provided
		/// </summary>
		/// <value>module name or null when none provided</value>
		public string ModuleName => moduleName;
	}
} // end of namespace
