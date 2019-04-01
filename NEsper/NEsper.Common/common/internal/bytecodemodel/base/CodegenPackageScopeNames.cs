///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.bytecodemodel.@base
{
    public static class CodegenPackageScopeNames
    {
        public const string AGG_MR = "aggTop_mr_";

        public static string AggTop()
        {
            return "aggTop";
        }

        public static string AggView(int streamNumber)
        {
            return "aggview_" + streamNumber;
        }

        public static string Previous(int streamNumber)
        {
            return "prev_" + streamNumber;
        }

        public static string PreviousMatchRecognize()
        {
            return "prevmr";
        }

        public static string Prior(int streamNumber)
        {
            return "prior_" + streamNumber;
        }

        public static string PriorSubquery(int subqueryNum)
        {
            return "prior_subq_" + subqueryNum;
        }

        public static string AnyField(int number)
        {
            return "f" + number;
        }

        public static string AnySubstitutionParam(int number)
        {
            return "p" + number;
        }

        public static string SubqueryResultFuture(int subselectNumber)
        {
            return "subq_" + subselectNumber;
        }

        public static string PreviousSubquery(int subqueryNum)
        {
            return "prev_subq_" + subqueryNum;
        }

        public static string AggregationSubquery(int subqueryNum)
        {
            return "aggTop_subq_" + subqueryNum;
        }

        public static string AggregationMatchRecognize(int streamNum)
        {
            return AGG_MR + streamNum;
        }

        public static string ClassPostfixAggregationForView(int streamNumber)
        {
            return "view_" + streamNumber;
        }

        public static string ClassPostfixAggregationForSubquery(int subqueryNumber)
        {
            return "subq_" + subqueryNumber;
        }

        public static string TableAccessResultFuture(int tableAccessNumber)
        {
            return "ta_" + tableAccessNumber;
        }
    }
} // end of namespace