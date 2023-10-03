///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.compiler.@internal.util;

namespace com.espertech.esper.compiler.client.option
{
	/// <summary>
	/// For optional use with the compiler, for speeding up compilation when the compiler path has one or more
	/// <seealso cref="EPCompiled" /> instances that provide visible EPL objects (named windows, tables etc.),
	/// this cache retains and helps reuse the information in respect to each <seealso cref="EPCompiled" />
	/// instance and the visible EPL objects it provides.
	/// 
	/// <para>
	///		The compiler is a stateless service and does not retain information between invocations.
	/// </para>
	/// <para>
	///		The compiler uses the cache, when provided, for any <seealso cref="EPCompiled" /> instances in the compiler
	///		path to determine the visible EPL objects for that <seealso cref="EPCompiled" />. Thus the compiler does not
	///		need to perform any classloading or initialization of EPL objects for the <seealso cref="EPCompiled" /> thus
	///		reducing compilation time when there is a compiler path with <seealso cref="EPCompiled" /> instances in the
	///		path.
	/// </para>
	/// <para>
	///		The compiler, upon successful compilation of an EPL module (not a fire-and-forget query), populates the
	///		cache with the output <seealso cref="EPCompiled" /> and its EPL objects.
	/// </para>
	/// <para>
	///		The compiler, upon successful loading of an <seealso cref="EPCompiled" /> from the compiler path, populates
	///		the cache with the loaded <seealso cref="EPCompiled" /> and its EPL objects.
	/// </para>
	/// <para>
	///		Alternatively an application can deploy to a runtime and use the runtime path.
	/// </para>
	/// </summary>
	public class CompilerPathCache
	{
		/// <summary>
		/// Returns a cache that keeps a synchronized map of <seealso cref="EPCompiled" /> to EPL objects
		/// </summary>
		/// <returns>cache</returns>
		public static CompilerPathCache GetInstance()
		{
			return new CompilerPathCacheImpl();
		}
	}
} // end of namespace
