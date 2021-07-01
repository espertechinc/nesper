///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat;

namespace com.espertech.esper.common.client.configuration.compiler
{
    /// <summary>
    ///     Expression evaluation settings in the runtime are for results of expressions.
    /// </summary>
    [Serializable]
    public class ConfigurationCompilerExpression
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        public ConfigurationCompilerExpression()
        {
            IsIntegerDivision = false;
            IsDivisionByZeroReturnsNull = false;
            IsUdfCache = true;
            IsExtendedAggregation = true;
        }

        /// <summary>
        ///     Returns false (the default) for integer division returning double values.
        ///     <para />
        ///     Returns true to signal that convention integer division semantics
        ///     are used for divisions, whereas the division between two non-FP numbers
        ///     returns only the whole number part of the result and any fractional part is dropped.
        /// </summary>
        /// <value>indicator</value>
        public bool IsIntegerDivision { get; set; }

        /// <summary>
        ///     Set to false (default) for integer division returning double values.
        ///     Set to true to signal the convention integer division semantics
        ///     are used for divisions, whereas the division between two non-FP numbers
        ///     returns only the whole number part of the result and any fractional part is dropped.
        /// </summary>
        /// <value>true for integer division returning integer, false (default) for</value>
        public bool IntegerDivision {
            get => IsIntegerDivision;
            set => IsIntegerDivision = value;
        }

        /// <summary>
        ///     Returns false (default) when division by zero returns Double.Infinity.
        ///     Returns true when division by zero return null.
        ///     <para />
        ///     If integer division is set, then division by zero for non-FP operands also returns null.
        /// </summary>
        /// <value>indicator for division-by-zero results</value>
        public bool IsDivisionByZeroReturnsNull { get; set; }

        /// <summary>
        ///     Set to false (default) to have division by zero return Double.Infinity.
        ///     Set to true to have division by zero return null.
        ///     <para />
        ///     If integer division is set, then division by zero for non-FP operands also returns null.
        /// </summary>
        /// <value>indicator for division-by-zero results</value>
        public bool DivisionByZeroReturnsNull {
            get => IsDivisionByZeroReturnsNull;
            set => IsDivisionByZeroReturnsNull = value;
        }

        /// <summary>
        ///     By default true, indicates that user-defined functions cache return results
        ///     if the parameter set is empty or has constant-only return values.
        /// </summary>
        /// <value>cache flag</value>
        public bool IsUdfCache { get; set; }

        /// <summary>
        ///     Set to true (the default) to indicate that user-defined functions cache return results
        ///     if the parameter set is empty or has constant-only return values.
        /// </summary>
        /// <value>cache flag</value>
        public bool UdfCache {
            get => IsUdfCache;
            set => IsUdfCache = value;
        }

        /// <summary>
        ///     Enables or disables non-SQL standard builtin aggregation functions.
        /// </summary>
        /// <value>indicator</value>
        public bool IsExtendedAggregation { get; set; }

        /// <summary>
        ///     Enables or disables non-SQL standard builtin aggregation functions.
        /// </summary>
        /// <value>indicator</value>
        public bool ExtendedAggregation {
            get => IsExtendedAggregation;
            set => IsExtendedAggregation = value;
        }

        /// <summary>
        ///     Returns true to indicate that duck typing is enable for the specific syntax where it is allowed (check the
        ///     documentation).
        /// </summary>
        /// <value>indicator</value>
        public bool IsDuckTyping { get; set; }

        /// <summary>
        ///     Set to true to indicate that duck typing is enable for the specific syntax where it is allowed (check the
        ///     documentation).
        /// </summary>
        /// <value>indicator</value>
        public bool DuckTyping {
            get => IsDuckTyping;
            set => IsDuckTyping = value;
        }

        /// <summary>
        ///     Returns the math context for big decimal operations, or null to leave the math context undefined.
        /// </summary>
        /// <value>math context or null</value>
        public MathContext MathContext { get; set; }
    }
} // end of namespace