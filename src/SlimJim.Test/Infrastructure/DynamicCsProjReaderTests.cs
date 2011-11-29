using System.IO;
using NUnit.Framework;
using SlimJim.Infrastructure;
using SlimJim.Model;

namespace SlimJim.Test.Infrastructure
{
	[TestFixture]
	public class DynamicCsProjReaderTests
	{
		private FileInfo file;

		[Test]
		public void InvokesTargetBeforeGettingReferences()
		{
			CsProj project = GetProject("Dynamic");

			Assert.That(project.Guid, Is.EqualTo("{4A37C916-5AA3-4C12-B7A8-E5F878A5CDBA}"));
			Assert.That(project.AssemblyName, Is.EqualTo("MyProject"));
			Assert.That(project.Path, Is.EqualTo(file.FullName));
			Assert.That(project.ReferencedAssemblyNames, Is.EqualTo(new[]
			                                                       	{
                                                                                            "Dynamic.Bang",
																							"System.Core"
																							
			                                                       	}));
			Assert.That(project.ReferencedProjectGuids, Is.EqualTo(new[]
			                                                      	{
																							"{99036BB6-4F97-4FCC-AF6C-0345A5089099}",
																							"{69036BB3-4F97-4F9C-AF2C-0349A5049060}"
			                                                      	}));
		}

        [Test]
        public void IgnoresUnloadableProjects()
        {
            Assert.That(GetProject("Malformed"), Is.Null);
        }

		private CsProj GetProject(string fileName)
		{
			file = SampleFiles.SampleFileHelper.GetCsProjFile(fileName);
			var reader = new DynamicCsProjReader();
			return reader.Read(file);
		}
	}
}