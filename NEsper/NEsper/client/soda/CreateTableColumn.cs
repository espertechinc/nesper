///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;

using com.espertech.esper.compat.collections;

namespace com.espertech.esper.client.soda
{
    /// <summary>
    /// Table column in a create-table statement.
    /// </summary>
    [Serializable]
    public class CreateTableColumn
    {
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="columnName">the table column name</param>
        /// <param name="optionalExpression">an optional aggregation expression (exclusive of type name)</param>
        /// <param name="optionalTypeName">a type name (exclusive of aggregation expression)</param>
        /// <param name="optionalTypeIsArray">flag whether type is array</param>
        /// <param name="optionalTypeIsPrimitiveArray">flag whether array of primitive (requires array flag)</param>
        /// <param name="annotations">optional annotations</param>
        /// <param name="primaryKey">flag indicating whether the column is a primary key</param>
        public CreateTableColumn(string columnName, Expression optionalExpression, string optionalTypeName, bool? optionalTypeIsArray, bool? optionalTypeIsPrimitiveArray, IList<AnnotationPart> annotations, bool? primaryKey)
        {
            ColumnName = columnName;
            OptionalExpression = optionalExpression;
            OptionalTypeName = optionalTypeName;
            OptionalTypeIsArray = optionalTypeIsArray;
            OptionalTypeIsPrimitiveArray = optionalTypeIsPrimitiveArray;
            Annotations = annotations;
            PrimaryKey = primaryKey;
        }
    
        /// <summary>
        /// Ctor.
        /// </summary>
        public CreateTableColumn()
        {
        }

        /// <summary>
        /// Returns the table column name
        /// </summary>
        /// <value>column name</value>
        public string ColumnName { get; set; }

        /// <summary>
        /// Returns optional annotations, or null if there are none
        /// </summary>
        /// <value>annotations</value>
        public IList<AnnotationPart> Annotations { get; set; }

        /// <summary>
        /// Returns the aggragtion expression, if the type of the column is aggregation,
        /// or null if a type name is provided instead.
        /// </summary>
        /// <value>expression</value>
        public Expression OptionalExpression { get; set; }

        /// <summary>
        /// Returns the type name, or null if the column is an aggregation and an
        /// aggregation expression is provided instead.
        /// </summary>
        /// <value>type name</value>
        public string OptionalTypeName { get; set; }

        /// <summary>
        /// Returns indicator whether type is an array type, applicable only if a type name is provided
        /// </summary>
        /// <value>array type indicator</value>
        public bool? OptionalTypeIsArray { get; set; }

        /// <summary>
        /// Returns indicator whether the column is a primary key
        /// </summary>
        /// <value>primary key indicator</value>
        public bool? PrimaryKey { get; set; }

        /// <summary>
        /// Returns indicator whether the array is an array of primitives or boxed types (only when a type name is provided and array flag set)
        /// </summary>
        /// <value>primitive array indicator</value>
        public bool? OptionalTypeIsPrimitiveArray { get; set; }

        /// <summary>
        /// RenderAny create-table column
        /// </summary>
        /// <param name="writer">to render to</param>
        public void ToEPL(TextWriter writer)
        {
            writer.Write(ColumnName);
            writer.Write(" ");
            if (OptionalExpression != null) {
                OptionalExpression.ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
            }
            else {
                writer.Write(OptionalTypeName);
                if (OptionalTypeIsArray != null && OptionalTypeIsArray.Value) {
                    if (OptionalTypeIsPrimitiveArray != null && OptionalTypeIsPrimitiveArray.Value) {
                        writer.Write("[primitive]");
                    }
                    else {
                        writer.Write("[]");
                    }
                }
                if (PrimaryKey.GetValueOrDefault()) {
                    writer.Write(" primary key");
                }
            }
            if (Annotations != null && !Annotations.IsEmpty()) {
                writer.Write(" ");
                string delimiter = "";
                foreach (AnnotationPart part in Annotations) {
                    if (part.Name == null) {
                        continue;
                    }
                    writer.Write(delimiter);
                    delimiter = " ";
                    part.ToEPL(writer);
                }
            }
        }
    }
}
