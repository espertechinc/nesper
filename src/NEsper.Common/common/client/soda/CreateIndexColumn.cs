///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.client.soda
{
    /// <summary>Create an index on a named window.</summary>
    public class CreateIndexColumn
    {
        private IList<Expression> _columns;
        private string _indexType;
        private IList<Expression> _parameters;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateIndexColumn"/> class.
        /// </summary>
        /// <param name="columnName">Name of the column.</param>
        public CreateIndexColumn(string columnName)
            : this(columnName, CreateIndexColumnType.HASH)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateIndexColumn"/> class.
        /// </summary>
        /// <param name="columnName">Name of the column.</param>
        /// <param name="type">The index type.</param>
        public CreateIndexColumn(
            string columnName,
            CreateIndexColumnType type)
        {
            _columns = Collections.SingletonList<Expression>(Expressions.Property(columnName));
            _indexType = type.GetName();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateIndexColumn" /> class.
        /// </summary>
        /// <param name="columns">The columns.</param>
        /// <param name="indexType">The type.</param>
        /// <param name="parameters">The parameters.</param>
        [JsonConstructor]
        public CreateIndexColumn(
            IList<Expression> columns,
            string indexType,
            IList<Expression> parameters)
        {
            _columns = columns;
            _indexType = indexType;
            _parameters = parameters;
        }

        /// <summary>
        /// Renders the clause in textual representation.
        /// </summary>
        /// <param name="writer">The writer.</param>
        public virtual void ToEPL(TextWriter writer)
        {
            if (_columns.Count > 1) {
                writer.Write("(");
            }

            ExpressionBase.ToPrecedenceFreeEPL(_columns, writer);
            if (_columns.Count > 1) {
                writer.Write(")");
            }

            if (_indexType != null &&
                !string.Equals(
                    _indexType,
                    CreateIndexColumnType.HASH.GetName(),
                    StringComparison.InvariantCultureIgnoreCase)) {
                writer.Write(' ');
                writer.Write(_indexType.ToLowerInvariant());
            }

            if (!_parameters.IsEmpty()) {
                writer.Write("(");
                ExpressionBase.ToPrecedenceFreeEPL(_parameters, writer);
                writer.Write(")");
            }
        }

        /// <summary>
        /// Returns index column expressions
        /// </summary>
        public IList<Expression> Columns {
            get => _columns;
            set => _columns = value;
        }

        /// <summary>
        /// Gets or sets the type of the index.
        /// </summary>
        /// <value>
        /// The type of the index.
        /// </value>
        public string IndexType {
            get => _indexType;
            set => _indexType = value;
        }

        /// <summary>
        /// Gets or sets the parameters.
        /// </summary>
        /// <value>
        /// The parameters.
        /// </value>
        public IList<Expression> Parameters {
            get => _parameters;
            set => _parameters = value;
        }
    }
} // end of namespace