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
using System.Text;

using com.espertech.esper.client;
using com.espertech.esper.client.util;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

using DataMap = System.Collections.Generic.IDictionary<string, object>;

namespace com.espertech.esper.events.util
{
    /// <summary>
    /// Renderer for XML-formatted properties.
    /// </summary>
    public class XMLRendererImpl : XMLEventRenderer
    {
        private static readonly ILog Log =
            LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly String NEWLINE = Environment.NewLine;

        private readonly RendererMeta _meta;
        private readonly XMLRenderingOptions _options;
        private readonly RendererMetaOptions _rendererMetaOptions;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="eventType">type of event to render</param>
        /// <param name="options">rendering options</param>
        public XMLRendererImpl(EventType eventType, XMLRenderingOptions options)
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

        #region XMLEventRenderer Members

        public String Render(String rootElementName, EventBean theEvent)
        {
            if (_options.IsDefaultAsAttribute) {
                return RenderAttributeXML(rootElementName, theEvent);
            }
            return RenderElementXML(rootElementName, theEvent);
        }

        #endregion

        private String RenderElementXML(String rootElementName, EventBean theEvent)
        {
            var buf = new StringBuilder();

            buf.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            buf.Append(NEWLINE);

            buf.Append('<');
            buf.Append(rootElementName);
            buf.Append('>');
            buf.Append(NEWLINE);

            RecursiveRender(theEvent, buf, 1, _meta, _rendererMetaOptions);

            buf.Append("</");
            buf.Append(GetFirstWord(rootElementName));
            buf.Append('>');

            return buf.ToString();
        }

        private String RenderAttributeXML(String rootElementName, EventBean theEvent)
        {
            var buf = new StringBuilder();

            buf.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            buf.Append(NEWLINE);

            buf.Append('<');
            buf.Append(rootElementName);
            RenderAttributes(theEvent, buf, _meta);

            String inner = RenderAttElements(theEvent, 1, _meta);

            if ((inner == null) || (inner.Trim().Length == 0))
            {
                buf.Append("/>");
                buf.Append(NEWLINE);
            }
            else
            {
                buf.Append(">");
                buf.Append(NEWLINE);
                buf.Append(inner);
                buf.Append("</");
                buf.Append(GetFirstWord(rootElementName));
                buf.Append('>');
            }

            return buf.ToString();
        }

