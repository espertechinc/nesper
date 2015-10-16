///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using System.Xml.Schema;
using com.espertech.esper.client;
using com.espertech.esper.client.util;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.util;

namespace com.espertech.esper.events.util
{
    using DataMap = IDictionary<string, object>;

    public class XElementRendererImpl
    {
        private readonly RendererMeta _meta;
        private readonly XMLRenderingOptions _options;
        private readonly RendererMetaOptions _rendererMetaOptions;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="eventType">type of event to render</param>
        /// <param name="options">rendering options</param>
        public XElementRendererImpl(EventType eventType, XMLRenderingOptions options)
        {
            EventPropertyRenderer propertyRenderer = null;
            EventPropertyRendererContext propertyRendererContext = null;
            if (options.Renderer != null)
            {
                propertyRenderer = options.Renderer;
                propertyRendererContext = new EventPropertyRendererContext(eventType, false);
            }

            _rendererMetaOptions = new RendererMetaOptions(options.PreventLooping, true, propertyRenderer, propertyRendererContext);
            _meta = new RendererMeta(eventType, new Stack<EventTypePropertyPair>(), _rendererMetaOptions);
            _options = options;
        }

        public XElement Render(String rootElementName, EventBean theEvent)
        {
            return _options.IsDefaultAsAttribute
                ? RenderWithAttributes(rootElementName, theEvent) 
                : RenderWithElements(rootElementName, theEvent);
        }

        private XElement RenderWithElements(String rootElementName, EventBean theEvent)
        {
            var element = new XElement(
                rootElementName, 
                RenderRecursive(theEvent, _meta));
            return element;
        }

        private XElement RenderWithAttributes(String rootElementName, EventBean theEvent)
        {
            var attributes = RenderAttributes(theEvent, _meta).Cast<object>();
            var attributeElements = RenderAttributeElements(theEvent, _meta).Cast<object>().ToArray();
            var element = new XElement(
                rootElementName,
                attributes.Concat(attributeElements).ToArray());

            return element;
        }

        private static IEnumerable<XElement> RenderAttributeElements(
            EventBean theEvent, RendererMeta meta)
        {
            var indexProps = meta.IndexProperties;
            foreach (var indexProp in indexProps) {
                var value = indexProp.Getter.Get(theEvent);
                if ((value == null) || (value is string)) {
                    continue;
                }

                var asArray = value as Array;
                if (asArray == null) {
                    Log.Warn("Property '{0}' returned a non-array object", indexProp.Name);
                    continue;
                }

                for (int ii = 0; ii < asArray.Length; ii++) {
                    var arrayItem = asArray.GetValue(ii);
                    if (arrayItem == null) {
                        continue;
                    }

                    yield return new XElement(indexProp.Name, arrayItem);
                }
            }

            var mappedProps = meta.MappedProperties;
            foreach (var mappedProp in mappedProps) {
                var value = mappedProp.Getter.Get(theEvent);
                if ((value != null) && (!(value is DataMap))) {
                    Log.Warn("Property '" + mappedProp.Name + "' expected to return Map and returned " + value.GetType() +
                             " instead");
                    continue;
                }

                var mapElementChildren = RenderDataMap(value).Cast<object>().ToArray();
                var mapElement = new XElement(mappedProp.Name, mapElementChildren);
                yield return mapElement;
            }

            var nestedProps = meta.NestedProperties;
            foreach (var nestedProp in nestedProps) {
                var value = nestedProp.Getter.GetFragment(theEvent);
                if (value == null) {
                    continue;
                }

                if (!nestedProp.IsArray) {
                    if (!(value is EventBean)) {
                        Log.Warn("Property '{0}' expected to return EventBean and returned '{1}' instead",
                            nestedProp.Name, value.GetType());
                        yield return new XElement("null");
                        //buf.Append("null");
                        continue;
                    }

                    var nestedEventBean = (EventBean) value;
                    yield return RenderAttributeInner(nestedEventBean, nestedProp);
                }
                else {
                    if (!(value is EventBean[])) {
                        Log.Warn("Property '{0}' expected to return EventBean[] and returned '{1}' instead",
                            nestedProp.Name, value.GetType());
                        yield return new XElement("null");
                        //buf.Append("null");
                        continue;
                    }

                    var nestedEventArray = (EventBean[]) value;
                    for (int ii = 0; ii < nestedEventArray.Length; ii++) {
                        var arrayItem = nestedEventArray[ii];
                        yield return RenderAttributeInner(arrayItem, nestedProp);
                    }
                }
            }
        }

