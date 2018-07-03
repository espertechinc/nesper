///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.client;
using com.espertech.esper.compat.threading;

namespace com.espertech.esper.events.vaevent
{
    /// <summary>
    /// A property resolution strategy that allows any type, wherein all properties 
    /// are Object type.
    /// </summary>
    public class VariantPropResolutionStrategyAny : VariantPropResolutionStrategy
    {
        private int _currentPropertyNumber;
        private readonly VariantPropertyGetterCache _propertyGetterCache;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="lockManager">The lock manager.</param>
        /// <param name="variantSpec">specified the preconfigured types</param>
        public VariantPropResolutionStrategyAny(ILockManager lockManager, VariantSpec variantSpec)
        {
            _propertyGetterCache = new VariantPropertyGetterCache(
                lockManager, variantSpec.EventTypes);
        }
    
        public VariantPropertyDesc ResolveProperty(String propertyName, EventType[] variants)
        {
            // property numbers should start at zero since the serve as array index
            int assignedPropertyNumber = _currentPropertyNumber;
            _currentPropertyNumber++;
            _propertyGetterCache.AddGetters(assignedPropertyNumber, propertyName);

            var getter = new VariantEventPropertyGetterAny(_propertyGetterCache, assignedPropertyNumber);
            return new VariantPropertyDesc(typeof(Object), getter, true);
        }
    }
}
