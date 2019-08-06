///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.regressionlib.framework;

using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil;

namespace com.espertech.esper.regressionlib.suite.rowrecog
{
    public class RowRecogInvalid : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            var text = "@Name('s0') select * from SupportRecogBean " +
                       "match_recognize (" +
                       " measures A as a_array" +
                       " pattern (A+ B)" +
                       " define" +
                       " A as A.TheString = B.TheString)";
            TryInvalidCompile(
                env,
                text,
                "Failed to validate condition expression for variable 'A': Failed to validate match-recognize define expression 'A.TheString=B.TheString': Failed to find a stream named 'B' (did you mean 'A'?) ");

            // invalid after syntax
            text = "select * from SupportRecogBean#keepall " +
                   "match_recognize (" +
                   "  measures A.TheString as a" +
                   "  AFTER MATCH SKIP TO OTHER ROW " +
                   "  pattern (A B*) " +
                   "  define " +
                   "    A as A.TheString like 'A%'" +
                   ")";
            TryInvalidCompile(
                env,
                text,
                "Match-recognize AFTER clause must be either AFTER MATCH SKIP TO LAST ROW or AFTER MATCH SKIP TO NEXT ROW or AFTER MATCH SKIP TO CURRENT ROW ");

            // property cannot resolve
            text = "select * from SupportRecogBean#keepall " +
                   "match_recognize (" +
                   "  measures A.TheString as a, D.TheString as x" +
                   "  pattern (A B*) " +
                   "  define " +
                   "    A as A.TheString like 'A%'" +
                   ")";
            TryInvalidCompile(
                env,
                text,
                "Failed to validate match-recognize measure expression 'D.TheString': Failed to resolve property 'D.TheString' to a stream or nested property in a stream");

            // property not named
            text = "select * from SupportRecogBean#keepall " +
                   "match_recognize (" +
                   "  measures A.TheString, A.TheString as xxx" +
                   "  pattern (A B*) " +
                   "  define " +
                   "    A as A.TheString like 'A%'" +
                   ")";
            TryInvalidCompile(
                env,
                text,
                "The measures clause requires that each expression utilizes the AS keyword to assign a column name");

            // grouped property not indexed
            text = "select * from SupportRecogBean#keepall " +
                   "match_recognize (" +
                   "  measures B.TheString as b1" +
                   "  pattern (A B*) " +
                   "  define " +
                   "    A as A.TheString like 'A%'" +
                   ")";
            TryInvalidCompile(
                env,
                text,
                "Failed to validate match-recognize measure expression 'B.TheString': Failed to resolve property 'B.TheString' (property 'B' is an indexed property and requires an index or enumeration method to access values)");

            // define twice
            text = "select * from SupportRecogBean#keepall " +
                   "match_recognize (" +
                   "  measures A.TheString as a" +
                   "  pattern (A B*) " +
                   "  define " +
                   "    A as A.TheString like 'A%'," +
                   "    A as A.TheString like 'A%'" +
                   ")";
            TryInvalidCompile(env, text, "Variable 'A' has already been defined");

            // define for not used variable
            text = "select * from SupportRecogBean#keepall " +
                   "match_recognize (" +
                   "  measures A.TheString as a" +
                   "  pattern (A B*) " +
                   "  define " +
                   "    X as X.TheString like 'A%'" +
                   ")";
            TryInvalidCompile(env, text, "Variable 'X' does not occur in pattern");

            // define mentions another variable
            text = "select * from SupportRecogBean#keepall " +
                   "match_recognize (" +
                   "  measures A.TheString as a" +
                   "  pattern (A B*) " +
                   "  define " +
                   "    A as B.TheString like 'A%'" +
                   ")";
            TryInvalidCompile(
                env,
                text,
                "Failed to validate condition expression for variable 'A': Failed to validate match-recognize define expression 'B.TheString like \"A%\"': Failed to find a stream named 'B' (did you mean 'A'?)");

            // aggregation over multiple groups
            text = "select * from SupportRecogBean#keepall " +
                   "match_recognize (" +
                   "  measures sum(A.Value+B.Value) as mytotal" +
                   "  pattern (A* B*) " +
                   "  define " +
                   "    A as A.TheString like 'A%'" +
                   ")";
            TryInvalidCompile(
                env,
                text,
                "Aggregation functions in the measure-clause must only refer to properties of exactly one group variable returning multiple events");

            // aggregation over no groups
            text = "select * from SupportRecogBean#keepall " +
                   "match_recognize (" +
                   "  measures sum(A.Value) as mytotal" +
                   "  pattern (A B*) " +
                   "  define " +
                   "    A as A.TheString like 'A%'" +
                   ")";
            TryInvalidCompile(
                env,
                text,
                "Aggregation functions in the measure-clause must refer to one or more properties of exactly one group variable returning multiple events");

            // aggregation in define
            text = "select * from SupportRecogBean#keepall " +
                   "match_recognize (" +
                   "  measures A.TheString as astring" +
                   "  pattern (A B) " +
                   "  define " +
                   "    A as sum(A.Value + A.Value) > 3000" +
                   ")";
            TryInvalidCompile(
                env,
                text,
                "Failed to validate condition expression for variable 'A': An aggregate function may not appear in a DEFINE clause");

            // join disallowed
            text = "select * from SupportRecogBean#keepall, SupportRecogBean#keepall " +
                   "match_recognize (" +
                   "  measures A.Value as aval" +
                   "  pattern (A B*) " +
                   "  define " +
                   "    A as A.TheString like 'A%'" +
                   ")";
            TryInvalidCompile(env, text, "Joins are not allowed when using match-recognize");
        }
    }
} // end of namespace