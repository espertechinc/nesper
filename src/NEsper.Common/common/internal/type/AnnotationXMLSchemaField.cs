///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client.annotation;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;

namespace com.espertech.esper.common.@internal.type
{
    public class AnnotationXMLSchemaField : XMLSchemaFieldAttribute
    {
        public Type AnnotationType => typeof(XMLSchemaFieldAttribute);

        public static CodegenExpression ToExpression(
            XMLSchemaFieldAttribute field,
            CodegenMethod parent,
            CodegenClassScope scope)
        {
            return new CodegenSetterBuilder(typeof(AnnotationXMLSchemaField), typeof(AnnotationXMLSchemaField), "field", parent, scope)
                .Constant("Name", field.Name)
                .Constant("XPath", field.XPath)
                .Constant("Type", field.Type)
                .Constant("EventTypeName", field.EventTypeName)
                .Constant("CastToType", field.CastToType)
                .Build();
        }
    }
} // end of namespace