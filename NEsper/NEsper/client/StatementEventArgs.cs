///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;

namespace com.espertech.esper.client
{
    public class StatementEventArgs : EventArgs
    {
        private readonly EPStatement _statement;

        /// <summary>
        /// Gets the statement.
        /// </summary>
        /// <value>The statement.</value>
        public EPStatement Statement
        {
            get { return _statement;  }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StatementEventArgs"/> class.
        /// </summary>
        /// <param name="_statement">The _statement.</param>
        public StatementEventArgs(EPStatement _statement)
        {
            this._statement = _statement;
        }
    }
}
