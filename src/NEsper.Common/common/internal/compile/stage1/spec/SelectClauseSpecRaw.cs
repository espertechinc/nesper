///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace com.espertech.esper.common.@internal.compile.stage1.spec
{
    /// <summary>
    /// Encapsulates the parsed select expressions in a select-clause in an EPL statement.
    /// </summary>
    [Serializable]
    public class SelectClauseSpecRaw
    {
        private readonly List<SelectClauseElementRaw> _selectClauseElements;

        /// <summary>
        /// Ctor.
        /// </summary>
        public SelectClauseSpecRaw()
        {
            _selectClauseElements = new List<SelectClauseElementRaw>();
            IsDistinct = false;
        }

        /// <summary>
        /// Adds an select expression within the select clause.
        /// </summary>
        /// <param name="element">is the expression to add</param>
        public void Add(SelectClauseElementRaw element)
        {
            _selectClauseElements.Add(element);
        }

        /// <summary>
        /// Adds select expressions within the select clause.
        /// </summary>
        /// <param name="elements">is the expressions to add</param>
        public void AddAll(IEnumerable<SelectClauseElementRaw> elements)
        {
            _selectClauseElements.AddRange(elements);
        }

        /// <summary>
        /// Gets a value indicating whether this instance is only wildcard.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is only wildcard; otherwise, <c>false</c>.
        /// </value>
        public bool IsOnlyWildcard =>
            _selectClauseElements.Count == 1 &&
            _selectClauseElements[0] is SelectClauseElementWildcard;

        /// <summary>
        /// Gets or sets a value indicating whether this instance is distinct.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is distinct; otherwise, <c>false</c>.
        /// </value>
        public bool IsDistinct { get; set; }

        /// <summary>Returns the list of select expressions. </summary>
        /// <returns>list of expressions</returns>
        public IList<SelectClauseElementRaw> SelectExprList => _selectClauseElements;

        /// <summary>Returns true if the select clause contains at least one wildcard. </summary>
        /// <returns>true if clause contains wildcard, false if not</returns>
        public bool IsUsingWildcard {
            get {
                foreach (var element in _selectClauseElements) {
                    if (element is SelectClauseElementWildcard) {
                        return true;
                    }
                }

                return false;
            }
        }
    }
}