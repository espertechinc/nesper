///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;

namespace NEsper.Avro.Core
{
    public class AvroTypeWidenerCustomizerDefault : TypeWidenerCustomizer
    {
        public static readonly AvroTypeWidenerCustomizerDefault INSTANCE = new AvroTypeWidenerCustomizerDefault();

        private AvroTypeWidenerCustomizerDefault()
        {
        }

        public TypeWidenerSPI WidenerFor(
            string columnName,
            Type columnType,
            Type writeablePropertyType,
            string writeablePropertyName,
            string statementName)
        {
            if (columnType != null && columnType.IsArray && writeablePropertyType.IsGenericCollection()) {
                return TypeWidenerFactory.GetArrayToCollectionCoercer(columnType.GetElementType());
            }

            return null;
        }
    }
} // end of namespace