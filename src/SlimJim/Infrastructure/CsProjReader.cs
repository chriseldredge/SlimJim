using System.IO;
using SlimJim.Model;

namespace SlimJim.Infrastructure
{
    public abstract class CsProjReader
    {
        public abstract CsProj Read(FileInfo csProjFile);

        protected string UnQualify(string name)
        {
            if (!name.Contains(",")) return name;

            return name.Substring(0, name.IndexOf(","));
        }
    }
}