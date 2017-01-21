///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat;

namespace com.espertech.esper.pattern.guard
{
    public enum GuardEnum
    {
        TIMER_WITHIN,
        TIMER_WITHINMAX,
        WHILE_GUARD
    }

    /// <summary> 
    /// Enum for all build-in guards.
    /// </summary>

    public static class GuardEnumExtensions
    {
        /// <summary>
        /// Gets the namespace.
        /// </summary>
        /// <value>The namespace.</value>
        public static string GetNamespace(this GuardEnum value)
        {
            switch (value)
            {
                case GuardEnum.TIMER_WITHIN:
                    return "timer";
                case GuardEnum.TIMER_WITHINMAX:
                    return "timer";
                case GuardEnum.WHILE_GUARD:
                    return "internal";
            }

            throw new ArgumentException();
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public static string GetName(this GuardEnum value)
        {
            switch (value)
            {
                case GuardEnum.TIMER_WITHIN:
                    return "within";
                case GuardEnum.TIMER_WITHINMAX:
                    return "withinmax";
                case GuardEnum.WHILE_GUARD:
                    return "while";
            }

            throw new ArgumentException();
        }

        /// <summary>Returns the enum for the given namespace and name.</summary>
        /// <param name="nspace">guard namespace</param>
        /// <param name="name">guard name</param>
        /// <returns>enum</returns>

        public static GuardEnum? ForName(String nspace, String name)
        {
            foreach (var value in EnumHelper.GetValues<GuardEnum>())
            {
                if ((value.GetNamespace() == nspace) &&
                    (value.GetName() == name))
                {
                    return value;
                }
            }

            return null;
        }

        public static bool IsWhile(String nspace, String name)
        {
            return 
                (GetNamespace(GuardEnum.WHILE_GUARD) == nspace) &&
                (GetName(GuardEnum.WHILE_GUARD) == name);
        }

        /// <summary>
        /// Gets the class associated with the guard enum.
        /// </summary>
        /// <param name="guardEnum">The guard enum.</param>
        /// <returns></returns>
        public static Type GetClazz(this GuardEnum guardEnum)
        {
            switch (guardEnum)
            {
                case GuardEnum.TIMER_WITHIN:
                    return typeof(TimerWithinGuardFactory);
                case GuardEnum.TIMER_WITHINMAX:
                    return typeof(TimerWithinOrMaxCountGuardFactory);
                case GuardEnum.WHILE_GUARD:
                    return typeof(ExpressionGuardFactory);
            }

            throw new ArgumentException("invalid value", "guardEnum");
        }
    }
}
