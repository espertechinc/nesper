///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.support
{
    public class SupportEventPropDesc
    {
        private readonly string _propertyName;
        private readonly Type _propertyType;
        private bool _isRequiresIndex;
        private bool _isRequiresMapkey;
        private bool _isIndexed;
        private bool _isMapped;
        private bool _isFragment;
        private Type _componentType;

        public SupportEventPropDesc(
            string name,
            Type type)
        {
            _propertyName = name;
            _propertyType = type;
            Presets();
        }

        private void Presets()
        {
            if (_propertyType == null) {
                return;
            }

            var propertyClass = _propertyType;
            if (propertyClass.IsArray) {
                WithIndexed().WithComponentType(propertyClass.GetComponentType());
            }

            if (propertyClass.IsGenericDictionary()) {
                WithMapped().WithComponentType(_propertyType.GetDictionaryValueType());
            }
            else if (propertyClass.IsGenericEnumerable()) {
                WithIndexed().WithComponentType(_propertyType.GetComponentType());
            }
        }

        public string PropertyName => _propertyName;

        public Type PropertyType => _propertyType;

        public bool IsRequiresIndex => _isRequiresIndex;

        public bool IsRequiresMapkey => _isRequiresMapkey;

        public bool IsIndexed => _isIndexed;

        public bool IsMapped => _isMapped;

        public bool IsFragment => _isFragment;

        public Type ComponentType => _componentType;

        public SupportEventPropDesc WithMapped()
        {
            _isMapped = true;
            return this;
        }

        public SupportEventPropDesc WithMapped(bool flag)
        {
            _isMapped = flag;
            return this;
        }

        public SupportEventPropDesc WithMappedRequiresKey()
        {
            _isMapped = true;
            _isRequiresMapkey = true;
            return this;
        }

        public SupportEventPropDesc WithIndexed()
        {
            _isIndexed = true;
            return this;
        }

        public SupportEventPropDesc WithIndexed(bool flag)
        {
            _isIndexed = flag;
            return this;
        }

        public SupportEventPropDesc WithIndexedRequiresIndex()
        {
            _isIndexed = true;
            _isRequiresIndex = true;
            return this;
        }

        public SupportEventPropDesc WithComponentType(Type componentType)
        {
            _componentType = componentType;
            return this;
        }

        public SupportEventPropDesc WithComponentType<T>()
        {
            _componentType = typeof(T);
            return this;
        }

        public SupportEventPropDesc WithFragment()
        {
            _isFragment = true;
            return this;
        }

        public SupportEventPropDesc WithFragment(bool flag)
        {
            _isFragment = flag;
            return this;
        }
    }
} // end of namespace