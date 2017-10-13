///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;

namespace com.espertech.esper.client.deploy
{
    /// <summary>
    /// Service to package and deploy EPL statements organized into an EPL module.
    /// </summary>
    public interface EPDeploymentAdmin
    {
        /// <summary>Read the input stream and return the module. It is up to the calling method to close the stream when done. </summary>
        /// <param name="stream">to read</param>
        /// <param name="moduleUri">uri of the module</param>
        /// <returns>module</returns>
        /// <throws>IOException when the io operation failed</throws>
        /// <throws>ParseException when parsing of the module failed</throws>
        Module Read(Stream stream, String moduleUri);

        /// <summary>Read the resource by opening from classpath and return the module. </summary>
        /// <param name="resource">name of the classpath resource</param>
        /// <returns>module</returns>
        /// <throws>IOException when the resource could not be read</throws>
        /// <throws>ParseException when parsing of the module failed</throws>
        Module Read(String resource);

        /// <summary>Read the module by reading the text file and return the module. </summary>
        /// <param name="file">the file to read</param>
        /// <returns>module</returns>
        /// <throws>IOException when the file could not be read</throws>
        /// <throws>ParseException when parsing of the module failed</throws>
        Module Read(FileInfo file);

        /// <summary>Read the module by reading from the URL provided and return the module. </summary>
        /// <param name="url">the URL to read</param>
        /// <returns>module</returns>
        /// <throws>IOException when the url input stream could not be read</throws>
        /// <throws>ParseException when parsing of the module failed</throws>
        Module Read(Uri url);

        /// <summary>Parse the module text passed in, returning the module. </summary>
        /// <param name="eplModuleText">to parse</param>
        /// <returns>module</returns>
        /// <throws>IOException when the parser failed to read the string buffer</throws>
        /// <throws>ParseException when parsing of the module failed</throws>
        Module Parse(String eplModuleText);

        /// <summary>Compute a deployment order among the modules passed in considering their uses-dependency declarations and considering the already-deployed modules. <para />The operation also checks and reports circular dependencies. <para />Pass in @{link DeploymentOrderOptions} to customize the behavior if this method. When passing no options or passing default options, the default behavior checks uses-dependencies and circular dependencies. </summary>
        /// <param name="modules">to determine ordering for</param>
        /// <param name="options">operation options or null for default options</param>
        /// <returns>ordered modules</returns>
        /// <throws>DeploymentOrderException when any module dependencies are not satisfied</throws>
        DeploymentOrder GetDeploymentOrder(ICollection<Module> modules, DeploymentOrderOptions options);

        /// <summary>Deploy a single module returning a generated deployment id to use when undeploying statements as well as additional statement-level information. <para />Pass in @{link DeploymentOptions} to customize the behavior. When passing no options or passing default options, the operation first compiles all EPL statements before starting each statement, fails-fast on the first statement that fails to start and rolls back (destroys) any started statement on a failure. <para />When setting validate-only in the deployment options, the method returns a null-value on success. </summary>
        /// <param name="module">to deploy</param>
        /// <param name="options">operation options or null for default options</param>
        /// <returns>result object with statement detail, or null for pass on validate-only</returns>
        /// <throws>DeploymentActionException when the deployment fails, contains a list of deployment failures</throws>
        DeploymentResult Deploy(Module module, DeploymentOptions options);

        /// <summary>Deploy a single module using the deployment id provided as a parameter. <para />Pass in @{link DeploymentOptions} to customize the behavior. When passing no options or passing default options, the operation first compiles all EPL statements before starting each statement, fails-fast on the first statement that fails to start and rolls back (destroys) any started statement on a failure. <para />When setting validate-only in the deployment options, the method returns a null-value on success. </summary>
        /// <param name="module">to deploy</param>
        /// <param name="options">operation options or null for default options</param>
        /// <param name="assignedDeploymentId">the deployment id to assign</param>
        /// <returns>result object with statement detail, or null for pass on validate-only</returns>
        /// <throws>DeploymentActionException when the deployment fails, contains a list of deployment failures</throws>
        DeploymentResult Deploy(Module module, DeploymentOptions options, String assignedDeploymentId);

