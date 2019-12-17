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
    /// Soft references are like weak references in that they allow an object to go out of scope
    /// when it is not referenced.  Any "hard" reference to an object causes the object to remain
    /// out of the eyes of the GC.  The soft reference splits the difference by marking an object
    /// with a hard reference and using a counter.  When the object is accessed, the reference
    /// count increases and over time decays to zero.  When it decays to zero, the reference
    /// effectively becomes a weak reference and is available to the GC.
    /// </summary>
    public class SoftReference<T> : WeakReference<T> where T : class
    {
        private T _ref;
        private int _refCount;
        private int _refIncrementCount;
        private int _refDecrementCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="SoftReference{T}"/> class.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="incrementCount">The increment count.</param>
        /// <param name="decrementCount">The decrement count.</param>
        public SoftReference(T target, int incrementCount = 20, int decrementCount = 1)
            : base(target)
        {
            _ref = target;
            _refCount = incrementCount;
            _refIncrementCount = incrementCount;
            _refDecrementCount = decrementCount;
        }
    }
}
