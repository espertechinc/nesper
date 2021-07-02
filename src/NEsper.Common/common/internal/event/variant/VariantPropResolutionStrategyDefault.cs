///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.type;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.@event.variant
{
    /// <summary>
    ///     A property resolution strategy that allows only the preconfigured types, wherein all properties
    ///     that are common (name and type) to all properties are considered.
    /// </summary>
    public class VariantPropResolutionStrategyDefault : VariantPropResolutionStrategy
    {
        private readonly VariantEventType _variantEventType;

        public VariantPropResolutionStrategyDefault(VariantEventType variantEventType)
        {
            _variantEventType = variantEventType;
        }

        public VariantPropertyDesc ResolveProperty(
            string propertyName,
            EventType[] variants)
        {
            var existsInAll = true;
            Type commonType = null;
            var mustCoerce = false;
            for (var i = 0; i < variants.Length; i++) {
                var propertyType = variants[i].GetPropertyType(propertyName).GetBoxedType();
                if (propertyType == null) {
                    existsInAll = false;
                    continue;
                }

                if (commonType == null) {
                    commonType = propertyType;
                    continue;
                }

                // compare types
                if (propertyType == commonType) {
                    continue;
                }
                if (commonType.IsNullType()) {
                    continue;
                }

                // coercion
                if (propertyType.IsNumeric()) {
                    if (propertyType.CanCoerce(commonType)) {
                        mustCoerce = true;
                        continue;
                    }

                    if (commonType.CanCoerce(propertyType)) {
                        mustCoerce = true;
                        commonType = propertyType;
                    }
                }
                else if (commonType == typeof(object)) {
                    continue;
                }
                else if (!propertyType.IsBuiltinDataType()) {
                    // common interface or base class
                    ISet<Type> supersForType = new LinkedHashSet<Type>();
                    TypeHelper.GetBase(propertyType, supersForType);
                    supersForType.Remove(typeof(object));

                    if (supersForType.Contains(commonType)) {
                        continue; // type implements or : common type
                    }

                    if (TypeHelper.IsSubclassOrImplementsInterface(commonType, propertyType)) {
                        commonType = propertyType; // common type implements type
                        continue;
                    }

                    // find common interface or type both implement
                    ISet<Type> supersForCommonType = new LinkedHashSet<Type>();
                    TypeHelper.GetBase(commonType, supersForCommonType);
                    supersForCommonType.Remove(typeof(object));

                    // Take common classes first, ignoring interfaces
                    var found = false;
                    foreach (var superClassType in supersForType) {
                        if (!superClassType.IsInterface && supersForCommonType.Contains(superClassType)) {
                            commonType = superClassType;
                            found = true;
                            break;
                        }
                    }

                    if (found) {
                        continue;
                    }

                    // Take common interfaces
                    foreach (var superClassType in supersForType) {
                        if (superClassType.IsInterface && supersForCommonType.Contains(superClassType)) {
                            break;
                        }
                    }
                }
            }

            if (!existsInAll) {
                return null;
            }

            if (commonType == null) {
                return null;
            }

            // property numbers should start at zero since the serve as array index
            var propertyGetterCache = _variantEventType.VariantPropertyGetterCache;
            propertyGetterCache.AddGetters(propertyName);

            EventPropertyGetterSPI getter;
            if (mustCoerce) {
                var caster = SimpleTypeCasterFactory.GetCaster(TypeHelper.NullType, commonType);
                getter = new VariantEventPropertyGetterAnyWCast(_variantEventType, propertyName, caster);
            }
            else {
                getter = new VariantEventPropertyGetterAny(_variantEventType, propertyName);
            }

            return new VariantPropertyDesc(commonType, getter, true);
        }
    }
} // end of namespace