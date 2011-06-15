﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using SlimJim.Model;

namespace SlimJim.Infrastructure
{
	public class CsProjReader
	{
		private static readonly XNamespace Ns = "http://schemas.microsoft.com/developer/msbuild/2003";

		public virtual CsProj Read(FileInfo csProjFile)
		{
			XElement xml = LoadXml(csProjFile);
			XElement properties = xml.Element(Ns + "PropertyGroup");
			XElement assemblyName = properties.Element(Ns + "AssemblyName");

			if (assemblyName == null)
			{
				return null;
			}

			return new CsProj
			{
				Path = csProjFile.FullName,
				AssemblyName = assemblyName.Value,
				Guid = properties.Element(Ns + "ProjectGuid").Value,
				ReferencedAssemblyNames = ReadReferencedAssemblyNames(xml),
				ReferencedProjectGuids = ReadReferencedProjectGuids(xml)
			};
		}

		private XElement LoadXml(FileInfo csProjFile)
		{
			XElement xml;
			using (StreamReader reader = csProjFile.OpenText())
			{
				xml = XElement.Load(reader);
			}
			return xml;
		}

		private List<string> ReadReferencedAssemblyNames(XElement xml)
		{
			List<string> rawAssemblyNames = (from r in xml.DescendantsAndSelf(Ns + "Reference")
														select r.Attribute("Include").Value).ToList();
			List<string> unQualifiedAssemblyNames = rawAssemblyNames.ConvertAll(UnQualify);
			return unQualifiedAssemblyNames;
		}

		private string UnQualify(string name)
		{
			if (!name.Contains(",")) return name;

			return name.Substring(0, name.IndexOf(","));
		}

		private List<string> ReadReferencedProjectGuids(XElement xml)
		{
			return (from pr in xml.DescendantsAndSelf(Ns + "ProjectReference")
					  select pr.Element(Ns + "Project").Value).ToList();
		}
	}
}