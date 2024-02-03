using System;
using System.Xml.Schema;
using System.Xml.XPath;

using com.espertech.esper.common.@internal.@event.xml;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.container;

namespace com.espertech.esper.common.@internal.xmlxsd.core
{
    public class EventTypeXMLXSDHandlerImpl : EventTypeXMLXSDHandler
    {
        public XPathResultType SimpleTypeToResultType(XmlSchemaSimpleType type)
        {
            return SchemaUtil.SimpleTypeToResultType(type);
        }

        public Type ToReturnType(
            XmlSchemaSimpleType xsType,
            string typeName,
            int? optionalFractionDigits)
        {
            return SchemaUtil.ToReturnType(xsType, typeName);
        }

        public SchemaModel LoadAndMap(
            string schemaResource,
            string schemaText,
            ImportService importService)
        {
            return XSDSchemaMapper.LoadAndMap(
                schemaResource,
                schemaText,
                importService.Container.ResourceManager());
        }
    }
}