///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

using com.espertech.esper.common.@internal.@event.json.getter.core;

namespace com.espertech.esper.common.@internal.@event.json.getter.provided
{
    public sealed class JsonGetterNestedArrayIndexedProvided : JsonGetterNestedArrayIndexedBase
    {
        private readonly FieldInfo _field;

        public JsonGetterNestedArrayIndexedProvided(
            int index,
            JsonEventPropertyGetter innerGetter,
            string underlyingClassName,
            FieldInfo field) : base(index, innerGetter, underlyingClassName)
        {
            this._field = field;
        }

        public override string FieldName => _field.Name;

        public override Type FieldType => _field.FieldType;

        public override object GetJsonProp(object @object)
        {
            var item = JsonFieldGetterHelperProvided.GetJsonProvidedIndexedProp(@object, _field, Index);
            if (item == null) {
                return null;
            }

            return InnerGetter.GetJsonProp(item);
        }

        public override bool GetJsonExists(object @object)
        {
            var item = JsonFieldGetterHelperProvided.GetJsonProvidedIndexedProp(@object, _field, Index);
            if (item == null) {
                return false;
            }

            return InnerGetter.GetJsonExists(item);
        }

        public override object GetJsonFragment(object @object)
        {
            var item = JsonFieldGetterHelperProvided.GetJsonProvidedIndexedProp(@object, _field, Index);
            if (item == null) {
                return null;
            }

            return InnerGetter.GetJsonFragment(item);
        }
    }
} // end of namespace