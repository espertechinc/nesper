///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
        private string propertyName;
        private Type propertyType;
        private bool isRequiresIndex;
        private bool isRequiresMapkey;
        private bool isIndexed;
        private bool isMapped;
        private bool isFragment;
        private Type componentType;

        public SupportEventPropDesc(
            string name,
            Type type)
        {
            propertyName = name;
            propertyType = type;
            Presets();
        }

        private void Presets()
        {
            if (propertyType == null) {
                return;
            }

            var propertyClass = propertyType;
            if (propertyClass.IsArray) {
                WithIndexed().WithComponentType(propertyClass.GetComponentType());
            }

            if (propertyClass.IsGenericDictionary()) {
                WithIndexed().WithComponentType(propertyType.GetDictionaryValueType());
            }

            if (propertyClass.IsGenericEnumerable()) {
                WithIndexed().WithComponentType(propertyType.GetComponentType());
            }
        }

        public string PropertyName => propertyName;

        public Type PropertyType => propertyType;

        public bool IsRequiresIndex => isRequiresIndex;

        public bool IsRequiresMapkey => isRequiresMapkey;

        public bool IsIndexed => isIndexed;

        public bool IsMapped => isMapped;

        public bool IsFragment => isFragment;

        public Type ComponentType => componentType;

        public SupportEventPropDesc WithMapped()
        {
            isMapped = true;
            return this;
        }

        public SupportEventPropDesc WithMapped(bool flag)
        {
            isMapped = flag;
            return this;
        }

        public SupportEventPropDesc WithMappedRequiresKey()
        {
            isMapped = true;
            isRequiresMapkey = true;
            return this;
        }

        public SupportEventPropDesc WithIndexed()
        {
            isIndexed = true;
            return this;
        }

        public SupportEventPropDesc WithIndexed(bool flag)
        {
            isIndexed = flag;
            return this;
        }

        public SupportEventPropDesc WithIndexedRequiresIndex()
        {
            isIndexed = true;
            isRequiresIndex = true;
            return this;
        }

        public SupportEventPropDesc WithComponentType(Type componentType)
        {
            this.componentType = componentType;
            return this;
        }

        public SupportEventPropDesc WithFragment()
        {
            isFragment = true;
            return this;
        }

        public SupportEventPropDesc WithFragment(bool flag)
        {
            isFragment = flag;
            return this;
        }
    }
} // end of namespace