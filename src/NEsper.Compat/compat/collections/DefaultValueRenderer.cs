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
using System.IO;
using System.Text;

using com.espertech.esper.compat.attributes;

namespace com.espertech.esper.compat.collections
{
    class DefaultValueRenderer : IValueRenderer
    {
        private bool shouldQuoteStrings;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="shouldQuoteStrings"></param>
        public DefaultValueRenderer(bool shouldQuoteStrings)
        {
            this.shouldQuoteStrings = shouldQuoteStrings;
        }

        /// <summary>
        /// Returns true if the renderer should quote strings.
        /// </summary>
        public bool ShouldQuoteStrings => shouldQuoteStrings;

        /// <summary>
        /// Render any value.
        /// </summary>
        /// <param name="value">a value</param>
        /// <param name="textWriter">the text writer to write to</param>
        public void RenderAny(
            object value,
            TextWriter textWriter)
        {
            if (value == null) {
                textWriter.Write("null");
                return;
            }

            if (value is char asChar) {
                textWriter.Write('\'');
                textWriter.Write(asChar);
                textWriter.Write('\'');
                return;
            }

            if (value is string s) {
                if (shouldQuoteStrings) {
                    textWriter.Write('"');
                }

                textWriter.Write(s);
                
                if (shouldQuoteStrings) {
                    textWriter.Write('"');
                }

                return;
            }

            if (value is decimal) {
                var text = value.ToString();
                if (text.IndexOf('.') == -1) {
                    text += ".0";
                }

                textWriter.Write(text);
                textWriter.Write('m');
                return;
            }

            if (value is double) {
                var text = value.ToString();
                if (text.IndexOf('.') == -1) {
                    text += ".0";
                }

                textWriter.Write(text);
                textWriter.Write('d');
                return; // + 'd'
            }

            if (value is float) {
                var text = value.ToString();
                if (text.IndexOf('.') == -1) {
                    text += ".0";
                }

                textWriter.Write(text);
                textWriter.Write('f');
                return;
            }

            if (value is long) {
                textWriter.Write(value.ToString());
                textWriter.Write('L');
                return;
            }

            if (value is int) {
                textWriter.Write(value.ToString());
                return;
            }

            if (value is DateTimeOffset dateTimeOffset) {
                var dateOnly = dateTimeOffset.Date;
                if (dateTimeOffset == dateOnly) {
                    textWriter.Write(dateTimeOffset.ToString("yyyy-MM-dd z"));
                    return;
                }

                if (dateTimeOffset.Millisecond == 0) {
                    textWriter.Write(dateTimeOffset.ToString("yyyy-MM-dd hh:mm:ss z"));
                    return;
                }

                textWriter.Write(dateTimeOffset.ToString("yyyy-MM-dd hh:mm:ss.ffff z"));
                return;
            }

            if (value is DateTime dateTime) {
                var dateOnly = dateTime.Date;
                if (dateTime == dateOnly) {
                    textWriter.Write(dateTime.ToString("yyyy-MM-dd"));
                    return;
                }

                if (dateTime.Millisecond == 0) {
                    textWriter.Write(dateTime.ToString("yyyy-MM-dd hh:mm:ss"));
                    return;
                }

                textWriter.Write(dateTime.ToString("yyyy-MM-dd hh:mm:ss.ffff"));
                return;
            }

            if (value is bool asBool) {
                textWriter.Write(asBool ? "true" : "false");
                return;
            }

            if (value is Array array) {
                Render(array, textWriter);
                return;
            }

            var valueType = value.GetType();
            if (valueType.IsGenericKeyValuePair()) {
                RenderAny(valueType.GetProperty("Key")?.GetValue(value), textWriter);
                textWriter.Write('=');
                RenderAny(valueType.GetProperty("Value")?.GetValue(value), textWriter);
                return;
            }
            
            if (valueType.GetCustomAttributes(typeof(RenderWithToStringAttribute), true).Length > 0) {
                textWriter.Write(value.ToString());
                return;
            }

            if (valueType.IsGenericDictionary()) {
                RenderDictionary((IEnumerable) value, textWriter);
                return;
            }
            
            if (value is IEnumerable enumerable) {
                Render(enumerable, textWriter);
                return;
            }

            textWriter.Write(value.ToString());
        }
        
        /// <summary>
        /// Render any value.
        /// </summary>
        /// <param name="value">a value</param>
        public string RenderAny(object value)
        {
            using (var writer = new StringWriter()) {
                RenderAny(value, writer);
                return writer.ToString();
            }
        }

