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
    public class AnnotationXMLSchemaNamespacePrefix : XMLSchemaNamespacePrefixAttribute
    {
        public Type AnnotationType => typeof(XMLSchemaNamespacePrefixAttribute);

        public static CodegenExpression ToExpression(
            XMLSchemaNamespacePrefixAttribute prefix,
            CodegenMethod parent,
            CodegenClassScope scope)
        {
            return new CodegenSetterBuilder(
                    typeof(AnnotationXMLSchemaNamespacePrefix),
                    typeof(AnnotationXMLSchemaNamespacePrefix),
                    "nsprefix",
                    parent,
                    scope)
                .ConstantExplicit("Prefix", prefix.Prefix)
                .ConstantExplicit("Namespace", prefix.Namespace)
                .Build();
        }
    }
} // end of namespace