///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.soda;

namespace com.espertech.esper.common.@internal.compile.stage1.spec
{
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

                case "json":
                    return AssignedType.JSON;

                case "xml":
                    return AssignedType.XML;
            }

            throw new EPException(
                "Expected 'variant', 'map', 'json', 'xml', 'avro' or 'objectarray' keyword after create-schema clause but encountered '" +
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

                    case CreateSchemaClauseTypeDef.JSON:
                        return AssignedType.JSON;

                    case CreateSchemaClauseTypeDef.XML:
                        return AssignedType.XML;
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

                case AssignedType.JSON:
                    return CreateSchemaClauseTypeDef.JSON;

                case AssignedType.XML:
                    return CreateSchemaClauseTypeDef.XML;
            }

            return CreateSchemaClauseTypeDef.NONE;
        }
    }
}