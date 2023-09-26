///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.XPath;

using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.@event.xml
{
    public class EventTypeXMLXSDHandlerUnsupported : EventTypeXMLXSDHandler
    {
        public static readonly EventTypeXMLXSDHandlerUnsupported INSTANCE = new EventTypeXMLXSDHandlerUnsupported();

        public XmlQualifiedName SimpleTypeToQName(short type)
        {
            throw Unsupported;
        }

        public XPathResultType SimpleTypeToResultType(XmlSchemaSimpleType type)
        {
            throw Unsupported;
        }

        public Type ToReturnType(
            XmlSchemaSimpleType xsType,
            string typeName,
            int? optionalFractionDigits)
        {
            throw Unsupported;
        }

        public SchemaModel LoadAndMap(
            string schemaResource,
            string schemaText,
            ImportService importService)
        {
            throw Unsupported;
        }

        public UnsupportedOperationException Unsupported => throw new UnsupportedOperationException(
            "Esper-Compiler-XMLXSD is not enabled in the configuration or is not part of your classpath");
    }
} // end of namespace