///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.serde.compiletime.resolve;

namespace com.espertech.esper.common.client.serde
{
    /// <summary>
    ///     For use with high-availability and scale-out only, this class instructs the compiler that the serializer and de-serializer (serde)
    ///     is available using a parameterized constructor that accepts expressions as represents by the functions provided.
    /// </summary>
    public class SerdeProvisionParameterized : SerdeProvision
    {
        private readonly Func<DataInputOutputSerdeForgeParameterizedVars, CodegenExpression>[] _functions;
        private readonly Type _serdeClass;

        /// <summary>
        ///     Ctor
        /// </summary>
        /// <param name="serdeClass">serde class</param>
        /// <param name="functions">parameter expressions</param>
        public SerdeProvisionParameterized(
            Type serdeClass,
            params Func<DataInputOutputSerdeForgeParameterizedVars, CodegenExpression>[] functions)
        {
            _serdeClass = serdeClass;
            _functions = functions;
        }

        public override DataInputOutputSerdeForge ToForge()
        {
            return new DataInputOutputSerdeForgeParameterized(_serdeClass.Name, _functions);
        }
    }
} // end of namespace