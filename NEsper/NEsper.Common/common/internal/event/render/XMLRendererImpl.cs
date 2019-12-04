///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.render;
using com.espertech.esper.common.@internal.@event.util;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compat.magic;

namespace com.espertech.esper.common.@internal.@event.render
{
    /// <summary>
    ///     Renderer for XML-formatted properties.
    /// </summary>
    public class XMLRendererImpl : XMLEventRenderer
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly string Newline = Environment.NewLine;

        private readonly RendererMeta meta;
        private readonly XMLRenderingOptions options;
        private readonly RendererMetaOptions rendererMetaOptions;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="eventType">type of event to render</param>
        /// <param name="options">rendering options</param>
        public XMLRendererImpl(
            EventType eventType,
            XMLRenderingOptions options)
        {
            EventPropertyRenderer propertyRenderer = null;
            EventPropertyRendererContext propertyRendererContext = null;
            if (options.Renderer != null) {
                propertyRenderer = options.Renderer;
                propertyRendererContext = new EventPropertyRendererContext(eventType, false);
            }

            rendererMetaOptions = new RendererMetaOptions(
                options.PreventLooping,
                true,
                propertyRenderer,
                propertyRendererContext);
            meta = new RendererMeta(eventType, new Stack<EventTypePropertyPair>(), rendererMetaOptions);
            this.options = options;
        }

        public string Render(
            string rootElementName,
            EventBean theEvent)
        {
            if (options.IsDefaultAsAttribute) {
                return RenderAttributeXML(rootElementName, theEvent);
            }

            return RenderElementXML(rootElementName, theEvent);
        }

        private string RenderElementXML(
            string rootElementName,
            EventBean theEvent)
        {
            var buf = new StringBuilder();

            buf.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            buf.Append(Newline);

            buf.Append('<');
            buf.Append(rootElementName);
            buf.Append('>');
            buf.Append(Newline);

            RecursiveRender(theEvent, buf, 1, meta, rendererMetaOptions);

            buf.Append("</");
            buf.Append(GetFirstWord(rootElementName));
            buf.Append('>');

            return buf.ToString();
        }

        private string RenderAttributeXML(
            string rootElementName,
            EventBean theEvent)
        {
            var buf = new StringBuilder();

            buf.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            buf.Append(Newline);

            buf.Append('<');
            buf.Append(rootElementName);
            RenderAttributes(theEvent, buf, meta);

            var inner = RenderAttElements(theEvent, 1, meta);

            if (inner == null || inner.Trim().Length == 0) {
                buf.Append("/>");
                buf.Append(Newline);
            }
            else {
                buf.Append(">");
                buf.Append(Newline);
                buf.Append(inner);
                buf.Append("</");
                buf.Append(GetFirstWord(rootElementName));
                buf.Append('>');
            }

            return buf.ToString();
        }

