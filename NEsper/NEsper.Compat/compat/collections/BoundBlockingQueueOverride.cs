///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.compat.collections
{
    internal sealed class BoundBlockingQueueOverride
    {
        internal static BoundBlockingQueueOverride Default;

        /// <summary>
        /// Gets a value indicating whether the override is engaged.
        /// </summary>
        /// <value><c>true</c> if the override is engaged; otherwise, <c>false</c>.</value>
        internal static bool IsEngaged => ScopedInstance<BoundBlockingQueueOverride>.Current != null;

        /// <summary>
        /// Initializes the <see cref="BoundBlockingQueueOverride"/> class.
        /// </summary>
        static BoundBlockingQueueOverride()
        {
            Default = new BoundBlockingQueueOverride();
        }
    }
}
