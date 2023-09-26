///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.@event.json.compiletime;
using com.espertech.esper.common.@internal.@event.json.getter.core;

namespace com.espertech.esper.common.@internal.@event.json.getter.fromschema
{
    /// <summary>
    ///     Property getter for Json underlying fields.
    /// </summary>
    public sealed class JsonGetterMappedSchema : JsonGetterMappedBase
    {
        private readonly JsonUnderlyingField _field;

        public JsonGetterMappedSchema(
            string key,
            string underlyingClassName,
            JsonUnderlyingField field)
            : base(key, underlyingClassName)
        {
            _field = field;
        }

        public override string FieldName => _field.FieldName;

        public override object GetJsonProp(object @object)
        {
            return JsonFieldGetterHelperSchema.GetJsonMappedProp(@object, _field.PropertyNumber, Key);
        }

        public override bool GetJsonExists(object @object)
        {
            return JsonFieldGetterHelperSchema.GetJsonMappedExists(@object, _field.PropertyNumber, Key);
        }
    }
} // end of namespace