        private string RenderAttElements(
            EventBean theEvent,
            int level,
            RendererMeta meta)
        {
            var buf = new StringBuilder();

            var indexProps = meta.IndexProperties;
            foreach (var indexProp in indexProps) {
                var value = indexProp.Getter.Get(theEvent);

                if (value == null) {
                    continue;
                }

                var array = value as Array;
                if (array == null) {
                    Log.Warn("Property '" + indexProp.Name + "' returned a non-array object");
                    continue;
                }

                for (var i = 0; i < array.Length; i++) {
                    var arrayItem = array.GetValue(i);

                    if (arrayItem == null) {
                        continue;
                    }

                    Ident(buf, level);
                    buf.Append('<');
                    buf.Append(indexProp.Name);
                    buf.Append('>');
                    if (rendererMetaOptions.Renderer == null) {
                        indexProp.Output.Render(arrayItem, buf);
                    }
                    else {
                        var context = rendererMetaOptions.RendererContext;
                        context.SetStringBuilderAndReset(buf);
                        context.PropertyName = indexProp.Name;
                        context.PropertyValue = arrayItem;
                        context.IndexedPropertyIndex = i;
                        context.DefaultRenderer = indexProp.Output;
                        rendererMetaOptions.Renderer.Render(context);
                    }

                    buf.Append("</");
                    buf.Append(indexProp.Name);
                    buf.Append('>');
                    buf.Append(Newline);
                }
            }

            var mappedProps = meta.MappedProperties;
            foreach (var mappedProp in mappedProps) {
                var value = mappedProp.Getter.Get(theEvent);

                if (value != null && !(value is IDictionary<string, object>)) {
                    Log.Warn(
                        "Property '" +
                        mappedProp.Name +
                        "' expected to return Map and returned " +
                        value.GetType() +
                        " instead");
                    continue;
                }

                Ident(buf, level);
                buf.Append('<');
                buf.Append(mappedProp.Name);

                if (value != null) {
                    var map = (IDictionary<string, object>) value;
                    if (!map.IsEmpty()) {
                        using (var enumerator = map.GetEnumerator()) {
                            while (enumerator.MoveNext()) {
                                var entry = enumerator.Current;
                                if (entry.Key == null || entry.Value == null) {
                                    continue;
                                }

                                buf.Append(" ");
                                buf.Append(entry.Key);
                                buf.Append("=\"");
                                var outputValueRenderer =
                                    OutputValueRendererFactory.GetOutputValueRenderer(
                                        entry.Value.GetType(),
                                        rendererMetaOptions);

                                if (rendererMetaOptions.Renderer == null) {
                                    outputValueRenderer.Render(entry.Value, buf);
                                }
                                else {
                                    var context = rendererMetaOptions.RendererContext;
                                    context.SetStringBuilderAndReset(buf);
                                    context.PropertyName = mappedProp.Name;
                                    context.PropertyValue = entry.Value;
                                    context.MappedPropertyKey = entry.Key;
                                    context.DefaultRenderer = outputValueRenderer;
                                    rendererMetaOptions.Renderer.Render(context);
                                }

                                buf.Append("\"");
                            }
                        }
                    }
                }

                buf.Append("/>");
                buf.Append(Newline);
            }

            var nestedProps = meta.NestedProperties;
            foreach (var nestedProp in nestedProps) {
                var value = nestedProp.Getter.GetFragment(theEvent);

                if (value == null) {
                    continue;
                }

                if (!nestedProp.IsArray) {
                    if (!(value is EventBean)) {
                        Log.Warn(
                            "Property '" +
                            nestedProp.Name +
                            "' expected to return EventBean and returned " +
                            value.GetType() +
                            " instead");
                        buf.Append("null");
                        continue;
                    }

                    var nestedEventBean = (EventBean) value;
                    RenderAttInner(buf, level, nestedEventBean, nestedProp);
                }
                else {
                    if (!(value is EventBean[])) {
                        Log.Warn(
                            "Property '" +
                            nestedProp.Name +
                            "' expected to return EventBean[] and returned " +
                            value.GetType() +
                            " instead");
                        buf.Append("null");
                        continue;
                    }

                    var nestedEventArray = (EventBean[]) value;
                    for (var i = 0; i < nestedEventArray.Length; i++) {
                        var arrayItem = nestedEventArray[i];
                        RenderAttInner(buf, level, arrayItem, nestedProp);
                    }
                }
            }

            return buf.ToString();
        }

        private void RenderAttributes(
            EventBean theEvent,
            StringBuilder buf,
            RendererMeta meta)
        {
            var delimiter = " ";
            var simpleProps = meta.SimpleProperties;
            foreach (var simpleProp in simpleProps) {
                var value = simpleProp.Getter.Get(theEvent);
                if (value == null) {
                    continue;
                }

                buf.Append(delimiter);
                buf.Append(simpleProp.Name);
                buf.Append("=\"");
                if (rendererMetaOptions.Renderer == null) {
                    simpleProp.Output.Render(value, buf);
                }
                else {
                    var context = rendererMetaOptions.RendererContext;
                    context.SetStringBuilderAndReset(buf);
                    context.PropertyName = simpleProp.Name;
                    context.PropertyValue = value;
                    context.DefaultRenderer = simpleProp.Output;
                    rendererMetaOptions.Renderer.Render(context);
                }

                buf.Append('"');
            }
        }

        private static void Ident(
            StringBuilder buf,
            int level)
        {
            for (var i = 0; i < level; i++) {
                IndentChar(buf);
            }
        }

        private static void IndentChar(StringBuilder buf)
        {
            buf.Append(' ');
            buf.Append(' ');
        }

