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
    public class AnnotationXMLSchema : XMLSchemaAttribute
    {
        public Type AnnotationType => typeof(XMLSchemaAttribute);

        public static CodegenExpression ToExpression(
            XMLSchemaAttribute xmlSchema,
            CodegenMethodScope parent,
            CodegenClassScope scope)
        {
            return new CodegenSetterBuilder(typeof(AnnotationXMLSchema), typeof(AnnotationXMLSchema), "xmlschema", parent, scope)
                .Constant("RootElementName", xmlSchema.RootElementName)
                .Constant("SchemaResource", xmlSchema.SchemaResource)
                .Constant("SchemaText", xmlSchema.SchemaText)
                .Constant("XPathPropertyExpr", xmlSchema.XPathPropertyExpr)
                .Constant("DefaultNamespace", xmlSchema.DefaultNamespace)
                .Constant("EventSenderValidatesRoot", xmlSchema.EventSenderValidatesRoot)
                .Constant("AutoFragment", xmlSchema.AutoFragment)
                .Constant("XPathFunctionResolver", xmlSchema.XPathFunctionResolver)
                .Constant("XPathVariableResolver", xmlSchema.XPathVariableResolver)
                .Constant("RootElementNamespace", xmlSchema.RootElementNamespace)
                .Constant("XPathResolvePropertiesAbsolute", xmlSchema.XPathResolvePropertiesAbsolute)
                .Build();
        }
    }
} // end of namespace