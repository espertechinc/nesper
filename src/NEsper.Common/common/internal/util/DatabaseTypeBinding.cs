///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.bytecodemodel.model.expression;

namespace com.espertech.esper.common.@internal.util
{
    /// <summary>
    ///     Binding from database output column type to object.
    /// </summary>
    public interface DatabaseTypeBinding
    {
        /// <summary>Returns the target data type.</summary>
        /// <returns>Data type</returns>
        Type DataType { get; }

        /// <summary>
        ///     Returns the object for the given column.
        /// </summary>
        /// <param name="rawObject">The raw object.</param>
        /// <param name="columnName">is the column name</param>
        /// <returns>object</returns>
        /// <throws>SQLException if the mapping cannot be performed</throws>
        object GetValue(
            object rawObject,
            string columnName);

        CodegenExpression Make();
    }

    /// <summary>
    ///     Returns the object for the given column.
    /// </summary>
    public delegate object DataRetriever(
        object rawObject,
        string columnName);

    /// <summary>
    ///     Implementation of the DataTypeBinding that uses delegates
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class ProxyDatabaseTypeBinding<T> : DatabaseTypeBinding
    {
        private readonly DataRetriever dataRetriever;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ProxyDatabaseTypeBinding&lt;T&gt;" /> class.
        /// </summary>
        /// <param name="retriever">The retriever.</param>
        /// <param name="codeGenerator">the code generator</param>
        public ProxyDatabaseTypeBinding(
            DataRetriever retriever,
            Func<CodegenExpression> codeGenerator)
        {
            dataRetriever = retriever;
        }

        /// <summary>
        ///     Returns the object for the given column.
        /// </summary>
        /// <param name="rawObject">The raw object.</param>
        /// <param name="columnName">is the column name</param>
        /// <returns>object</returns>
        /// <throws>SQLException if the mapping cannot be performed</throws>
        public object GetValue(
            object rawObject,
            string columnName)
        {
            return dataRetriever.Invoke(rawObject, columnName);
        }

        /// <summary>Returns the target data type.</summary>
        /// <returns>Data type</returns>
        public Type DataType => typeof(T);

        public CodegenExpression Make()
        {
            throw new NotImplementedException();
        }
    }
} // End of namespace