        private static void RecursiveRender(
            EventBean theEvent,
            StringBuilder buf,
            int level,
            RendererMeta meta,
            RendererMetaOptions rendererMetaOptions)
        {
            foreach (var simpleProp in meta.SimpleProperties.OrderBy(p => p.Name)) {
                var value = simpleProp.Getter.Get(theEvent);

                if (value == null) {
                    continue;
                }

                Ident(buf, level);
                buf.Append('<');
                buf.Append(simpleProp.Name);
                buf.Append('>');

                if (rendererMetaOptions.Renderer == null) {
                    simpleProp.Output.Render(value, buf);
                }
                else {
                    var context = rendererMetaOptions.RendererContext;
                    context.SetStringBuilderAndReset(buf);
                    context.PropertyName = simpleProp.Name;
                    context.PropertyValue = value;
                    context.DefaultRenderer = simpleProp.Output;
                    rendererMetaOptions.Renderer.Render(context);
                }

                buf.Append("</");
                buf.Append(simpleProp.Name);
                buf.Append('>');
                buf.Append(Newline);
            }

            foreach (var indexProp in meta.IndexProperties.OrderBy(p => p.Name)) {
                var value = indexProp.Getter.Get(theEvent);

                if (value == null) {
                    continue;
                }

                // Strings are rendered like simple properties
                if (value is string) {
                    Ident(buf, level);
                    buf.Append('<');
                    buf.Append(indexProp.Name);
                    buf.Append('>');

                    if (rendererMetaOptions.Renderer == null) {
                        indexProp.Output.Render(value, buf);
                    }
                    else {
                        var context = rendererMetaOptions.RendererContext;
                        context.SetStringBuilderAndReset(buf);
                        context.PropertyName = indexProp.Name;
                        context.PropertyValue = value;
                        context.DefaultRenderer = indexProp.Output;
                        rendererMetaOptions.Renderer.Render(context);
                    }

                    buf.Append("</");
                    buf.Append(indexProp.Name);
                    buf.Append('>');
                    buf.Append(Newline);
                }
                // Arrays
                else if (value is Array array) {
                    for (var i = 0; i < array.Length; i++) {
                        var arrayItem = array.GetValue(i);
                        if (arrayItem == null) {
                            continue;
                        }

                        Ident(buf, level);
                        buf.Append('<');
                        buf.Append(indexProp.Name);
                        buf.Append('>');
                        if (rendererMetaOptions.Renderer == null) {
                            indexProp.Output.Render(arrayItem, buf);
                        }
                        else {
                            var context = rendererMetaOptions.RendererContext;
                            context.SetStringBuilderAndReset(buf);
                            context.PropertyName = indexProp.Name;
                            context.PropertyValue = arrayItem;
                            context.IndexedPropertyIndex = i;
                            context.DefaultRenderer = indexProp.Output;
                            rendererMetaOptions.Renderer.Render(context);
                        }

                        buf.Append("</");
                        buf.Append(indexProp.Name);
                        buf.Append('>');
                        buf.Append(Newline);
                    }
                }
                // Lists
                else if (value.GetType().IsGenericList()) {
                    // All lists are generically enumerable
                    int listItemIndex = 0;
                    foreach (var listItem in value.UnwrapEnumerable<object>()) {
                        if (listItem != null) {
                            Ident(buf, level);
                            buf.Append('<');
                            buf.Append(indexProp.Name);
                            buf.Append('>');
                            if (rendererMetaOptions.Renderer == null)
                            {
                                indexProp.Output.Render(listItem, buf);
                            }
                            else
                            {
                                var context = rendererMetaOptions.RendererContext;
                                context.SetStringBuilderAndReset(buf);
                                context.PropertyName = indexProp.Name;
                                context.PropertyValue = listItem;
                                context.IndexedPropertyIndex = listItemIndex;
                                context.DefaultRenderer = indexProp.Output;
                                rendererMetaOptions.Renderer.Render(context);
                            }

                            buf.Append("</");
                            buf.Append(indexProp.Name);
                            buf.Append('>');
                            buf.Append(Newline);
                        }

                        listItemIndex++;
                    }
                }
                else {
                    Log.Warn("Property '" + indexProp.Name + "' returned a non-array object");
                }
            }

            foreach (var mappedProp in meta.MappedProperties.OrderBy(p => p.Name)) {
                var value = mappedProp.Getter.Get(theEvent);

                if (value != null && !(value is IDictionary<string, object>)) {
                    Log.Warn(
                        "Property '" +
                        mappedProp.Name +
                        "' expected to return Map and returned " +
                        value.GetType() +
                        " instead");
                    continue;
                }

                Ident(buf, level);
                buf.Append('<');
                buf.Append(mappedProp.Name);
                buf.Append('>');
                buf.Append(Newline);

                if (value != null) {
                    var map = (IDictionary<string, object>) value;
                    if (!map.IsEmpty()) {
                        var localDelimiter = "";
                        foreach (var entry in map)
                        {
                            if (entry.Key == null) {
                                continue;
                            }

                            buf.Append(localDelimiter);
                            Ident(buf, level + 1);
                            buf.Append('<');
                            buf.Append(entry.Key);
                            buf.Append('>');

                            if (entry.Value != null) {
                                var outputValueRenderer =
                                    OutputValueRendererFactory.GetOutputValueRenderer(
                                        entry.Value.GetType(),
                                        rendererMetaOptions);
                                if (rendererMetaOptions.Renderer == null) {
                                    outputValueRenderer.Render(entry.Value, buf);
                                }
                                else {
                                    var context = rendererMetaOptions.RendererContext;
                                    context.SetStringBuilderAndReset(buf);
                                    context.PropertyName = mappedProp.Name;
                                    context.PropertyValue = entry.Value;
                                    context.MappedPropertyKey = entry.Key;
                                    context.DefaultRenderer = outputValueRenderer;
                                    rendererMetaOptions.Renderer.Render(context);
                                }
                            }

                            buf.Append("</");
                            buf.Append(entry.Key);
                            buf.Append('>');
                            localDelimiter = Newline;
                        }
                    }
                }

                buf.Append(Newline);
                Ident(buf, level);
                buf.Append("</");
                buf.Append(mappedProp.Name);
                buf.Append('>');
                buf.Append(Newline);
            }

            var nestedProps = meta.NestedProperties;
            foreach (var nestedProp in nestedProps.OrderBy(p => p.Name)) {
                var value = nestedProp.Getter.GetFragment(theEvent);

                if (value == null) {
                    continue;
                }

                if (!nestedProp.IsArray) {
                    if (!(value is EventBean)) {
                        Log.Warn(
                            "Property '" +
                            nestedProp.Name +
                            "' expected to return EventBean and returned " +
                            value.GetType() +
                            " instead");
                        buf.Append("null");
                        continue;
                    }

                    RenderElementFragment((EventBean) value, buf, level, nestedProp, rendererMetaOptions);
                }
                else {
                    if (!(value is EventBean[])) {
                        Log.Warn(
                            "Property '" +
                            nestedProp.Name +
                            "' expected to return EventBean[] and returned " +
                            value.GetType() +
                            " instead");
                        buf.Append("null");
                        continue;
                    }

                    var nestedEventArray = (EventBean[]) value;
                    for (var i = 0; i < nestedEventArray.Length; i++) {
                        var arrayItem = nestedEventArray[i];
                        if (arrayItem == null) {
                            continue;
                        }

                        RenderElementFragment(arrayItem, buf, level, nestedProp, rendererMetaOptions);
                    }
                }
            }
        }

