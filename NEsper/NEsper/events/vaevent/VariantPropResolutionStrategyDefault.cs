///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.util;

namespace com.espertech.esper.events.vaevent
{
    /// <summary>
    /// A property resolution strategy that allows only the preconfigured types, wherein
    /// all properties that are common (name and type) to all properties are considered.
    /// </summary>
    public class VariantPropResolutionStrategyDefault : VariantPropResolutionStrategy
    {
        private int currentPropertyNumber;
        private readonly VariantPropertyGetterCache propertyGetterCache;
    
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="variantSpec">specified the preconfigured types</param>
        public VariantPropResolutionStrategyDefault(VariantSpec variantSpec)
        {
            propertyGetterCache = new VariantPropertyGetterCache(variantSpec.EventTypes);
        }
    
        public VariantPropertyDesc ResolveProperty(String propertyName, EventType[] variants)
        {
            bool existsInAll = true;
            Type commonType = null;
            bool mustCoerce = false;
            for (int i = 0; i < variants.Length; i++)
            {
                Type type = variants[i].GetPropertyType(propertyName); //.GetBoxedType();
                if (type == null)
                {
                    existsInAll = false;
                    continue;
                }
    
                if (commonType == null)
                {
                    commonType = type;
                    continue;
                }
    
                // compare types
                if (type == commonType)
                {
                    continue;
                }

                if (type.GetBoxedType() == commonType.GetBoxedType())
                {
                    commonType = commonType.GetBoxedType();
                    continue;
                }
    
                // coercion
                if (type.IsNumeric())
                {
                    if (TypeHelper.CanCoerce(type, commonType))
                    {
                        mustCoerce = true;
                        continue;
                    }
                    if (TypeHelper.CanCoerce(commonType, type))
                    {
                        mustCoerce = true;
                        commonType = type;
                    }
                }
                else if (commonType == typeof(Object))
                {
                    continue;
                }
                // common interface or base class
                else if (!type.IsBuiltinDataType())
                {
                    var supersForType = new FIFOHashSet<Type>();
                    TypeHelper.GetBase(type, supersForType);
                    supersForType.Remove(typeof(Object));
    
                    if (supersForType.Contains(commonType))
                    {
                        continue;   // type, or : common type
                    }
                    if (TypeHelper.IsSubclassOrImplementsInterface(commonType, type))
                    {
                        commonType = type;  // common type : type
                        continue;
                    }
    
                    // find common interface or type both implement
                    var supersForCommonType = new FIFOHashSet<Type>();
                    TypeHelper.GetBase(commonType, supersForCommonType);
                    supersForCommonType.Remove(typeof(Object));
    
                    // Take common classes first, ignoring interfaces
                    bool found = false;
                    foreach (Type superClassType in supersForType)
                    {
                        if (!superClassType.IsInterface && (supersForCommonType.Contains(superClassType)))
                        {
                            break;
                        }
                    }
                    if (found)
                    {
                        continue;
                    }
                    // Take common interfaces
                    foreach (var superClassType in supersForType)
                    {
                        if (superClassType.IsInterface && supersForCommonType.Contains(superClassType))
                        {
                            commonType = superClassType;
                            found = true;
                            break;
                        }
                    }
                }
    
                commonType = typeof(Object);
            }
    
            if (!existsInAll)
            {
                return null;
            }
    
            if (commonType == null)
            {
                return null;
            }
    
            // property numbers should start at zero since the serve as array index
            var assignedPropertyNumber = currentPropertyNumber;
            currentPropertyNumber++;
            propertyGetterCache.AddGetters(assignedPropertyNumber, propertyName);
    
            EventPropertyGetter getter;
            if (mustCoerce)
            {
                SimpleTypeCaster caster = SimpleTypeCasterFactory.GetCaster(null, commonType);
                getter = new ProxyEventPropertyGetter
                {
                    ProcGet = eventBean =>
                    {
                        var variant = (VariantEvent)eventBean;
                        var propertyGetter = propertyGetterCache.GetGetter(assignedPropertyNumber, variant.UnderlyingEventBean.EventType);
                        if (propertyGetter == null)
                        {
                            return null;
                        }
                        var value = propertyGetter.Get(variant.UnderlyingEventBean);
                        if (value == null)
                        {
                            return value;
                        }
                        return caster.Invoke(value);
                    },
                    ProcGetFragment = eventBean => null,
                    ProcIsExistsProperty = eventBean =>
                    {
                        var variant = (VariantEvent)eventBean;
                        var propertyGetter = propertyGetterCache.GetGetter(assignedPropertyNumber, variant.UnderlyingEventBean.EventType);
                        if (propertyGetter == null)
                        {
                            return false;
                        }
                        return propertyGetter.IsExistsProperty(variant.UnderlyingEventBean);
                    }
                };
            }
            else {
                getter = new ProxyEventPropertyGetter
                {
                    ProcGet = eventBean =>
                    {
                        var variant = (VariantEvent) eventBean;
                        var propertyGetter = propertyGetterCache.GetGetter(assignedPropertyNumber, variant.UnderlyingEventBean.EventType);
                        if (propertyGetter == null)
                        {
                            return null;
                        }
                        return propertyGetter.Get(variant.UnderlyingEventBean);
                    },
                    ProcGetFragment = eventBean => null,
                    ProcIsExistsProperty = eventBean =>
                    {
                        var variant = (VariantEvent) eventBean;
                        var propertyGetter = propertyGetterCache.GetGetter(assignedPropertyNumber, variant.UnderlyingEventBean.EventType);
                        if (propertyGetter == null)
                        {
                            return false;
                        }
                        return propertyGetter.IsExistsProperty(variant.UnderlyingEventBean);
                    }
                };
            }

            return new VariantPropertyDesc(commonType, getter, true);
        }
    }
}
