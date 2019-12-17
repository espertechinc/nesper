///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
using System.Text;

namespace com.espertech.esper.runtime.@internal.metrics.codahale_metrics.metrics.core
{
    /// <summary>
    ///     A value class encapsulating a metric's owning class and name.
    /// </summary>
    public class MetricName : IComparable<MetricName>
    {
        /// <summary>
        ///     Creates a new <seealso cref="MetricName" /> without a scope.
        /// </summary>
        /// <param name="klass">the <seealso cref="Type" /> to which the <seealso cref="Metric" /> belongs</param>
        /// <param name="name">the name of the <seealso cref="Metric" /></param>
        public MetricName(
            Type klass,
            string name)
            : this(klass, name, null)
        {
        }

        /// <summary>
        ///     Creates a new <seealso cref="MetricName" /> without a scope.
        /// </summary>
        /// <param name="group">the group to which the <seealso cref="Metric" /> belongs</param>
        /// <param name="type">the type to which the <seealso cref="Metric" /> belongs</param>
        /// <param name="name">the name of the <seealso cref="Metric" /></param>
        public MetricName(
            string group,
            string type,
            string name)
            : this(group, type, name, null)
        {
        }

        /// <summary>
        ///     Creates a new <seealso cref="MetricName" /> without a scope.
        /// </summary>
        /// <param name="klass">the <seealso cref="Type" /> to which the <seealso cref="Metric" /> belongs</param>
        /// <param name="name">the name of the <seealso cref="Metric" /></param>
        /// <param name="scope">the scope of the <seealso cref="Metric" /></param>
        public MetricName(
            Type klass,
            string name,
            string scope)
            :
            this(
                klass.Namespace == null ? "" : klass.Namespace,
                klass.Name,
                name,
                scope)
        {
        }

        /// <summary>
        ///     Creates a new <seealso cref="MetricName" /> without a scope.
        /// </summary>
        /// <param name="group">the group to which the <seealso cref="Metric" /> belongs</param>
        /// <param name="type">the type to which the <seealso cref="Metric" /> belongs</param>
        /// <param name="name">the name of the <seealso cref="Metric" /></param>
        /// <param name="scope">the scope of the <seealso cref="Metric" /></param>
        public MetricName(
            string group,
            string type,
            string name,
            string scope)
            :
            this(group, type, name, scope, CreateMBeanName(group, type, name, scope))
        {
        }

        /// <summary>
        ///     Creates a new <seealso cref="MetricName" /> without a scope.
        /// </summary>
        /// <param name="group">the group to which the <seealso cref="Metric" /> belongs</param>
        /// <param name="type">the type to which the <seealso cref="Metric" /> belongs</param>
        /// <param name="name">the name of the <seealso cref="Metric" /></param>
        /// <param name="scope">the scope of the <seealso cref="Metric" /></param>
        /// <param name="mBeanName">
        ///     the 'ObjectName', represented as a string, to use when registering theMBean.
        /// </param>
        public MetricName(
            string group,
            string type,
            string name,
            string scope,
            string mBeanName)
        {
            if (group == null || type == null)
            {
                throw new ArgumentException("Both group and type need to be specified");
            }

            if (name == null)
            {
                throw new ArgumentException("Name needs to be specified");
            }

            Group = group;
            Type = type;
            Name = name;
            Scope = scope;
            MBeanName = mBeanName;
        }

        /// <summary>
        ///     Returns the group to which the <seealso cref="Metric" /> belongs. For class-based metrics, this will be
        ///     the package name of the <seealso cref="Type" /> to which the <seealso cref="Metric" /> belongs.
        /// </summary>
        /// <returns>the group to which the <seealso cref="Metric" /> belongs</returns>
        public string Group { get; }

        /// <summary>
        ///     Returns the type to which the <seealso cref="Metric" /> belongs. For class-based metrics, this will be
        ///     the simple class name of the <seealso cref="Type" /> to which the <seealso cref="Metric" /> belongs.
        /// </summary>
        /// <returns>the type to which the <seealso cref="Metric" /> belongs</returns>
        public string Type { get; }

        /// <summary>
        ///     Returns the name of the <seealso cref="Metric" />.
        /// </summary>
        /// <returns>the name of the <seealso cref="Metric" /></returns>
        public string Name { get; }

        /// <summary>
        ///     Returns the scope of the <seealso cref="Metric" />.
        /// </summary>
        /// <returns>the scope of the <seealso cref="Metric" /></returns>
        public string Scope { get; }

        /// <summary>
        ///     Returns the MBean name for the <seealso cref="Metric" /> identified by this metric name.
        /// </summary>
        /// <returns>the MBean name</returns>
        public string MBeanName { get; }

        public int CompareTo(MetricName o)
        {
            return MBeanName.CompareTo(o.MBeanName);
        }

        /// <summary>
        ///     Returns {@code true} if the <seealso cref="Metric" /> has a scope, {@code false} otherwise.
        /// </summary>
        /// <value>{@code true} if the <seealso cref="Metric" /> has a scope</value>
        public bool HasScope {
            get { return Scope != null; }
        }

        protected bool Equals(MetricName other)
        {
            return string.Equals(MBeanName, other.MBeanName);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((MetricName) obj);
        }

        public override int GetHashCode()
        {
            return MBeanName != null ? MBeanName.GetHashCode() : 0;
        }

        public override string ToString()
        {
            return MBeanName;
        }

        private static string CreateMBeanName(
            string group,
            string type,
            string name,
            string scope)
        {
            var nameBuilder = new StringBuilder();
            nameBuilder.Append(group);
            nameBuilder.Append(":type=");
            nameBuilder.Append(type);
            if (scope != null)
            {
                nameBuilder.Append(",scope=");
                nameBuilder.Append(scope);
            }

            if (name.Length > 0)
            {
                nameBuilder.Append(",name=");
                nameBuilder.Append(name);
            }

            return nameBuilder.ToString();
        }

        /// <summary>
        ///     If the group is empty, use the package name of the given class. Otherwise use group
        /// </summary>
        /// <param name="group">The group to use by default</param>
        /// <param name="klass">The class being tracked</param>
        /// <returns>a group for the metric</returns>
        public static string ChooseGroup(
            string group,
            Type klass)
        {
            if (string.IsNullOrWhiteSpace(group))
            {
                group = klass.Namespace ?? "";
            }

            return group;
        }

        /// <summary>
        ///     If the type is empty, use the simple name of the given class. Otherwise use type
        /// </summary>
        /// <param name="type">The type to use by default</param>
        /// <param name="klass">The class being tracked</param>
        /// <returns>a type for the metric</returns>
        public static string ChooseType(
            string type,
            Type klass)
        {
            if (string.IsNullOrWhiteSpace(type))
            {
                type = klass.Name;
            }

            return type;
        }

        /// <summary>
        ///     If name is empty, use the name of the given method. Otherwise use name
        /// </summary>
        /// <param name="name">The name to use by default</param>
        /// <param name="method">The method being tracked</param>
        /// <returns>a name for the metric</returns>
        public static string ChooseName(
            string name,
            MethodInfo method)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                name = method.Name;
            }

            return name;
        }
    }
} // end of namespace