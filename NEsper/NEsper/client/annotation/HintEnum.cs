///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.annotation;

namespace com.espertech.esper.client.annotation
{
    /// <summary>
    /// Enumeration of hint values. Since hints may be a comma-separate list in a single
    /// @Hint annotation they are listed as enumeration values here.
    /// </summary>
    public enum HintEnum
    {
        /// <summary>
        /// For use with match_recognize, iterate-only matching.
        /// </summary>
        ITERATE_ONLY,

        /// <summary>
        /// For use with group-by, disabled reclaim groups.
        /// </summary>
        DISABLE_RECLAIM_GROUP,

        /// <summary>
        /// For use with group-by and std:groupwin, reclaim groups for unbound streams based on time.
        /// The number of seconds after which a groups is reclaimed if inactive.
        /// </summary>
        RECLAIM_GROUP_AGED,

        /// <summary>
        /// For use with group-by and std:groupwin, reclaim groups for unbound streams based on time,
        /// this number is the frequency in seconds at which a sweep occurs for aged groups, if not
        /// provided then the sweep frequency is the same number as the age.
        /// </summary>
        RECLAIM_GROUP_FREQ,

        /// <summary>
        /// For use with create-named-window statements only, to indicate that statements that subquery
        /// the named window use named window data structures (unless the subquery statement specifies
        /// below DISBABLE hint and as listed below).
        /// <para>
        /// By default and if this hint is not specified or subqueries specify a stream filter on a
        /// named window, subqueries use statement-local data structures representing named window
        /// contents (table, index). Such data structure is maintained by consuming the named window
        /// insert and remove stream.
        /// </para>
        /// </summary>
        ENABLE_WINDOW_SUBQUERY_INDEXSHARE,

        /// <summary>
        /// If ENABLE_WINDOW_SUBQUERY_INDEXSHARE is not specified for a named window (the default)
        /// then this instruction is ignored.
        /// <para>
        /// For use with statements that subquery a named window and that benefit from a statement-local
        /// data structure representing named window contents (table, index), maintained through consuming
        /// the named window insert and remove stream.
        /// </para>
        /// </summary>
        DISABLE_WINDOW_SUBQUERY_INDEXSHARE,

        /// <summary>
        /// For use with subqueries and on-select, on-merge, on-Update and on-delete to specify the
        /// query engine neither build an implicit index nor use an existing index, always performing
        /// a full table scan.
        /// </summary>
        SET_NOINDEX,

        /// <summary>
        /// For use with join query plans to force a nested iteration plan.
        /// </summary>
        FORCE_NESTED_ITER,

        /// <summary>
        /// For use with join query plans to indicate preferance of the merge-join query plan.
        /// </summary>
        PREFER_MERGE_JOIN,

        /// <summary>
        /// For use everywhere where indexes are used (subquery, joins, fire-and-forget, on-select etc.), index hint.
        /// </summary>
        INDEX,

        /// <summary>
        /// For use where query planning applies.
        /// </summary>
        EXCLUDE_PLAN,

        /// <summary>
        /// For use everywhere where unique data window are used
        /// </summary>
        DISABLE_UNIQUE_IMPLICIT_IDX,

        /// <summary>
        /// For use when filter expression optimization may widen the filter expression
        /// </summary>
        MAX_FILTER_WIDTH,

        /// <summary>
        ///  For use everywhere where unique data window are used
        /// </summary>
        DISABLE_WHEREEXPR_MOVETO_FILTER,

        /// <summary>
        /// For use with output rate limiting to enable certain optimization that may however change output.
        /// </summary>
        ENABLE_OUTPUTLIMIT_OPT
    }

