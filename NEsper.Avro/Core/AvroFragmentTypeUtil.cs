///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using Avro;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.configuration.common;
using com.espertech.esper.common.client.meta;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.@event.avro;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.property;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;

using NEsper.Avro.Extensions;

namespace NEsper.Avro.Core
{
    public class AvroFragmentTypeUtil
    {
        internal static FragmentEventType GetFragmentType(
            Schema schema,
            string propertyName,
            string moduleName,
            IDictionary<string, PropertySetDescriptorItem> propertyItems,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            EventTypeAvroHandler eventTypeAvroHandler,
            AvroEventTypeFragmentTypeCache fragmentTypeCache)
        {
            var unescapePropName = StringValue.UnescapeDot(propertyName);
            var item = propertyItems.Get(unescapePropName);
            if (item != null) {
                return item.FragmentEventType;
            }

            var property = PropertyParser.ParseAndWalkLaxToSimple(propertyName);
            var desc = AvroFieldUtil.FieldForProperty(schema, property);
            if (desc == null) {
                return null;
            }

            if (desc.IsDynamic) {
                return null;
            }

            var fieldSchemaByAccess = desc.Field.Schema;
            if (desc.IsAccessedByIndex) {
                fieldSchemaByAccess = fieldSchemaByAccess.AsArraySchema().ItemSchema;
            }

            return GetFragmentEventTypeForField(
                fieldSchemaByAccess,
                moduleName,
                eventBeanTypedEventFactory,
                eventTypeAvroHandler,
                fragmentTypeCache);
        }

        internal static FragmentEventType GetFragmentEventTypeForField(
            Schema fieldSchema,
            string moduleName,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            EventTypeAvroHandler eventTypeAvroHandler,
            AvroEventTypeFragmentTypeCache fragmentTypeCache)
        {
            Schema recordSchema;
            var indexed = false;
            if (fieldSchema.Tag == Schema.Type.Record) {
                recordSchema = fieldSchema;
            }
            else if (fieldSchema.Tag == Schema.Type.Array &&
                     fieldSchema.AsArraySchema().ItemSchema.Tag == Schema.Type.Record) {
                recordSchema = fieldSchema.AsArraySchema().ItemSchema;
                indexed = true;
            }
            else {
                return null;
            }

            var cached = fragmentTypeCache.Get(recordSchema.Name);
            if (cached != null) {
                return new FragmentEventType(cached, indexed, false);
            }

            var metadata = new EventTypeMetadata(
                recordSchema.Name,
                moduleName,
                EventTypeTypeClass.STREAM,
                EventTypeApplicationType.AVRO,
                NameAccessModifier.TRANSIENT,
                EventTypeBusModifier.NONBUS,
                false,
                EventTypeIdPair.Unassigned());
            var config = new ConfigurationCommonEventTypeAvro();
            config.AvroSchema = recordSchema;

            var fragmentType = eventTypeAvroHandler.NewEventTypeFromSchema(
                metadata,
                eventBeanTypedEventFactory,
                config,
                null,
                null);

            fragmentTypeCache.Add(recordSchema.Name, fragmentType);
            return new FragmentEventType(fragmentType, indexed, false);
        }
    }
} // end of namespace