        /// <summary>
        /// Renders the value assuming that it is a data map.  If it is not a data map or
        /// if the data map is empty, then no elements are returned.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        private static IEnumerable<XElement> RenderDataMap(Object value)
        {
            if (value != null)
            {
                var map = (DataMap)value;
                if (map.IsNotEmpty())
                {
                    foreach (var entry in map.Where(entry => entry.Key != null & entry.Value != null))
                    {
                        yield return new XElement(
                            entry.Key,
                            entry.Value);
                    }
                }
            }
        }

        private static IEnumerable<XAttribute> RenderAttributes(
            EventBean theEvent, RendererMeta meta)
        {
            var simpleProps = meta.SimpleProperties;
            foreach (GetterPair simpleProp in simpleProps) {
                var value = simpleProp.Getter.Get(theEvent);
                if (value == null) {
                    continue;
                }

                yield return new XAttribute(simpleProp.Name, value);
            }

            var indexProps = meta.IndexProperties;
            foreach (GetterPair indexProp in indexProps) {
                var value = indexProp.Getter.Get(theEvent);
                if (value == null) {
                    continue;
                }

                var asString = value as string;
                if (asString != null) {
                    yield return new XAttribute(indexProp.Name, value);
                }
            }
        }

        /// <summary>
        /// Renders the event recursively passing through the structure.
        /// </summary>
        /// <param name="theEvent">The theEvent.</param>
        /// <param name="meta">The meta.</param>
        /// <returns></returns>
        private static IEnumerable<XElement> RenderRecursive(
            EventBean theEvent, RendererMeta meta)
        {
            return RenderSimpleProperties(theEvent, meta)
                .Concat(RenderIndexProperties(theEvent, meta))
                .Concat(RenderMappedProperties(theEvent, meta))
                .Concat(RenderNestedProperties(theEvent, meta));
        }

