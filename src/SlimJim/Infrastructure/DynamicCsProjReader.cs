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

        public override CsProj Read(FileInfo csProjFile)
        {
            Log.Debug("Loading " + csProjFile);

            const string targetName = "ResolveAssemblyReferences";

            ProjectInstance projectInstance;
            IEnumerable<string> assemblyReferences = new string[0];
            IEnumerable<string> projectReferences = new string[0];
            var assemblyName = "";
            var projectGuid = "";
            
            try
            {
                projectInstance = new Project(csProjFile.FullName).CreateProjectInstance();

                var result = BuildManager.DefaultBuildManager.Build(new BuildParameters(new ProjectCollection()), new BuildRequestData(projectInstance, new[] { targetName }));

                if (result.OverallResult != BuildResultCode.Success)
                {
                    Log.WarnFormat("Failed to execute target {0} on project {1}: {2}", targetName, csProjFile, result.Exception.Message);
                }

                assemblyReferences = projectInstance.GetItems("Reference").Select(r => UnQualify(r.EvaluatedInclude));
                projectReferences = projectInstance.GetItems("ProjectReference").Select(r => r.GetMetadataValue("Project"));
                
                assemblyName = projectInstance.GetPropertyValue("AssemblyName");
                projectGuid = projectInstance.GetPropertyValue("ProjectGuid");
            }
            catch (InvalidProjectFileException e)
            {
                Log.ErrorFormat("Failed to load project: {0}", e.Message);
            }

            return new CsProj
            {
                Path = csProjFile.FullName,
                AssemblyName = assemblyName,
                Guid = projectGuid,
                ReferencedAssemblyNames = assemblyReferences.ToList(),
                ReferencedProjectGuids = projectReferences.ToList()
            };
        }
    }
}