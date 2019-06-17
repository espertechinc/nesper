///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


namespace com.espertech.esper.compat
{
	/// <summary>
	/// Provides a weak reference to an object of the given type to be used in
	/// a WeakDictionary along with the given comparer.
    /// </summary>
	internal sealed class WeakKeyReference<T> : WeakReference<T> where T : class
	{
		public readonly int HashCode;

		public WeakKeyReference( T key, WeakKeyComparer<T> comparer )
			: base( key )
		{
			// retain the object's hash code immediately so that even
			// if the target is GC'ed we will be able to find and
			// remove the dead weak reference.
			this.HashCode = comparer.GetHashCode( key );
		}
	}
}
