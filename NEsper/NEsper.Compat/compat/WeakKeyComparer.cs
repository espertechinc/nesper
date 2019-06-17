///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System.Collections.Generic;

namespace com.espertech.esper.compat
{
	// Compares objects of the given type or WeakKeyReferences to them
	// for equality based on the given comparer. Note that we can only
	// implement IEqualityComparer<T> for T = object as there is no
	// other common base between T and WeakKeyReference<T>. We need a
	// single comparer to handle both types because we don't want to
	// allocate a new weak reference for every lookup.
	internal sealed class WeakKeyComparer<T> : IEqualityComparer<object>
		where T : class
	{
		private IEqualityComparer<T> comparer;

		internal WeakKeyComparer( IEqualityComparer<T> comparer )
		{
			if ( comparer == null )
				comparer = EqualityComparer<T>.Default;

			this.comparer = comparer;
		}

		public int GetHashCode( object obj )
		{
			WeakKeyReference<T> weakKey = obj as WeakKeyReference<T>;
			if ( weakKey != null ) return weakKey.HashCode;
			return this.comparer.GetHashCode( (T) obj );
		}

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"></see> is equal to the current <see cref="T:System.Object"></see>.
        /// </summary>
        /// <param name="x">The first object of type T to compare.</param>
        /// <param name="y">The second object of type T to compare.</param>
        /// <returns>
        /// true if the specified <see cref="T:System.Object"></see> is equal to the current <see cref="T:System.Object"></see>; otherwise, false.
        /// </returns>
        /// <remark>
        /// Note: There are actually 9 cases to handle here.
        /// Let Wa = Alive Weak Reference
        /// Let Wd = Dead Weak Reference
        /// Let S  = Strong Reference
        /// x  | y  | Equals(x,y)
        /// -------------------------------------------------
        /// Wa | Wa | comparer.Equals(x.Target, y.Target)
        /// Wa | Wd | false
        /// Wa | S  | comparer.Equals(x.Target, y)
        /// Wd | Wa | false
        /// Wd | Wd | x == y
        /// Wd | S  | false
        /// S  | Wa | comparer.Equals(x, y.Target)
        /// S  | Wd | false
        /// S  | S  | comparer.Equals(x, y)
        /// -------------------------------------------------
        /// </remark>
		public new bool Equals( object x, object y )
		{
			bool xIsDead, yIsDead;
			T first = GetTarget( x, out xIsDead );
			T second = GetTarget( y, out yIsDead );

			if ( xIsDead )
				return yIsDead ? x == y : false;

			if ( yIsDead )
				return false;

			return this.comparer.Equals( first, second );
		}

		private static T GetTarget( object obj, out bool isDead )
		{
			WeakKeyReference<T> wref = obj as WeakKeyReference<T>;
			T target;
			if ( wref != null )
			{
				target = wref.Target;
				isDead = !wref.IsAlive;
			}
			else
			{
				target = (T) obj;
				isDead = false;
			}
			return target;
		}
	}
}
