///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using Avro;
using Avro.Generic;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using NEsper.Avro.Extensions;

namespace NEsper.Avro.Core
{
    public class AvroTypeUtil
    {
        private static readonly IDictionary<Schema.Type, AvroTypeDesc> TYPES_PER_AVRO_ORD;

        static AvroTypeUtil()
        {
            TYPES_PER_AVRO_ORD = new Dictionary<Schema.Type, AvroTypeDesc>();

            var schemaTypes = EnumHelper.GetValues<Schema.Type>().ToArray();
            foreach (var type in schemaTypes)
            {
                if (type == Schema.Type.Int)
                {
                    TYPES_PER_AVRO_ORD[type] = new AvroTypeDesc(typeof(int));
                }
                else if (type == Schema.Type.Long)
                {
                    TYPES_PER_AVRO_ORD[type] = new AvroTypeDesc(typeof(long));
                }
                else if (type == Schema.Type.Double)
                {
                    TYPES_PER_AVRO_ORD[type] = new AvroTypeDesc(typeof(double));
                }
                else if (type == Schema.Type.Float)
                {
                    TYPES_PER_AVRO_ORD[type] = new AvroTypeDesc(typeof(float));
                }
                else if (type == Schema.Type.Boolean)
                {
                    TYPES_PER_AVRO_ORD[type] = new AvroTypeDesc(typeof(bool));
                }
                else if (type == Schema.Type.Bytes)
                {
                    TYPES_PER_AVRO_ORD[type] = new AvroTypeDesc(typeof(byte[]));
                }
                else if (type == Schema.Type.Null)
                {
                    TYPES_PER_AVRO_ORD[type] = new AvroTypeDesc(null);
                }
            }
        }

        public static Type PropertyType(Schema fieldSchema)
        {
            if (fieldSchema.Tag == Schema.Type.Union)
            {
                var hasNull = false;
                var unionTypes = new HashSet<Type>();
                foreach (var memberSchema in fieldSchema.AsUnionSchema().Schemas)
                {
                    if (memberSchema.Tag == Schema.Type.Null)
                    {
                        hasNull = true;
                    }
                    else
                    {
                        var type = PropertyType(memberSchema);
                        if (type != null)
                        {
                            unionTypes.Add(type);
                        }
                    }
                }
                if (unionTypes.IsEmpty())
                {
                    return null;
                }
                if (unionTypes.Count == 1)
                {
                    if (hasNull)
                    {
                        return Boxing.GetBoxedType(unionTypes.First());
                    }

                    return unionTypes.First();
                }

#if NOT_NEEDED
                var allNumeric = true;
                foreach (var unioned in unionTypes)
                {
                    if (!TypeHelper.IsNumeric(unioned))
                    {
                        allNumeric = false;
                    }
                }
                if (allNumeric)
                {
                    return typeof(Number);
                }
#endif
                return typeof(object);
            }
            else if (fieldSchema.Tag == Schema.Type.Record)
            {
                return typeof(GenericRecord);
            }
            else if (fieldSchema.Tag == Schema.Type.Array)
            {
                var arrayItemSchema = ((ArraySchema) fieldSchema).ItemSchema;
                var arrayItemType = PropertyType(arrayItemSchema);
                if (arrayItemType == null)
                {
                    return typeof(ICollection<object>);
                }

                return arrayItemType.MakeArrayType();
            }
            else if (fieldSchema.Tag == Schema.Type.Map)
            {
                var mapValueSchema = ((MapSchema) fieldSchema).ValueSchema;
                var mapValueType = PropertyType(mapValueSchema);
                if (mapValueType == null)
                {
                    return typeof(IDictionary<string, object>);
                }

                return typeof(IDictionary<,>).MakeGenericType(new Type[] { typeof(string), mapValueType });
            }
            else if (fieldSchema.Tag == Schema.Type.Fixed)
            {
                return typeof(GenericFixed);
            }
            else if (fieldSchema.Tag == Schema.Type.Enumeration)
            {
                return typeof(GenericEnum);
            }
            else if (fieldSchema.Tag == Schema.Type.String)
            {
                string prop = fieldSchema.GetProp(AvroConstant.PROP_STRING_KEY);
                // there is a bug in the AVRO parser that adds quotes to properties
                if ((prop == null) || (prop.Length <= 2))
                {
#if AVRO_STRINGS_AND_CHARARRAY
                    return typeof(char[]);
#else
                    return typeof(string);
                }
#endif

                if ((prop[0] == '"') && (prop[prop.Length - 1] == '"'))
                {
                    prop = prop.Substring(1, prop.Length - 2);
                }

#if AVRO_STRINGS_AND_CHARARRAY
                return Equals(prop, AvroConstant.PROP_STRING_VALUE)
                    ? typeof(string)
                    : typeof(char[]);
#else
                return Equals(prop, AvroConstant.PROP_ARRAY_VALUE)
                    ? typeof(char[])
                    : typeof(string);
#endif
            }

            return TYPES_PER_AVRO_ORD.TryGetValue(fieldSchema.Tag, out var desc) ? desc.Type : null;
        }
    }
} // end of namespace