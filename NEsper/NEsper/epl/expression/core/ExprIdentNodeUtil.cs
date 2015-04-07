///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
	    public static Pair<PropertyResolutionDescriptor, string> GetTypeFromStream(StreamTypeService streamTypeService, string propertyNameNestable, bool explicitPropertiesOnly, bool obtainFragment)
        {
	        string streamOrProp = null;
	        string prop = propertyNameNestable;
	        if (propertyNameNestable.IndexOf('.') != -1) {
	            prop = propertyNameNestable.Substring(propertyNameNestable.IndexOf('.') + 1);
	            streamOrProp = propertyNameNestable.Substring(0, propertyNameNestable.IndexOf('.'));
	        }
	        if (explicitPropertiesOnly) {
	            return GetTypeFromStreamExplicitProperties(streamTypeService, prop, streamOrProp, obtainFragment);
	        }
	        return GetTypeFromStream(streamTypeService, prop, streamOrProp, obtainFragment);
	    }

	    /// <summary>
	    /// Determine stream id and property type given an unresolved property name and
	    /// a stream name that may also be part of the property name.
	    /// <para />For example: select s0.p1 from...    p1 is the property name, s0 the stream name, however this could also be a nested property
	    /// </summary>
	    /// <param name="streamTypeService">service for type infos</param>
	    /// <param name="unresolvedPropertyName">property name</param>
	    /// <param name="streamOrPropertyName">stream name, this can also be the first part of the property name</param>
	    /// <returns>pair of stream number and property type</returns>
	    /// <throws>ExprValidationPropertyException if no such property exists</throws>
	    internal static Pair<PropertyResolutionDescriptor, string> GetTypeFromStream(StreamTypeService streamTypeService, string unresolvedPropertyName, string streamOrPropertyName, bool obtainFragment)
	    {
	        PropertyResolutionDescriptor propertyInfo = null;

	        // no stream/property name supplied
	        if (streamOrPropertyName == null) {
	            try {
	                propertyInfo = streamTypeService.ResolveByPropertyName(unresolvedPropertyName, obtainFragment);
	            }
	            catch (StreamTypesException ex) {
	                throw GetSuggestionException(ex);
	            }
	            catch (PropertyAccessException ex) {
	                throw new ExprValidationPropertyException("Failed to find property '" + unresolvedPropertyName + "', the property name does not parse (are you sure?): " + ex.Message, ex);
	            }

	            // resolves without a stream name, return descriptor and null stream name
	            return new Pair<PropertyResolutionDescriptor, string>(propertyInfo, propertyInfo.StreamName);
	        }

	        // try to resolve the property name and stream name as it is (ie. stream name as a stream name)
	        StreamTypesException typeExceptionOne;
	        try {
	            propertyInfo = streamTypeService.ResolveByStreamAndPropName(streamOrPropertyName, unresolvedPropertyName, obtainFragment);
	            // resolves with a stream name, return descriptor and stream name
	            return new Pair<PropertyResolutionDescriptor, string>(propertyInfo, streamOrPropertyName);
	        }
	        catch (StreamTypesException ex) {
	            typeExceptionOne = ex;
	        }

	        // try to resolve the property name to a nested property 's0.p0'
	        StreamTypesException typeExceptionTwo;
	        string propertyNameCandidate = streamOrPropertyName + '.' + unresolvedPropertyName;
	        try {
	            propertyInfo = streamTypeService.ResolveByPropertyName(propertyNameCandidate, obtainFragment);
	            // resolves without a stream name, return null for stream name
	            return new Pair<PropertyResolutionDescriptor, string>(propertyInfo, null);
	        }
	        catch (StreamTypesException ex) {
	            typeExceptionTwo = ex;
	        }

	        // not resolved yet, perhaps the table name did not match an event type
	        if (streamTypeService.HasTableTypes && streamOrPropertyName != null) {
	            for (int i = 0; i < streamTypeService.EventTypes.Length; i++) {
	                EventType eventType = streamTypeService.EventTypes[i];
	                string tableName = TableServiceUtil.GetTableNameFromEventType(eventType);
	                if (tableName != null && tableName.Equals(streamOrPropertyName)) {
	                    try {
	                        propertyInfo = streamTypeService.ResolveByStreamAndPropName(eventType.Name, unresolvedPropertyName, obtainFragment);
	                    }
	                    catch (Exception ex) {
	                    }
	                    if (propertyInfo != null) {
	                        return new Pair<PropertyResolutionDescriptor, string>(propertyInfo, streamOrPropertyName);
	                    }
	                }
	            }
	        }

	        // see if the stream or property name (the prefix) can be resolved by itself, without suffix
	        // the property available may be indexed or mapped
	        try {
	            PropertyResolutionDescriptor desc = streamTypeService.ResolveByPropertyName(streamOrPropertyName, false);
	            if (desc != null) {
	                EventPropertyDescriptor d2 = desc.StreamEventType.GetPropertyDescriptor(streamOrPropertyName);
	                if (d2 != null)
	                {
	                    string text = null;
	                    if (d2.PropertyType != typeof (string))
	                    {
	                        if (d2.IsIndexed)
	                        {
	                            text = "an indexed property and requires an index or enumeration method to access values";
	                        }
	                        if (d2.IsMapped)
	                        {
	                            text = "a mapped property and requires keyed access";
	                        }
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
	        catch (StreamTypesException e) {
	            // need not be handled
	        }

	        throw GetSuggestionExceptionSecondStep(propertyNameCandidate, typeExceptionOne, typeExceptionTwo);
	    }

	    /// <summary>
	    /// This method only resolves against explicitly-listed properties (for use with XML or other types that allow any name as a property name).
	    /// </summary>
	    /// <param name="streamTypeService">stream types</param>
	    /// <param name="unresolvedPropertyName">property name</param>
	    /// <param name="streamOrPropertyName">optional stream name</param>
	    /// <returns>property info</returns>
	    /// <throws>ExprValidationPropertyException if the property could not be resolved</throws>
	    internal static Pair<PropertyResolutionDescriptor, string> GetTypeFromStreamExplicitProperties(StreamTypeService streamTypeService, string unresolvedPropertyName, string streamOrPropertyName, bool obtainFragment)
	    {
	        PropertyResolutionDescriptor propertyInfo;

	        // no stream/property name supplied
	        if (streamOrPropertyName == null)
	        {
	            try
	            {
	                propertyInfo = streamTypeService.ResolveByPropertyNameExplicitProps(unresolvedPropertyName, obtainFragment);
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
	            propertyInfo = streamTypeService.ResolveByStreamAndPropNameExplicitProps(streamOrPropertyName, unresolvedPropertyName, obtainFragment);
	            // resolves with a stream name, return descriptor and stream name
	            return new Pair<PropertyResolutionDescriptor, string>(propertyInfo, streamOrPropertyName);
	        }
	        catch (StreamTypesException ex)
	        {
	            typeExceptionOne = ex;
	        }

	        // try to resolve the property name to a nested property 's0.p0'
	        StreamTypesException typeExceptionTwo;
	        string propertyNameCandidate = streamOrPropertyName + '.' + unresolvedPropertyName;
	        try
	        {
	            propertyInfo = streamTypeService.ResolveByPropertyNameExplicitProps(propertyNameCandidate, obtainFragment);
	            // resolves without a stream name, return null for stream name
	            return new Pair<PropertyResolutionDescriptor, string>(propertyInfo, null);
	        }
	        catch (StreamTypesException ex)
	        {
	            typeExceptionTwo = ex;
	        }

	        throw GetSuggestionExceptionSecondStep(propertyNameCandidate, typeExceptionOne, typeExceptionTwo);
	    }

	    private static ExprValidationPropertyException GetSuggestionExceptionSecondStep(string propertyNameCandidate, StreamTypesException typeExceptionOne, StreamTypesException typeExceptionTwo) {
	        string suggestionOne = GetSuggestion(typeExceptionOne);
	        string suggestionTwo = GetSuggestion(typeExceptionTwo);
	        if (suggestionOne != null)
	        {
	            return new ExprValidationPropertyException(typeExceptionOne.Message + suggestionOne);
	        }
	        if (suggestionTwo != null)
	        {
	            return new ExprValidationPropertyException(typeExceptionTwo.Message + suggestionTwo);
	        }

	        // fail to resolve
	        return new ExprValidationPropertyException("Failed to resolve property '" + propertyNameCandidate + "' to a stream or nested property in a stream");
	    }

	    private static ExprValidationPropertyException GetSuggestionException(StreamTypesException ex) {
	        string suggestion = GetSuggestion(ex);
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
	        var optionalSuggestion = ex.OptionalSuggestion;
	        if (optionalSuggestion == null)
	        {
	            return null;
	        }
	        if (optionalSuggestion.First > LevenshteinDistance.ACCEPTABLE_DISTANCE)
	        {
	            return null;
	        }
	        return " (did you mean '" + optionalSuggestion.Second + "'?)";
	    }
	}
} // end of namespace
