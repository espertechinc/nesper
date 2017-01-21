///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.client.deploy
{
    /// <summary>
    /// Statement level information for deployed modules.
    /// </summary>
    [Serializable]
    public class DeploymentInformationItem
    {
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="statementName">name of statement</param>
        /// <param name="expression">EPL text</param>
        public DeploymentInformationItem(String statementName, String expression)
        {
            StatementName = statementName;
            Expression = expression;
        }

        /// <summary>
        /// Returns statement name.
        /// </summary>
        /// <value>name</value>
        public string StatementName { get; private set; }

        /// <summary>
        /// Returns EPL text.
        /// </summary>
        /// <value>expression</value>
        public string Expression { get; private set; }

        public override String ToString()
        {
            return "name '" + StatementName + "' " +
                   " expression " + Expression;
        }
    }
}
