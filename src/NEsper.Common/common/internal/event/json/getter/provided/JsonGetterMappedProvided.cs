///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Reflection;

using com.espertech.esper.common.@internal.@event.json.getter.core;

namespace com.espertech.esper.common.@internal.@event.json.getter.provided
{
    /// <summary>
    ///     Property getter for Json underlying fields.
    /// </summary>
    public sealed class JsonGetterMappedProvided : JsonGetterMappedBase
    {
        private readonly FieldInfo _field;

        public JsonGetterMappedProvided(
            string key,
            string underlyingClassName,
            FieldInfo field) : base(key, underlyingClassName)
        {
            _field = field;
        }

        public override string FieldName => _field.Name;

        public override object GetJsonProp(object @object)
        {
            return JsonFieldGetterHelperProvided.GetJsonProvidedMappedProp(@object, _field, Key);
        }

        public override bool GetJsonExists(object @object)
        {
            return JsonFieldGetterHelperProvided.GetJsonProvidedMappedExists(@object, _field, Key);
        }
    }
} // end of namespace