///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Linq;

namespace com.espertech.esper.client.annotation
{
    /// <summary>
    /// Enumeration of audit values. Since audits may be a comma-separate list in a single 
    /// @Audit annotation they are listed as enumeration values here.
    /// </summary>
    public enum AuditEnum
    {
        /// <summary>For use with property audit. </summary>
        PROPERTY,

        /// <summary>For use with expression audit. </summary>
        EXPRESSION,

        /// <summary>For use with expression audit.</summary>
        EXPRESSION_NESTED,

        /// <summary>For use with expression-definition audit. </summary>
        EXPRDEF,

        /// <summary>For use with view audit. </summary>
        VIEW,

        /// <summary>For use with pattern audit. </summary>
        PATTERN,

        /// <summary>For use with pattern audit. </summary>
        PATTERNINSTANCES,

        /// <summary>For use with stream-audit. </summary>
        STREAM,

        /// <summary>For use with stream-audit. </summary>
        SCHEDULE,

        /// <summary>For use with insert-into audit.</summary>
        INSERT,

        /// <summary>For use with data flow source operators.</summary>
        DATAFLOW_SOURCE,

        /// <summary>
        /// For use with data flow (non-source and source) operators.
        /// </summary>
        DATAFLOW_OP,

        /// <summary>
        /// For use with data flows specifically for transitions.
        /// </summary>
        DATAFLOW_TRANSITION,
        
        /// <summary>
        /// For use with insert-into audit.
        /// </summary>
        CONTEXTPARTITION
    } ;

    public static class AuditEnumExtensions {
        /// <summary>Returns the constant. </summary>
        /// <param name="enumValue">The enum value.</param>
        /// <returns>constant</returns>
        public static string GetValue(this AuditEnum enumValue)
        {
            switch (enumValue) {
                case AuditEnum.PROPERTY:
                    return "PROPERTY";
                case AuditEnum.EXPRESSION:
                    return "EXPRESSION";
                case AuditEnum.EXPRESSION_NESTED:
                    return "EXPRESSION-NESTED";
                case AuditEnum.EXPRDEF:
                    return "EXPRDEF";
                case AuditEnum.VIEW:
                    return "VIEW";
                case AuditEnum.PATTERN:
                    return "PATTERN";
                case AuditEnum.PATTERNINSTANCES:
                    return "PATTERN-INSTANCES";
                case AuditEnum.STREAM:
                    return "STREAM";
                case AuditEnum.SCHEDULE:
                    return "SCHEDULE";
                case AuditEnum.INSERT:
                    return "INSERT";
                case AuditEnum.DATAFLOW_SOURCE:
                    return "DATAFLOW-SOURCE";
                case AuditEnum.DATAFLOW_OP:
                    return "DATAFLOW-OP";
                case AuditEnum.DATAFLOW_TRANSITION:
                    return "DATAFLOW-TRANSITION";
                case AuditEnum.CONTEXTPARTITION:
                    return "CONTEXTPARTITION";
                default:
                    throw new ArgumentException("invalid value for enum value", "enumValue");
            }
        }

        /// <summary>
        /// Returns text used for the category of the audit log item.
        /// </summary>
        /// <param name="enumValue"></param>
        /// <returns></returns>

        public static string GetPrettyPrintText(this AuditEnum enumValue)
        {
            return GetValue(enumValue).ToLower();
        }

        /// <summary>
        /// Check if the hint is present in the attributes provided.
        /// </summary>
        /// <param name="enumValue">The enum value.</param>
        /// <param name="attributes">the attributes to inspect</param>
        /// <returns>indicator</returns>
        public static AuditAttribute GetAudit(this AuditEnum enumValue, Attribute[] attributes)
        {
            if (attributes == null)
            {
                return null;
            }

            foreach (Attribute attribute in attributes)
            {
                var auditAnnotation = attribute as AuditAttribute;
                if (auditAnnotation == null) {
                    continue;
                }

                var auditAnnoValue = auditAnnotation.Value;
                if (auditAnnoValue == "*") {
                    return auditAnnotation;
                }

                var isListed = IsListed(auditAnnoValue, GetValue(enumValue));
                if (isListed) {
                    return auditAnnotation;
                }
            }

            return null;
        }

        private static bool IsListed(String list, String lookedForValue)
        {
            if (list == null)
            {
                return false;
            }

            lookedForValue = lookedForValue.Trim().ToUpper();
            list = list.Trim().ToUpper();

            if (list.ToUpper() == lookedForValue)
            {
                return true;
            }

            return list.Split(',')
                .Select(item => item.Trim().ToUpper())
                .Any(listItem => listItem == lookedForValue);
        }
    }
}
