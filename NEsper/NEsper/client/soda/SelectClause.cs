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
    /// A select-clause consists of a list of selection elements (expressions, Wildcard(s), stream wildcard 
    /// and the like) and an optional stream selector. 
    /// </summary>
    [Serializable]
    public class SelectClause
    {
        private IList<SelectClauseElement> _selectList;
        private StreamSelector _streamSelector;

        /// <summary>Ctor. </summary>
        public SelectClause()
        {
        }

        /// <summary>Ctor. </summary>
        /// <param name="streamSelector">selects the stream</param>
        /// <param name="selectList">is a list of elements in the select-clause</param>
        protected SelectClause(StreamSelector streamSelector,
                               List<SelectClauseElement> selectList)
        {
            _streamSelector = streamSelector;
            _selectList = selectList;
        }

        /// <summary>Creates a wildcard select-clause, additional expressions can still be added. </summary>
        /// <returns>select-clause</returns>
        public static SelectClause CreateWildcard()
        {
            var selectList = new List<SelectClauseElement>();
            selectList.Add(new SelectClauseWildcard());
            return new SelectClause(soda.StreamSelector.ISTREAM_ONLY, selectList);
        }

        /// <summary>Creates an empty select-clause to be added to via add methods. </summary>
        /// <returns>select-clause</returns>
        public static SelectClause Create()
        {
            return new SelectClause(soda.StreamSelector.ISTREAM_ONLY, new List<SelectClauseElement>());
        }

        /// <summary>Creates a select-clause consisting of a list of property names. </summary>
        /// <param name="propertyNames">is the names of properties to select</param>
        /// <returns>select-clause</returns>
        public static SelectClause Create(params String[] propertyNames)
        {
            var selectList = new List<SelectClauseElement>();
            foreach (String name in propertyNames)
            {
                selectList.Add(new SelectClauseExpression(new PropertyValueExpression(name)));
            }
            return new SelectClause(soda.StreamSelector.ISTREAM_ONLY, selectList);
        }

        /// <summary>Creates a select-clause with a single stream wildcard selector (e.g. select streamName.* from MyStream as streamName) </summary>
        /// <param name="streamName">is the name given to a stream</param>
        /// <returns>select-clause</returns>
        public static SelectClause CreateStreamWildcard(String streamName)
        {
            var selectList = new List<SelectClauseElement>();
            selectList.Add(new SelectClauseStreamWildcard(streamName, null));
            return new SelectClause(soda.StreamSelector.ISTREAM_ONLY, selectList);
        }

        /// <summary>Creates a wildcard select-clause, additional expressions can still be added. </summary>
        /// <param name="streamSelector">can be used to select insert or remove streams</param>
        /// <returns>select-clause</returns>
        public static SelectClause CreateWildcard(StreamSelector streamSelector)
        {
            var selectList = new List<SelectClauseElement>();
            selectList.Add(new SelectClauseWildcard());
            return new SelectClause(streamSelector, selectList);
        }

        /// <summary>Creates an empty select-clause. </summary>
        /// <param name="streamSelector">can be used to select insert or remove streams</param>
        /// <returns>select-clause</returns>
        public static SelectClause Create(StreamSelector streamSelector)
        {
            return new SelectClause(streamSelector, new List<SelectClauseElement>());
        }

        /// <summary>Creates a select-clause consisting of a list of property names. </summary>
        /// <param name="propertyNames">is the names of properties to select</param>
        /// <param name="streamSelector">can be used to select insert or remove streams</param>
        /// <returns>select-clause</returns>
        public static SelectClause Create(StreamSelector streamSelector,
                                          params String[] propertyNames)
        {
            var selectList = new List<SelectClauseElement>();
            foreach (String name in propertyNames)
            {
                selectList.Add(new SelectClauseExpression(new PropertyValueExpression(name)));
            }
            return new SelectClause(streamSelector, selectList);
        }

        /// <summary>Adds property names to be selected. </summary>
        /// <param name="propertyNames">is a list of property names to add</param>
        /// <returns>clause</returns>
        public SelectClause Add(params String[] propertyNames)
        {
            foreach (String name in propertyNames)
            {
                _selectList.Add(new SelectClauseExpression(new PropertyValueExpression(name)));
            }
            return this;
        }

        /// <summary>Adds a single property name and an "as"-asName for the column. </summary>
        /// <param name="propertyName">name of property</param>
        /// <param name="asName">is the "as"-asName for the column</param>
        /// <returns>clause</returns>
        public SelectClause AddWithAsProvidedName(String propertyName,
                                                  String asName)
        {
            _selectList.Add(new SelectClauseExpression(new PropertyValueExpression(propertyName), asName));
            return this;
        }

        /// <summary>Adds an expression to the select clause. </summary>
        /// <param name="expression">to add</param>
        /// <returns>clause</returns>
        public SelectClause Add(Expression expression)
        {
            _selectList.Add(new SelectClauseExpression(expression));
            return this;
        }

        /// <summary>Adds an expression to the select clause and an "as"-asName for the column. </summary>
        /// <param name="expression">to add</param>
        /// <param name="asName">is the "as"-provided for the column</param>
        /// <returns>clause</returns>
        public SelectClause Add(Expression expression,
                                String asName)
        {
            _selectList.Add(new SelectClauseExpression(expression, asName));
            return this;
        }

        /// <summary>Returns the list of expressions in the select clause. </summary>
        /// <value>list of expressions with column names</value>
        public IList<SelectClauseElement> SelectList
        {
            get { return _selectList; }
            set { _selectList = value; }
        }

        /// <summary>Adds to the select-clause a stream wildcard selector (e.g. select streamName.* from MyStream as streamName) </summary>
        /// <param name="streamName">is the name given to a stream</param>
        /// <returns>select-clause</returns>
        public SelectClause AddStreamWildcard(String streamName)
        {
            _selectList.Add(new SelectClauseStreamWildcard(streamName, null));
            return this;
        }

        /// <summary>Adds to the select-clause a  wildcard selector (e.g. select * from MyStream as streamName) </summary>
        /// <returns>select-clause</returns>
        public SelectClause AddWildcard()
        {
            _selectList.Add(new SelectClauseWildcard());
            return this;
        }

        /// <summary>Adds to the select-clause a stream wildcard selector with column name (e.g. select streamName.* as colName from MyStream as streamName) </summary>
        /// <param name="streamName">is the name given to a stream</param>
        /// <param name="columnName">the name given to the column</param>
        /// <returns>select-clause</returns>
        public SelectClause AddStreamWildcard(String streamName,
                                              String columnName)
        {
            _selectList.Add(new SelectClauseStreamWildcard(streamName, columnName));
            return this;
        }

        /// <summary>Returns the stream selector. </summary>
        /// <returns>stream selector</returns>
        public StreamSelector GetStreamSelector()
        {
            return _streamSelector;
        }

        /// <summary>Sets the stream selector. </summary>
        /// <param name="streamSelector">stream selector to set</param>
        public SelectClause SetStreamSelector(StreamSelector streamSelector)
        {
            _streamSelector = streamSelector;
            return this;
        }

        /// <summary>
        /// Gets or sets the stream selector.
        /// </summary>
        /// <value>The stream selector.</value>
        public StreamSelector StreamSelector
        {
            get { return _streamSelector; }
            set { _streamSelector = value; }
        }


        /// <summary>Add a select expression element. </summary>
        /// <param name="selectClauseElements">to add</param>
        public void AddElements(IEnumerable<SelectClauseElement> selectClauseElements)
        {
            _selectList.AddAll(selectClauseElements);
        }

        /// <summary>
        /// Renders the clause in textual representation.
        /// </summary>
        /// <param name="writer">to output to</param>
        /// <param name="formatter">for NewLine-whitespace formatting</param>
        /// <param name="isTopLevel">to indicate if this select-clause is inside other clauses.</param>
        /// <param name="andDelete">indicator whether select and delete.</param>
        public void ToEPL(TextWriter writer, EPStatementFormatter formatter, Boolean isTopLevel, Boolean andDelete)
        {
            formatter.BeginSelect(writer, isTopLevel);
            writer.Write("select ");
            if (andDelete)
            {
                writer.Write("and delete ");
            }
            if (IsDistinct)
            {
                writer.Write("distinct ");
            }
            if (_streamSelector == soda.StreamSelector.ISTREAM_ONLY)
            {
                // the default, no action
            }
            else if (_streamSelector == soda.StreamSelector.RSTREAM_ONLY)
            {
                writer.Write("rstream ");
            }
            else if (_streamSelector == soda.StreamSelector.RSTREAM_ISTREAM_BOTH)
            {
                writer.Write("irstream ");
            }

            if (_selectList != null && !_selectList.IsEmpty())
            {
                String delimiter = "";
                foreach (SelectClauseElement element in _selectList)
                {
                    writer.Write(delimiter);
                    element.ToEPLElement(writer);
                    delimiter = ", ";
                }
            }
            else
            {
                writer.Write('*');
            }
        }

        /// <summary>Returns indicator whether distinct or not. </summary>
        /// <value>distinct indicator</value>
        public bool IsDistinct { get; set; }

        /// <summary>Sets distinct </summary>
        /// <param name="distinct">distinct indicator</param>
        /// <returns>the select clause</returns>
        public SelectClause Distinct(bool distinct)
        {
            IsDistinct = distinct;
            return this;
        }

        /// <summary>Sets distinct to true. </summary>
        /// <returns>the select clause</returns>
        public SelectClause Distinct()
        {
            IsDistinct = true;
            return this;
        }
    }
}