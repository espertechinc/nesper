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
using com.espertech.esper.common.@internal.epl.annotation;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.client.annotation
{
    public static class HintEnumExtensions
    {
        /// <summary>Returns the constant. </summary>
        /// <returns>constant</returns>
        public static string GetValue(this HintEnum @enum)
        {
            switch (@enum) {
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
                case HintEnum.DISABLE_OUTPUTLIMIT_OPT:
                    return "DISABLE_OUTPUTLIMIT_OPT";
            }

            throw new ArgumentException();
        }

        /// <summary>True if the hint accepts params. </summary>
        /// <returns>indicator</returns>
        public static bool IsAcceptsParameters(this HintEnum @enum)
        {
            switch (@enum) {
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
                case HintEnum.DISABLE_OUTPUTLIMIT_OPT:
                    return false;
            }

            throw new ArgumentException();
        }

        /// <summary>True if the hint requires params. </summary>
        /// <returns>indicator</returns>
        public static bool IsRequiresParameters(this HintEnum @enum)
        {
            if (IsAcceptsParameters(@enum)) {
                return true;
            }

            switch (@enum) {
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
                case HintEnum.DISABLE_OUTPUTLIMIT_OPT:
                    return false;
            }

            throw new ArgumentException();
        }

        public static bool IsRequiresParentheses(this HintEnum @enum)
        {
            switch (@enum) {
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
                case HintEnum.DISABLE_OUTPUTLIMIT_OPT:
                    return false;
            }

            throw new ArgumentException();
        }

        /// <summary>
        ///     Check if the hint is present in the attributes provided.
        /// </summary>
        /// <param name="enum">The @enum.</param>
        /// <param name="attributes">the attributes to inspect</param>
        /// <returns>indicator</returns>
        public static HintAttribute GetHint(
            this HintEnum @enum,
            IEnumerable<Attribute> attributes)
        {
            if (attributes == null) {
                return null;
            }

            foreach (var hintAnnotation in attributes.OfType<HintAttribute>()) {
                try {
                    var setOfHints = ValidateGetListed(hintAnnotation);
                    if (setOfHints.ContainsKey(@enum)) {
                        return hintAnnotation;
                    }
                }
                catch (AnnotationException e) {
                    throw new EPException("Invalid hint: " + e.Message, e);
                }
            }

            return null;
        }

        /// <summary>Validate a hint attribute ensuring it contains only recognized hints. </summary>
        /// <param name="attribute">to validate</param>
        /// <throws>AnnotationException if an invalid text was found</throws>
        public static IDictionary<HintEnum, IList<string>> ValidateGetListed(Attribute attribute)
        {
            if (!(attribute is HintAttribute)) {
                return new EmptyDictionary<HintEnum, IList<string>>();
            }

            var hint = (HintAttribute) attribute;
            var hintValueCaseNeutral = hint.Value.Trim();
            var hintValueUppercase = hintValueCaseNeutral.ToUpper();

            foreach (var val in EnumHelper.GetValues<HintEnum>()) {
                if (val.GetValue() == hintValueUppercase && !val.IsRequiresParentheses()) {
                    ValidateParameters(val, hint.Value.Trim());
                    IList<string> parameters;
                    if (val.IsAcceptsParameters()) {
                        var assignment = GetAssignedValue(hint.Value.Trim(), val.GetValue());
                        if (assignment == null) {
                            parameters = Collections.GetEmptyList<string>();
                        }
                        else {
                            parameters = assignment.AsSingleton();
                        }
                    }
                    else {
                        parameters = Collections.GetEmptyList<string>();
                    }

                    return Collections.SingletonMap(val, parameters);
                }
            }

            var hints = SplitCommaUnlessInParen(hint.Value);
            var listed = new Dictionary<HintEnum, IList<string>>();
            for (var i = 0; i < hints.Length; i++) {
                var hintValUppercase = hints[i].Trim().ToUpper();
                var hintValNeutralcase = hints[i].Trim();

                HintEnum? found = null;
                string parameter = null;

                foreach (var val in EnumHelper.GetValues<HintEnum>()) {
                    if (val.GetValue() == hintValUppercase && !val.IsRequiresParentheses()) {
                        found = val;
                        parameter = GetAssignedValue(hint.Value.Trim(), val.GetValue());
                        break;
                    }

                    if (val.IsRequiresParentheses()) {
                        var indexOpen = hintValUppercase.IndexOf('(');
                        var indexClosed = hintValUppercase.LastIndexOf(')');
                        if (indexOpen != -1) {
                            var hintNameNoParen = hintValUppercase.Substring(0, indexOpen);
                            if (val.GetValue() == hintNameNoParen) {
                                if (indexClosed == -1 || indexClosed < indexOpen) {
                                    throw new AnnotationException("Hint '" + val + "' mismatches parentheses");
                                }

                                if (indexClosed != hintValUppercase.Length - 1) {
                                    throw new AnnotationException(
                                        "Hint '" + val + "' has additional text after parentheses");
                                }

                                found = val;
                                parameter = hintValNeutralcase.Substring(indexOpen + 1, indexClosed - indexOpen - 1);
                                break;
                            }
                        }

                        if (hintValUppercase == val.GetValue() && indexOpen == -1) {
                            throw new AnnotationException(
                                "Hint '" + val + "' requires additional parameters in parentheses");
                        }
                    }

                    if (hintValUppercase.IndexOf('=') != -1) {
                        var hintName = hintValUppercase.Substring(0, hintValUppercase.IndexOf('='));
                        if (val.GetValue() == hintName.Trim().ToUpper()) {
                            found = val;
                            parameter = GetAssignedValue(hint.Value.Trim(), val.GetValue());
                            break;
                        }
                    }
                }

                if (found == null) {
                    var hintName = hints[i].Trim();
                    if (hintName.IndexOf('=') != -1) {
                        hintName = hintName.Substring(0, hintName.IndexOf('='));
                    }

                    throw new AnnotationException(
                        "Hint annotation value '" + hintName.Trim() + "' is not one of the known values");
                }

                if (!found.Value.IsRequiresParentheses()) {
                    ValidateParameters(found.Value, hintValUppercase);
                }

                var existing = listed.Get(found.Value);
                if (existing == null) {
                    existing = new List<string>();
                    listed.Put(found.Value, existing);
                }

                if (parameter != null) {
                    existing.Add(parameter);
                }
            }

            return listed;
        }

        private static void ValidateParameters(
            HintEnum val,
            string hintVal)
        {
            if (IsRequiresParameters(val)) {
                if (hintVal.IndexOf('=') == -1) {
                    throw new AnnotationException("Hint '" + val + "' requires a parameter value");
                }
            }

            if (!IsAcceptsParameters(val)) {
                if (hintVal.IndexOf('=') != -1) {
                    throw new AnnotationException("Hint '" + val + "' does not accept a parameter value");
                }
            }
        }

        /// <summary>
        ///     Returns hint value.
        /// </summary>
        /// <param name="hintEnum">The hint enum.</param>
        /// <param name="annotation">The annotation to look for.</param>
        /// <returns>hint assigned first value provided</returns>
        public static string GetHintAssignedValue(
            this HintEnum hintEnum,
            HintAttribute annotation)
        {
            try {
                var hintValues = ValidateGetListed(annotation);
                if (hintValues == null || !hintValues.ContainsKey(hintEnum)) {
                    return null;
                }

                return hintValues.Get(hintEnum)[0];
            }
            catch (AnnotationException ex) {
                throw new EPException("Failed to interpret hint annotation: " + ex.Message, ex);
            }
        }


        /// <summary>
        ///     Returns all values assigned for a given hint, if any
        /// </summary>
        /// <param name="hintEnum">The hint enum.</param>
        /// <param name="annotations">the to be interogated</param>
        /// <returns>
        ///     hint assigned values or null if none found
        /// </returns>
        public static IList<string> GetHintAssignedValues(
            this HintEnum hintEnum,
            IEnumerable<Attribute> annotations)
        {
            IList<string> allHints = null;
            try {
                foreach (var annotation in annotations) {
                    var hintValues = ValidateGetListed(annotation);
                    if (hintValues == null || !hintValues.ContainsKey(hintEnum)) {
                        continue;
                    }

                    if (allHints == null) {
                        allHints = hintValues.Get(hintEnum);
                    }
                    else {
                        allHints.AddAll(hintValues.Get(hintEnum));
                    }
                }
            }
            catch (AnnotationException ex) {
                throw new EPException("Failed to interpret hint annotation: " + ex.Message, ex);
            }

            return allHints;
        }

        private static string GetAssignedValue(
            string value,
            string enumValue)
        {
            var valMixed = value.Trim();
            var val = valMixed.ToUpper();

            if (!val.Contains(",")) {
                if (val.IndexOf('=') == -1) {
                    return null;
                }

                var hintName = val.Substring(0, val.IndexOf('='));
                if (hintName != enumValue) {
                    return null;
                }

                return valMixed.Substring(val.IndexOf('=') + 1);
            }

            var hints = valMixed.Split(',');
            foreach (var hint in hints) {
                var indexOfEquals = hint.IndexOf('=');
                if (indexOfEquals == -1) {
                    continue;
                }

                val = hint.Substring(0, indexOfEquals).Trim().ToUpper();
                if (val != enumValue) {
                    continue;
                }

                var strValue = hint.Substring(indexOfEquals + 1).Trim();
                if (strValue.Length == 0) {
                    return null;
                }

                return strValue;
            }

            return null;
        }

        /// <summary>
        ///     Split a line of comma-separated values allowing parenthesis.
        /// </summary>
        /// <param name="line">The line to split.</param>
        /// <returns></returns>
        public static string[] SplitCommaUnlessInParen(string line)
        {
            var nestingLevelParen = 0;
            var lastComma = -1;
            var parts = new List<string>();
            for (var i = 0; i < line.Length; i++) {
                var c = line[i];
                if (c == '(') {
                    nestingLevelParen++;
                }

                if (c == ')') {
                    if (nestingLevelParen == 0) {
                        throw new EPException("Close parenthesis ')' found but none open");
                    }

                    nestingLevelParen--;
                }

                if (c == ',' && nestingLevelParen == 0) {
                    var part = line.Substring(lastComma + 1, i - lastComma - 1);
                    if (!string.IsNullOrWhiteSpace(part)) {
                        parts.Add(part);
                    }

                    lastComma = i;
                }
            }

            var lastPart = line.Substring(lastComma + 1);
            if (!string.IsNullOrWhiteSpace(lastPart)) {
                parts.Add(lastPart);
            }

            return parts.ToArray();
        }
    }
}