    public static class HintEnumExtensions
    {
        /// <summary>Returns the constant. </summary>
        /// <returns>constant</returns>
        public static String GetValue(this HintEnum @enum)
        {
            switch (@enum)
            {
                case HintEnum.ITERATE_ONLY:
                    return "ITERATE_ONLY";
                case HintEnum.DISABLE_RECLAIM_GROUP:
                    return "DISABLE_RECLAIM_GROUP";
                case HintEnum.RECLAIM_GROUP_AGED:
                    return "RECLAIM_GROUP_AGED";
                case HintEnum.RECLAIM_GROUP_FREQ:
                    return "RECLAIM_GROUP_FREQ";
                case HintEnum.ENABLE_WINDOW_SUBQUERY_INDEXSHARE:
                    return "ENABLE_WINDOW_SUBQUERY_INDEXSHARE";
                case HintEnum.DISABLE_WINDOW_SUBQUERY_INDEXSHARE:
                    return "DISABLE_WINDOW_SUBQUERY_INDEXSHARE";
                case HintEnum.SET_NOINDEX:
                    return "SET_NOINDEX";
                case HintEnum.FORCE_NESTED_ITER:
                    return "FORCE_NESTED_ITER";
                case HintEnum.PREFER_MERGE_JOIN:
                    return "PREFER_MERGE_JOIN";
                case HintEnum.INDEX:
                    return "INDEX";
                case HintEnum.EXCLUDE_PLAN:
                    return "EXCLUDE_PLAN";
                case HintEnum.DISABLE_UNIQUE_IMPLICIT_IDX:
                    return "DISABLE_UNIQUE_IMPLICIT_IDX";
                case HintEnum.MAX_FILTER_WIDTH:
                    return "MAX_FILTER_WIDTH";
                case HintEnum.DISABLE_WHEREEXPR_MOVETO_FILTER:
                    return "DISABLE_WHEREEXPR_MOVETO_FILTER";
                case HintEnum.ENABLE_OUTPUTLIMIT_OPT:
                    return "ENABLE_OUTPUTLIMIT_OPT";
            }

            throw new ArgumentException();
        }

        /// <summary>True if the hint accepts params. </summary>
        /// <returns>indicator</returns>
        public static bool IsAcceptsParameters(this HintEnum @enum)
        {
            switch (@enum)
            {
                case HintEnum.ITERATE_ONLY:
                    return false;
                case HintEnum.DISABLE_RECLAIM_GROUP:
                    return false;
                case HintEnum.RECLAIM_GROUP_AGED:
                    return true;
                case HintEnum.RECLAIM_GROUP_FREQ:
                    return true;
                case HintEnum.ENABLE_WINDOW_SUBQUERY_INDEXSHARE:
                    return false;
                case HintEnum.DISABLE_WINDOW_SUBQUERY_INDEXSHARE:
                    return false;
                case HintEnum.SET_NOINDEX:
                    return false;
                case HintEnum.FORCE_NESTED_ITER:
                    return false;
                case HintEnum.PREFER_MERGE_JOIN:
                    return false;
                case HintEnum.INDEX:
                    return false;
                case HintEnum.EXCLUDE_PLAN:
                    return false;
                case HintEnum.DISABLE_UNIQUE_IMPLICIT_IDX:
                    return false;
                case HintEnum.MAX_FILTER_WIDTH:
                    return true;
                case HintEnum.DISABLE_WHEREEXPR_MOVETO_FILTER:
                    return false;
                case HintEnum.ENABLE_OUTPUTLIMIT_OPT:
                    return false;
            }

            throw new ArgumentException();
        }