        /// <summary>Undeploy a single module, if its in deployed state, and removes it from the known modules. <para />This operation destroys all statements previously associated to the deployed module and also removes this module from the list deployments list. </summary>
        /// <param name="deploymentId">of the deployment to undeploy.</param>
        /// <returns>result object with statement-level detail</returns>
        /// <throws>DeploymentNotFoundException when the deployment id could not be resolved to a deployment</throws>
        UndeploymentResult UndeployRemove(String deploymentId);

        /// <summary>Undeploy a single module, if its in deployed state, and removes it from the known modules. <para />This operation, by default, destroys all statements previously associated to the deployed module and also removes this module from the list deployments list. Use the options object to control whether statements get destroyed. </summary>
        /// <param name="deploymentId">of the deployment to undeploy.</param>
        /// <param name="undeploymentOptions">for controlling undeployment, can be a null value</param>
        /// <returns>result object with statement-level detail</returns>
        /// <throws>DeploymentNotFoundException when the deployment id could not be resolved to a deployment</throws>
        UndeploymentResult UndeployRemove(String deploymentId, UndeploymentOptions undeploymentOptions);

        /// <summary>Return deployment ids of all currently known modules. </summary>
        /// <value>array of deployment ids</value>
        string[] Deployments { get; }

        /// <summary>Returns the deployment information for a given deployment. </summary>
        /// <param name="deploymentId">to return the deployment information for.</param>
        /// <returns>deployment info</returns>
        DeploymentInformation GetDeployment(String deploymentId);

        /// <summary>Returns deployment information for all known modules. </summary>
        /// <value>deployment information.</value>
        DeploymentInformation[] DeploymentInformation { get; }

        /// <summary>Determine if a named module is already deployed (in deployed state), returns true if one or more modules of the same name are deployed or false when no module of that name is deployed. </summary>
        /// <param name="moduleName">to look up</param>
        /// <returns>indicator</returns>
        bool IsDeployed(String moduleName);

        /// <summary>Shortcut method to read and deploy a single module from a classpath resource. <para />Uses default options for performing deployment dependency checking and deployment. </summary>
        /// <param name="resource">to read</param>
        /// <param name="moduleURI">uri of module to assign or null if not applicable</param>
        /// <param name="moduleArchive">archive name of module to assign or null if not applicable</param>
        /// <param name="userObject">user object to assign to module, passed along unused as part of deployment information, or null if not applicable</param>
        /// <returns>deployment result object</returns>
        /// <throws>IOException when the file could not be read</throws>
        /// <throws>ParseException when parsing of the module failed</throws>
        /// <throws>DeploymentOrderException when any module dependencies are not satisfied</throws>
        /// <throws>DeploymentActionException when the deployment fails, contains a list of deployment failures</throws>
        DeploymentResult ReadDeploy(String resource, String moduleURI, String moduleArchive, Object userObject);

        /// <summary>Shortcut method to read and deploy a single module from an input stream. <para />Uses default options for performing deployment dependency checking and deployment. <para />Leaves the stream unclosed. </summary>
        /// <param name="stream">to read</param>
        /// <param name="moduleURI">uri of module to assign or null if not applicable</param>
        /// <param name="moduleArchive">archive name of module to assign or null if not applicable</param>
        /// <param name="userObject">user object to assign to module, passed along unused as part of deployment information, or null if not applicable</param>
        /// <returns>deployment result object</returns>
        /// <throws>IOException when the file could not be read</throws>
        /// <throws>ParseException when parsing of the module failed</throws>
        /// <throws>DeploymentOrderException when any module dependencies are not satisfied</throws>
        /// <throws>DeploymentActionException when the deployment fails, contains a list of deployment failures</throws>
        DeploymentResult ReadDeploy(Stream stream, String moduleURI, String moduleArchive, Object userObject);

