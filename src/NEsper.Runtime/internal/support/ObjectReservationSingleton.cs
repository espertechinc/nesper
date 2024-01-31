///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Reflection;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compat.threading.locks;

namespace com.espertech.esper.runtime.@internal.support
{
    /// <summary>
    ///     Singleton class for testing out multi-threaded code.
    ///     Allows reservation and de-reservation of any Object. Reserved objects are added to a HashSet and
    ///     removed from the HashSet upon de-reservation.
    /// </summary>
    public class ObjectReservationSingleton
    {
        private static readonly ObjectReservationSingleton INSTANCE = new ObjectReservationSingleton();

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly ILockable reservedIdsLock;

        private readonly HashSet<object> reservedObjects;


        private ObjectReservationSingleton()
        {
            reservedObjects = new HashSet<object>();
            reservedIdsLock = new MonitorSlimLock(LockConstants.DefaultTimeout);
        }

        public static ObjectReservationSingleton GetInstance()
        {
            return INSTANCE;
        }

        /// <summary>
        ///     Reserve an object, returning true when successfully reserved or false when the object is already reserved.
        /// </summary>
        /// <param name="object">object to reserve</param>
        /// <returns>true if reserved, false to indicate already reserved</returns>
        public bool Reserve(object @object)
        {
            using (reservedIdsLock.Acquire()) {
                if (reservedObjects.Contains(@object)) {
                    return false;
                }

                reservedObjects.Add(@object);
                return true;
            }
        }

        /// <summary>
        ///     Unreserve an object. Logs a fatal error if the unreserve failed.
        /// </summary>
        /// <param name="object">object to unreserve</param>
        public void Unreserve(object @object)
        {
            using (reservedIdsLock.Acquire()) {
                if (!reservedObjects.Contains(@object)) {
                    Log.Error(".unreserve FAILED, object=" + @object);
                    return;
                }

                reservedObjects.Remove(@object);
            }
        }
    }
} // end of namespace