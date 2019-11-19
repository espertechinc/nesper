///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.soda;

namespace com.espertech.esper.common.@internal.compile.stage1.spec
{
    /// <summary>
    /// Specification for creating an event type/schema.
    /// </summary>
    [Serializable]
    public class CreateSchemaDesc
    {
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="schemaName">name</param>
        /// <param name="types">event type Name(s)</param>
        /// <param name="columns">column definition</param>
        /// <param name="inherits">supertypes</param>
        /// <param name="assignedType">any type assignment such as Map, Object-array or variant or none-specified</param>
        /// <param name="startTimestampProperty">name of start-interval prop</param>
        /// <param name="endTimestampProperty">name of end-interval prop</param>
        /// <param name="copyFrom">copy-from</param>
        public CreateSchemaDesc(
            string schemaName,
            ICollection<string> types,
            IList<ColumnDesc> columns,
            ICollection<string> inherits,
            AssignedType assignedType,
            string startTimestampProperty,
            string endTimestampProperty,
            ICollection<string> copyFrom)
        {
            SchemaName = schemaName;
            Types = types;
            Columns = columns;
            Inherits = inherits;
            AssignedType = assignedType;
            StartTimestampProperty = startTimestampProperty;
            EndTimestampProperty = endTimestampProperty;
            CopyFrom = copyFrom;
        }

        /// <summary>
        /// Returns schema name.
        /// </summary>
        /// <value>schema name</value>
        public string SchemaName { get; private set; }

        /// <summary>
        /// Returns column definitions.
        /// </summary>
        /// <value>column defs</value>
        public IList<ColumnDesc> Columns { get; private set; }

        /// <summary>
        /// Returns supertypes.
        /// </summary>
        /// <value>supertypes</value>
        public ICollection<string> Inherits { get; private set; }

        /// <summary>
        /// Returns type Name(s).
        /// </summary>
        /// <value>types</value>
        public ICollection<string> Types { get; private set; }

        public AssignedType AssignedType { get; private set; }

        public string StartTimestampProperty { get; private set; }

        public string EndTimestampProperty { get; private set; }

        public ICollection<string> CopyFrom { get; private set; }
    }

    public enum AssignedType
    {
        VARIANT,
        MAP,
        OBJECTARRAY,
        AVRO,
        NONE
    }

    public static class AssignedTypeExtensions
    {
        public static AssignedType ParseKeyword(string keywordNodeText)
        {
            switch (keywordNodeText.ToLowerInvariant()) {
                case "variant":
                    return AssignedType.VARIANT;

                case "map":
                    return AssignedType.MAP;

                case "objectarray":
                    return AssignedType.OBJECTARRAY;

                case "avro":
                    return AssignedType.AVRO;
            }

            throw new EPException(
                "Expected 'variant', 'map', 'objectarray' or 'avro' keyword after create-schema clause but encountered '" +
                keywordNodeText +
                "'");
        }

        public static AssignedType MapFrom(CreateSchemaClauseTypeDef? typeDefinition)
        {
            if (typeDefinition.HasValue) {
                switch (typeDefinition.Value) {
                    case CreateSchemaClauseTypeDef.NONE:
                        return AssignedType.NONE;

                    case CreateSchemaClauseTypeDef.MAP:
                        return AssignedType.MAP;

                    case CreateSchemaClauseTypeDef.OBJECTARRAY:
                        return AssignedType.OBJECTARRAY;

                    case CreateSchemaClauseTypeDef.AVRO:
                        return AssignedType.AVRO;
                }
            }

            return AssignedType.VARIANT;
        }

        public static CreateSchemaClauseTypeDef MapToSoda(this AssignedType value)
        {
            switch (value) {
                case AssignedType.VARIANT:
                    return CreateSchemaClauseTypeDef.VARIANT;

                case AssignedType.MAP:
                    return CreateSchemaClauseTypeDef.MAP;

                case AssignedType.OBJECTARRAY:
                    return CreateSchemaClauseTypeDef.OBJECTARRAY;

                case AssignedType.AVRO:
                    return CreateSchemaClauseTypeDef.AVRO;

                default:
                    return CreateSchemaClauseTypeDef.NONE;
            }
        }
    }
} // end of namespace