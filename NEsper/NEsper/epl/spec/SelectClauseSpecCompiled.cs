///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Linq;

namespace com.espertech.esper.epl.spec
{
    /// <summary>
    /// Encapsulates the parsed select expressions in a select-clause in an EPL statement.
    /// </summary>
    public class SelectClauseSpecCompiled
    {
        private readonly static SelectClauseElementCompiled[] EMPTY = new SelectClauseElementCompiled[0];
    
        private readonly bool _isDistinct;
    	private SelectClauseElementCompiled[] _selectClauseElements;
    
        /// <summary>Ctor. </summary>
        /// <param name="isDistinct">indicates distinct or not</param>
        public SelectClauseSpecCompiled(bool isDistinct)
    	{
    		_selectClauseElements = EMPTY;
            _isDistinct = isDistinct;
        }
    
        /// <summary>Ctor. </summary>
        /// <param name="selectList">for a populates list of select expressions</param>
        /// <param name="isDistinct">indicates distinct or not</param>
        public SelectClauseSpecCompiled(SelectClauseElementCompiled[] selectList, bool isDistinct)
    	{
            _selectClauseElements = selectList;
            _isDistinct = isDistinct;
    	}
    
        public void SetSelectExprList(params SelectClauseElementWildcard[] selectClauseElements)
        {
            _selectClauseElements = selectClauseElements;
        }

        /// <summary>Returns the list of select expressions. </summary>
        /// <value>list of expressions</value>
        public SelectClauseElementCompiled[] SelectExprList
        {
            get { return _selectClauseElements; }
            set { _selectClauseElements = value; }
        }

        /// <summary>Returns true if the select clause contains at least one wildcard. </summary>
        /// <value>true if clause contains wildcard, false if not</value>
        public bool IsUsingWildcard
        {
            get { return _selectClauseElements.OfType<SelectClauseElementWildcard>().Any(); }
        }

        /// <summary>Returns indictor whether distinct or not. </summary>
        /// <value>distinct indicator</value>
        public bool IsDistinct
        {
            get { return _isDistinct; }
        }
    }
}
