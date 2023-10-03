///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.compile.stage3;

namespace com.espertech.esper.compiler.client.option
{
	/// <summary>
	/// Provides the environment to <seealso cref="AccessModifierInlinedClassContext" />.
	/// </summary>
	public class AccessModifierInlinedClassContext : StatementOptionContextBase
	{
		private readonly string inlinedClassName;

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="base">statement info</param>
		/// <param name="inlinedClassName">returns the name of the inlined class</param>
		public AccessModifierInlinedClassContext(
			StatementBaseInfo @base,
			string inlinedClassName) : base(@base)
		{
			this.inlinedClassName = inlinedClassName;
		}

		/// <summary>
		/// Returns the inlined-class name
		/// </summary>
		/// <value>the inlined-class name</value>
		public string InlinedClassName => inlinedClassName;
	}
} // end of namespace
