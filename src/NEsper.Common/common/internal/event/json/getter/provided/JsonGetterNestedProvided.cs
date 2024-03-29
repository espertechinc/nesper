///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
    /// <summary>
    ///     Property getter for Json underlying fields.
    /// </summary>
    public sealed class JsonGetterNestedProvided : JsonGetterNestedBase
    {
        private readonly FieldInfo _field;

        public JsonGetterNestedProvided(
            JsonEventPropertyGetter innerGetter,
            string underlyingClassName,
            FieldInfo field) : base(innerGetter, underlyingClassName)
        {
            _field = field;
        }

        public override string FieldName => _field.Name;

        public override Type FieldType => _field.FieldType;

        public override object GetJsonProp(object @object)
        {
            var value = JsonFieldGetterHelperProvided.GetJsonProvidedSimpleProp(@object, _field);
            if (value == null) {
                return null;
            }

            return InnerGetter.GetJsonProp(value);
        }

        public override bool GetJsonExists(object @object)
        {
            var value = JsonFieldGetterHelperProvided.GetJsonProvidedSimpleProp(@object, _field);
            if (value == null) {
                return false;
            }

            return InnerGetter.GetJsonExists(value);
        }

        public override object GetJsonFragment(object @object)
        {
            var value = JsonFieldGetterHelperProvided.GetJsonProvidedSimpleProp(@object, _field);
            if (value == null) {
                return null;
            }

            return InnerGetter.GetJsonFragment(value);
        }
    }
} // end of namespace