        private String RenderAttElements(EventBean theEvent, int level, RendererMeta meta)
        {
            var buf = new StringBuilder();

            GetterPair[] indexProps = meta.IndexProperties;
            foreach (GetterPair indexProp in indexProps.OrderBy(prop => prop.Name)) {
                var value = indexProp.Getter.Get(theEvent);
                if (value == null) {
                    continue;
                }

                if (value is string) {
                    continue;
                }

                var asArray = value as Array;
                if (asArray == null) {
                    Log.Warn("Property '" + indexProp.Name + "' returned a non-array object");
                    continue;
                }

                for (int i = 0; i < asArray.Length; i++) {
                    object arrayItem = asArray.GetValue(i);
                    if (arrayItem == null) {
                        continue;
                    }

                    Ident(buf, level);
                    buf.Append('<');
                    buf.Append(indexProp.Name);
                    buf.Append('>');

                    if (_rendererMetaOptions.Renderer == null)
                    {
                        indexProp.Output.Render(arrayItem, buf);
                    }
                    else
                    {
                        EventPropertyRendererContext context = _rendererMetaOptions.RendererContext;
                        context.SetStringBuilderAndReset(buf);
                        context.PropertyName = indexProp.Name;
                        context.PropertyValue = arrayItem;
                        context.IndexedPropertyIndex = i;
                        context.DefaultRenderer = indexProp.Output;
                        _rendererMetaOptions.Renderer.Render(context);
                    }
                    
                    buf.Append("</");
                    buf.Append(indexProp.Name);
                    buf.Append('>');
                    buf.Append(NEWLINE);
                }
            }

            GetterPair[] mappedProps = meta.MappedProperties;
            foreach (GetterPair mappedProp in mappedProps.OrderBy(prop => prop.Name))
            {
                object value = mappedProp.Getter.Get(theEvent);

                if ((value != null) && (!(value is DataMap))) {
                    Log.Warn("Property '" + mappedProp.Name + "' expected to return Map and returned " + value.GetType() +
                             " instead");
                    continue;
                }

                Ident(buf, level);
                buf.Append('<');
                buf.Append(mappedProp.Name);

                if (value != null) {
                    var map = (DataMap) value;
                    if (map.IsNotEmpty()) {
                        foreach (var entry in map) {
                            if ((entry.Key == null) || (entry.Value == null)) {
                                continue;
                            }

                            buf.Append(" ");
                            buf.Append(entry.Key);
                            buf.Append("=\"");

                            OutputValueRenderer outputValueRenderer = 
                                OutputValueRendererFactory.GetOutputValueRenderer(entry.Value.GetType(), _rendererMetaOptions);

                            if (_rendererMetaOptions.Renderer == null)
                            {
                                outputValueRenderer.Render(entry.Value, buf);
                            }
                            else
                            {
                                EventPropertyRendererContext context = _rendererMetaOptions.RendererContext;
                                context.SetStringBuilderAndReset(buf);
                                context.PropertyName = mappedProp.Name;
                                context.PropertyValue = entry.Value;
                                context.MappedPropertyKey = entry.Key;
                                context.DefaultRenderer = outputValueRenderer;
                                _rendererMetaOptions.Renderer.Render(context);
                            }

                            buf.Append("\"");
                        }
                    }
                }

                buf.Append("/>");
                buf.Append(NEWLINE);
            }

            NestedGetterPair[] nestedProps = meta.NestedProperties;
            foreach (NestedGetterPair nestedProp in nestedProps.OrderBy(prop => prop.Name))
            {
                object value = nestedProp.Getter.GetFragment(theEvent);

                if (value == null) {
                    continue;
                }

                if (!nestedProp.IsArray) {
                    if (!(value is EventBean)) {
                        Log.Warn("Property '" + nestedProp.Name + "' expected to return EventBean and returned " +
                                 value.GetType() + " instead");
                        buf.Append("null");
                        continue;
                    }
                    var nestedEventBean = (EventBean) value;
                    RenderAttInner(buf, level, nestedEventBean, nestedProp);
                }
                else {
                    if (!(value is EventBean[])) {
                        Log.Warn("Property '" + nestedProp.Name + "' expected to return EventBean[] and returned " +
                                 value.GetType() + " instead");
                        buf.Append("null");
                        continue;
                    }

                    var nestedEventArray = (EventBean[]) value;
                    for (int i = 0; i < nestedEventArray.Length; i++) {
                        EventBean arrayItem = nestedEventArray[i];
                        RenderAttInner(buf, level, arrayItem, nestedProp);
                    }
                }
            }

            return buf.ToString();
        }

        private void RenderAttributes(EventBean theEvent, StringBuilder buf, RendererMeta meta)
        {
            const string delimiter = " ";
            GetterPair[] simpleProps = meta.SimpleProperties;
            foreach (GetterPair simpleProp in simpleProps.OrderBy(prop => prop.Name))
            {
                var value = simpleProp.Getter.Get(theEvent);
                if (value == null) {
                    continue;
                }

                buf.Append(delimiter);
                buf.Append(simpleProp.Name);
                buf.Append("=\"");
                
                if (_rendererMetaOptions.Renderer == null)
                {
                    simpleProp.Output.Render(value, buf);
                }
                else
                {
                    EventPropertyRendererContext context = _rendererMetaOptions.RendererContext;
                    context.SetStringBuilderAndReset(buf);
                    context.PropertyName = simpleProp.Name;
                    context.PropertyValue = value;
                    context.DefaultRenderer = simpleProp.Output;
                    _rendererMetaOptions.Renderer.Render(context);
                } 
                
                buf.Append('"');
            }

            GetterPair[] indexProps = meta.IndexProperties;
            foreach (GetterPair indexProp in indexProps.OrderBy(prop => prop.Name))
            {
                var value = indexProp.Getter.Get(theEvent);
                if (value == null)
                {
                    continue;
                }

                var asString = value as string;
                if (asString != null)
                {
                    buf.Append(delimiter);
                    buf.Append(indexProp.Name);
                    buf.Append("=\"");

                    if (_rendererMetaOptions.Renderer == null)
                    {
                        indexProp.Output.Render(value, buf);
                    }
                    else
                    {
                        EventPropertyRendererContext context = _rendererMetaOptions.RendererContext;
                        context.SetStringBuilderAndReset(buf);
                        context.PropertyName = indexProp.Name;
                        context.PropertyValue = value;
                        context.DefaultRenderer = indexProp.Output;
                        _rendererMetaOptions.Renderer.Render(context);
                    } 

                    buf.Append('"');
                }
            }
        }

