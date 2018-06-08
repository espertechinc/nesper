///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.hook;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.util;

namespace NEsper.Avro.Core
{
    public class AvroTypeWidenerCustomizerWHook : TypeWidenerCustomizer
    {
        private readonly ObjectValueTypeWidenerFactory _factory;
        private readonly EventType _eventType;
    
        public AvroTypeWidenerCustomizerWHook(ObjectValueTypeWidenerFactory factory, EventType eventType)
        {
            _factory = factory;
            _eventType = eventType;
        }
    
        public TypeWidener WidenerFor(string columnName, Type columnType, Type writeablePropertyType, string writeablePropertyName, string statementName, string engineURI)
        {
            TypeWidener widener;

            try {
                var context = new ObjectValueTypeWidenerFactoryContext(columnType, writeablePropertyName, _eventType, statementName, engineURI);
                widener = _factory.Make(context);
            } catch (Exception e) {
                throw new ExprValidationException("Widener not available: " + e.Message, e);
            }
    
            if (widener != null) {
                return widener;
            }
            return AvroTypeWidenerCustomizerDefault.INSTANCE.WidenerFor(columnName, columnType, writeablePropertyType, writeablePropertyName, statementName, engineURI); // default behavior applies otherwise
        }
    }
} // end of namespace
