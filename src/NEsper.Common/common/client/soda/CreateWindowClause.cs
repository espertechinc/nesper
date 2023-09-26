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
    ///     Create a named window, defining the parameter of the named window such as window name and data window view name(s).
    /// </summary>
    [Serializable]
    public class CreateWindowClause
    {
        private string asEventTypeName;
        private IList<SchemaColumnDesc> columns = new List<SchemaColumnDesc>();
        private bool insert;
        private Expression insertWhereClause;
        private bool retainUnion;
        private IList<View> views = new List<View>();
        private string windowName;

        /// <summary>
        ///     Ctor.
        /// </summary>
        public CreateWindowClause()
        {
        }

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="windowName">is the name of the window to create</param>
        /// <param name="viewArr">is the list of data window views</param>
        public CreateWindowClause(
            string windowName,
            View[] viewArr)
        {
            this.windowName = windowName;
            views = new List<View>();
            if (viewArr != null) {
                views.AddAll(viewArr);
            }
        }

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="windowName">is the name of the window to create</param>
        /// <param name="views">is a list of data window views</param>
        public CreateWindowClause(
            string windowName,
            IList<View> views)
        {
            this.windowName = windowName;
            this.views = views;
        }

        /// <summary>
        ///     Returns the window name.
        /// </summary>
        /// <returns>window name</returns>
        public string WindowName {
            get => windowName;
            set => windowName = value;
        }

        /// <summary>
        ///     Returns the views onto the named window.
        /// </summary>
        /// <returns>named window data views</returns>
        public IList<View> Views {
            get => views;
            set => views = value;
        }

        /// <summary>
        ///     Returns true if inserting from another named window, false if not.
        /// </summary>
        /// <returns>insert from named window</returns>
        public bool IsInsert {
            get => insert;
            set => insert = value;
        }

        /// <summary>
        ///     Returns true if inserting from another named window, false if not.
        /// </summary>
        /// <returns>insert from named window</returns>
        public bool Insert {
            get => insert;
            set => insert = value;
        }

        /// <summary>
        ///     Filter expression for inserting from another named window, or null if not inserting from another named window.
        /// </summary>
        /// <returns>filter expression</returns>
        public Expression InsertWhereClause {
            get => insertWhereClause;
            set => insertWhereClause = value;
        }

        /// <summary>
        ///     Returns all columns for use when create-table syntax is used to define the named window type.
        /// </summary>
        /// <returns>columns</returns>
        public IList<SchemaColumnDesc> Columns {
            get => columns;
            set => columns = value;
        }

        /// <summary>
        ///     Returns the as-name.
        /// </summary>
        /// <returns>as-name</returns>
        public string AsEventTypeName {
            get => asEventTypeName;
            set => asEventTypeName = value;
        }

        /// <summary>
        ///     Returns the retain-union flag
        /// </summary>
        /// <returns>indicator</returns>
        public bool IsRetainUnion {
            get => retainUnion;
            set => retainUnion = value;
        }

        /// <summary>
        ///     Creates a clause to create a named window.
        /// </summary>
        /// <param name="windowName">is the name of the named window</param>
        /// <param name="view">is a data window view</param>
        /// <returns>create window clause</returns>
        public static CreateWindowClause Create(
            string windowName,
            View view)
        {
            return new CreateWindowClause(windowName, new[] { view });
        }

        /// <summary>
        ///     Creates a clause to create a named window.
        /// </summary>
        /// <param name="windowName">is the name of the named window</param>
        /// <param name="views">is the data window views</param>
        /// <returns>create window clause</returns>
        public static CreateWindowClause Create(
            string windowName,
            params View[] views)
        {
            return new CreateWindowClause(windowName, views);
        }

        /// <summary>
        ///     Adds an un-parameterized view to the named window.
        /// </summary>
        /// <param name="namespace">is the view namespace, for example "win" for most data windows</param>
        /// <param name="name">is the view name, for example "length" for a length window</param>
        /// <returns>named window creation clause</returns>
        public CreateWindowClause AddView(
            string @namespace,
            string name)
        {
            views.Add(View.Create(@namespace, name));
            return this;
        }

        /// <summary>
        ///     Adds an un-parameterized view to the named window.
        /// </summary>
        /// <param name="name">is the view name, for example "length" for a length window</param>
        /// <returns>named window creation clause</returns>
        public CreateWindowClause AddView(string name)
        {
            views.Add(View.Create(null, name));
            return this;
        }

        /// <summary>
        ///     Adds a parameterized view to the named window.
        /// </summary>
        /// <param name="namespace">is the view namespace, for example "win" for most data windows</param>
        /// <param name="name">is the view name, for example "length" for a length window</param>
        /// <param name="parameters">is a list of view parameters</param>
        /// <returns>named window creation clause</returns>
        public CreateWindowClause AddView(
            string @namespace,
            string name,
            IList<Expression> parameters)
        {
            views.Add(View.Create(@namespace, name, parameters));
            return this;
        }

        /// <summary>
        ///     Adds a parameterized view to the named window.
        /// </summary>
        /// <param name="name">is the view name, for example "length" for a length window</param>
        /// <param name="parameters">is a list of view parameters</param>
        /// <returns>named window creation clause</returns>
        public CreateWindowClause AddView(
            string name,
            IList<Expression> parameters)
        {
            views.Add(View.Create(name, parameters));
            return this;
        }

        /// <summary>
        ///     Adds a parameterized view to the named window.
        /// </summary>
        /// <param name="namespace">is the view namespace, for example "win" for most data windows</param>
        /// <param name="name">is the view name, for example "length" for a length window</param>
        /// <param name="parameters">is a list of view parameters</param>
        /// <returns>named window creation clause</returns>
        public CreateWindowClause AddView(
            string @namespace,
            string name,
            params Expression[] parameters)
        {
            views.Add(View.Create(@namespace, name, parameters));
            return this;
        }

        /// <summary>
        ///     Adds a parameterized view to the named window.
        /// </summary>
        /// <param name="name">is the view name, for example "length" for a length window</param>
        /// <param name="parameters">is a list of view parameters</param>
        /// <returns>named window creation clause</returns>
        public CreateWindowClause AddView(
            string name,
            params Expression[] parameters)
        {
            views.Add(View.Create(null, name, parameters));
            return this;
        }

        /// <summary>
        ///     Renders the clause in textual representation.
        /// </summary>
        /// <param name="writer">to output to</param>
        public void ToEPL(TextWriter writer)
        {
            writer.Write("create window ");
            writer.Write(windowName);
            ProjectedStream.ToEPLViews(writer, views);
            if (retainUnion) {
                writer.Write(" retain-union");
            }
        }

        /// <summary>
        ///     Renders the clause in textual representation.
        /// </summary>
        /// <param name="writer">to output to</param>
        public void ToEPLInsertPart(TextWriter writer)
        {
            if (insert) {
                writer.Write(" insert");
                if (insertWhereClause != null) {
                    writer.Write(" where ");
                    insertWhereClause.ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
                }
            }
        }

        /// <summary>
        ///     Sets the window name.
        /// </summary>
        /// <param name="windowName">is the name to set</param>
        public CreateWindowClause WithWindowName(string windowName)
        {
            this.windowName = windowName;
            return this;
        }

        /// <summary>
        ///     Sets flag indicating that an insert from another named window should take place at the time of window creation.
        /// </summary>
        /// <param name="insert">true for insert from another named window</param>
        /// <returns>clause</returns>
        public CreateWindowClause WithInsert(bool insert)
        {
            this.insert = insert;
            return this;
        }

        /// <summary>
        ///     Sets the filter expression for inserting from another named window
        /// </summary>
        /// <param name="insertWhereClause">filter expression</param>
        public CreateWindowClause WithInsertWhereClause(Expression insertWhereClause)
        {
            this.insertWhereClause = insertWhereClause;
            return this;
        }

        /// <summary>
        ///     Sets the views onto the named window.
        /// </summary>
        /// <param name="views">to set</param>
        public CreateWindowClause WithViews(IList<View> views)
        {
            this.views = views;
            return this;
        }

        /// <summary>
        ///     Adds a column for use when create-table syntax is used to define the named window type.
        /// </summary>
        /// <param name="col">column to add</param>
        public CreateWindowClause WithColumn(SchemaColumnDesc col)
        {
            columns.Add(col);
            return this;
        }

        /// <summary>
        ///     Sets the columns for use when create-table syntax is used to define the named window type.
        /// </summary>
        /// <param name="columns">to set</param>
        public CreateWindowClause WithColumns(IList<SchemaColumnDesc> columns)
        {
            this.columns = columns;
            return this;
        }

        /// <summary>
        ///     Sets the as-name.
        /// </summary>
        /// <param name="asEventTypeName">as-name</param>
        /// <returns>itself</returns>
        public CreateWindowClause WithAsEventTypeName(string asEventTypeName)
        {
            this.asEventTypeName = asEventTypeName;
            return this;
        }

        /// <summary>
        ///     Sets the retain-union flag
        /// </summary>
        /// <param name="retainUnion">indicator</param>
        public CreateWindowClause WithRetainUnion(bool retainUnion)
        {
            this.retainUnion = retainUnion;
            return this;
        }

        /// <summary>
        ///     To-EPL for create-table syntax.
        /// </summary>
        /// <param name="writer">to use</param>
        public void ToEPLCreateTablePart(TextWriter writer)
        {
            var delimiter = "";
            writer.Write('(');
            foreach (var col in columns) {
                writer.Write(delimiter);
                col.ToEPL(writer);
                delimiter = ", ";
            }

            writer.Write(')');
        }
    }
} // end of namespace