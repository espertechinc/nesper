///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.client.hook.vdw
{
    /// <summary>
    /// Enumeration for indicating the type of operator for a lookup against a virtual data window,
    /// see <see cref="VirtualDataWindowLookupContext" />.
    ///  </summary>
    public enum VirtualDataWindowLookupOp
    {
        /// <summary>Equals (=). </summary>
        EQUALS,

        /// <summary>Less (&lt;). </summary>
        LESS,

        /// <summary>Less or equal (&lt;=). </summary>
        LESS_OR_EQUAL,

        /// <summary>Greater or equal (&gt;=). </summary>
        GREATER_OR_EQUAL,

        /// <summary>Greater (&gt;). </summary>
        GREATER,

        /// <summary>Range contains neither endpoint, i.e. (a,b) </summary>
        RANGE_OPEN,

        /// <summary>Range contains low and high endpoint, i.e. [a,b] </summary>
        RANGE_CLOSED,

        /// <summary>Range includes low endpoint but not high endpoint, i.e. [a,b) </summary>
        RANGE_HALF_OPEN,

        /// <summary>Range includes high endpoint but not low endpoint, i.e. (a,b] </summary>
        RANGE_HALF_CLOSED,

        /// <summary>Inverted-Range contains neither endpoint, i.e. (a,b) </summary>
        NOT_RANGE_OPEN,

        /// <summary>Inverted-Range contains low and high endpoint, i.e. [a,b] </summary>
        NOT_RANGE_CLOSED,

        /// <summary>Inverted-Range includes low endpoint but not high endpoint, i.e. [a,b) </summary>
        NOT_RANGE_HALF_OPEN,

        /// <summary>Inverted-Range includes high endpoint but not low endpoint, i.e. (a,b] </summary>
        NOT_RANGE_HALF_CLOSED
    }

    public static class VirtualDataWindowLookupOpExtensions
    {
        public static string GetOp(this VirtualDataWindowLookupOp enumValue)
        {
            switch (enumValue) {
                case VirtualDataWindowLookupOp.EQUALS:
                    return ("=");

                case VirtualDataWindowLookupOp.LESS:
                    return ("<");

                case VirtualDataWindowLookupOp.LESS_OR_EQUAL:
                    return ("<=");

                case VirtualDataWindowLookupOp.GREATER_OR_EQUAL:
                    return (">=");

                case VirtualDataWindowLookupOp.GREATER:
                    return (">");

                case VirtualDataWindowLookupOp.RANGE_OPEN:
                    return ("(,)");

                case VirtualDataWindowLookupOp.RANGE_CLOSED:
                    return ("[,]");

                case VirtualDataWindowLookupOp.RANGE_HALF_OPEN:
                    return ("[,)");

                case VirtualDataWindowLookupOp.RANGE_HALF_CLOSED:
                    return ("(,]");

                case VirtualDataWindowLookupOp.NOT_RANGE_OPEN:
                    return ("-(,)");

                case VirtualDataWindowLookupOp.NOT_RANGE_CLOSED:
                    return ("-[,]");

                case VirtualDataWindowLookupOp.NOT_RANGE_HALF_OPEN:
                    return ("-[,)");

                case VirtualDataWindowLookupOp.NOT_RANGE_HALF_CLOSED:
                    return ("-(,]");

                default:
                    throw new ArgumentException("invalid value", nameof(enumValue));
            }
        }

        /// <summary>Map the operator from a string-value. </summary>
        /// <param name="stringOp">to map from</param>
        /// <returns>operator</returns>
        /// <throws>ArgumentException if the string operator cannot be understood</throws>
        public static VirtualDataWindowLookupOp FromOpString(this string stringOp)
        {
            switch (stringOp) {
                case ("="):
                    return VirtualDataWindowLookupOp.EQUALS;

                case ("<"):
                    return VirtualDataWindowLookupOp.LESS;

                case ("<="):
                    return VirtualDataWindowLookupOp.LESS_OR_EQUAL;

                case (">="):
                    return VirtualDataWindowLookupOp.GREATER_OR_EQUAL;

                case (">"):
                    return VirtualDataWindowLookupOp.GREATER;

                case ("(,)"):
                    return VirtualDataWindowLookupOp.RANGE_OPEN;

                case ("[,]"):
                    return VirtualDataWindowLookupOp.RANGE_CLOSED;

                case ("[,)"):
                    return VirtualDataWindowLookupOp.RANGE_HALF_OPEN;

                case ("(,]"):
                    return VirtualDataWindowLookupOp.RANGE_HALF_CLOSED;

                case ("-(,)"):
                    return VirtualDataWindowLookupOp.NOT_RANGE_OPEN;

                case ("-[,]"):
                    return VirtualDataWindowLookupOp.NOT_RANGE_CLOSED;

                case ("-[,)"):
                    return VirtualDataWindowLookupOp.NOT_RANGE_HALF_OPEN;

                case ("-(,]"):
                    return VirtualDataWindowLookupOp.NOT_RANGE_HALF_CLOSED;

                default:
                    throw new ArgumentException("invalid value", "stringOp");
            }
        }
    }
}