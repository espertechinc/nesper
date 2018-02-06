///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

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

        private static readonly ObjectReservationSingleton ourInstance = new ObjectReservationSingleton();

        private readonly HashSet<Object> _reservedObjects = new HashSet<Object>();
        private readonly ILockable _reservedIdsLock = LockManager.CreateLock(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    
        public static ObjectReservationSingleton Instance
        {
            get { return ourInstance; }
        }

        private ObjectReservationSingleton()
        {
        }
    
        /// <summary>
        /// Reserve an object, returning true when successfully reserved or false when the
        /// object is already reserved.
        /// </summary>
        /// <param name="object">object to reserve</param>
        /// <returns>
        /// true if reserved, false to indicate already reserved
        /// </returns>
        public bool Reserve(Object @object)
        {
            bool rvalue = false;
            _reservedIdsLock.Call(() => rvalue = !_reservedObjects.Add(@object));
            return rvalue;
        }

        /// <summary>
        /// Unreserve an object. Logs a fatal error if the unreserve failed.
        /// </summary>
        /// <param name="object">object to unreserve</param>
        public void Unreserve(Object @object)
        {
            bool wasRemoved;
            using (_reservedIdsLock.Acquire()) {
                wasRemoved = _reservedObjects.Remove(@object);
            }

            if (!wasRemoved) {
                Log.Error(".unreserve FAILED, object=" + @object);
                return;
            }
        }
    }
}
