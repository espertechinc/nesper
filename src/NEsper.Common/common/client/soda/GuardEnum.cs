///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.epl.pattern.guard;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.client.soda
{
    public enum GuardEnum
    {
        TIMER_WITHIN,
        TIMER_WITHINMAX,
        WHILE_GUARD
    }

    /// <summary>
    ///     Enum for all build-in guards.
    /// </summary>
    public static class GuardEnumExtensions
    {
        /// <summary>
        ///     Gets the namespace.
        /// </summary>
        /// <value>The namespace.</value>
        public static string GetNamespace(this GuardEnum value)
        {
            return value switch {
                GuardEnum.TIMER_WITHIN => "timer",
                GuardEnum.TIMER_WITHINMAX => "timer",
                GuardEnum.WHILE_GUARD => "internal",
                _ => throw new ArgumentException()
            };
        }

        /// <summary>
        ///     Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public static string GetName(this GuardEnum value)
        {
            return value switch {
                GuardEnum.TIMER_WITHIN => "within",
                GuardEnum.TIMER_WITHINMAX => "withinmax",
                GuardEnum.WHILE_GUARD => "while",
                _ => throw new ArgumentException()
            };
        }

        /// <summary>Returns the enum for the given namespace and name.</summary>
        /// <param name="nspace">guard namespace</param>
        /// <param name="name">guard name</param>
        /// <returns>enum</returns>
        public static GuardEnum? ForName(
            string nspace,
            string name)
        {
            foreach (var value in EnumHelper.GetValues<GuardEnum>())
            {
                if (value.GetNamespace() == nspace &&
                    value.GetName() == name)
                {
                    return value;
                }
            }

            return null;
        }

        public static bool IsWhile(
            string nspace,
            string name)
        {
            return
                GetNamespace(GuardEnum.WHILE_GUARD) == nspace &&
                GetName(GuardEnum.WHILE_GUARD) == name;
        }

        /// <summary>
        ///     Gets the class associated with the guard enum.
        /// </summary>
        /// <param name="guardEnum">The guard enum.</param>
        /// <returns></returns>
        public static Type GetClazz(this GuardEnum guardEnum)
        {
            return guardEnum switch {
                GuardEnum.TIMER_WITHIN => typeof(TimerWithinGuardForge),
                GuardEnum.TIMER_WITHINMAX => typeof(TimerWithinOrMaxCountGuardForge),
                GuardEnum.WHILE_GUARD => typeof(ExpressionGuardForge),
                _ => throw new ArgumentException("invalid value", nameof(guardEnum))
            };
        }
    }
}