        private static void Ident(StringBuilder buf, int level)
        {
            for (int i = 0; i < level; i++) {
                IndentChar(buf);
            }
        }

        private static void IndentChar(StringBuilder buf)
        {
            buf.Append(' ');
            buf.Append(' ');
        }

        private static void RecursiveRender(EventBean theEvent, StringBuilder buf, int level, RendererMeta meta,
                                            RendererMetaOptions rendererMetaOptions)
        {
            GetterPair[] simpleProps = meta.SimpleProperties;
            foreach (GetterPair simpleProp in simpleProps.OrderBy(prop => prop.Name)) {
                var value = simpleProp.Getter.Get(theEvent);
                if (value == null) {
                    continue;
                }

                Ident(buf, level);
                buf.Append('<');
                buf.Append(simpleProp.Name);
                buf.Append('>');

                if (rendererMetaOptions.Renderer == null)
                {
                    simpleProp.Output.Render(value, buf);
                }
                else
                {
                    EventPropertyRendererContext context = rendererMetaOptions.RendererContext;
                    context.SetStringBuilderAndReset(buf);
                    context.PropertyName = simpleProp.Name;
                    context.PropertyValue = value;
                    context.DefaultRenderer = simpleProp.Output;
                    rendererMetaOptions.Renderer.Render(context);
                }
                
                buf.Append("</");
                buf.Append(simpleProp.Name);
                buf.Append('>');
                buf.Append(NEWLINE);
            }

            GetterPair[] indexProps = meta.IndexProperties;
            foreach (GetterPair indexProp in indexProps.OrderBy(prop => prop.Name))
            {
                object value = indexProp.Getter.Get(theEvent);

                if (value == null) {
                    continue;
                }

                if (value is string)
                {
                    Ident(buf, level);

                    buf.Append('<');
                    buf.Append(indexProp.Name);
                    buf.Append('>');

                    if (rendererMetaOptions.Renderer == null)
                    {
                        indexProp.Output.Render(value, buf);
                    }
                    else
                    {
                        EventPropertyRendererContext context = rendererMetaOptions.RendererContext;
                        context.SetStringBuilderAndReset(buf);
                        context.PropertyName = indexProp.Name;
                        context.PropertyValue = value;
                        context.DefaultRenderer = indexProp.Output;
                        rendererMetaOptions.Renderer.Render(context);
                    }

                    buf.Append("</");
                    buf.Append(indexProp.Name);
                    buf.Append('>');
                    buf.Append(NEWLINE);

                    continue;
                }

                var asArray = value as Array;
                if (asArray == null) {
                    Log.Warn("Property '" + indexProp.Name + "' returned a non-array object");
                    continue;
                }

                for (int i = 0; i < asArray.Length; i++) {
                    object arrayItem = asArray.GetValue(i);

                    if (arrayItem == null) {
                        continue;
                    }

                    Ident(buf, level);
                    buf.Append('<');
                    buf.Append(indexProp.Name);
                    buf.Append('>');

                    if (rendererMetaOptions.Renderer == null)
                    {
                        indexProp.Output.Render(arrayItem, buf);
                    }
                    else
                    {
                        EventPropertyRendererContext context = rendererMetaOptions.RendererContext;
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
                    buf.Append(NEWLINE);
                }
            }

            GetterPair[] mappedProps = meta.MappedProperties;
            foreach (GetterPair mappedProp in mappedProps.OrderBy(prop => prop.Name))
            {
                object value = mappedProp.Getter.Get(theEvent);

                if ((value != null) && (!(value is DataMap))) {
                    Log.Warn("Property '" + mappedProp.Name + "' expected to return Map and returned " + value.GetType() +
                             " instead");
                    continue;
                }

                Ident(buf, level);
                buf.Append('<');
                buf.Append(mappedProp.Name);
                buf.Append('>');
                buf.Append(NEWLINE);

                if (value != null) {
                    var map = (DataMap) value;
                    if (map.IsNotEmpty()) {
                        String localDelimiter = "";
                        foreach (var entry in map) {
                            if (entry.Key == null) {
                                continue;
                            }

                            buf.Append(localDelimiter);
                            Ident(buf, level + 1);
                            buf.Append('<');
                            buf.Append(entry.Key);
                            buf.Append('>');

                            if (entry.Value != null) {
                                OutputValueRenderer outputValueRenderer = OutputValueRendererFactory.GetOutputValueRenderer(
                                    entry.Value.GetType(), rendererMetaOptions);
                                if (rendererMetaOptions.Renderer == null)
                                {
                                    outputValueRenderer.Render(entry.Value, buf);
                                }
                                else
                                {
                                    EventPropertyRendererContext context = rendererMetaOptions.RendererContext;
                                    context.SetStringBuilderAndReset(buf);
                                    context.PropertyName = mappedProp.Name;
                                    context.PropertyValue = entry.Value;
                                    context.MappedPropertyKey = entry.Key;
                                    context.DefaultRenderer = outputValueRenderer;
                                    rendererMetaOptions.Renderer.Render(context);
                                }
                            }

                            buf.Append('<');
                            buf.Append(entry.Key);
                            buf.Append('>');
                            localDelimiter = NEWLINE;
                        }
                    }
                }

                buf.Append(NEWLINE);
                Ident(buf, level);
                buf.Append("</");
                buf.Append(mappedProp.Name);
                buf.Append('>');
                buf.Append(NEWLINE);
            }

            var nestedProps = meta.NestedProperties;
            foreach (NestedGetterPair nestedProp in nestedProps.OrderBy(prop => prop.Name))
            {
                var value = nestedProp.Getter.GetFragment(theEvent);
                if (value == null) {
                    continue;
                }

                if (!nestedProp.IsArray) {
                    if (!(value is EventBean)) {
                        Log.Warn("Property '" + nestedProp.Name + "' expected to return EventBean and returned " +
                                 value.GetType() + " instead");
                        buf.Append("null");
                        continue;
                    }
                    RenderElementFragment((EventBean) value, buf, level, nestedProp, rendererMetaOptions);
                }
                else {
                    if (!(value is EventBean[])) {
                        Log.Warn("Property '" + nestedProp.Name + "' expected to return EventBean[] and returned " +
                                 value.GetType() + " instead");
                        buf.Append("null");
                        continue;
                    }

                    var nestedEventArray = (EventBean[]) value;
                    for (int i = 0; i < nestedEventArray.Length; i++) {
                        EventBean arrayItem = nestedEventArray[i];
                        if (arrayItem == null) {
                            continue;
                        }
                        RenderElementFragment(arrayItem, buf, level, nestedProp, rendererMetaOptions);
                    }
                }
            }
        }

        private static void RenderElementFragment(EventBean eventBean, StringBuilder buf, int level,
                                                  NestedGetterPair nestedProp, RendererMetaOptions rendererMetaOptions)
        {
            Ident(buf, level);
            buf.Append('<');
            buf.Append(nestedProp.Name);
            buf.Append('>');
            buf.Append(NEWLINE);

            RecursiveRender(eventBean, buf, level + 1, nestedProp.Metadata, rendererMetaOptions);

            Ident(buf, level);
            buf.Append("</");
            buf.Append(nestedProp.Name);
            buf.Append('>');
            buf.Append(NEWLINE);
        }

        private void RenderAttInner(StringBuilder buf, int level, EventBean nestedEventBean, NestedGetterPair nestedProp)
        {
            Ident(buf, level);
            buf.Append('<');
            buf.Append(nestedProp.Name);

            RenderAttributes(nestedEventBean, buf, nestedProp.Metadata);

            String inner = RenderAttElements(nestedEventBean, level + 1, nestedProp.Metadata);

            if ((inner == null) || (inner.Trim().Length == 0)) {
                buf.Append("/>");
                buf.Append(NEWLINE);
            }
            else {
                buf.Append(">");
                buf.Append(NEWLINE);
                buf.Append(inner);

                Ident(buf, level);
                buf.Append("</");
                buf.Append(nestedProp.Name);
                buf.Append('>');
                buf.Append(NEWLINE);
            }
        }

        private static String GetFirstWord(String rootElementName)
        {
            if ((rootElementName == null) || (rootElementName.Trim().Length == 0)) {
                return rootElementName;
            }
            int index = rootElementName.IndexOf(' ');
            if (index < 0) {
                return rootElementName;
            }
            return rootElementName.Substring(0, index);
        }
    }
}
