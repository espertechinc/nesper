///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;

using Antlr4.Runtime;

using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compiler.@internal.parse;
using com.espertech.esper.grammar.@internal.util;

using static com.espertech.esper.common.@internal.epl.historical.database.core.
    HistoricalEventViewableDatabaseForgeFactory;

namespace com.espertech.esper.compiler.@internal.util
{
    public class SQLLexer
    {
        private static readonly ILog Log =
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Lexes the sample SQL and inserts a "where 1=0" where-clause.
        /// </summary>
        /// <param name="querySQL">to inspect using lexer</param>
        /// <returns>sample SQL with where-clause inserted</returns>
        /// <throws>ExprValidationException to indicate a lexer problem</throws>
        public static string LexSampleSQL(string querySQL)
        {
            querySQL = querySQL.RegexReplaceAll("\\s\\s+|\\n|\\r", " ");
            var input = CaseChangingCharStreamFactory.Make(querySQL);
            var whereIndex = -1;
            var groupbyIndex = -1;
            var havingIndex = -1;
            var orderByIndex = -1;
            IList<int> unionIndexes = new List<int>();

            var lex = ParseHelper.NewLexer(input);
            var tokens = new CommonTokenStream(lex);
            tokens.Fill();
            var tokenList = tokens.GetTokens();

            for (var i = 0; i < tokenList.Count; i++) {
                var token = tokenList[i];
                if ((token == null) || token.Text == null) {
                    break;
                }

                var text = token.Text.ToLowerInvariant().Trim();
                switch (text) {
                    case "where":
                        whereIndex = token.Column + 1;
                        break;

                    case "group":
                        groupbyIndex = token.Column + 1;
                        break;

                    case "having":
                        havingIndex = token.Column + 1;
                        break;

                    case "order":
                        orderByIndex = token.Column + 1;
                        break;

                    case "union":
                        unionIndexes.Add(token.Column + 1);
                        break;
                }
            }

            // If we have a union, break string into subselects and process each
            if (unionIndexes.Count != 0) {
                var changedSQL = new StringWriter();
                var lastIndex = 0;
                for (var i = 0; i < unionIndexes.Count; i++) {
                    var index = unionIndexes[i];
                    string fragmentX;
                    if (i > 0) {
                        fragmentX = querySQL.Between(lastIndex + 5, index - 1);
                    }
                    else {
                        fragmentX = querySQL.Between(lastIndex, index - 1);
                    }

                    var lexedFragmentX = LexSampleSQL(fragmentX);

                    if (i > 0) {
                        changedSQL.Write("union ");
                    }

                    changedSQL.Write(lexedFragmentX);
                    lastIndex = index - 1;
                }

                // last part after last union
                var fragment = querySQL.Substring(lastIndex + 5);
                var lexedFragment = LexSampleSQL(fragment);
                changedSQL.Write("union ");
                changedSQL.Write(lexedFragment);

                return changedSQL.ToString();
            }

            // Found a where clause, simplest cases
            if (whereIndex != -1) {
                var changedSQL = new StringWriter();
                var prefix = querySQL.Substring(0, whereIndex + 5);
                var suffix = querySQL.Substring(whereIndex + 5);
                changedSQL.Write(prefix);
                changedSQL.Write("1=0 and ");
                changedSQL.Write(suffix);
                return changedSQL.ToString();
            }

            // No where clause, find group-by
            int insertIndex;
            if (groupbyIndex != -1) {
                insertIndex = groupbyIndex;
            }
            else if (havingIndex != -1) {
                insertIndex = havingIndex;
            }
            else if (orderByIndex != -1) {
                insertIndex = orderByIndex;
            }
            else {
                var changedSQL = new StringWriter();
                changedSQL.Write(querySQL);
                changedSQL.Write(" where 1=0 ");
                return changedSQL.ToString();
            }

            try {
                var changedSQL = new StringWriter();
                var prefix = querySQL.Substring(0, insertIndex - 1);
                changedSQL.Write(prefix);
                changedSQL.Write("where 1=0 ");
                var suffix = querySQL.Substring(insertIndex - 1);
                changedSQL.Write(suffix);
                return changedSQL.ToString();
            }
            catch (Exception ex) {
                var text =
                    "Error constructing sample SQL to retrieve metadata for ADO-drivers that don't support metadata, consider using the " +
                    SAMPLE_WHERECLAUSE_PLACEHOLDER +
                    " placeholder or providing a sample SQL";
                Log.Error(text, ex);
                throw new ExprValidationException(text, ex);
            }
        }
    }
} // end of namespace