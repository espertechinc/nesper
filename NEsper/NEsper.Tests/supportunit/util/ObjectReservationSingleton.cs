///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.compat.container;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compat.threading;

namespace com.espertech.esper.supportunit.util
{
    /// <summary>
    /// Singleton class for testing out multi-threaded code. Allows reservation and
    /// de-reservation of any Object. Reserved objects are added to a HashSet and removed
    /// from the HashSet upon de-reservation.
    /// </summary>
    public class ObjectReservationSingleton
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly ObjectReservationSingleton OurInstance = new ObjectReservationSingleton();

        private readonly HashSet<object> _reservedObjects;
        private readonly ILockable _reservedIdsLock;

        public static ObjectReservationSingleton Instance { get; } = new ObjectReservationSingleton();

        private ObjectReservationSingleton()
        {
            _reservedObjects = new HashSet<object>();
            _reservedIdsLock = SupportContainer.Instance.LockManager().CreateLock(GetType());
        }

        /// <summary>
        /// Reserve an object, returning true when successfully reserved or false when the
        /// object is already reserved.
        /// </summary>
        /// <param name="object">object to reserve</param>
        /// <returns>
        /// true if reserved, false to indicate already reserved
        /// </returns>
        public bool Reserve(object @object)
        {
            var rvalue = _reservedIdsLock.Call(() => {
#if DEBUG && DIAGNOSTIC
                Log.Info("Reserved / Value = {0} / {1}", @object, _reservedObjects.Count);
#endif
                return _reservedObjects.Add(@object);
            });

#if DEBUG && DIAGNOSTIC
            Log.Info("Reserved / Result = {0} / {1}", @object, rvalue);
#endif
            return rvalue;
        }

        /// <summary>
        /// Unreserve an object. Logs a fatal error if the unreserve failed.
        /// </summary>
        /// <param name="object">object to unreserve</param>
        public void Unreserve(object @object)
        {
            bool wasRemoved;
            using (_reservedIdsLock.Acquire()) {
                wasRemoved = _reservedObjects.Remove(@object);
            }

            if (!wasRemoved) {
                Log.Error(".Unreserve[FAILED]: object={0}", @object);
                return;
            }

#if DEBUG && DIAGNOSTIC
            Log.Info(".Unreserve: object = {0}", @object);
#endif
        }
    }
}
