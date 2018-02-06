///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;

using static com.espertech.esper.supportregression.util.SupportMessageAssertUtil;

namespace com.espertech.esper.regression.client
{
    public class ExecClientInvalidSyntaxMsg : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            TryInvalidSyntax(epService, "insert into 7event select * from " + typeof(SupportBeanReservedKeyword).FullName,
                    "Incorrect syntax near '7' at line 1 column 12");
    
            TryInvalidSyntax(epService, "select foo, create from " + typeof(SupportBeanReservedKeyword).FullName,
                    "Incorrect syntax near 'create' (a reserved keyword) at line 1 column 12, please check the select clause");
    
            TryInvalidSyntax(epService, "select * from pattern [",
                    "Unexpected end-of-input at line 1 column 23, please check the pattern expression within the from clause");
    
            TryInvalidSyntax(epService, "select * from A, into",
                    "Incorrect syntax near 'into' (a reserved keyword) at line 1 column 17, please check the from clause");

            TryInvalidSyntax(epService, "select * from pattern[A -> B - C]",
                    "Incorrect syntax near '-' expecting a right angle bracket ']' but found a minus '-' at line 1 column 29, please check the from clause [select * from pattern[A -> B - C]]");

            TryInvalidSyntax(epService, "insert into A (a",
                    "Unexpected end-of-input at line 1 column 16 [insert into A (a]");
    
            TryInvalidSyntax(epService, "select case when 1>2 from A",
                    "Incorrect syntax near 'from' (a reserved keyword) expecting 'then' but found 'from' at line 1 column 21, please check the case expression within the select clause [select case when 1>2 from A]");
    
            TryInvalidSyntax(epService, "select * from A full outer join B on A.field < B.field",
                    "Incorrect syntax near '<' expecting an equals '=' but found a lesser then '<' at line 1 column 45, please check the outer join within the from clause [select * from A full outer join B on A.field < B.field]");
    
            TryInvalidSyntax(epService, "select A.B('aa\") from A",
                    "Unexpected end-of-input at line 1 column 23, please check the select clause [select A.B('aa\") from A]");
    
            TryInvalidSyntax(epService, "select * from A, sql:mydb [\"",
                    "Unexpected end-of-input at line 1 column 28, please check the relational data join within the from clause [select * from A, sql:mydb [\"]");
    
            TryInvalidSyntax(epService, "select * google",
                    "Incorrect syntax near 'google' at line 1 column 9 [");
    
            TryInvalidSyntax(epService, "insert into into",
                    "Incorrect syntax near 'into' (a reserved keyword) at line 1 column 12 [insert into into]");
    
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            SupportMessageAssertUtil.TryInvalid(epService, "on SupportBean select 1",
                    "Error starting statement: Required insert-into clause is not provided, the clause is required for split-stream syntax");
        }
    }
} // end of namespace