        /// <summary>True if the hint requires params. </summary>
        /// <returns>indicator</returns>
        public static bool IsRequiresParameters(this HintEnum @enum)
        {
            if (IsAcceptsParameters(@enum))
                return true;

            switch (@enum)
            {
                case HintEnum.ITERATE_ONLY:
                    return false;
                case HintEnum.DISABLE_RECLAIM_GROUP:
                    return false;
                case HintEnum.RECLAIM_GROUP_AGED:
                    return true;
                case HintEnum.RECLAIM_GROUP_FREQ:
                    return true;
                case HintEnum.ENABLE_WINDOW_SUBQUERY_INDEXSHARE:
                    return false;
                case HintEnum.DISABLE_WINDOW_SUBQUERY_INDEXSHARE:
                    return false;
                case HintEnum.SET_NOINDEX:
                    return false;
                case HintEnum.FORCE_NESTED_ITER:
                    return false;
                case HintEnum.PREFER_MERGE_JOIN:
                    return false;
                case HintEnum.INDEX:
                    return false;
                case HintEnum.EXCLUDE_PLAN:
                    return false;
                case HintEnum.DISABLE_UNIQUE_IMPLICIT_IDX:
                    return false;
                case HintEnum.MAX_FILTER_WIDTH:
                    return true;
                case HintEnum.DISABLE_WHEREEXPR_MOVETO_FILTER:
                    return false;
                case HintEnum.ENABLE_OUTPUTLIMIT_OPT:
                    return false;
            }

            throw new ArgumentException();
        }

        public static bool IsRequiresParentheses(this HintEnum @enum)
        {
            switch (@enum)
            {
                case HintEnum.ITERATE_ONLY:
                    return false;
                case HintEnum.DISABLE_RECLAIM_GROUP:
                    return false;
                case HintEnum.RECLAIM_GROUP_AGED:
                    return false;
                case HintEnum.RECLAIM_GROUP_FREQ:
                    return false;
                case HintEnum.ENABLE_WINDOW_SUBQUERY_INDEXSHARE:
                    return false;
                case HintEnum.DISABLE_WINDOW_SUBQUERY_INDEXSHARE:
                    return false;
                case HintEnum.SET_NOINDEX:
                    return false;
                case HintEnum.FORCE_NESTED_ITER:
                    return false;
                case HintEnum.PREFER_MERGE_JOIN:
                    return false;
                case HintEnum.INDEX:
                    return true;
                case HintEnum.EXCLUDE_PLAN:
                    return true;
                case HintEnum.DISABLE_UNIQUE_IMPLICIT_IDX:
                    return false;
                case HintEnum.MAX_FILTER_WIDTH:
                    return false;
                case HintEnum.DISABLE_WHEREEXPR_MOVETO_FILTER:
                    return false;
                case HintEnum.ENABLE_OUTPUTLIMIT_OPT:
                    return false;
            }

            throw new ArgumentException();
        }

        /// <summary>
        /// Check if the hint is present in the attributes provided.
        /// </summary>
        /// <param name="enum">The @enum.</param>
        /// <param name="attributes">the attributes to inspect</param>
        /// <returns>indicator</returns>
        public static HintAttribute GetHint(this HintEnum @enum, IEnumerable<Attribute> attributes)
        {
            if (attributes == null)
            {
                return null;
            }

            foreach (HintAttribute hintAnnotation in attributes.OfType<HintAttribute>())
            {
                try
                {
                    var setOfHints = ValidateGetListed(hintAnnotation);
                    if (setOfHints.ContainsKey(@enum))
                    {
                        return hintAnnotation;
                    }
                }
                catch (AttributeException e)
                {
                    throw new EPException("Invalid hint: " + e.Message, e);
                }
            }
            return null;
        }

