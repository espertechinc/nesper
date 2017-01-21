///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat;

namespace com.espertech.esper.pattern.observer
{
    /// <summary>
    /// Enum for all build-in observers.
    /// </summary>
    public enum ObserverEnum
    {
        /// <summary>
        /// Observer for letting pass/waiting an interval amount of time.
        /// </summary>
        TIMER_INTERVAL,
        /// <summary>
        /// Observer for 'at' (crontab) observation of timer events.
        /// </summary>
        TIMER_CRON,
        /// <summary>
        /// Observer for iso8601 date observation of timer events.
        /// </summary>
        TIMER_ISO8601
    }

    public static class ObserverEnumExtensions
    {
        /// <summary>
        /// Gets the observer namespace name.
        /// </summary>
        /// <value>The observer namespace name.</value>

        public static string GetNamespace(this ObserverEnum observerEnum)
        {
            switch (observerEnum)
            {
                case ObserverEnum.TIMER_INTERVAL:
                case ObserverEnum.TIMER_CRON:
                case ObserverEnum.TIMER_ISO8601:
                    return "timer";
            }

            throw new ArgumentException();
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>

        public static string GetName(this ObserverEnum observerEnum)
        {
            switch (observerEnum)
            {
                case ObserverEnum.TIMER_INTERVAL:
                    return "interval";
                case ObserverEnum.TIMER_CRON:
                    return "at";
                case ObserverEnum.TIMER_ISO8601:
                    return "schedule";
            }

            throw new ArgumentException();
        }

        /// <summary>
        /// Gets the implementation clazz.
        /// </summary>
        /// <value>The implementation clazz.</value>

        public static Type GetImplementationType(this ObserverEnum observerEnum)
        {
            switch (observerEnum)
            {
                case ObserverEnum.TIMER_INTERVAL:
                    return typeof(TimerIntervalObserverFactory);
                case ObserverEnum.TIMER_CRON:
                    return typeof (TimerAtObserverFactory);
                case ObserverEnum.TIMER_ISO8601:
                    return typeof (TimerScheduleObserverFactory);
            }

            throw new ArgumentException();
        }

		/// <summary>
		/// Returns observer enum for namespace name and observer name.
		/// </summary>
		/// <param name="nspace">namespace name</param>
		/// <param name="name">observer name</param>
        
        public static ObserverEnum? ForName(String nspace, String name)
        {
            foreach (var observerEnum in EnumHelper.GetValues<ObserverEnum>())
            {
                if ((observerEnum.GetNamespace() == nspace) &&
                    (observerEnum.GetName() == name))
                {
                    return observerEnum;
                }
            }

            return null;
        }
    }
}
