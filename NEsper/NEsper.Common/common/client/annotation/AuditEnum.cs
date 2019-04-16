///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.client.annotation
{
    /// <summary>
    ///     Enumeration of audit values. Since audits may be a comma-separate list in a single @Audit annotation
    ///     they are listed as enumeration values here.
    /// </summary>
    public class AuditEnum
    {
        /// <summary>
        ///     For use with property audit.
        /// </summary>
        public static readonly AuditEnum PROPERTY = new AuditEnum("PROPERTY");

        /// <summary>
        ///     For use with expression audit.
        /// </summary>
        public static readonly AuditEnum EXPRESSION = new AuditEnum("EXPRESSION");

        /// <summary>
        ///     For use with expression audit.
        /// </summary>
        public static readonly AuditEnum EXPRESSION_NESTED = new AuditEnum("EXPRESSION-NESTED");

        /// <summary>
        ///     For use with expression-definition audit.
        /// </summary>
        public static readonly AuditEnum EXPRDEF = new AuditEnum("EXPRDEF");

        /// <summary>
        ///     For use with view audit.
        /// </summary>
        public static readonly AuditEnum VIEW = new AuditEnum("VIEW");

        /// <summary>
        ///     For use with pattern audit.
        /// </summary>
        public static readonly AuditEnum PATTERN = new AuditEnum("PATTERN");

        /// <summary>
        ///     For use with pattern audit.
        /// </summary>
        public static readonly AuditEnum PATTERNINSTANCES = new AuditEnum("PATTERN-INSTANCES");

        /// <summary>
        ///     For use with stream-audit.
        /// </summary>
        public static readonly AuditEnum STREAM = new AuditEnum("STREAM");

        /// <summary>
        ///     For use with schedule-audit.
        /// </summary>
        public static readonly AuditEnum SCHEDULE = new AuditEnum("SCHEDULE");

        /// <summary>
        ///     For use with insert-into audit.
        /// </summary>
        public static readonly AuditEnum INSERT = new AuditEnum("INSERT");

        /// <summary>
        ///     For use with data flow source operators.
        /// </summary>
        public static readonly AuditEnum DATAFLOW_SOURCE = new AuditEnum("DATAFLOW-SOURCE");

        /// <summary>
        ///     For use with data flow (non-source and source) operators.
        /// </summary>
        public static readonly AuditEnum DATAFLOW_OP = new AuditEnum("DATAFLOW-OP");

        /// <summary>
        ///     For use with data flows specifically for transitions.
        /// </summary>
        public static readonly AuditEnum DATAFLOW_TRANSITION = new AuditEnum("DATAFLOW-TRANSITION");

        /// <summary>
        ///     For use with insert-into audit.
        /// </summary>
        public static readonly AuditEnum CONTEXTPARTITION = new AuditEnum("CONTEXTPARTITION");

        private AuditEnum(string auditInput)
        {
            Value = auditInput.ToUpperInvariant();
            PrettyPrintText = auditInput.ToLowerInvariant();
        }

        /// <summary>
        ///     Returns the constant.
        /// </summary>
        /// <returns>constant</returns>
        public string Value { get; }

        /// <summary>
        ///     Returns text used for the category of the audit log item.
        /// </summary>
        /// <returns>category name</returns>
        public string PrettyPrintText { get; }

        /// <summary>
        ///     Check if the hint is present in the annotations provided.
        /// </summary>
        /// <param name="annotations">the annotations to inspect</param>
        /// <returns>indicator</returns>
        public AuditAttribute GetAudit(Attribute[] annotations)
        {
            if (annotations == null) {
                return null;
            }

            foreach (var annotation in annotations) {
                if (!(annotation is AuditAttribute auditAnnotation)) {
                    continue;
                }

                string auditAnnoValue = auditAnnotation.Value;
                if (auditAnnoValue.Equals("*")) {
                    return auditAnnotation;
                }

                var isListed = IsListed(auditAnnoValue, Value);
                if (isListed) {
                    return auditAnnotation;
                }
            }

            return null;
        }

        private static bool IsListed(
            string list,
            string lookedForValueInput)
        {
            if (list == null) {
                return false;
            }

            var lookedForValue = lookedForValueInput.Trim().ToUpperInvariant();
            list = list.Trim().ToUpperInvariant();

            if (list.ToUpperInvariant().Equals(lookedForValue)) {
                return true;
            }

            var items = list.SplitCsv();
            foreach (var item in items) {
                var listItem = item.Trim().ToUpperInvariant();
                if (listItem.Equals(lookedForValue)) {
                    return true;
                }
            }

            return false;
        }
    }
} // end of namespace