        /// <summary>Shortcut method to parse and deploy a single module from a string text buffer. <para />Uses default options for performing deployment dependency checking and deployment. </summary>
        /// <param name="eplModuleText">to parse</param>
        /// <param name="moduleURI">uri of module to assign or null if not applicable</param>
        /// <param name="moduleArchive">archive name of module to assign or null if not applicable</param>
        /// <param name="userObject">user object to assign to module, passed along unused as part of deployment information, or null if not applicable</param>
        /// <returns>deployment result object</returns>
        /// <throws>IOException when the file could not be read</throws>
        /// <throws>ParseException when parsing of the module failed</throws>
        /// <throws>DeploymentOrderException when any module dependencies are not satisfied</throws>
        /// <throws>DeploymentActionException when the deployment fails, contains a list of deployment failures</throws>
        DeploymentResult ParseDeploy(String eplModuleText, String moduleURI, String moduleArchive, Object userObject);

        /// <summary>Shortcut method to parse and deploy a single module from a string text buffer, without providing a module URI name or archive name or user object. The module URI, archive name and user object are defaulted to null. <para />Uses default options for performing deployment dependency checking and deployment. </summary>
        /// <param name="eplModuleText">to parse</param>
        /// <returns>deployment result object</returns>
        /// <throws>IOException when the file could not be read</throws>
        /// <throws>ParseException when parsing of the module failed</throws>
        /// <throws>DeploymentOrderException when any module dependencies are not satisfied</throws>
        /// <throws>DeploymentActionException when the deployment fails, contains a list of deployment failures</throws>
        DeploymentResult ParseDeploy(String eplModuleText);

        /// <summary>Adds a module in undeployed state, generating a deployment id and returning the generated deployment id of the module. </summary>
        /// <param name="module">to add</param>
        /// <returns>The deployment id assigned to the module</returns>
        String Add(Module module);

        /// <summary>Adds a module in undeployed state, using the provided deployment id as a unique identifier for the module. </summary>
        /// <param name="module">to add</param>
        /// <param name="assignedDeploymentId">deployment id to assign</param>
        void Add(Module module, String assignedDeploymentId);

        /// <summary>Remove a module that is currently in undeployed state. <para />This call may only be used on undeployed modules. </summary>
        /// <param name="deploymentId">of the module to remove</param>
        /// <throws>DeploymentStateException when attempting to remove a module that does not exist or a module that is not in undeployed state</throws>
        /// <throws>DeploymentNotFoundException if no such deployment id is known</throws>
        void Remove(String deploymentId);

        /// <summary>Deploy a previously undeployed module. </summary>
        /// <param name="deploymentId">of the module to deploy</param>
        /// <param name="options">deployment options</param>
        /// <returns>deployment result</returns>
        /// <throws>DeploymentStateException when attempting to deploy a module that does not exist is already deployed</throws>
        /// <throws>DeploymentOrderException when deployment dependencies are not satisfied</throws>
        /// <throws>DeploymentActionException when the deployment (or validation when setting validate-only) failed</throws>
        /// <throws>DeploymentNotFoundException if no such deployment id is known</throws>
        DeploymentResult Deploy(String deploymentId, DeploymentOptions options);

        /// <summary>Undeploy a previously deployed module. </summary>
        /// <param name="deploymentId">of the module to undeploy</param>
        /// <returns>undeployment result</returns>
        /// <throws>DeploymentStateException when attempting to undeploy a module that does not exist is already undeployed</throws>
        /// <throws>DeploymentNotFoundException when the deployment id could not be resolved</throws>
        UndeploymentResult Undeploy(String deploymentId);

        /// <summary>Undeploy a previously deployed module. </summary>
        /// <param name="deploymentId">of the module to undeploy</param>
        /// <param name="undeploymentOptions">undeployment options, or null for default behavior</param>
        /// <returns>undeployment result</returns>
        /// <throws>DeploymentStateException when attempting to undeploy a module that does not exist is already undeployed</throws>
        /// <throws>DeploymentNotFoundException when the deployment id could not be resolved</throws>
        UndeploymentResult Undeploy(String deploymentId, UndeploymentOptions undeploymentOptions);
    }
}
