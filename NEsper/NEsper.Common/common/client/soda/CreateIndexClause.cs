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

namespace com.espertech.esper.common.client.soda
{
    /// <summary>Create an index on a named window. </summary>
    [Serializable]
    public class CreateIndexClause
    {
        /// <summary>Ctor. </summary>
        public CreateIndexClause()
        {
            Columns = new List<CreateIndexColumn>();
        }

        /// <summary>Ctor. </summary>
        /// <param name="indexName">index name</param>
        /// <param name="windowName">named window name</param>
        /// <param name="columns">columns indexed</param>
        public CreateIndexClause(
            string indexName,
            string windowName,
            List<CreateIndexColumn> columns)
        {
        }

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="indexName">index name</param>
        /// <param name="windowName">named window name</param>
        /// <param name="columns">columns indexed</param>
        /// <param name="isUnique">if set to <c>true</c> [is unique].</param>
        public CreateIndexClause(
            string indexName,
            string windowName,
            ICollection<CreateIndexColumn> columns,
            bool isUnique)
        {
            IndexName = indexName;
            WindowName = windowName;
            Columns = columns;
            IsUnique = isUnique;
        }


        /// <summary>Ctor. </summary>
        /// <param name="windowName">is the name of the window to create</param>
        /// <param name="indexName">index name</param>
        /// <param name="properties">properties to index</param>
        public CreateIndexClause(
            string indexName,
            string windowName,
            string[] properties)
            : this(indexName, windowName, properties, false)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="CreateIndexClause" /> class.
        /// </summary>
        /// <param name="windowName">is the name of the window to create</param>
        /// <param name="indexName">index name</param>
        /// <param name="properties">properties to index</param>
        /// <param name="isUnique">if set to <c>true</c> [is unique].</param>
        public CreateIndexClause(
            string indexName,
            string windowName,
            string[] properties,
            bool isUnique)
        {
            Columns = new List<CreateIndexColumn>();
            IndexName = indexName;
            WindowName = windowName;
            IsUnique = isUnique;
            foreach (var prop in properties) {
                Columns.Add(new CreateIndexColumn(prop));
            }
        }

        /// <summary>Returns index name. </summary>
        /// <value>name of index</value>
        public string IndexName { get; set; }

        /// <summary>Returns window name. </summary>
        /// <value>name of window</value>
        public string WindowName { get; set; }

        /// <summary>Returns columns. </summary>
        /// <value>columns</value>
        public ICollection<CreateIndexColumn> Columns { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether this instance is unique.
        /// </summary>
        /// <value><c>true</c> if this instance is unique; otherwise, <c>false</c>.</value>
        public bool IsUnique { get; set; }

        /// <summary>Creates a clause to create a named window. </summary>
        /// <param name="windowName">is the name of the named window</param>
        /// <param name="properties">properties to index</param>
        /// <param name="indexName">name of index</param>
        /// <returns>create variable clause</returns>
        public static CreateIndexClause Create(
            string indexName,
            string windowName,
            params string[] properties)
        {
            return new CreateIndexClause(indexName, windowName, properties);
        }

        /// <summary>Creates a clause to create a named window. </summary>
        /// <param name="windowName">is the name of the named window</param>
        /// <param name="properties">properties to index</param>
        /// <param name="indexName">name of index</param>
        /// <param name="unique">for unique index</param>
        /// <returns>create variable clause</returns>
        public static CreateIndexClause Create(
            bool unique,
            string indexName,
            string windowName,
            params string[] properties)
        {
            return new CreateIndexClause(indexName, windowName, properties, unique);
        }

        /// <summary>Renders the clause in textual representation. </summary>
        /// <param name="writer">to output to</param>
        public void ToEPL(TextWriter writer)
        {
            writer.Write("create ");
            if (IsUnique) {
                writer.Write("unique ");
            }

            writer.Write("index ");
            writer.Write(IndexName);
            writer.Write(" on ");
            writer.Write(WindowName);
            writer.Write('(');
            var delimiter = "";

            foreach (var prop in Columns) {
                writer.Write(delimiter);
                prop.ToEPL(writer);
                delimiter = ", ";
            }

            writer.Write(')');
        }
    }
}