///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.expression.core
{
    public class ExprIdentNodeUtil
    {
        public static Pair<PropertyResolutionDescriptor, string> GetTypeFromStream(
            StreamTypeService streamTypeService,
            string propertyNameNestable,
            bool explicitPropertiesOnly,
            bool obtainFragment)
        {
            string streamOrProp = null;
            var prop = propertyNameNestable;
            if (propertyNameNestable.IndexOf('.') != -1)
            {
                prop = propertyNameNestable.Substring(propertyNameNestable.IndexOf('.') + 1);
                streamOrProp = propertyNameNestable.Substring(0, propertyNameNestable.IndexOf('.'));
            }
            if (explicitPropertiesOnly)
            {
                return GetTypeFromStreamExplicitProperties(streamTypeService, prop, streamOrProp, obtainFragment);
            }
            return GetTypeFromStream(streamTypeService, prop, streamOrProp, obtainFragment);
        }

        internal static Pair<PropertyResolutionDescriptor, string> GetTypeFromStream(
            StreamTypeService streamTypeService,
            string unresolvedPropertyName,
            string streamOrPropertyName,
            bool obtainFragment)
        {
            PropertyResolutionDescriptor propertyInfo = null;

            // no stream/property name supplied
            if (streamOrPropertyName == null)
            {
                try
                {
                    propertyInfo = streamTypeService.ResolveByPropertyName(unresolvedPropertyName, obtainFragment);
                }
                catch (StreamTypesException ex)
                {
                    throw GetSuggestionException(ex);
                }
                catch (PropertyAccessException ex)
                {
                    throw new ExprValidationPropertyException(
                        "Failed to find property '" + unresolvedPropertyName +
                        "', the property name does not parse (are you sure?): " + ex.Message, ex);
                }

                // resolves without a stream name, return descriptor and null stream name
                return new Pair<PropertyResolutionDescriptor, string>(propertyInfo, propertyInfo.StreamName);
            }

            // try to resolve the property name and stream name as it is (ie. stream name as a stream name)
            StreamTypesException typeExceptionOne;
            try
            {
                propertyInfo = streamTypeService.ResolveByStreamAndPropName(
                    streamOrPropertyName, unresolvedPropertyName, obtainFragment);
                // resolves with a stream name, return descriptor and stream name
                return new Pair<PropertyResolutionDescriptor, string>(propertyInfo, streamOrPropertyName);
            }
            catch (StreamTypesException ex)
            {
                typeExceptionOne = ex;
            }

            // try to resolve the property name to a nested property 's0.p0'
            StreamTypesException typeExceptionTwo;
            var propertyNameCandidate = streamOrPropertyName + '.' + unresolvedPropertyName;
            try
            {
                propertyInfo = streamTypeService.ResolveByPropertyName(propertyNameCandidate, obtainFragment);
                // resolves without a stream name, return null for stream name
                return new Pair<PropertyResolutionDescriptor, string>(propertyInfo, null);
            }
            catch (StreamTypesException ex)
            {
                typeExceptionTwo = ex;
            }

            // not resolved yet, perhaps the table name did not match an event type
            if (streamTypeService.HasTableTypes && streamOrPropertyName != null)
            {
                for (var i = 0; i < streamTypeService.EventTypes.Length; i++)
                {
                    var eventType = streamTypeService.EventTypes[i];
                    var tableName = TableServiceUtil.GetTableNameFromEventType(eventType);
                    if (tableName != null && tableName.Equals(streamOrPropertyName))
                    {
                        try
                        {
                            propertyInfo = streamTypeService.ResolveByStreamAndPropName(
                                eventType.Name, unresolvedPropertyName, obtainFragment);
                        }
                        catch (Exception)
                        {
                        }
                        if (propertyInfo != null)
                        {
                            return new Pair<PropertyResolutionDescriptor, string>(propertyInfo, streamOrPropertyName);
                        }
                    }
                }
            }

            // see if the stream or property name (the prefix) can be resolved by itself, without suffix
            // the property available may be indexed or mapped
            try
            {
                var desc = streamTypeService.ResolveByPropertyName(streamOrPropertyName, false);
                if (desc != null)
                {
                    var d2 = desc.StreamEventType.GetPropertyDescriptor(streamOrPropertyName);
                    if (d2 != null)
                    {
                        string text = null;
                        if (d2.IsIndexed)
                        {
                            text = "an indexed property and requires an index or enumeration method to access values";
                        }
                        if (d2.IsMapped)
                        {
                            text = "a mapped property and requires keyed access";
                        }
                        if (text != null)
                        {
                            throw new ExprValidationPropertyException(
                                "Failed to resolve property '" + propertyNameCandidate + "' (property '" +
                                streamOrPropertyName + "' is " + text + ")");
                        }
                    }
                }
            }
            catch (StreamTypesException)
            {
                // need not be handled
            }

            throw GetSuggestionExceptionSecondStep(propertyNameCandidate, typeExceptionOne, typeExceptionTwo);
        }

        internal static Pair<PropertyResolutionDescriptor, string> GetTypeFromStreamExplicitProperties(
            StreamTypeService streamTypeService,
            string unresolvedPropertyName,
            string streamOrPropertyName,
            bool obtainFragment)
        {
            PropertyResolutionDescriptor propertyInfo;

            // no stream/property name supplied
            if (streamOrPropertyName == null)
            {
                try
                {
                    propertyInfo = streamTypeService.ResolveByPropertyNameExplicitProps(
                        unresolvedPropertyName, obtainFragment);
                }
                catch (StreamTypesException ex)
                {
                    throw GetSuggestionException(ex);
                }
                catch (PropertyAccessException ex)
                {
                    throw new ExprValidationPropertyException(ex.Message);
                }

                // resolves without a stream name, return descriptor and null stream name
                return new Pair<PropertyResolutionDescriptor, string>(propertyInfo, propertyInfo.StreamName);
            }

            // try to resolve the property name and stream name as it is (ie. stream name as a stream name)
            StreamTypesException typeExceptionOne;
            try
            {
                propertyInfo = streamTypeService.ResolveByStreamAndPropNameExplicitProps(
                    streamOrPropertyName, unresolvedPropertyName, obtainFragment);
                // resolves with a stream name, return descriptor and stream name
                return new Pair<PropertyResolutionDescriptor, string>(propertyInfo, streamOrPropertyName);
            }
            catch (StreamTypesException ex)
            {
                typeExceptionOne = ex;
            }

            // try to resolve the property name to a nested property 's0.p0'
            StreamTypesException typeExceptionTwo;
            var propertyNameCandidate = streamOrPropertyName + '.' + unresolvedPropertyName;
            try
            {
                propertyInfo = streamTypeService.ResolveByPropertyNameExplicitProps(
                    propertyNameCandidate, obtainFragment);
                // resolves without a stream name, return null for stream name
                return new Pair<PropertyResolutionDescriptor, string>(propertyInfo, null);
            }
            catch (StreamTypesException ex)
            {
                typeExceptionTwo = ex;
            }

            throw GetSuggestionExceptionSecondStep(propertyNameCandidate, typeExceptionOne, typeExceptionTwo);
        }

        private static ExprValidationPropertyException GetSuggestionExceptionSecondStep(
            string propertyNameCandidate,
            StreamTypesException typeExceptionOne,
            StreamTypesException typeExceptionTwo)
        {
            var suggestionOne = GetSuggestion(typeExceptionOne);
            var suggestionTwo = GetSuggestion(typeExceptionTwo);
            if (suggestionOne != null)
            {
                return new ExprValidationPropertyException(typeExceptionOne.Message + suggestionOne);
            }
            if (suggestionTwo != null)
            {
                return new ExprValidationPropertyException(typeExceptionTwo.Message + suggestionTwo);
            }

            // fail to resolve
            return
                new ExprValidationPropertyException(
                    "Failed to resolve property '" + propertyNameCandidate +
                    "' to a stream or nested property in a stream");
        }

        private static ExprValidationPropertyException GetSuggestionException(StreamTypesException ex)
        {
            var suggestion = GetSuggestion(ex);
            if (suggestion != null)
            {
                return new ExprValidationPropertyException(ex.Message + suggestion);
            }
            else
            {
                return new ExprValidationPropertyException(ex.Message);
            }
        }

        private static string GetSuggestion(StreamTypesException ex)
        {
            if (ex == null)
            {
                return null;
            }
            var suggestion = ex.OptionalSuggestion;
            if (suggestion == null)
            {
                return null;
            }
            if (suggestion.First > LevenshteinDistance.ACCEPTABLE_DISTANCE)
            {
                return null;
            }
            return " (did you mean '" + ex.OptionalSuggestion.Second + "'?)";
        }
    }
} // end of namespace