///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.@event.core;

namespace com.espertech.esper.common.@internal.@event.variant
{
    /// <summary>
    ///     A property resolution strategy that allows any type, wherein all properties are Object type.
    /// </summary>
    public class VariantPropResolutionStrategyAny : VariantPropResolutionStrategy
    {
        private readonly VariantEventType _variantEventType;

        public VariantPropResolutionStrategyAny(VariantEventType variantEventType)
        {
            this._variantEventType = variantEventType;
        }

        public VariantPropertyDesc ResolveProperty(
            string propertyName,
            EventType[] variants)
        {
            // property numbers should start at zero since the serve as array index
            var propertyGetterCache = _variantEventType.VariantPropertyGetterCache;
            propertyGetterCache.AddGetters(propertyName);
            EventPropertyGetterSPI getter = new VariantEventPropertyGetterAny(_variantEventType, propertyName);
            return new VariantPropertyDesc(typeof(object), getter, true);
        }
    }
} // end of namespace