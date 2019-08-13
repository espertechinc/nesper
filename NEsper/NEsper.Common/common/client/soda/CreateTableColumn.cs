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
    [Serializable]
    public class CreateTableColumn
    {
        private string columnName;
        private Expression optionalExpression;
        private string optionalTypeName;
        private IList<AnnotationPart> annotations;
        private bool? primaryKey;

        /// <summary>Initializes a new instance of the <see cref="CreateTableColumn"/> class.</summary>
        /// <param name="columnName">Name of the column.</param>
        /// <param name="optionalExpression">The optional expression.</param>
        /// <param name="optionalTypeName">Name of the optional type.</param>
        /// <param name="annotations">The annotations.</param>
        /// <param name="primaryKey">The primary key.</param>
        public CreateTableColumn(
            string columnName,
            Expression optionalExpression,
            string optionalTypeName,
            IList<AnnotationPart> annotations,
            bool? primaryKey)
        {
            this.columnName = columnName;
            this.optionalExpression = optionalExpression;
            this.optionalTypeName = optionalTypeName;
            this.annotations = annotations;
            this.primaryKey = primaryKey;
        }

        /// <summary>
        ///   <para>
        ///  Initializes a new instance of the <see cref="CreateTableColumn"/> class.
        /// </para>
        /// </summary>
        public CreateTableColumn()
        {
        }

        /// <summary>
        /// The table column name
        /// </summary>
        public string ColumnName
        {
            get => columnName;
            set => columnName = value;
        }

        /// <summary>
        /// The aggregation expression, if the type of the column is aggregation,
        /// or null if a type name is provided instead.
        /// </summary>
        public Expression OptionalExpression
        {
            get => optionalExpression;
            set => optionalExpression = value;
        }

        /// <summary>
        /// Returns the type name, or null if the column is an aggregation and an
        /// aggregation expression is provided instead.
        /// </summary>
        public string OptionalTypeName
        {
            get => optionalTypeName;
            set => optionalTypeName = value;
        }

        /// <summary>
        /// Returns optional annotations, or null if there are none.
        /// </summary>
        public IList<AnnotationPart> Annotations
        {
            get => annotations;
            set => annotations = value;
        }

        /// <summary>
        /// Returns indicator whether the column is a primary key.
        /// </summary>
        public bool? PrimaryKey
        {
            get => primaryKey;
            set => primaryKey = value;
        }

        /// <summary>
        /// Render create-table column
        /// </summary>
        /// <param name="writer">writer to render to</param>
        public void ToEPL(TextWriter writer)
        {
            writer.Write(columnName);
            writer.Write(" ");
            if (optionalExpression != null)
            {
                optionalExpression.ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
            }
            else
            {
                writer.Write(optionalTypeName);
                if (primaryKey ?? false)
                {
                    writer.Write(" primary key");
                }
            }

            if (annotations != null && !annotations.IsEmpty())
            {
                writer.Write(" ");
                string delimiter = "";
                foreach (AnnotationPart part in annotations)
                {
                    if (part.Name == null)
                    {
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