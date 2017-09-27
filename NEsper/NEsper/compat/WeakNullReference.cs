///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.compat
{
	// Provides a weak reference to a null target object, which, unlike
	// other weak references, is always considered to be alive. This
	// facilitates handling null dictionary values, which are perfectly
	// legal.
	internal class WeakNullReference<T> : WeakReference<T> where T : class
	{
		public static readonly WeakNullReference<T> Singleton = new WeakNullReference<T>();

		private WeakNullReference() : base( null ) { }

		public override bool IsAlive
		{
			get { return true; }
		}
	}
}
