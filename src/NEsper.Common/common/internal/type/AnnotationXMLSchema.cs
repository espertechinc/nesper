///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
            return new CodegenSetterBuilder(
                    typeof(AnnotationXMLSchema),
                    typeof(AnnotationXMLSchema),
                    "xmlschema",
                    parent,
                    scope)
                .ConstantExplicit("RootElementName", xmlSchema.RootElementName)
                .ConstantExplicit("SchemaResource", xmlSchema.SchemaResource)
                .ConstantExplicit("SchemaText", xmlSchema.SchemaText)
                .ConstantExplicit("XPathPropertyExpr", xmlSchema.XPathPropertyExpr)
                .ConstantExplicit("DefaultNamespace", xmlSchema.DefaultNamespace)
                .ConstantExplicit("EventSenderValidatesRoot", xmlSchema.EventSenderValidatesRoot)
                .ConstantExplicit("AutoFragment", xmlSchema.AutoFragment)
                .ConstantExplicit("XPathFunctionResolver", xmlSchema.XPathFunctionResolver)
                .ConstantExplicit("XPathVariableResolver", xmlSchema.XPathVariableResolver)
                .ConstantExplicit("RootElementNamespace", xmlSchema.RootElementNamespace)
                .ConstantExplicit("XPathResolvePropertiesAbsolute", xmlSchema.XPathResolvePropertiesAbsolute)
                .Build();
        }
    }
} // end of namespace