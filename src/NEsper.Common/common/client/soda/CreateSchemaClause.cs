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
    /// <summary>Represents a create-schema syntax for creating a new event type. </summary>
    [Serializable]
    public class CreateSchemaClause
    {
        /// <summary>Ctor. </summary>
        public CreateSchemaClause()
        {
        }

        /// <summary>Ctor. </summary>
        /// <param name="schemaName">name of type</param>
        /// <param name="types">are for model-after, could be multiple when declaring a variant stream, or a single fully-qualified class name</param>
        /// <param name="typeDefinition">type definition</param>
        public CreateSchemaClause(
            string schemaName,
            ICollection<string> types,
            CreateSchemaClauseTypeDef typeDefinition)
        {
            SchemaName = schemaName;
            Types = types;
            TypeDefinition = typeDefinition;
        }

        /// <summary>Ctor. </summary>
        /// <param name="schemaName">name of type</param>
        /// <param name="columns">column definition</param>
        /// <param name="inherits">inherited types, if any</param>
        public CreateSchemaClause(
            string schemaName,
            IList<SchemaColumnDesc> columns,
            ICollection<string> inherits)
        {
            SchemaName = schemaName;
            Columns = columns;
            Inherits = inherits;
        }

        /// <summary>Ctor. </summary>
        /// <param name="schemaName">name of type</param>
        /// <param name="types">are for model-after, could be multiple when declaring a variant stream, or a single fully-qualified class name</param>
        /// <param name="typeDefinition">for variant streams, map or object array</param>
        /// <param name="columns">column definition</param>
        /// <param name="inherits">inherited types, if any</param>
        public CreateSchemaClause(
            string schemaName,
            ICollection<string> types,
            IList<SchemaColumnDesc> columns,
            ICollection<string> inherits,
            CreateSchemaClauseTypeDef typeDefinition)
        {
            SchemaName = schemaName;
            Types = types;
            Columns = columns;
            Inherits = inherits;
            TypeDefinition = typeDefinition;
        }

        /// <summary>Returns the type name, aka. schema name. </summary>
        /// <value>type name</value>
        public string SchemaName { get; set; }

        /// <summary>Returns model-after types, i.e. (fully-qualified) class name or event type Name(s), multiple for variant types. </summary>
        /// <value>type names or class names</value>
        public ICollection<string> Types { get; set; }

        /// <summary>Returns the column definition. </summary>
        /// <value>column def</value>
        public IList<SchemaColumnDesc> Columns { get; set; }

        /// <summary>Returns the names of event types inherited from, if any </summary>
        /// <value>types inherited</value>
        public ICollection<string> Inherits { get; set; }

        /// <summary>
        /// Gets or sets the type definition.
        /// </summary>
        /// <value>The type definition.</value>
        public CreateSchemaClauseTypeDef? TypeDefinition { get; set; }

        /// <summary>Returns the property name of the property providing the start timestamp value. </summary>
        /// <value>start timestamp property name</value>
        public string StartTimestampPropertyName { get; set; }

        /// <summary>Returns the property name of the property providing the end timestamp value. </summary>
        /// <value>end timestamp property name</value>
        public string EndTimestampPropertyName { get; set; }

        /// <summary>Returns the optional set of event type names that properties are copied from. </summary>
        /// <value>copy-from event types</value>
        public ICollection<string> CopyFrom { get; set; }

        /// <summary>
        /// Gets or sets the id of expression assigned by tools.
        /// </summary>
        /// <value>The name of the tree object.</value>
        public string TreeObjectName { get; set; }

        /// <summary>RenderAny as EPL. </summary>
        /// <param name="writer">to output to</param>
        public void ToEPL(TextWriter writer)
        {
            writer.Write("create");
            TypeDefinition?.Write(writer);

            writer.Write(" schema ");
            writer.Write(SchemaName);
            writer.Write(" as ");
            if (Types != null && Types.IsNotEmpty()) {
                var delimiter = "";
                foreach (var type in Types) {
                    writer.Write(delimiter);
                    writer.Write(type);
                    delimiter = ", ";
                }
            }
            else {
                writer.Write("(");
                var delimiter = "";
                foreach (var col in Columns) {
                    writer.Write(delimiter);
                    col.ToEPL(writer);
                    delimiter = ", ";
                }

                writer.Write(")");
            }

            if (Inherits != null && Inherits.IsNotEmpty()) {
                writer.Write(" inherits ");
                var delimiter = "";
                foreach (var name in Inherits) {
                    writer.Write(delimiter);
                    writer.Write(name);
                    delimiter = ", ";
                }
            }

            if (StartTimestampPropertyName != null) {
                writer.Write(" starttimestamp ");
                writer.Write(StartTimestampPropertyName);
            }

            if (EndTimestampPropertyName != null) {
                writer.Write(" endtimestamp ");
                writer.Write(EndTimestampPropertyName);
            }

            if (CopyFrom != null && CopyFrom.IsNotEmpty()) {
                writer.Write(" copyFrom ");
                var delimiter = "";
                foreach (var name in CopyFrom) {
                    writer.Write(delimiter);
                    writer.Write(name);
                    delimiter = ", ";
                }
            }
        }
    }
}