using System.Collections.Generic;
using System.IO;
using System.Linq;
using log4net;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Exceptions;
using Microsoft.Build.Execution;
using SlimJim.Model;

namespace SlimJim.Infrastructure
{
    public class DynamicCsProjReader : CsProjReader
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(DynamicCsProjReader));
        const string targetName = "ResolveAssemblyReferences";

        public override CsProj Read(FileInfo csProjFile)
        {
            Log.Debug("Loading " + csProjFile);

            ProjectInstance projectInstance;

            try
            {
                projectInstance = new Project(csProjFile.FullName).CreateProjectInstance();
            }
            catch (InvalidProjectFileException e)
            {
                Log.ErrorFormat("Failed to load project: {0}", e.Message);
                return null;
            }

            InvokeMSBuildTarget(projectInstance);

            var assemblyReferences = projectInstance.GetItems("Reference").Select(r => UnQualify(r.EvaluatedInclude));
            var projectReferences = projectInstance.GetItems("ProjectReference").Select(r => r.GetMetadataValue("Project"));
                
            var assemblyName = projectInstance.GetPropertyValue("AssemblyName");
            var projectGuid = projectInstance.GetPropertyValue("ProjectGuid");

            return new CsProj
            {
                Path = csProjFile.FullName,
                AssemblyName = assemblyName,
                Guid = projectGuid,
                ReferencedAssemblyNames = assemblyReferences.ToList(),
                ReferencedProjectGuids = projectReferences.ToList()
            };
        }

        public virtual void InvokeMSBuildTarget(ProjectInstance projectInstance)
        {
            var result = BuildManager.DefaultBuildManager.Build(new BuildParameters(new ProjectCollection()), new BuildRequestData(projectInstance, new[] { targetName }));

            if (result.OverallResult != BuildResultCode.Success)
            {
                Log.WarnFormat("Failed to execute target {0} on project {1}: {2}", targetName, projectInstance.FullPath, result.Exception.Message);
            }
        }
    }
}