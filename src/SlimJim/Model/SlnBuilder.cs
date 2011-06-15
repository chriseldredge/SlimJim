﻿using System.Collections.Generic;
using System.Reflection;
using log4net;

namespace SlimJim.Model
{
	public class SlnBuilder
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		private readonly List<CsProj> projectsList;
		private Sln builtSln;
		private static SlnBuilder overriddenBuilder;
	    private SlnGenerationOptions options;

	    public SlnBuilder(List<CsProj> projectsList)
		{
			this.projectsList = projectsList;
		}

		public virtual Sln BuildSln(SlnGenerationOptions options)
		{
	        this.options = options;
	        builtSln = new Sln(options.SolutionName)
				{
					Version = options.VisualStudioVersion,
					ProjectsRootDirectory = options.ProjectsRootDirectory
				};

			AddProjectsToSln(options);

			return builtSln;
		}

		private void AddProjectsToSln(SlnGenerationOptions options)
		{
			if (options.Mode == SlnGenerationMode.PartialGraph)
			{
				AddPartialProjectGraphToSln(options);
			}
			else
			{
				AddAllProjectsToSln();
			}
		}

		private void AddPartialProjectGraphToSln(SlnGenerationOptions options)
		{
			Log.Info("Building partial graph solution for target projects: " + string.Join(", ", options.TargetProjectNames));

	        foreach (string targetProjectName in options.TargetProjectNames)
			{
				CsProj rootProject = AddAssemblySubtree(targetProjectName);

				if (rootProject == null)
				{
					Log.WarnFormat("Project {0} not found.", targetProjectName);
				}

				AddAfferentReferencesToProject(rootProject);
			}
		}

		private void AddAllProjectsToSln()
		{
			Log.Info("Building full graph solution.");

			projectsList.ForEach(AddProject);
		}

	    private CsProj AddAssemblySubtree(string assemblyName)
	    {
	        CsProj project = FindProjectByAssemblyName(assemblyName);

	        AddProjectAndReferences(project);

	        return project;
	    }

	    private CsProj FindProjectByAssemblyName(string assemblyName)
	    {
	    	return projectsList.Find(csp => csp.AssemblyName == assemblyName);
	    }

		private void AddProjectAndReferences(CsProj project)
	    {
	        if (project != null)
	        {
				AddProject(project);

	            IncludeEfferentProjectReferences(project);

                if (options.IncludeEfferentAssemblyReferences)
                {
                    IncludeEfferentAssemblyReferences(project);
                }
	        }
	    }

		private void AddProject(CsProj project)
		{
			builtSln.AddProjects(project);
		}

	    private void IncludeEfferentProjectReferences(CsProj project)
	    {
	        foreach (string projectGuid in project.ReferencedProjectGuids)
	        {
	            AddProjectSubtree(projectGuid);
	        }
	    }

	    private void IncludeEfferentAssemblyReferences(CsProj project)
	    {
	        foreach (string assemblyName in project.ReferencedAssemblyNames)
	        {
	            AddAssemblySubtree(assemblyName);
	        }
	    }

	    private void AddProjectSubtree(string projectGuid)
	    {
	        CsProj referencedProject = FindProjectByProjectGuid(projectGuid);

	        AddProjectAndReferences(referencedProject);
	    }

	    private void AddAfferentReferencesToProject(CsProj project)
		{
			if (project != null)
			{
				List<CsProj> afferentAssemblyReferences = projectsList.FindAll(
					csp => csp.ReferencedAssemblyNames.Contains(project.AssemblyName));

				AddAfferentReferences(afferentAssemblyReferences);

				List<CsProj> afferentProjectReferences =
					projectsList.FindAll(csp => csp.ReferencedProjectGuids.Contains(project.Guid));

				AddAfferentReferences(afferentProjectReferences);
			}
		}

	    private void AddAfferentReferences(List<CsProj> afferentReferences)
		{
			foreach (CsProj assemblyReference in afferentReferences)
			{
				AddProjectAndReferences(assemblyReference);
			}
		}

	    private CsProj FindProjectByProjectGuid(string projectGuid)
		{
			return projectsList.Find(csp => csp.Guid == projectGuid);
		}

	    public static SlnBuilder GetSlnBuilder(List<CsProj> projects)
		{
			return overriddenBuilder ?? new SlnBuilder(projects);
		}

		public static void OverrideDefaultBuilder(SlnBuilder slnBuilder)
		{
			overriddenBuilder = slnBuilder;
		}
	}
}