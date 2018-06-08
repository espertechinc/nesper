///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;

namespace com.espertech.esper.supportregression.subscriber
{
    public class SupportSubscriberRowByRowStaticWStatement
    {
        private static readonly List<object[]> Indicate = new List<object[]>();
        private static readonly List<EPStatement> Statements = new List<EPStatement>();

        public static void Update(EPStatement statement, string theString, int intPrimitive)
        {
            Indicate.Add(
                new object[]
                {
                    theString,
                    intPrimitive
                });
            Statements.Add(statement);
        }

        public static List<object[]> GetIndicate()
        {
            return Indicate;
        }

        public static List<EPStatement> GetStatements()
        {
            return Statements;
        }

        public void Reset()
        {
            Indicate.Clear();
            Statements.Clear();
        }
    }
} // end of namespace