        /// <summary>Validate a hint attribute ensuring it contains only recognized hints. </summary>
        /// <param name="attribute">to validate</param>
        /// <throws>AnnotationException if an invalid text was found</throws>
        public static IDictionary<HintEnum, IList<String>> ValidateGetListed(Attribute attribute)
        {
            if (!(attribute is HintAttribute))
            {
                return new EmptyDictionary<HintEnum, IList<string>>();
            }

            var hint = (HintAttribute)attribute;
            var hintValueCaseNeutral = hint.Value.Trim();
            var hintValueUppercase = hintValueCaseNeutral.ToUpper();

            foreach (HintEnum val in EnumHelper.GetValues<HintEnum>())
            {
                if ((val.GetValue() == hintValueUppercase) && !val.IsRequiresParentheses())
                {
                    ValidateParameters(val, hint.Value.Trim());
                    IList<string> parameters;
                    if (val.IsAcceptsParameters())
                    {
                        var assignment = GetAssignedValue(hint.Value.Trim(), val.GetValue());
                        if (assignment == null)
                        {
                            parameters = Collections.GetEmptyList<string>();
                        }
                        else
                        {
                            parameters = assignment.AsSingleton();
                        }
                    }
                    else
                    {
                        parameters = Collections.GetEmptyList<string>();
                    }
                    return Collections.SingletonMap(val, parameters);
                }
            }

            var hints = SplitCommaUnlessInParen(hint.Value);
            var listed = new Dictionary<HintEnum, IList<string>>();
            for (int i = 0; i < hints.Length; i++)
            {
                var hintValUppercase = hints[i].Trim().ToUpper();
                var hintValNeutralcase = hints[i].Trim();

                HintEnum? found = null;
                String parameter = null;

                foreach (HintEnum val in EnumHelper.GetValues<HintEnum>())
                {
                    if ((val.GetValue() == hintValUppercase) && !val.IsRequiresParentheses())
                    {
                        found = val;
                        parameter = GetAssignedValue(hint.Value.Trim(), val.GetValue());
                        break;
                    }

                    if (val.IsRequiresParentheses())
                    {
                        int indexOpen = hintValUppercase.IndexOf('(');
                        int indexClosed = hintValUppercase.LastIndexOf(')');
                        if (indexOpen != -1)
                        {
                            var hintNameNoParen = hintValUppercase.Substring(0, indexOpen);
                            if (val.GetValue() == hintNameNoParen)
                            {
                                if (indexClosed == -1 || indexClosed < indexOpen)
                                {
                                    throw new AttributeException("Hint '" + val + "' mismatches parentheses");
                                }
                                if (indexClosed != hintValUppercase.Length - 1)
                                {
                                    throw new AttributeException(
                                        "Hint '" + val + "' has additional text after parentheses");
                                }
                                found = val;
                                parameter = hintValNeutralcase.Substring(indexOpen + 1, indexClosed - indexOpen - 1);
                                break;
                            }
                        }
                        if ((hintValUppercase == val.GetValue()) && indexOpen == -1)
                        {
                            throw new AttributeException(
                                "Hint '" + val + "' requires additional parameters in parentheses");
                        }
                    }

                    if (hintValUppercase.IndexOf('=') != -1)
                    {
                        var hintName = hintValUppercase.Substring(0, hintValUppercase.IndexOf('='));
                        if (val.GetValue() == hintName.Trim().ToUpper())
                        {
                            found = val;
                            parameter = GetAssignedValue(hint.Value.Trim(), val.GetValue());
                            break;
                        }
                    }
                }

                if (found == null)
                {
                    var hintName = hints[i].Trim();
                    if (hintName.IndexOf('=') != -1)
                    {
                        hintName = hintName.Substring(0, hintName.IndexOf('='));
                    }
                    throw new AttributeException(
                        "Hint annotation value '" + hintName.Trim() + "' is not one of the known values");
                }
                else
                {
                    if (!found.Value.IsRequiresParentheses())
                    {
                        ValidateParameters(found.Value, hintValUppercase);
                    }
                    var existing = listed.Get(found.Value);
                    if (existing == null)
                    {
                        existing = new List<String>();
                        listed.Put(found.Value, existing);
                    }
                    if (parameter != null)
                    {
                        existing.Add(parameter);
                    }
                }
            }
            return listed;
        }

        private static void ValidateParameters(HintEnum val, String hintVal)
        {
            if (IsRequiresParameters(val))
            {
                if (hintVal.IndexOf('=') == -1)
                {
                    throw new AttributeException("Hint '" + val + "' requires a parameter value");
                }
            }
            if (!IsAcceptsParameters(val))
            {
                if (hintVal.IndexOf('=') != -1)
                {
                    throw new AttributeException("Hint '" + val + "' does not accept a parameter value");
                }
            }
        }

