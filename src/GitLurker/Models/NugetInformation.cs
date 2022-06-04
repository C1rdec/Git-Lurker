using System;
using System.IO;
using System.Linq;
using NuGet.Versioning;

namespace GitLurker.Models
{
    public class NugetInformation
    {
        private NugetInformation(string fileName)
        {
            var segments = fileName.Split(".");

            var versionValue = string.Join('.', segments.TakeLast(3));

            Version = new NuGetVersion(versionValue);
            PackageName = string.Join(".", segments.Take(segments.Length - 3));
        }

        public string Name => $"{PackageName} {Version.ToNormalizedString()}";

        public NuGetVersion Version { get; init; }

        public string PackageName { get; init; }

        public string FilePath { get; set; }

        public static NugetInformation Parse(string value)
        {
            if (Path.GetExtension(value) != ".nupkg")
            {
                throw new InvalidOperationException("Not a nuget package");
            }

            var fileName = Path.GetFileNameWithoutExtension(value);
            return new NugetInformation(fileName)
            {
                FilePath = value,
            };
        }

        public static NugetInformation ParseFromFeed(string value)
        {
            return new NugetInformation(value.Replace(' ', '.'));
        }

        public bool Ahead(NugetInformation informationToCompare)
        {
            return CompareTo(informationToCompare) > 0;
        }

        public int CompareTo(NugetInformation informationToCompare)
        {
            return Version.CompareTo(informationToCompare.Version);
        }
    }
}
