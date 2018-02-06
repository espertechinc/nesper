/*
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 * 
 *      http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;

using com.espertech.esper.compat;

namespace com.espertech.esper.epl.datetime.calop
{
    public class ApacheCommonsDateUtils
    {
        /// <summary>
        /// Number of milliseconds in a standard second.
        /// </summary>
        public const long MILLIS_PER_SECOND = 1000;
        /// <summary>
        /// Number of milliseconds in a standard minute.
        /// </summary>
        public const long MILLIS_PER_MINUTE = 60 * MILLIS_PER_SECOND;
        /// <summary>
        /// Number of milliseconds in a standard hour.
        /// </summary>
        public const long MILLIS_PER_HOUR = 60 * MILLIS_PER_MINUTE;
        /// <summary>
        /// Number of milliseconds in a standard day.
        /// </summary>
        public const long MILLIS_PER_DAY = 24 * MILLIS_PER_HOUR;

        /// <summary>
        /// This is half a month, so this represents whether a date is in the top
        /// or bottom half of the month.
        /// </summary>
        public const int SEMI_MONTH = 1001;

        /// <summary>
        /// IsConstant marker for truncating 
        /// </summary>
        public const int MODIFY_TRUNCATE = 0;

        /// <summary>
        /// IsConstant marker for rounding
        /// </summary>
        public const int MODIFY_ROUND = 1;

        /// <summary>
        /// IsConstant marker for ceiling
        /// </summary>
        public const int MODIFY_CEILING = 2;

        private static readonly int[][] Fields =
            {
                new[] {DateTimeFieldEnum.MILLISEC},
                new[] {DateTimeFieldEnum.SECOND},
                new[] {DateTimeFieldEnum.MINUTE},
                new[] {DateTimeFieldEnum.HOUR_OF_DAY, DateTimeFieldEnum.HOUR},
                new[] {DateTimeFieldEnum.DATE, DateTimeFieldEnum.DAY_OF_MONTH, DateTimeFieldEnum.AM_PM},
                new[] {DateTimeFieldEnum.MONTH, ApacheCommonsDateUtils.SEMI_MONTH},
                new[] {DateTimeFieldEnum.YEAR}
            };

        public static DateTime Modify(DateTime val, int field, int modType)
        {
            if (val.Year > 280000000)
            {
                throw new ArithmeticException("Calendar value too large for accurate calculations");
            }

            if (field == DateTimeFieldEnum.MILLISEC)
            {
                return val;
            }

            // ----------------- Fix for LANG-59 ---------------------- START ---------------
            // see http://issues.apache.org/jira/browse/LANG-59
            //
            // Manually truncate milliseconds, seconds and minutes, rather than using
            // Calendar methods.

            var time = val.UtcMillis();
            var done = false;

            // truncate milliseconds
            int millisecs = val.Millisecond;
            if (MODIFY_TRUNCATE == modType || millisecs < 500)
            {
                time = time - millisecs;
            }
            if (field == DateTimeFieldEnum.SECOND)
            {
                done = true;
            }

            // truncate seconds
            int seconds = val.Second;
            if (!done && (MODIFY_TRUNCATE == modType || seconds < 30))
            {
                time = time - (seconds*1000L);
            }
            if (field == DateTimeFieldEnum.MINUTE)
            {
                done = true;
            }

            // truncate minutes
            int minutes = val.Minute;
            if (!done && (MODIFY_TRUNCATE == modType || minutes < 30))
            {
                time = time - (minutes*60000L);
            }

            // reset time
            if (val.UtcMillis() != time)
            {
                val = DateTimeHelper.FromMillis(time);
            }
            // ----------------- Fix for LANG-59 ----------------------- END ----------------

            var roundUp = false;
            for (int i = 0; i < Fields.Length; i++)
            {
                for (int j = 0; j < Fields[i].Length; j++)
                {
                    if (Fields[i][j] == field)
                    {
                        //This is our field... we stop looping
                        if (modType == MODIFY_CEILING || (modType == MODIFY_ROUND && roundUp))
                        {
                            if (field == ApacheCommonsDateUtils.SEMI_MONTH)
                            {
                                //This is a special case that's hard to generalize
                                //If the date is 1, we round up to 16, otherwise
                                //  we subtract 15 days and add 1 month
                                if (val.Day == 1)
                                {
                                    val = val.AddDays(15);
                                }
                                else
                                {
                                    val = val.AddDays(-15).AddMonths(1);
                                }
                                // ----------------- Fix for LANG-440 ---------------------- START ---------------
                            }
                            else if (field == DateTimeFieldEnum.AM_PM)
                            {
                                // This is a special case
                                // If the time is 0, we round up to 12, otherwise
                                //  we subtract 12 hours and add 1 day
                                if (val.Hour == 0)
                                {
                                    val = val.AddHours(12);
                                }
                                else
                                {
                                    val = val.AddHours(-12).AddDays(1);
                                }
                                // ----------------- Fix for LANG-440 ---------------------- END ---------------
                            }
                            else
                            {
                                //We need at add one to this field since the
                                //  last number causes us to round up
                                val = val.AddUsingField(Fields[i][0], 1);
                            }
                        }
                        return val;
                    }
                }
                //We have various fields that are not easy roundings
                var offset = 0;
                var offsetSet = false;
                //These are special types of fields that require different rounding rules
                switch (field)
                {
                    case ApacheCommonsDateUtils.SEMI_MONTH:
                        if (Fields[i][0] == DateTimeFieldEnum.DATE)
                        {
                            //If we're going to drop the DATE field's value,
                            //  we want to do this our own way.
                            //We need to subtrace 1 since the date has a minimum of 1
                            offset = val.Day - 1;
                            //If we're above 15 days adjustment, that means we're in the
                            //  bottom half of the month and should stay accordingly.
                            if (offset >= 15)
                            {
                                offset -= 15;
                            }
                            //Record whether we're in the top or bottom half of that range
                            roundUp = offset > 7;
                            offsetSet = true;
                        }
                        break;
                    case DateTimeFieldEnum.AM_PM:
                        if (Fields[i][0] == DateTimeFieldEnum.HOUR_OF_DAY)
                        {
                            //If we're going to drop the HOUR field's value,
                            //  we want to do this our own way.
                            offset = val.Hour;
                            if (offset >= 12)
                            {
                                offset -= 12;
                            }
                            roundUp = offset >= 6;
                            offsetSet = true;
                        }
                        break;
                }
                if (!offsetSet)
                {
                    int min = val.GetActualMinimum(Fields[i][0]);
                    int max = val.GetActualMaximum(Fields[i][0]);
                    //Calculate the offset from the minimum allowed value
                    offset = val.GetFieldValue(Fields[i][0]) - min;
                    //Set roundUp if this is more than half way between the minimum and maximum
                    roundUp = offset > ((max - min)/2);
                }
                //We need to remove this field
                if (offset != 0)
                {
                    val = val.SetFieldValue(Fields[i][0], val.GetFieldValue(Fields[i][0]) - offset);
                }
            }

            throw new ArgumentException("The field " + field + " is not supported");
        }

        public static DateTimeOffset Modify(DateTimeOffset val, int field, int modType, TimeZoneInfo timeZone)
        {
            if (val.Year > 280000000)
            {
                throw new ArithmeticException("Calendar value too large for accurate calculations");
            }

            if (field == DateTimeFieldEnum.MILLISEC)
            {
                return val;
            }

            // ----------------- Fix for LANG-59 ---------------------- START ---------------
            // see http://issues.apache.org/jira/browse/LANG-59
            //
            // Manually truncate milliseconds, seconds and minutes, rather than using
            // Calendar methods.

            var time = val.TimeInMillis();
            var done = false;

            // truncate milliseconds
            int millisecs = val.Millisecond;
            if (MODIFY_TRUNCATE == modType || millisecs < 500)
            {
                time = time - millisecs;
            }
            if (field == DateTimeFieldEnum.SECOND)
            {
                done = true;
            }

            // truncate seconds
            int seconds = val.Second;
            if (!done && (MODIFY_TRUNCATE == modType || seconds < 30))
            {
                time = time - (seconds * 1000L);
            }
            if (field == DateTimeFieldEnum.MINUTE)
            {
                done = true;
            }

            // truncate minutes
            int minutes = val.Minute;
            if (!done && (MODIFY_TRUNCATE == modType || minutes < 30))
            {
                time = time - (minutes * 60000L);
            }

            // reset time
            if (val.TimeInMillis() != time)
            {
                val = DateTimeOffsetHelper.TimeFromMillis(time, timeZone);
            }
            // ----------------- Fix for LANG-59 ----------------------- END ----------------

            var roundUp = false;
            for (int i = 0; i < Fields.Length; i++)
            {
                for (int j = 0; j < Fields[i].Length; j++)
                {
                    if (Fields[i][j] == field)
                    {
                        //This is our field... we stop looping
                        if (modType == MODIFY_CEILING || (modType == MODIFY_ROUND && roundUp))
                        {
                            if (field == ApacheCommonsDateUtils.SEMI_MONTH)
                            {
                                //This is a special case that's hard to generalize
                                //If the date is 1, we round up to 16, otherwise
                                //  we subtract 15 days and add 1 month
                                if (val.Day == 1)
                                {
                                    val = val.AddDays(15);
                                }
                                else
                                {
                                    val = val.AddDays(-15).AddMonthsLikeJava(1);
                                }
                                // ----------------- Fix for LANG-440 ---------------------- START ---------------
                            }
                            else if (field == DateTimeFieldEnum.AM_PM)
                            {
                                // This is a special case
                                // If the time is 0, we round up to 12, otherwise
                                //  we subtract 12 hours and add 1 day
                                if (val.Hour == 0)
                                {
                                    val = val.AddHours(12);
                                }
                                else
                                {
                                    val = val.AddHours(-12).AddDays(1);
                                }
                                // ----------------- Fix for LANG-440 ---------------------- END ---------------
                            }
                            else
                            {
                                //We need at add one to this field since the
                                //  last number causes us to round up
                                val = val.AddUsingField(Fields[i][0], 1);
                            }
                        }
                        return val;
                    }
                }
                //We have various fields that are not easy roundings
                var offset = 0;
                var offsetSet = false;
                //These are special types of fields that require different rounding rules
                switch (field)
                {
                    case ApacheCommonsDateUtils.SEMI_MONTH:
                        if (Fields[i][0] == DateTimeFieldEnum.DATE)
                        {
                            //If we're going to drop the DATE field's value,
                            //  we want to do this our own way.
                            //We need to subtrace 1 since the date has a minimum of 1
                            offset = val.Day - 1;
                            //If we're above 15 days adjustment, that means we're in the
                            //  bottom half of the month and should stay accordingly.
                            if (offset >= 15)
                            {
                                offset -= 15;
                            }
                            //Record whether we're in the top or bottom half of that range
                            roundUp = offset > 7;
                            offsetSet = true;
                        }
                        break;
                    case DateTimeFieldEnum.AM_PM:
                        if (Fields[i][0] == DateTimeFieldEnum.HOUR_OF_DAY)
                        {
                            //If we're going to drop the HOUR field's value,
                            //  we want to do this our own way.
                            offset = val.Hour;
                            if (offset >= 12)
                            {
                                offset -= 12;
                            }
                            roundUp = offset >= 6;
                            offsetSet = true;
                        }
                        break;
                }
                if (!offsetSet)
                {
                    int min = val.GetActualMinimum(Fields[i][0]);
                    int max = val.GetActualMaximum(Fields[i][0]);
                    //Calculate the offset from the minimum allowed value
                    offset = val.GetFieldValue(Fields[i][0]) - min;
                    //Set roundUp if this is more than half way between the minimum and maximum
                    roundUp = offset > ((max - min) / 2);
                }
                //We need to remove this field
                if (offset != 0)
                {
                    val = val.SetFieldValue(Fields[i][0], val.GetFieldValue(Fields[i][0]) - offset, timeZone);
                }
            }

            throw new ArgumentException("The field " + field + " is not supported");
        }
    }
}
