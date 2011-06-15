﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace SlimJim.Model
{
	public class SlnGenerationOptions
	{
		private const string DefaultSolutionName = "SlimJim";
		private string projectsRootDirectory;
		private string solutionName;
		private string slnOutputPath;
		private readonly List<string> additionalSearchPaths;
		private readonly List<string> ignoreDirectoryPatterns;

		public SlnGenerationOptions(string workingDirectory)
		{
			ProjectsRootDirectory = workingDirectory;
			additionalSearchPaths = new List<string>();
			ignoreDirectoryPatterns = new List<string>();
			TargetProjectNames = new List<string>();
			VisualStudioVersion = VisualStudioVersion.VS2010;
		}

		public List<string> TargetProjectNames { get; private set; }

		public string ProjectsRootDirectory
		{
			get { return projectsRootDirectory; }
			set
			{
				projectsRootDirectory = ResolvePath(value);
			}
		}

		public VisualStudioVersion VisualStudioVersion { get; set; }
		public bool IncludeEfferentAssemblyReferences { get; set; }
		public bool ShowHelp { get; set; }
		public bool OpenInVisualStudio { get; set; }

		public List<string> AdditionalSearchPaths
		{
			get
			{
				return additionalSearchPaths.ConvertAll(ResolvePath);
			}
		}

		private string ResolvePath(string p)
		{
			return !Path.IsPathRooted(p) ? Path.Combine(ProjectsRootDirectory, p) : p;
		}

		public string SlnOutputPath
		{
			get { return slnOutputPath != null ? ResolvePath(slnOutputPath) : ProjectsRootDirectory; }
			set { slnOutputPath = value; }
		}

		public string SolutionName
		{
			get
			{
				if (string.IsNullOrEmpty(solutionName))
				{
					if (TargetProjectNames.Count == 1)
					{
						return TargetProjectNames.First();
					}

					if (TargetProjectNames.Count > 1)
					{
						return string.Join("_", TargetProjectNames);
					}

					if (!string.IsNullOrEmpty(ProjectsRootDirectory))
					{
						return GetLastSegmentNameOfProjectsRootDirectory();
					}

					return DefaultSolutionName;
				}

				return solutionName;
			}
			set { solutionName = value; }
		}

		private string GetLastSegmentNameOfProjectsRootDirectory()
		{
			MatchCollection matches = Regex.Matches(ProjectsRootDirectory, @"([^\\:]+)\\?");
			string lastSegment = DefaultSolutionName;
			foreach (Match match in matches)
			{
				string segmentName = match.Groups[1].Value;
				if (segmentName != "")
				{
					lastSegment = segmentName;
				}
			}

			return lastSegment;
		}

		public SlnGenerationMode Mode
		{
			get
			{
				return TargetProjectNames.Count == 0
					? SlnGenerationMode.FullGraph
					: SlnGenerationMode.PartialGraph;
			}
		}

		public List<string> IgnoreDirectoryPatterns
		{
			get { return ignoreDirectoryPatterns; }
		}

		public void AddAdditionalSearchPaths(params string[] searchPaths)
		{
			additionalSearchPaths.AddRange(searchPaths);
		}

		public void AddTargetProjectNames(params string[] targetProjectNames)
		{
			TargetProjectNames.AddRange(targetProjectNames);
		}

		public void AddIgnoreDirectoryPatterns(params string [] patterns)
		{
			ignoreDirectoryPatterns.AddRange(patterns);
		}
	}
}