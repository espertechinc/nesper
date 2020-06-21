///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.@event.json.compiletime;
using com.espertech.esper.common.@internal.@event.json.getter.core;

namespace com.espertech.esper.common.@internal.@event.json.getter.fromschema
{
    public sealed class JsonGetterNestedSchema : JsonGetterNestedBase
    {
        private readonly JsonUnderlyingField field;

        public JsonGetterNestedSchema(
            JsonEventPropertyGetter innerGetter,
            string underlyingClassName,
            JsonUnderlyingField field)
            : base(innerGetter, underlyingClassName)
        {
            this.field = field;
        }

        public override string FieldName {
            get { return field.FieldName; }
        }

        public override Type FieldType {
            get { return field.PropertyType; }
        }

        public override object GetJsonProp(object @object)
        {
            var value = JsonFieldGetterHelperSchema.GetJsonSimpleProp(field, @object);
            if (value == null) {
                return null;
            }

            return InnerGetter.GetJsonProp(value);
        }

        public override bool GetJsonExists(object @object)
        {
            var value = JsonFieldGetterHelperSchema.GetJsonSimpleProp(field, @object);
            if (value == null) {
                return false;
            }

            return InnerGetter.GetJsonExists(value);
        }

        public override object GetJsonFragment(object @object)
        {
            var value = JsonFieldGetterHelperSchema.GetJsonSimpleProp(field, @object);
            if (value == null) {
                return null;
            }

            return InnerGetter.GetJsonFragment(value);
        }
    }
} // end of namespace