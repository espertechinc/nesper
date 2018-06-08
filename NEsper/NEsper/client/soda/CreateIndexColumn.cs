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

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.client.soda
{
    /// <summary>Create an index on a named window.</summary>
    [Serializable]
    public class CreateIndexColumn
    {
        private IList<Expression> columns;
        private String indexType;
        private IList<Expression> parameters;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateIndexColumn"/> class.
        /// </summary>
        /// <param name="columnName">Name of the column.</param>
        public CreateIndexColumn(String columnName) : this(columnName, CreateIndexColumnType.HASH)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateIndexColumn"/> class.
        /// </summary>
        /// <param name="columnName">Name of the column.</param>
        /// <param name="type">The index type.</param>
        public CreateIndexColumn(String columnName, CreateIndexColumnType type)
        {
            this.columns = Collections.SingletonList<Expression>(Expressions.Property(columnName));
            this.indexType = type.GetName();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateIndexColumn" /> class.
        /// </summary>
        /// <param name="columns">The columns.</param>
        /// <param name="type">The type.</param>
        /// <param name="parameters">The parameters.</param>

        public CreateIndexColumn(IList<Expression> columns, String type, IList<Expression> parameters)
        {
            this.columns = columns;
            this.indexType = type;
            this.parameters = parameters;
        }

        /// <summary>
        /// Renders the clause in textual representation.
        /// </summary>
        /// <param name="writer">The writer.</param>

        public virtual void ToEPL(TextWriter writer)
        {
            if (columns.Count > 1)
            {
                writer.Write("(");
            }

            ExpressionBase.ToPrecedenceFreeEPL(columns, writer);
            if (columns.Count > 1)
            {
                writer.Write(")");
            }

            if ((indexType != null) && !String.Equals(indexType, CreateIndexColumnType.HASH.GetName(), StringComparison.InvariantCultureIgnoreCase))
            {
                writer.Write(' ');
                writer.Write(indexType.ToLowerInvariant());
            }

            if (!parameters.IsEmpty())
            {
                writer.Write("(");
                ExpressionBase.ToPrecedenceFreeEPL(parameters, writer);
                writer.Write(")");
            }
        }

        /// <summary>
        /// Returns index column expressions
        /// </summary>
        public IList<Expression> Columns
        {
            get => columns;
            set => columns = value;
        }

        /// <summary>
        /// Gets or sets the type of the index.
        /// </summary>
        /// <value>
        /// The type of the index.
        /// </value>
        public String IndexType
        {
            get => indexType;
            set => indexType = value;
        }

        /// <summary>
        /// Gets or sets the parameters.
        /// </summary>
        /// <value>
        /// The parameters.
        /// </value>
        public IList<Expression> Parameters
        {
            get => parameters;
            set => parameters = value;
        }
    }
} // end of namespace