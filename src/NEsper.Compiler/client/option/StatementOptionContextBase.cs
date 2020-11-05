///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.compat.function;

namespace com.espertech.esper.compiler.client.option
{
    /// <summary>
    ///     Base class providing statement information for compiler options.
    /// </summary>
    public abstract class StatementOptionContextBase
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="base">statement info</param>
        public StatementOptionContextBase(StatementBaseInfo @base)
        {
            EplSupplier = () => @base.Compilable.ToEPL();
            StatementName = @base.StatementName;
            ModuleName = @base.ModuleName;
            Annotations = @base.StatementRawInfo.Annotations;
            StatementNumber = @base.StatementNumber;
        }

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="eplSupplier">epl supplier</param>
        /// <param name="statementName">statement name</param>
        /// <param name="moduleName">module name</param>
        /// <param name="annotations">annotations</param>
        /// <param name="statementNumber">statement number</param>
        public StatementOptionContextBase(
            Supplier<string> eplSupplier,
            string statementName,
            string moduleName,
            Attribute[] annotations,
            int statementNumber)
        {
            EplSupplier = eplSupplier;
            StatementName = statementName;
            ModuleName = moduleName;
            Annotations = annotations;
            StatementNumber = statementNumber;
        }

        /// <summary>
        ///     Returns the supplier of the EPL textual representation
        /// </summary>
        /// <value>epl supplier</value>
        public Supplier<string> EplSupplier { get; }

        /// <summary>
        ///     Returns the statement name
        /// </summary>
        /// <value>statement name</value>
        public string StatementName { get; }

        /// <summary>
        ///     Returns the module name
        /// </summary>
        /// <value>module name</value>
        public string ModuleName { get; }

        /// <summary>
        ///     Returns the annotations
        /// </summary>
        /// <value>annotations</value>
        public Attribute[] Annotations { get; }

        /// <summary>
        ///     Returns the statement number
        /// </summary>
        /// <value>statement number</value>
        public int StatementNumber { get; }
    }
}