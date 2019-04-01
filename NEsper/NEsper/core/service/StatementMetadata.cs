///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;


namespace com.espertech.esper.core.service
{
    /// <summary>Statement metadata. </summary>
    [Serializable]
    public class StatementMetadata 
    {
        /// <summary>Ctor. </summary>
        /// <param name="statementType">the type of statement</param>
        public StatementMetadata(StatementType statementType)
        {
            StatementType = statementType;
        }

        /// <summary>Returns the statement type. </summary>
        /// <value>statement type.</value>
        public StatementType StatementType { get; private set; }
    }
}
