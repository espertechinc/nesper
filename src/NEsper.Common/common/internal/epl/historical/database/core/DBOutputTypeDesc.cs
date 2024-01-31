///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.util;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.historical.database.core
{
    /// <summary>
    ///     Descriptor for SQL output columns.
    /// </summary>
    public class DBOutputTypeDesc
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="sqlType">the type of the column</param>
        /// <param name="dataType">the type reflecting column type</param>
        /// <param name="optionalBinding">the optional mapping from output column type to built-in</param>
        public DBOutputTypeDesc(
            string sqlType,
            Type dataType,
            DatabaseTypeBinding optionalBinding)
        {
            SqlType = sqlType;
            DataType = dataType;
            OptionalBinding = optionalBinding;
        }

        /// <summary>
        ///     Returns the SQL type of the output column.
        /// </summary>
        /// <returns>sql type</returns>
        public string SqlType { get; }

        /// <summary> Returns the type that getObject on the output column produces.</summary>
        /// <returns>
        ///     type from statement metadata
        /// </returns>
        public Type DataType { get; }

        /// <summary>
        ///     Gets the optional mapping from output column type to built-in.
        /// </summary>
        public DatabaseTypeBinding OptionalBinding { get; }

        public override string ToString()
        {
            return
                $"{nameof(SqlType)}: {SqlType}, {nameof(DataType)}: {DataType}, {nameof(OptionalBinding)}: {OptionalBinding}";
        }

        public CodegenExpression Make()
        {
            return NewInstance<DBOutputTypeDesc>(
                Constant(SqlType),
                Constant(DataType),
                OptionalBinding == null ? ConstantNull() : OptionalBinding.Make());
        }
    }
} // end of namespace