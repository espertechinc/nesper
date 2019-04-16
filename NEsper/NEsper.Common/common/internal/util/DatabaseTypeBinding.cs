///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.@internal.util
{
    /// <summary>
    /// Binding from database output column type to object.
    /// </summary>
    public interface DatabaseTypeBinding
    {
        /// <summary>
        /// Returns the object for the given column.
        /// </summary>
        /// <param name="rawObject">The raw object.</param>
        /// <param name="columnName">is the column name</param>
        /// <returns>object</returns>
        /// <throws>SQLException if the mapping cannot be performed</throws>
        Object GetValue(
            Object rawObject,
            String columnName);

        /// <summary>Returns the target data type.</summary>
        /// <returns>Data type</returns>
        Type DataType { get; }
    }

    /// <summary>
    /// Returns the object for the given column.
    /// </summary>
    public delegate Object DataRetriever(
        Object rawObject,
        String columnName);

    /// <summary>
    /// Implementation of the DataTypeBinding that uses delegates
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class ProxyDatabaseTypeBinding<T> : DatabaseTypeBinding
    {
        private readonly DataRetriever dataRetriever;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProxyDatabaseTypeBinding&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="retriever">The retriever.</param>
        public ProxyDatabaseTypeBinding(DataRetriever retriever)
        {
            this.dataRetriever = retriever;
        }

        /// <summary>
        /// Returns the object for the given column.
        /// </summary>
        /// <param name="rawObject">The raw object.</param>
        /// <param name="columnName">is the column name</param>
        /// <returns>object</returns>
        /// <throws>SQLException if the mapping cannot be performed</throws>
        public Object GetValue(
            Object rawObject,
            String columnName)
        {
            return dataRetriever.Invoke(rawObject, columnName);
        }

        /// <summary>Returns the target data type.</summary>
        /// <returns>Data type</returns>
        public Type DataType {
            get { return typeof(T); }
        }
    }
} // End of namespace