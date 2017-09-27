///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using Avro;
using Avro.Generic;

namespace NEsper.Avro.Extensions
{
    public static class GenericRecordExtensions
    {
        /// <summary>
        /// Gets the value from the record.
        /// </summary>
        /// <param name="record">The record.</param>
        /// <param name="fieldName">The field name.</param>
        /// <returns></returns>
        public static object Get(this GenericRecord record, string fieldName)
        {
            object value;
            record.TryGetValue(fieldName, out value);
            return value;
        }

        /// <summary>
        /// Gets the value from the record.
        /// </summary>
        /// <param name="record">The record.</param>
        /// <param name="field">The field.</param>
        /// <returns></returns>
        public static object Get(this GenericRecord record, Field field)
        {
            object value;
            record.TryGetValue(field.Name, out value);
            return value;
        }

        /// <summary>
        /// Sets the specified record value.
        /// </summary>
        /// <param name="record">The record.</param>
        /// <param name="field">The field.</param>
        /// <param name="value">The value.</param>
        public static void Put(this GenericRecord record, Field field, object value)
        {
            record.Add(field.Name, value);
        }

        /// <summary>
        /// Sets the specified record value.
        /// </summary>
        /// <param name="record">The record.</param>
        /// <param name="name">The field name.</param>
        /// <param name="value">The value.</param>
        public static void Put(this GenericRecord record, string name, object value)
        {
            record.Add(name, value);
        }
    }
}
