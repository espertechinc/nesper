///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compat.threading;

namespace com.espertech.esper.support.util
{
    /// <summary>
    /// Singleton class for testing out multi-threaded code. Allows reservation and
    /// de-reservation of any Object. Reserved objects are added to a HashSet and removed
    /// from the HashSet upon de-reservation.
    /// </summary>
    public class ObjectReservationSingleton
    {
        private HashSet<Object> reservedObjects = new HashSet<Object>();
        private ILockable reservedIdsLock = LockManager.CreateLock(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    
        private static ObjectReservationSingleton ourInstance = new ObjectReservationSingleton();

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
            reservedIdsLock.Call(() => rvalue = !reservedObjects.Add(@object));
            return rvalue;
        }

        /// <summary>
        /// Unreserve an object. Logs a fatal error if the unreserve failed.
        /// </summary>
        /// <param name="object">object to unreserve</param>
        public void Unreserve(Object @object)
        {
            bool wasRemoved;
            using (reservedIdsLock.Acquire()) {
                wasRemoved = reservedObjects.Remove(@object);
            }

            if (!wasRemoved) {
                Log.Fatal(".unreserve FAILED, object=" + @object);
                return;
            }
        }

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
