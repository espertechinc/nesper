///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.compat.collections;
using com.espertech.esper.util;

namespace NEsper.Avro.Core
{
    public class AvroTypeWidenerCustomizerDefault : TypeWidenerCustomizer
    {
        public static readonly AvroTypeWidenerCustomizerDefault INSTANCE = new AvroTypeWidenerCustomizerDefault();
    
        private AvroTypeWidenerCustomizerDefault()
        {
        }
    
        public TypeWidener WidenerFor(string columnName, Type columnType, Type writeablePropertyType, string writeablePropertyName, string statementName, string engineURI)
        {
            //if (columnType == typeof(byte[]) && writeablePropertyType == typeof(ByteBuffer))
            //{
            //    return BYTE_ARRAY_TO_BYTE_BUFFER_COERCER;
            //}
            if (columnType != null && columnType.IsArray && writeablePropertyType.IsGenericCollection())
            {
                return TypeWidenerFactory.GetArrayToCollectionCoercer(
                    columnType.GetElementType());
            }
            return null;
        }
    }
} // end of namespace
