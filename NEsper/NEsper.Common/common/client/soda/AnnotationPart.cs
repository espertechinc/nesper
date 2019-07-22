///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;

using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.client.soda
{
    /// <summary>
    ///     Represents a single annotation.
    /// </summary>
    public class AnnotationPart
    {
        // Map of identifier name and value can be any of the following:
        //      <"value"|attribute name, constant|array of value (Object[])| AnnotationPart

        /// <summary>
        ///     Ctor.
        /// </summary>
        public AnnotationPart()
        {
        }

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="name">of annotation</param>
        public AnnotationPart(string name)
        {
            Name = name;
        }

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="name">name of annotation</param>
        /// <param name="attributes">are the attribute values</param>
        public AnnotationPart(
            string name,
            IList<AnnotationAttribute> attributes)
        {
            Name = name;
            Attributes = attributes;
        }

        /// <summary>
        ///     Returns the internal expression id assigned for tools to identify the expression.
        /// </summary>
        /// <returns>object name</returns>
        public string TreeObjectName { get; set; }

        /// <summary>
        ///     Returns annotation interface class name.
        /// </summary>
        /// <returns>name of class, can be fully qualified</returns>
        public string Name { get; set; }

        /// <summary>
        ///     Returns annotation attributes.
        /// </summary>
        /// <returns>the attribute values</returns>
        public IList<AnnotationAttribute> Attributes { get; private set; } = new List<AnnotationAttribute>();

        /// <summary>
        ///     Copy annotation values.
        /// </summary>
        /// <param name="other">to copy</param>
        public void Copy(AnnotationPart other)
        {
            Name = other.Name;
            Attributes = other.Attributes;
        }

        /// <summary>
        ///     Add value.
        /// </summary>
        /// <param name="value">to add</param>
        public void AddValue(object value)
        {
            Attributes.Add(new AnnotationAttribute("value", value));
        }

        /// <summary>
        ///     Add named value.
        /// </summary>
        /// <param name="name">name</param>
        /// <param name="value">value</param>
        public void AddValue(
            string name,
            object value)
        {
            Attributes.Add(new AnnotationAttribute(name, value));
        }

        /// <summary>
        ///     Print.
        /// </summary>
        /// <param name="writer">to print to</param>
        /// <param name="annotations">annotations</param>
        /// <param name="formatter">for newline-whitespace formatting</param>
        public static void ToEPL(
            TextWriter writer,
            IList<AnnotationPart> annotations,
            EPStatementFormatter formatter)
        {
            if (annotations == null || annotations.IsEmpty()) {
                return;
            }

            foreach (var part in annotations) {
                if (part.Name == null) {
                    continue;
                }

                formatter.BeginAnnotation(writer);
                part.ToEPL(writer);
            }
        }

        /// <summary>
        ///     Print part.
        /// </summary>
        /// <param name="writer">to write to</param>
        public void ToEPL(TextWriter writer)
        {
            writer.Write("@");
            writer.Write(Name);

            if (Attributes.IsEmpty()) {
                return;
            }

            if (Attributes.Count == 1) {
                if (Attributes[0].Name == null || Attributes[0].Name.Equals("value")) {
                    writer.Write("(");
                    ToEPL(writer, Attributes[0].Value);
                    writer.Write(")");
                    return;
                }
            }

            var delimiter = "";
            writer.Write("(");
            foreach (var attribute in Attributes) {
                if (attribute.Value == null) {
                    return;
                }

                writer.Write(delimiter);
                writer.Write(attribute.Name);
                writer.Write("=");
                ToEPL(writer, attribute.Value);
                delimiter = ",";
            }

            writer.Write(")");
        }

        private void ToEPL(
            TextWriter writer,
            object second)
        {
            if (second is string) {
                writer.Write("'");
                writer.Write(second.ToString());
                writer.Write("'");
            }
            else if (second is AnnotationPart) {
                ((AnnotationPart) second).ToEPL(writer);
            }
            else if (second.GetType().IsEnum) {
                writer.Write(second.GetType().Name);
                writer.Write(".");
                writer.Write(second.ToString());
            }
            else if (second.GetType().IsArray) {
                var array = (Array) second;
                var delimiter = "";
                writer.Write("{");
                for (var i = 0; i < array.Length; i++) {
                    writer.Write(delimiter);
                    ToEPL(writer, array.GetValue(i));
                    delimiter = ",";
                }

                writer.Write("}");
            }
            else {
                writer.Write(second.ToString());
            }
        }

        /// <summary>
        ///     Returns a name annotation for use with a statement object model
        /// </summary>
        /// <param name="name">the name to use</param>
        /// <returns>name annotation part</returns>
        public static AnnotationPart NameAnnotation(string name)
        {
            var part = new AnnotationPart();
            part.Name = "name";
            part.AddValue(name);
            return part;
        }
    }
} // end of namespace