        /// <summary>
        /// Returns hint value.
        /// </summary>
        /// <param name="hintEnum">The hint enum.</param>
        /// <param name="annotation">The annotation to look for.</param>
        /// <returns>hint assigned first value provided</returns>
        public static string GetHintAssignedValue(this HintEnum hintEnum, HintAttribute annotation)
        {
            try
            {
                var hintValues = ValidateGetListed(annotation);
                if (hintValues == null || !hintValues.ContainsKey(hintEnum))
                {
                    return null;
                }
                return hintValues.Get(hintEnum)[0];
            }
            catch (AttributeException ex)
            {
                throw new EPException("Failed to interpret hint annotation: " + ex.Message, ex);
            }
        }


        /// <summary>
        /// Returns all values assigned for a given hint, if any
        /// </summary>
        /// <param name="hintEnum">The hint enum.</param>
        /// <param name="annotations">the to be interogated</param>
        /// <returns>
        /// hint assigned values or null if none found
        /// </returns>
        public static IList<String> GetHintAssignedValues(this HintEnum hintEnum, IEnumerable<Attribute> annotations)
        {
            IList<String> allHints = null;
            try
            {
                foreach (Attribute annotation in annotations)
                {
                    var hintValues = ValidateGetListed(annotation);
                    if (hintValues == null || !hintValues.ContainsKey(hintEnum))
                    {
                        continue;
                    }
                    if (allHints == null)
                    {
                        allHints = hintValues.Get(hintEnum);
                    }
                    else
                    {
                        allHints.AddAll(hintValues.Get(hintEnum));
                    }
                }
            }
            catch (AttributeException ex)
            {
                throw new EPException("Failed to interpret hint annotation: " + ex.Message, ex);
            }
            return allHints;
        }

        private static String GetAssignedValue(String value, String enumValue)
        {
            String valMixed = value.Trim();
            String val = valMixed.ToUpper();

            if (!val.Contains(","))
            {
                if (val.IndexOf('=') == -1)
                {
                    return null;
                }

                String hintName = val.Substring(0, val.IndexOf('='));
                if (hintName != enumValue)
                {
                    return null;
                }
                return valMixed.Substring(val.IndexOf('=') + 1);
            }

            String[] hints = valMixed.Split(',');
            foreach (var hint in hints)
            {
                int indexOfEquals = hint.IndexOf('=');
                if (indexOfEquals == -1)
                {
                    continue;
                }

                val = hint.Substring(0, indexOfEquals).Trim().ToUpper();
                if (val != enumValue)
                {
                    continue;
                }

                var strValue = hint.Substring(indexOfEquals + 1).Trim();
                if (strValue.Length == 0)
                {
                    return null;
                }

                return strValue;
            }
            return null;
        }

        /// <summary>
        /// Split a line of comma-separated values allowing parenthesis.
        /// </summary>
        /// <param name="line">The line to split.</param>
        /// <returns></returns>
        public static String[] SplitCommaUnlessInParen(string line)
        {
            var nestingLevelParen = 0;
            var lastComma = -1;
            var parts = new List<String>();
            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (c == '(')
                {
                    nestingLevelParen++;
                }
                if (c == ')')
                {
                    if (nestingLevelParen == 0)
                    {
                        throw new EPException("Close parenthesis ')' found but none open");
                    }
                    nestingLevelParen--;
                }
                if (c == ',' && nestingLevelParen == 0)
                {
                    var part = line.Substring(lastComma + 1, i - lastComma - 1);
                    if (!string.IsNullOrWhiteSpace(part))
                    {
                        parts.Add(part);
                    }
                    lastComma = i;
                }
            }
            var lastPart = line.Substring(lastComma + 1);
            if (!string.IsNullOrWhiteSpace(lastPart))
            {
                parts.Add(lastPart);
            }

            return parts.ToArray();
        }
    }
}
