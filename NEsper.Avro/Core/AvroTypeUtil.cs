///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.util;

using NEsper.Avro.Extensions;

namespace NEsper.Avro.Core
{
    public class AvroTypeUtil
    {
        private static readonly IDictionary<Schema.Type, AvroTypeDesc> TYPES_PER_AVRO_ORD;

        static AvroTypeUtil()
        {
            Schema.Type[] schemaTypes = EnumHelper.GetValues<Schema.Type>().ToArray();

            TYPES_PER_AVRO_ORD = new Dictionary<Schema.Type, AvroTypeDesc>();

            foreach(var type in schemaTypes)
            {
                if (type == Schema.Type.Int)
                {
                    TYPES_PER_AVRO_ORD[type] = new AvroTypeDesc(typeof (int));
                }
                else if (type == Schema.Type.Long)
                {
                    TYPES_PER_AVRO_ORD[type] = new AvroTypeDesc(typeof (long));
                }
                else if (type == Schema.Type.Double)
                {
                    TYPES_PER_AVRO_ORD[type] = new AvroTypeDesc(typeof (double));
                }
                else if (type == Schema.Type.Float)
                {
                    TYPES_PER_AVRO_ORD[type] = new AvroTypeDesc(typeof(float));
                }
                else if (type == Schema.Type.Boolean)
                {
                    TYPES_PER_AVRO_ORD[type] = new AvroTypeDesc(typeof (bool));
                }
                else if (type == Schema.Type.Bytes)
                {
                    TYPES_PER_AVRO_ORD[type] = new AvroTypeDesc(typeof (byte[]));
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
                bool hasNull = false;
                var unionTypes = new HashSet<Type>();
                foreach (Schema memberSchema in ((UnionSchema) fieldSchema).Schemas)
                {
                    if (memberSchema.Tag == Schema.Type.Null)
                    {
                        hasNull = true;
                    }
                    else
                    {
                        Type type = PropertyType(memberSchema);
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
                        return unionTypes.First().GetBoxedType();
                    }
                    return unionTypes.First();
                }
#if NOT_NEEDED
                bool allNumeric = true;
                foreach (var unioned in unionTypes)
                {
                    if (!unioned.IsNumeric())
                    {
                        allNumeric = false;
                    }
                }
#endif
                //if (allNumeric)
                //{
                //    return typeof (Numeric);
                //}
                return typeof (Object);
            }
            else if (fieldSchema.Tag == Schema.Type.Record)
            {
                return typeof (GenericRecord);
            }
            else if (fieldSchema.Tag == Schema.Type.Array)
            {
                var arrayItemSchema = ((ArraySchema)fieldSchema).ItemSchema;
                var arrayItemType = PropertyType(arrayItemSchema);
                if (arrayItemType == null)
                    return typeof(ICollection<object>);

                return arrayItemType.MakeArrayType();
                //return typeof (ICollection<object>);
            }
            else if (fieldSchema.Tag == Schema.Type.Map)
            {
                var mapValueSchema = ((MapSchema) fieldSchema).ValueSchema;
                var mapValueType = PropertyType(mapValueSchema);
                if (mapValueType == null)
                    return typeof(IDictionary<string, object>);

                return typeof(IDictionary<,>).MakeGenericType(new Type[] { typeof(string), mapValueType });
                //return typeof (IDictionary<string, object>);
            }
            else if (fieldSchema.Tag == Schema.Type.Fixed)
            {
                return typeof (GenericFixed);
            }
            else if (fieldSchema.Tag == Schema.Type.Enumeration)
            {
                return typeof (GenericEnum);
            }
            else if (fieldSchema.Tag == Schema.Type.String)
            {
                string prop = fieldSchema.GetProp(AvroConstant.PROP_STRING_KEY);
                // there is a bug in the AVRO parser that adds quotes to properties
                if ((prop == null) || (prop.Length <= 2))
#if AVRO_STRINGS_AND_CHARARRAY
                    return typeof(char[]);
#else
                    return typeof(string);
#endif

                if ((prop[0] == '"') && (prop[prop.Length - 1] == '"'))
                    prop = prop.Substring(1, prop.Length - 2);

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

            AvroTypeDesc desc;
            if (TYPES_PER_AVRO_ORD.TryGetValue(fieldSchema.Tag, out desc))
            {
                return desc.UnderlyingType;
            }

            return null;
        }
    }
} // end of namespace