        /// <summary>
        /// Renders the nested properties.
        /// </summary>
        /// <param name="theEvent">The theEvent.</param>
        /// <param name="meta">The meta.</param>
        /// <returns></returns>
        private static IEnumerable<XElement> RenderNestedProperties(
            EventBean theEvent, RendererMeta meta)
        {
            NestedGetterPair[] nestedProps = meta.NestedProperties;
            foreach (NestedGetterPair nestedProp in nestedProps) {
                var value = nestedProp.Getter.GetFragment(theEvent);
                if (value == null) {
                    continue;
                }

                if (!nestedProp.IsArray)
                {
                    if (!(value is EventBean))
                    {
                        Log.Warn("Property '{0}' expected to return EventBean and returned {1} instead",
                                 nestedProp.Name, value.GetType());
                        yield return new XElement("null");
                    }
                    else
                    {
                        yield return RenderElementFragment((EventBean) value, nestedProp);
                    }
                }
                else
                {
                    if (!(value is EventBean[]))
                    {
                        Log.Warn("Property '{0}' expected to return EventBean[] and returned {1} instead",
                                 nestedProp.Name, value.GetType());
                        yield return new XElement("null");
                    }
                    else
                    {
                        var nestedEventArray = (EventBean[])value;
                        foreach (EventBean arrayItem in nestedEventArray.Where(arrayItem => arrayItem != null))
                        {
                            yield return RenderElementFragment(arrayItem, nestedProp);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Renders the simple properties.
        /// </summary>
        /// <param name="theEvent">The theEvent.</param>
        /// <param name="meta">The meta.</param>
        /// <returns></returns>
        private static IEnumerable<XElement> RenderSimpleProperties(
            EventBean theEvent, RendererMeta meta)
        {
            var simpleProps = meta.SimpleProperties;
            foreach (GetterPair simpleProp in simpleProps)
            {
                var value = simpleProp.Getter.Get(theEvent);
                if (value != null)
                {
                    var valueType = value.GetType();
                    var valueTypeCode = GetTypeCode(valueType);
                    yield return new XElement(
                        simpleProp.Name,
                        new XAttribute("type", valueTypeCode),
                        value);
                }
            }
        }

        /// <summary>
        /// Renders the index properties.
        /// </summary>
        /// <param name="theEvent">The theEvent.</param>
        /// <param name="meta">The meta.</param>
        /// <returns></returns>
        private static IEnumerable<XElement> RenderIndexProperties(
            EventBean theEvent, RendererMeta meta)
        {
            GetterPair[] indexProps = meta.IndexProperties;
            foreach (GetterPair indexProp in indexProps) {
                var value = indexProp.Getter.Get(theEvent);
                if (value == null) {
                    continue;
                }

                if (value is string) {
                    yield return new XElement(indexProp.Name, new XAttribute("type", XmlTypeCode.String), value);
                    continue;
                }

                var asArray = value as Array;
                if (asArray == null) {
                    Log.Warn("Property '{0}' returned a non-array object", indexProp.Name);
                    continue;
                }

                for (int ii = 0; ii < asArray.Length; ii++) {
                    var arrayItem = asArray.GetValue(ii);
                    if (arrayItem == null) {
                        continue;
                    }

                    yield return new XElement(indexProp.Name, arrayItem);
                }
            }
        }

        /// <summary>
        /// Renders the mapped properties.
        /// </summary>
        /// <param name="theEvent">The theEvent.</param>
        /// <param name="meta">The meta.</param>
        /// <returns></returns>
        private static IEnumerable<XElement> RenderMappedProperties(
            EventBean theEvent, RendererMeta meta)
        {
            GetterPair[] mappedProps = meta.MappedProperties;
            foreach (GetterPair mappedProp in mappedProps)
            {
                var value = mappedProp.Getter.Get(theEvent);
                if ((value != null) && (!(value is DataMap)))
                {
                    Log.Warn("Property '{0}' expected to return Map and returned {1} instead",
                             mappedProp.Name, value.GetType());
                    continue;
                }

                if (value != null) {
                    var map = (DataMap) value;
                    if (map.IsNotEmpty()) {
                        var mapElements = RenderMappedProperty(map).Cast<object>().ToArray();
                        yield return new XElement(mappedProp.Name, mapElements);
                    } else {
                        yield return new XElement(mappedProp.Name);
                    }
                } else {
                    yield return new XElement(mappedProp.Name);
                }
            }
        }

        /// <summary>
        /// Renders the mapped property.
        /// </summary>
        /// <param name="map">The map.</param>
        /// <returns></returns>
        private static IEnumerable<XElement> RenderMappedProperty(DataMap map)
        {
            foreach (var entry in map.Where(entry => entry.Key != null)) {
                if (entry.Value == null) {
                    yield return new XElement(entry.Key);
                } else {
                    yield return new XElement(entry.Key, entry.Value);
                }
            }
        }

        private static XElement RenderElementFragment(
            EventBean eventBean, NestedGetterPair nestedProp)
        {
            return new XElement(
                nestedProp.Name,
                RenderRecursive(eventBean, nestedProp.Metadata)
                    .Cast<object>()
                    .ToArray());
        }

        private static XElement RenderAttributeInner(EventBean nestedEventBean, NestedGetterPair nestedProp)
        {
            var attributes = RenderAttributes(
                nestedEventBean, nestedProp.Metadata).Cast<object>();
            var attributeElements = RenderAttributeElements(
                nestedEventBean, nestedProp.Metadata).Cast<object>();
            var elementParts = attributes
                .Concat(attributeElements)
                .ToArray();

            return new XElement(
                nestedProp.Name,
                elementParts);
        }

        private static XmlTypeCode GetTypeCode(Type type)
        {
            type = type.GetBoxedType();

            if (type == typeof(int?))
            {
                return XmlTypeCode.Integer;
            }
            else if (type == typeof(long?))
            {
                return XmlTypeCode.Long;
            }
            else if (type == typeof(short?))
            {
                return XmlTypeCode.Short;
            }
            else if (type == typeof(byte?))
            {
                return XmlTypeCode.Byte;
            }
            else if (type == typeof(float?))
            {
                return XmlTypeCode.Float;
            }
            else if (type == typeof(double?))
            {
                return XmlTypeCode.Double;
            }
            else if (type == typeof(decimal?))
            {
                return XmlTypeCode.Decimal;
            }
            else if (type == typeof(bool?))
            {
                return XmlTypeCode.Boolean;
            }
            else if (type == typeof(DateTime?))
            {
                return XmlTypeCode.DateTime;
            }
            else if (type == typeof(DateTimeOffset?))
            {
                return XmlTypeCode.DateTime;
            }
            else if (type == typeof(string))
            {
                return XmlTypeCode.String;
            }
            else
            {
                return XmlTypeCode.None;
            }
        }

        private static readonly ILog Log =
            LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    }
}