        private static void RenderElementFragment(
            EventBean eventBean,
            StringBuilder buf,
            int level,
            NestedGetterPair nestedProp,
            RendererMetaOptions rendererMetaOptions)
        {
            Ident(buf, level);
            buf.Append('<');
            buf.Append(nestedProp.Name);
            buf.Append('>');
            buf.Append(Newline);

            RecursiveRender(eventBean, buf, level + 1, nestedProp.Metadata, rendererMetaOptions);

            Ident(buf, level);
            buf.Append("</");
            buf.Append(nestedProp.Name);
            buf.Append('>');
            buf.Append(Newline);
        }

        private void RenderAttInner(
            StringBuilder buf,
            int level,
            EventBean nestedEventBean,
            NestedGetterPair nestedProp)
        {
            Ident(buf, level);
            buf.Append('<');
            buf.Append(nestedProp.Name);

            RenderAttributes(nestedEventBean, buf, nestedProp.Metadata);

            var inner = RenderAttElements(nestedEventBean, level + 1, nestedProp.Metadata);

            if (inner == null || inner.Trim().Length == 0) {
                buf.Append("/>");
                buf.Append(Newline);
            }
            else {
                buf.Append(">");
                buf.Append(Newline);
                buf.Append(inner);

                Ident(buf, level);
                buf.Append("</");
                buf.Append(nestedProp.Name);
                buf.Append('>');
                buf.Append(Newline);
            }
        }

        private string GetFirstWord(string rootElementName)
        {
            if (rootElementName == null || rootElementName.Trim().Length == 0) {
                return rootElementName;
            }

            var index = rootElementName.IndexOf(' ');
            if (index < 0) {
                return rootElementName;
            }

            return rootElementName.Substring(0, index);
        }
    }
} // end of namespace