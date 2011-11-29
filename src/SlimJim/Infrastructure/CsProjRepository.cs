using System.Collections.Generic;
using System.IO;
using SlimJim.Model;

namespace SlimJim.Infrastructure
{
	public class CsProjRepository : ICsProjRepository
	{
		public CsProjRepository()
		{
			Finder = new ProjectFileFinder();
		}

		public virtual List<CsProj> LookupCsProjsFromDirectory(SlnGenerationOptions options)
		{
			IgnoreConfiguredDirectoryPatterns(options);

			List<FileInfo> files = FindAllProjectFiles(options);
			List<CsProj> projects = ReadProjectFilesIntoCsProjObjects(files, options);

			return projects;
		}

        public virtual CsProjReader CreateCsProjReader(SlnGenerationOptions options)
        {
            return options.DynamicAssemblyReferenceResolution ? (CsProjReader)new DynamicCsProjReader() : new XmlParsingCsProjReader();
        }

		private void IgnoreConfiguredDirectoryPatterns(SlnGenerationOptions options)
		{
			if (options.IgnoreDirectoryPatterns.Count > 0)
			{
				Finder.IgnorePatterns(options.IgnoreDirectoryPatterns.ToArray());
			}
		}

		private List<FileInfo> FindAllProjectFiles(SlnGenerationOptions options)
		{
			List<FileInfo> files = Finder.FindAllProjectFiles(options.ProjectsRootDirectory);

			foreach (string path in options.AdditionalSearchPaths)
			{
				files.AddRange(Finder.FindAllProjectFiles(path));
			}

			return files;
		}

        private List<CsProj> ReadProjectFilesIntoCsProjObjects(List<FileInfo> files, SlnGenerationOptions options)
		{
            List<CsProj> projects = files.ConvertAll(f => CreateCsProjReader(options).Read(f));
			projects.RemoveAll(p => p == null);
			return projects;
		}

		public ProjectFileFinder Finder { get; set; }
	}
}