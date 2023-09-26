///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;

namespace com.espertech.esper.regressionlib.suite.rowrecog
{
    public class RowRecogInvalid : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            var text = "@name('s0') select * from SupportRecogBean " +
                       "match_recognize (" +
                       " measures A as a_array" +
                       " pattern (A+ B)" +
                       " define" +
                       " A as A.theString = B.theString)";
            env.TryInvalidCompile(
                text,
                "Failed to validate condition expression for variable 'A': Failed to validate match-recognize define expression 'A.theString=B.theString': Failed to find a stream named 'B' (did you mean 'A'?) ");

            // invalid after syntax
            text = "select * from SupportRecogBean#keepall " +
                   "match_recognize (" +
                   "  measures A.theString as a" +
                   "  AFTER MATCH SKIP TO OTHER ROW " +
                   "  pattern (A B*) " +
                   "  define " +
                   "    A as A.theString like 'A%'" +
                   ")";
            env.TryInvalidCompile(
                text,
                "Match-recognize AFTER clause must be either AFTER MATCH SKIP TO LAST ROW or AFTER MATCH SKIP TO NEXT ROW or AFTER MATCH SKIP TO CURRENT ROW ");

            // property cannot resolve
            text = "select * from SupportRecogBean#keepall " +
                   "match_recognize (" +
                   "  measures A.theString as a, D.theString as x" +
                   "  pattern (A B*) " +
                   "  define " +
                   "    A as A.theString like 'A%'" +
                   ")";
            env.TryInvalidCompile(
                text,
                "Failed to validate match-recognize measure expression 'D.theString': Failed to resolve property 'D.theString' to a stream or nested property in a stream");

            // property not named
            text = "select * from SupportRecogBean#keepall " +
                   "match_recognize (" +
                   "  measures A.theString, A.theString as xxx" +
                   "  pattern (A B*) " +
                   "  define " +
                   "    A as A.theString like 'A%'" +
                   ")";
            env.TryInvalidCompile(
                text,
                "The measures clause requires that each expression utilizes the AS keyword to assign a column name");

            // grouped property not indexed
            text = "select * from SupportRecogBean#keepall " +
                   "match_recognize (" +
                   "  measures B.theString as b1" +
                   "  pattern (A B*) " +
                   "  define " +
                   "    A as A.theString like 'A%'" +
                   ")";
            env.TryInvalidCompile(
                text,
                "Failed to validate match-recognize measure expression 'B.theString': Failed to resolve property 'B.theString' (property 'B' is an indexed property and requires an index or enumeration method to access values)");

            // define twice
            text = "select * from SupportRecogBean#keepall " +
                   "match_recognize (" +
                   "  measures A.theString as a" +
                   "  pattern (A B*) " +
                   "  define " +
                   "    A as A.theString like 'A%'," +
                   "    A as A.theString like 'A%'" +
                   ")";
            env.TryInvalidCompile(text, "Variable 'A' has already been defined");

            // define for not used variable
            text = "select * from SupportRecogBean#keepall " +
                   "match_recognize (" +
                   "  measures A.theString as a" +
                   "  pattern (A B*) " +
                   "  define " +
                   "    X as X.theString like 'A%'" +
                   ")";
            env.TryInvalidCompile(text, "Variable 'X' does not occur in pattern");

            // define mentions another variable
            text = "select * from SupportRecogBean#keepall " +
                   "match_recognize (" +
                   "  measures A.theString as a" +
                   "  pattern (A B*) " +
                   "  define " +
                   "    A as B.theString like 'A%'" +
                   ")";
            env.TryInvalidCompile(
                text,
                "Failed to validate condition expression for variable 'A': Failed to validate match-recognize define expression 'B.theString like \"A%\"': Failed to find a stream named 'B' (did you mean 'A'?)");

            // aggregation over multiple groups
            text = "select * from SupportRecogBean#keepall " +
                   "match_recognize (" +
                   "  measures sum(A.value+B.value) as mytotal" +
                   "  pattern (A* B*) " +
                   "  define " +
                   "    A as A.theString like 'A%'" +
                   ")";
            env.TryInvalidCompile(
                text,
                "Aggregation functions in the measure-clause must only refer to properties of exactly one group variable returning multiple events");

            // aggregation over no groups
            text = "select * from SupportRecogBean#keepall " +
                   "match_recognize (" +
                   "  measures sum(A.value) as mytotal" +
                   "  pattern (A B*) " +
                   "  define " +
                   "    A as A.theString like 'A%'" +
                   ")";
            env.TryInvalidCompile(
                text,
                "Aggregation functions in the measure-clause must refer to one or more properties of exactly one group variable returning multiple events");

            // aggregation in define
            text = "select * from SupportRecogBean#keepall " +
                   "match_recognize (" +
                   "  measures A.theString as astring" +
                   "  pattern (A B) " +
                   "  define " +
                   "    A as sum(A.value + A.value) > 3000" +
                   ")";
            env.TryInvalidCompile(
                text,
                "Failed to validate condition expression for variable 'A': An aggregate function may not appear in a DEFINE clause");

            // join disallowed
            text = "select * from SupportRecogBean#keepall, SupportRecogBean#keepall " +
                   "match_recognize (" +
                   "  measures A.value as aval" +
                   "  pattern (A B*) " +
                   "  define " +
                   "    A as A.theString like 'A%'" +
                   ")";
            env.TryInvalidCompile(text, "Joins are not allowed when using match-recognize");
        }

        public ISet<RegressionFlag> Flags()
        {
            return Collections.Set(RegressionFlag.INVALIDITY);
        }
    }
} // end of namespace