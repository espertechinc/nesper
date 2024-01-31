///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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

namespace com.espertech.esper.common.@internal.@event.xml
{
    public interface EventTypeXMLXSDHandler
    {
        const string HANDLER_IMPL = "com.espertech.esper.common.internal.xmlxsd.core.EventTypeXMLXSDHandlerImpl";

        //XmlQualifiedName SimpleTypeToQName(short type);

        XPathResultType SimpleTypeToResultType(XmlSchemaSimpleType type);

        Type ToReturnType(
            XmlSchemaSimpleType xsType,
            string typeName,
            int? optionalFractionDigits);

        SchemaModel LoadAndMap(
            string schemaResource,
            string schemaText,
            ImportService importService);
    }
} // end of namespace