        /// <summary>
        ///     Renders the array as a string.
        /// </summary>
        /// <param name="array">The array.</param>
        /// <returns></returns>
        public string Render(Array array)
        {
            using (var stringWriter = new StringWriter()) {
                Render(array, stringWriter, ", ", "[]");
                return stringWriter.ToString();
            }
        }

        /// <summary>
        ///     Renders the array as a string.
        /// </summary>
        /// <param name="array">The array.</param>
        /// <param name="textWriter">the destination to write to.</param>
        /// <returns></returns>
        public void Render(
            Array array,
            TextWriter textWriter)
        {
            Render(array, textWriter, ", ", "[]");
        }

        /// <summary>
        ///     Renders the array as a string
        /// </summary>
        /// <param name="array">The array.</param>
        /// <param name="textWriter">Destination for the content.</param>
        /// <param name="itemSeparator">The item separator.</param>
        /// <param name="firstAndLast">The first and last.</param>
        /// <returns></returns>
        public void Render(
            Array array,
            TextWriter textWriter,
            string itemSeparator,
            string firstAndLast)
        {
            var fieldDelimiter = string.Empty;

            textWriter.Write(firstAndLast[0]);

            if (array != null) {
                var length = array.Length;
                for (var ii = 0; ii < length; ii++) {
                    textWriter.Write(fieldDelimiter);
                    textWriter.Write(RenderAny(array.GetValue(ii)));
                    fieldDelimiter = itemSeparator;
                }
            }

            textWriter.Write(firstAndLast[1]);
        }

        /// <summary>
        ///     Renders the array as a string
        /// </summary>
        /// <param name="array">The array.</param>
        /// <param name="itemSeparator">The item separator.</param>
        /// <param name="firstAndLast">The first and last.</param>
        /// <returns></returns>
        public string Render(
            Array array,
            string itemSeparator,
            string firstAndLast)
        {
            using (var stringWriter = new StringWriter()) {
                Render(array, stringWriter, itemSeparator, firstAndLast);
                return stringWriter.ToString();
            }
        }
        
        /// <summary>
        ///     Renders an enumerable source
        /// </summary>
        /// <param name="source">the object to render.</param>
        /// <param name="textWriter">the destination to write to.</param>
        /// <returns></returns>
        public void Render(
            IEnumerable source,
            TextWriter textWriter)
        {
            var fieldDelimiter = string.Empty;

            if (source != null) {
                textWriter.Write('[');

                var sourceEnum = source.GetEnumerator();
                while (sourceEnum.MoveNext()) {
                    textWriter.Write(fieldDelimiter);
                    textWriter.Write(RenderAny(sourceEnum.Current));
                    fieldDelimiter = ", ";
                }

                textWriter.Write(']');
            }
            else {
                textWriter.Write("null");
            }
        }

        /// <summary>
        ///     Renders an enumerable source
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="renderEngine">The render engine.</param>
        /// <returns></returns>
        public string Render(
            IEnumerable source,
            Func<object, string> renderEngine)
        {
            var fieldDelimiter = string.Empty;

            var builder = new StringBuilder();
            builder.Append('[');

            var sourceEnum = source.GetEnumerator();
            while (sourceEnum.MoveNext()) {
                builder.Append(fieldDelimiter);
                builder.Append(renderEngine(sourceEnum.Current));
                fieldDelimiter = ", ";
            }

            builder.Append(']');
            return builder.ToString();
        }
        
        /// <summary>
        ///     Renders an enumerable source
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="renderEngine">The render engine.</param>
        /// <returns></returns>
        private void RenderDictionary(
            IEnumerable source,
            TextWriter textWriter)
        {
            var fieldDelimiter = string.Empty;

            if (source != null) {
                textWriter.Write('{');

                var mapType = source.GetType();
                var keyType = mapType.GetDictionaryKeyType();
                var valType = mapType.GetDictionaryValueType();
                var kvpType = typeof(KeyValuePair<,>).MakeGenericType(keyType, valType);
                var keyProp = kvpType.GetProperty("Key");
                var valProp = kvpType.GetProperty("Value");
                
                var sourceEnum = source.GetEnumerator();
                while (sourceEnum.MoveNext()) {
                    var current = sourceEnum.Current;
                    textWriter.Write(fieldDelimiter);
                    RenderAny(keyProp.GetValue(current), textWriter);
                    textWriter.Write("=");
                    RenderAny(valProp.GetValue(current), textWriter);
                    fieldDelimiter = ", ";
                }

                textWriter.Write('}');
            }
            else {
                textWriter.Write("null");
            }
        }
    }
}