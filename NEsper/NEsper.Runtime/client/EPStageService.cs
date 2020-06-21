///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.runtime.client.stage;

namespace com.espertech.esper.runtime.client
{
	/// <summary>
	///     Stages are used for staging deployments allowing independent control over event and time for subsets of deployments.
	///     <para>
	///         This API is under development for version 8.4 and newer, and is considered UNSTABLE.
	///     </para>
	///     <para>
	///         Stages are uniquely identified by a stage URI.
	///     </para>
	///     <para>
	///         Use {@link #getStage(String)} to allocate a stage, of if the stage is already allocated to obtain the stage.
	///     </para>
	///     <para>
	///         Use {@link #getExistingStage(String)} to obtain an existing stage without allocating.
	///     </para>
	/// </summary>
	public interface EPStageService
    {
	    /// <summary>
	    ///     Allocate a new stage or returns the existing stage if the stage for the same URI is already allocated.
	    /// </summary>
	    /// <param name="stageUri">unique identifier</param>
	    /// <returns>stage</returns>
	    /// <throws>EPRuntimeDestroyedException if the runtime is already destroyed</throws>
	    EPStage GetStage(string stageUri);

	    /// <summary>
	    ///     Returns the existing stage for the provided URI, or null if a stage for the URI has not already been allocated.
	    /// </summary>
	    /// <param name="stageUri">stage URI</param>
	    /// <returns>stage</returns>
	    /// <throws>EPRuntimeDestroyedException if the runtime is already destroyed</throws>
	    EPStage GetExistingStage(string stageUri);

	    /// <summary>
	    ///     Returns the URI values of all stages that are currently allocated.
	    /// </summary>
	    /// <value>stage URIs</value>
	    /// <throws>EPRuntimeDestroyedException if the runtime is already destroyed</throws>
	    string[] StageURIs { get; }
    }
} // end of namespace