///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace com.espertech.esper.common.client.soda
{
    /// <summary>
    /// Convenience factory for creating <see cref="PatternExpr"/> instances, which represent pattern expression trees.
    /// <para>
    /// Provides quick-access method to create all possible pattern expressions and provides typical parameter lists to each.
    /// </para>
    /// <para>
    /// Note that only the typical parameter lists are provided and pattern expressions can allow adding
    /// additional parameters.
    /// </para>
    /// <para>
    /// Many expressions, for example logical AND and OR (conjunction and disjunction), allow
    /// adding an unlimited number of additional sub-expressions to a pattern expression. For those pattern expressions
    /// there are additional add methods.
    /// </para>
    /// </summary>
    public class Patterns
    {
        /// <summary>
        /// Pattern-every expression control the lifecycle of the pattern sub-expression.
        /// </summary>
        /// <param name="inner">sub-expression to the every-keyword</param>
        /// <returns>pattern expression</returns>
        public static PatternEveryExpr Every(PatternExpr inner)
        {
            return new PatternEveryExpr(inner);
        }

        /// <summary>
        /// Pattern-AND expression, allows adding sub-expressions that are connected by a logical AND.
        /// </summary>
        /// <returns>pattern expression representing the AND relationship</returns>
        public static PatternAndExpr And()
        {
            return new PatternAndExpr();
        }

        /// <summary>
        /// Pattern-AND expression, allows adding sub-expressions that are connected by a logical AND.
        /// </summary>
        /// <param name="first">is the first pattern sub-expression to add to the AND</param>
        /// <param name="second">is a second pattern sub-expression to add to the AND</param>
        /// <param name="more">
        /// is optional additional pattern sub-expression to add to the AND
        /// </param>
        /// <returns>pattern expression representing the AND relationship</returns>
        public static PatternAndExpr And(
            PatternExpr first,
            PatternExpr second,
            params PatternExpr[] more)
        {
            return new PatternAndExpr(first, second, more);
        }

        /// <summary>
        /// Pattern-OR expression, allows adding sub-expressions that are connected by a logical OR.
        /// </summary>
        /// <param name="first">is the first pattern sub-expression to add to the OR</param>
        /// <param name="second">is a second pattern sub-expression to add to the OR</param>
        /// <param name="more">
        /// is optional additional pattern sub-expression to add to the OR
        /// </param>
        /// <returns>pattern expression representing the OR relationship</returns>
        public static PatternOrExpr Or(
            PatternExpr first,
            PatternExpr second,
            params PatternExpr[] more)
        {
            return new PatternOrExpr(first, second, more);
        }

        /// <summary>
        /// Pattern-OR expression, allows adding sub-expressions that are connected by a logical OR.
        /// </summary>
        /// <returns>pattern expression representing the OR relationship</returns>
        public static PatternOrExpr Or()
        {
            return new PatternOrExpr();
        }

        /// <summary>
        /// Pattern followed-by expression, allows adding sub-expressions that are connected by a followed-by.
        /// </summary>
        /// <param name="first">
        /// is the first pattern sub-expression to add to the followed-by
        /// </param>
        /// <param name="second">
        /// is a second pattern sub-expression to add to the followed-by
        /// </param>
        /// <param name="more">
        /// is optional additional pattern sub-expression to add to the followed-by
        /// </param>
        /// <returns>pattern expression representing the followed-by relationship</returns>
        public static PatternFollowedByExpr FollowedBy(
            PatternExpr first,
            PatternExpr second,
            params PatternExpr[] more)
        {
            return new PatternFollowedByExpr(first, second, more);
        }

        /// <summary>
        /// Pattern followed-by expression, allows adding sub-expressions that are connected by a followed-by.
        /// </summary>
        /// <returns>pattern expression representing the followed-by relationship</returns>
        public static PatternFollowedByExpr FollowedBy()
        {
            return new PatternFollowedByExpr();
        }

        /// <summary>
        /// Pattern every-operator and filter in combination, equivalent to the "every
        /// MyEvent" syntax.
        /// </summary>
        /// <param name="eventTypeName">is the event type name to filter for</param>
        /// <returns>
        /// pattern expression
        /// </returns>
        public static PatternEveryExpr EveryFilter(string eventTypeName)
        {
            PatternExpr filter = new PatternFilterExpr(soda.Filter.Create(eventTypeName));
            return new PatternEveryExpr(filter);
        }

        /// <summary>
        /// Pattern every-operator and filter in combination, equivalent to the "every
        /// tag=MyEvent" syntax.
        /// </summary>
        /// <param name="eventTypeName">is the event type name to filter for</param>
        /// <param name="tagName">is the tag name to assign to matching events</param>
        /// <returns>
        /// pattern expression
        /// </returns>
        public static PatternEveryExpr EveryFilter(
            string eventTypeName,
            string tagName)
        {
            PatternExpr filter = new PatternFilterExpr(soda.Filter.Create(eventTypeName), tagName);
            return new PatternEveryExpr(filter);
        }

        /// <summary>
        /// Pattern every-operator and filter in combination, equivalent to the "every MyEvent(vol &gt; 100)" syntax.
        /// </summary>
        /// <param name="filter">
        /// specifies the event type name and filter expression to filter for
        /// </param>
        /// <returns>pattern expression</returns>
        public static PatternEveryExpr EveryFilter(Filter filter)
        {
            PatternExpr inner = new PatternFilterExpr(filter);
            return new PatternEveryExpr(inner);
        }

        /// <summary>
        /// Pattern every-operator and filter in combination, equivalent to the "every tag=MyEvent(vol &gt; 100)" syntax.
        /// </summary>
        /// <param name="filter">
        /// specifies the event type name and filter expression to filter for
        /// </param>
        /// <param name="tagName">is the tag name to assign to matching events</param>
        /// <returns>pattern expression</returns>
        public static PatternEveryExpr EveryFilter(
            Filter filter,
            string tagName)
        {
            PatternExpr inner = new PatternFilterExpr(filter, tagName);
            return new PatternEveryExpr(inner);
        }

        /// <summary>
        /// Filter expression for use in patterns, equivalent to the simple "MyEvent"
        /// syntax.
        /// </summary>
        /// <param name="eventTypeName">is the event type name of the events to filter for</param>
        /// <returns>
        /// pattern expression
        /// </returns>
        public static PatternFilterExpr Filter(string eventTypeName)
        {
            return new PatternFilterExpr(soda.Filter.Create(eventTypeName));
        }

        /// <summary>
        /// Filter expression for use in patterns, equivalent to the simple "tag=MyEvent"
        /// syntax.
        /// </summary>
        /// <param name="eventTypeName">is the event type name of the events to filter for</param>
        /// <param name="tagName">is the tag name to assign to matching events</param>
        /// <returns>
        /// pattern expression
        /// </returns>
        public static PatternFilterExpr Filter(
            string eventTypeName,
            string tagName)
        {
            return new PatternFilterExpr(soda.Filter.Create(eventTypeName), tagName);
        }

        /// <summary>
        /// Filter expression for use in patterns, equivalent to the "MyEvent(vol &gt; 100)" syntax.
        /// </summary>
        /// <param name="filter">
        /// specifies the event type name and filter expression to filter for
        /// </param>
        /// <returns>pattern expression</returns>
        public static PatternFilterExpr Filter(Filter filter)
        {
            return new PatternFilterExpr(filter);
        }

        /// <summary>
        /// Filter expression for use in patterns, equivalent to the "tag=MyEvent(vol &gt; 100)" syntax.
        /// </summary>
        /// <param name="filter">
        /// specifies the event type name and filter expression to filter for
        /// </param>
        /// <param name="tagName">is the tag name to assign to matching events</param>
        /// <returns>pattern expression</returns>
        public static PatternFilterExpr Filter(
            Filter filter,
            string tagName)
        {
            return new PatternFilterExpr(filter, tagName);
        }

        /// <summary>
        /// Guard pattern expression guards a sub-expression, equivalent to the "every MyEvent where timer:within(1 sec)" syntax
        /// </summary>
        /// <param name="namespace">is the guard objects namespace, i.e. "timer"</param>
        /// <param name="name">is the guard objects name, i.e. ""within"</param>
        /// <param name="parameters">is the guard objects optional parameters, i.e. integer 1 for 1 second</param>
        /// <param name="guarded">is the pattern sub-expression to be guarded</param>
        /// <returns>pattern guard expression</returns>
        public static PatternGuardExpr Guard(
            string @namespace,
            string name,
            Expression[] parameters,
            PatternExpr guarded)
        {
            return new PatternGuardExpr(@namespace, name, parameters, guarded);
        }

        /// <summary>
        /// Observer pattern expression, equivalent to the "every timer:interval(1 sec)" syntax
        /// </summary>
        /// <param name="namespace">is the observer objects namespace, i.e. "timer"</param>
        /// <param name="name">is the observer objects name, i.e. ""within"</param>
        /// <param name="parameters">is the observer objects optional parameters, i.e. integer 1 for 1 second</param>
        /// <returns>pattern observer expression</returns>
        public static PatternObserverExpr Observer(
            string @namespace,
            string name,
            Expression[] parameters)
        {
            return new PatternObserverExpr(@namespace, name, parameters);
        }

        /// <summary>Timer-within guard expression.</summary>
        /// <param name="seconds">is the number of seconds for the guard</param>
        /// <param name="guarded">is the sub-expression to guard</param>
        /// <returns>pattern guard</returns>
        public static PatternGuardExpr TimerWithin(
            double seconds,
            PatternExpr guarded)
        {
            return new PatternGuardExpr("timer", "within", new Expression[] {Expressions.Constant(seconds)}, guarded);
        }

        /// <summary>While-guard expression. </summary>
        /// <param name="expression">expression to evaluate against matches</param>
        /// <param name="guarded">is the sub-expression to guard</param>
        /// <returns>pattern guard</returns>
        public static PatternGuardExpr WhileGuard(
            PatternExpr guarded,
            Expression expression)
        {
            return new PatternGuardExpr(
                GuardEnum.WHILE_GUARD.GetNamespace(),
                GuardEnum.WHILE_GUARD.GetName(),
                new[] {expression}, guarded);
        }

        /// <summary>Timer-within-max guard expression. </summary>
        /// <param name="seconds">is the number of seconds for the guard</param>
        /// <param name="max">the maximum number of invocations for the guard</param>
        /// <param name="guarded">is the sub-expression to guard</param>
        /// <returns>pattern guard</returns>
        public static PatternGuardExpr TimerWithinMax(
            double seconds,
            int max,
            PatternExpr guarded)
        {
            return new PatternGuardExpr(
                "timer", "withinmax",
                new Expression[] {Expressions.Constant(seconds), Expressions.Constant(max)},
                guarded);
        }

        /// <summary>Timer-interval observer expression.</summary>
        /// <param name="seconds">is the number of seconds in the interval</param>
        /// <returns>pattern observer</returns>
        public static PatternObserverExpr TimerInterval(double seconds)
        {
            return new PatternObserverExpr("timer", "interval", new Expression[] {Expressions.Constant(seconds)});
        }

        /// <summary>
        /// Pattern not-operator and filter in combination, equivalent to the "not MyEvent"
        /// syntax.
        /// </summary>
        /// <param name="eventTypeName">is the event type name to filter for</param>
        /// <returns>
        /// pattern expression
        /// </returns>
        public static PatternNotExpr NotFilter(string eventTypeName)
        {
            return new PatternNotExpr(new PatternFilterExpr(soda.Filter.Create(eventTypeName)));
        }

        /// <summary>
        /// Pattern not-operator and filter in combination, equivalent to the "not
        /// tag=MyEvent" syntax.
        /// </summary>
        /// <param name="name">is the event type name to filter for</param>
        /// <param name="tagName">is the tag name to assign to matching events</param>
        /// <returns>
        /// pattern expression
        /// </returns>
        public static PatternNotExpr NotFilter(
            string name,
            string tagName)
        {
            return new PatternNotExpr(new PatternFilterExpr(soda.Filter.Create(name), tagName));
        }

        /// <summary>
        /// Pattern not-operator and filter in combination, equivalent to the "not MyEvent(vol &gt; 100)" syntax.
        /// </summary>
        /// <param name="filter">
        /// specifies the event type name and filter expression to filter for
        /// </param>
        /// <returns>pattern expression</returns>
        public static PatternNotExpr NotFilter(Filter filter)
        {
            return new PatternNotExpr(new PatternFilterExpr(filter));
        }

        /// <summary>
        /// Pattern not-operator and filter in combination, equivalent to the "not tag=MyEvent(vol &gt; 100)" syntax.
        /// </summary>
        /// <param name="filter">
        /// specifies the event type name and filter expression to filter for
        /// </param>
        /// <param name="tagName">is the tag name to assign to matching events</param>
        /// <returns>pattern expression</returns>
        public static PatternNotExpr NotFilter(
            Filter filter,
            string tagName)
        {
            return new PatternNotExpr(new PatternFilterExpr(filter, tagName));
        }

        /// <summary>
        /// Not-keyword pattern expression flips the truth-value of the pattern sub-expression.
        /// </summary>
        /// <param name="subexpression">is the expression whose truth value to flip</param>
        /// <returns>pattern expression</returns>
        public static PatternNotExpr Not(PatternExpr subexpression)
        {
            return new PatternNotExpr(subexpression);
        }

        /// <summary>
        /// Match-until-pattern expression matches a certain number of
        /// occurances until a second expression becomes true.
        /// </summary>
        /// <param name="low">low number of matches, or null if no lower boundary</param>
        /// <param name="high">high number of matches, or null if no high boundary</param>
        /// <param name="match">the pattern expression that is sought to match repeatedly</param>
        /// <param name="until">the pattern expression that ends matching (optional, can be null)</param>
        /// <returns>pattern expression</returns>
        public static PatternMatchUntilExpr MatchUntil(
            Expression low,
            Expression high,
            PatternExpr match,
            PatternExpr until)
        {
            return new PatternMatchUntilExpr(low, high, match, until);
        }

        /// <summary>
        /// Timer-at observer
        /// </summary>
        /// <param name="minutes">a single integer value supplying the minute to fire the timer, or null for any (wildcard) minute</param>
        /// <param name="hours">a single integer value supplying the hour to fire the timer, or null for any (wildcard) hour</param>
        /// <param name="daysOfMonth">a single integer value supplying the day of the month to fire the timer, or null for any (wildcard) day of the month</param>
        /// <param name="month">a single integer value supplying the month to fire the timer, or null for any (wildcard) month</param>
        /// <param name="daysOfWeek">a single integer value supplying the days of the week to fire the timer, or null for any (wildcard) day of the week</param>
        /// <param name="seconds">a single integer value supplying the second to fire the timer, or null for any (wildcard) second</param>
        /// <returns>timer-at observer</returns>
        public static PatternObserverExpr TimerAt(
            int? minutes,
            int? hours,
            int? daysOfMonth,
            int? month,
            int? daysOfWeek,
            int? seconds)
        {
            Expression wildcard = new CrontabParameterExpression(ScheduleItemType.WILDCARD);

            var paramList = new List<Expression>();
            paramList.Add(minutes == null ? wildcard : Expressions.Constant(minutes));
            paramList.Add(hours == null ? wildcard : Expressions.Constant(hours));
            paramList.Add(daysOfMonth == null ? wildcard : Expressions.Constant(daysOfMonth));
            paramList.Add(month == null ? wildcard : Expressions.Constant(month));
            paramList.Add(daysOfWeek == null ? wildcard : Expressions.Constant(daysOfWeek));
            paramList.Add(seconds == null ? wildcard : Expressions.Constant(seconds));
            return new PatternObserverExpr("timer", "at", paramList);
        }
    }
} // End of namespace