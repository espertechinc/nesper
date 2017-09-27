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
    /// Create a named window, defining the parameter of the named window such as window name and data window view Name(s).
    /// </summary>
    [Serializable]
    public class CreateWindowClause  {
        /// <summary>Ctor.</summary>
        public CreateWindowClause()
        {
            Columns = new List<SchemaColumnDesc>();
            Views = new List<View>();
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="windowName">is the name of the window to create</param>
        /// <param name="viewArr">is the list of data window views</param>
        public CreateWindowClause(string windowName, View[] viewArr) {
            Columns = new List<SchemaColumnDesc>();
            WindowName = windowName;
            Views = new List<View>();
            if (viewArr != null) {
                Views.AddAll(viewArr);
            }
        }
    
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="windowName">is the name of the window to create</param>
        /// <param name="views">is a list of data window views</param>
        public CreateWindowClause(string windowName, IList<View> views) {
            Columns = new List<SchemaColumnDesc>();
            WindowName = windowName;
            Views = views;
        }
    
        /// <summary>
        /// Creates a clause to create a named window.
        /// </summary>
        /// <param name="windowName">is the name of the named window</param>
        /// <param name="view">is a data window view</param>
        /// <returns>create window clause</returns>
        public static CreateWindowClause Create(string windowName, View view) {
            return new CreateWindowClause(windowName, new View[]{view});
        }
    
        /// <summary>
        /// Creates a clause to create a named window.
        /// </summary>
        /// <param name="windowName">is the name of the named window</param>
        /// <param name="views">is the data window views</param>
        /// <returns>create window clause</returns>
        public static CreateWindowClause Create(string windowName, params View[] views) {
            return new CreateWindowClause(windowName, views);
        }
    
        /// <summary>
        /// Adds an un-parameterized view to the named window.
        /// </summary>
        /// <param name="namespace">is the view namespace, for example "win" for most data windows</param>
        /// <param name="name">is the view name, for example "length" for a length window</param>
        /// <returns>named window creation clause</returns>
        public CreateWindowClause AddView(string @namespace, string name) {
            Views.Add(View.Create(@namespace, name));
            return this;
        }
    
        /// <summary>
        /// Adds an un-parameterized view to the named window.
        /// </summary>
        /// <param name="name">is the view name, for example "length" for a length window</param>
        /// <returns>named window creation clause</returns>
        public CreateWindowClause AddView(string name) {
            Views.Add(View.Create(null, name));
            return this;
        }
    
        /// <summary>
        /// Adds a parameterized view to the named window.
        /// </summary>
        /// <param name="namespace">is the view namespace, for example "win" for most data windows</param>
        /// <param name="name">is the view name, for example "length" for a length window</param>
        /// <param name="parameters">is a list of view parameters</param>
        /// <returns>named window creation clause</returns>
        public CreateWindowClause AddView(string @namespace, string name, List<Expression> parameters) {
            Views.Add(View.Create(@namespace, name, parameters));
            return this;
        }
    
        /// <summary>
        /// Adds a parameterized view to the named window.
        /// </summary>
        /// <param name="name">is the view name, for example "length" for a length window</param>
        /// <param name="parameters">is a list of view parameters</param>
        /// <returns>named window creation clause</returns>
        public CreateWindowClause AddView(string name, List<Expression> parameters) {
            Views.Add(View.Create(name, parameters));
            return this;
        }
    
        /// <summary>
        /// Adds a parameterized view to the named window.
        /// </summary>
        /// <param name="namespace">is the view namespace, for example "win" for most data windows</param>
        /// <param name="name">is the view name, for example "length" for a length window</param>
        /// <param name="parameters">is a list of view parameters</param>
        /// <returns>named window creation clause</returns>
        public CreateWindowClause AddView(string @namespace, string name, params Expression[] parameters) {
            Views.Add(View.Create(@namespace, name, parameters));
            return this;
        }
    
        /// <summary>
        /// Adds a parameterized view to the named window.
        /// </summary>
        /// <param name="name">is the view name, for example "length" for a length window</param>
        /// <param name="parameters">is a list of view parameters</param>
        /// <returns>named window creation clause</returns>
        public CreateWindowClause AddView(string name, params Expression[] parameters) {
            Views.Add(View.Create(null, name, parameters));
            return this;
        }
    
        /// <summary>
        /// Renders the clause in textual representation.
        /// </summary>
        /// <param name="writer">to output to</param>
        public void ToEPL(TextWriter writer) {
            writer.Write("create window ");
            writer.Write(WindowName);
            ProjectedStream.ToEPLViews(writer, Views);
        }
    
        /// <summary>
        /// Renders the clause in textual representation.
        /// </summary>
        /// <param name="writer">to output to</param>
        public void ToEPLInsertPart(TextWriter writer) {
            if (IsInsert) {
                writer.Write(" insert");
                if (InsertWhereClause != null) {
                    writer.Write(" where ");
                    InsertWhereClause.ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
                }
            }
        }

        /// <summary>
        /// Returns the window name.
        /// </summary>
        /// <value>window name</value>
        public string WindowName { get; set; }

        /// <summary>
        /// Returns the views onto the named window.
        /// </summary>
        /// <value>named window data views</value>
        public IList<View> Views { get; set; }

        /// <summary>
        /// Returns true if inserting from another named window, false if not.
        /// </summary>
        /// <value>insert from named window</value>
        public bool IsInsert { get; set; }

        /// <summary>
        /// Filter expression for inserting from another named window, or null if not inserting from another named window.
        /// </summary>
        /// <value>filter expression</value>
        public Expression InsertWhereClause { get; set; }

        /// <summary>
        /// Returns all columns for use when create-table syntax is used to define the named window type.
        /// </summary>
        /// <value>columns</value>
        public IList<SchemaColumnDesc> Columns { get; set; }

        /// <summary>
        /// Adds a column for use when create-table syntax is used to define the named window type.
        /// </summary>
        /// <param name="col">column to add</param>
        public void AddColumn(SchemaColumnDesc col)
        {
            Columns.Add(col);
        }

        public CreateWindowClause SetColumns(IList<SchemaColumnDesc> value)
        {
            Columns = value;
            return this;
        }

        public CreateWindowClause SetIsInsert(bool value)
        {
            IsInsert = value;
            return this;
        }

        public CreateWindowClause SetInsertWhereClause(Expression value)
        {
            InsertWhereClause = value;
            return this;
        }
    
        /// <summary>
        /// To-EPL for create-table syntax.
        /// </summary>
        /// <param name="writer">to use</param>
        public void ToEPLCreateTablePart(TextWriter writer) {
            string delimiter = "";
            writer.Write('(');
            foreach (SchemaColumnDesc col in Columns) {
                writer.Write(delimiter);
                col.ToEPL(writer);
                delimiter = ", ";
            }
            writer.Write(')');
        }
    }
} // end of namespace
