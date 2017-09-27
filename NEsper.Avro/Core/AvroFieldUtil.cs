///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using Avro;

using com.espertech.esper.events.property;

using NEsper.Avro.Extensions;

namespace NEsper.Avro.Core
{
    public class AvroFieldUtil
    {
        public static AvroFieldDescriptor FieldForProperty(Schema fieldSchema, Property property)
        {
            if (property is SimpleProperty)
            {
                Field field = fieldSchema.GetField(property.PropertyNameAtomic);
                if (field == null)
                {
                    return null;
                }
                return new AvroFieldDescriptor(field, false, false, false);
            }
            else if (property is IndexedProperty)
            {
                Field field = fieldSchema.GetField(property.PropertyNameAtomic);
                if (field == null)
                {
                    return null;
                }

                switch(field.Schema.Tag)
                {
                    case Schema.Type.Array:
                    case Schema.Type.String:
                        return new AvroFieldDescriptor(field, false, true, false);
                    default:
                        return null;
                }
            }
            else if (property is MappedProperty)
            {
                Field field = fieldSchema.GetField(property.PropertyNameAtomic);
                if (field == null || field.Schema.Tag != Schema.Type.Map)
                {
                    return null;
                }
                return new AvroFieldDescriptor(field, false, false, true);
            }
            else if (property is DynamicProperty)
            {
                Field field = fieldSchema.GetField(property.PropertyNameAtomic);
                return new AvroFieldDescriptor(
                    field, true, property is DynamicIndexedProperty, property is DynamicMappedProperty);
            }

            var nested = (NestedProperty) property;
            var current = fieldSchema;
            Field currentField = null;
            var dynamic = false;
            for (var index = 0; index < nested.Properties.Count; index++)
            {
                var levelProperty = nested.Properties[index];
                if (levelProperty is SimpleProperty)
                {
                    if (current.Tag != Schema.Type.Record)
                    {
                        return null;
                    }
                    currentField = current.GetField(levelProperty.PropertyNameAtomic);
                    if (currentField == null)
                    {
                        return null;
                    }
                    current = currentField.Schema;
                }
                else if (levelProperty is IndexedProperty)
                {
                    if (current.Tag != Schema.Type.Record)
                    {
                        return null;
                    }
                    currentField = current.GetField(levelProperty.PropertyNameAtomic);
                    if (currentField == null || currentField.Schema.Tag != Schema.Type.Array)
                    {
                        return null;
                    }
                    current = currentField.Schema.GetElementType();
                }
                else if (levelProperty is MappedProperty)
                {
                    if (current.Tag != Schema.Type.Record)
                    {
                        return null;
                    }
                    currentField = current.GetField(levelProperty.PropertyNameAtomic);
                    if (currentField == null || currentField.Schema.Tag != Schema.Type.Map)
                    {
                        return null;
                    }
                    current = currentField.Schema.GetValueType();
                }
                else if (levelProperty is DynamicProperty)
                {
                    dynamic = true;
                    currentField = fieldSchema.GetField(levelProperty.PropertyNameAtomic);
                    if (currentField == null)
                    {
                        return new AvroFieldDescriptor(
                            null, true, levelProperty is DynamicIndexedProperty, levelProperty is DynamicMappedProperty);
                    }
                    current = currentField.Schema;
                }
            }
            var lastProperty = nested.Properties[nested.Properties.Count - 1];
            return new AvroFieldDescriptor(
                currentField, dynamic, lastProperty is PropertyWithIndex, lastProperty is PropertyWithKey);
        }
    }
} // end of namespace
