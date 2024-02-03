///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.hook.type;
using com.espertech.esper.common.@internal.util;

namespace NEsper.Avro.Core
{
    public class AvroTypeWidenerCustomizerWHook : TypeWidenerCustomizer
    {
        private readonly EventType _eventType;
        private readonly ObjectValueTypeWidenerFactory _factory;

        public AvroTypeWidenerCustomizerWHook(
            ObjectValueTypeWidenerFactory factory,
            EventType eventType)
        {
            _factory = factory;
            _eventType = eventType;
        }

        public TypeWidenerSPI WidenerFor(
            string columnName,
            Type columnType,
            Type writeablePropertyType,
            string writeablePropertyName,
            string statementName)
        {
            TypeWidenerSPI widener;
            try {
                var context = new ObjectValueTypeWidenerFactoryContext(
                    columnType,
                    writeablePropertyName,
                    _eventType,
                    statementName);
                widener = _factory.Make(context);
            }
            catch (Exception ex) {
                throw new TypeWidenerException("Widener not available: " + ex.Message, ex);
            }

            if (widener != null) {
                return widener;
            }

            return AvroTypeWidenerCustomizerDefault.INSTANCE.WidenerFor(
                columnName,
                columnType,
                writeablePropertyType,
                writeablePropertyName,
                statementName); // default behavior applies otherwise
        }
    }
} // end of namespace