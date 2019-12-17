///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.client.soda
{
    /// <summary>
    /// Fire-and-forget (on-demand) insert DML.
    /// <para/>
    /// The insert-into clause holds the named window name and column names. The select-clause
    /// list holds the values to be inserted.
    /// </summary>
    [Serializable]
    public class FireAndForgetInsert : FireAndForgetClause
    {
        private bool _useValuesKeyword = true;

        /// <summary>Ctor. </summary>
        /// <param name="useValuesKeyword">whether to use the "values" keyword or whether the syntax is based on select</param>
        public FireAndForgetInsert(bool useValuesKeyword)
        {
            this._useValuesKeyword = useValuesKeyword;
        }

        /// <summary>Ctor. </summary>
        public FireAndForgetInsert()
        {
        }

        /// <summary>Returns indicator whether to use the values keyword. </summary>
        /// <value>indicator</value>
        public bool IsUseValuesKeyword
        {
            get { return _useValuesKeyword; }
            set { this._useValuesKeyword = value; }
        }
    }
}