///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using Avro;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.parse;
using com.espertech.esper.events;
using com.espertech.esper.events.property;

using NEsper.Avro.Extensions;

namespace NEsper.Avro.Core
{
    public class AvroFragmentTypeUtil
    {
        internal static FragmentEventType GetFragmentType(
            Schema schema,
            string propertyName,
            IDictionary<string, PropertySetDescriptorItem> propertyItems,
            EventAdapterService eventAdapterService)
        {
            string unescapePropName = ASTUtil.UnescapeDot(propertyName);
            PropertySetDescriptorItem item = propertyItems.Get(unescapePropName);
            if (item != null)
            {
                return item.FragmentEventType;
            }

            Property property = PropertyParser.ParseAndWalkLaxToSimple(propertyName);
            AvroFieldDescriptor desc = AvroFieldUtil.FieldForProperty(schema, property);
            if (desc == null)
            {
                return null;
            }
            if (desc.IsDynamic)
            {
                return null;
            }
            Schema fieldSchemaByAccess = desc.Field.Schema;
            if (desc.IsAccessedByIndex)
            {
                fieldSchemaByAccess = fieldSchemaByAccess.GetElementType();
            }
            return GetFragmentEventTypeForField(fieldSchemaByAccess, eventAdapterService);
        }

        internal static FragmentEventType GetFragmentEventTypeForField(
            Schema fieldSchema,
            EventAdapterService eventAdapterService)
        {
            Schema recordSchema;
            bool indexed = false;
            if (fieldSchema.Tag == Schema.Type.Record)
            {
                recordSchema = fieldSchema;
            }
            else if (fieldSchema.Tag == Schema.Type.Array && fieldSchema.GetElementType().Tag == Schema.Type.Record)
            {
                recordSchema = fieldSchema.GetElementType();
                indexed = true;
            }
            else
            {
                return null;
            }

            // See if there is an existing type
            EventType existing = eventAdapterService.GetEventTypeByName(recordSchema.Name);
            if (existing != null && existing is AvroEventType)
            {
                return new FragmentEventType(existing, indexed, false);
            }

            EventType fragmentType = eventAdapterService.AddAvroType(
                recordSchema.Name, new ConfigurationEventTypeAvro().SetAvroSchema(recordSchema), false, false, false,
                false, false);
            return new FragmentEventType(fragmentType, indexed, false);
        }
    }
} // end of namespace
