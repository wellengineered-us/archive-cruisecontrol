using System;
using System.IO;

using WellEngineered.CruiseControl.Core;
using WellEngineered.CruiseControl.Core.State;
using WellEngineered.CruiseControl.Core.Util;
using WellEngineered.CruiseControl.PrivateBuild.NetReflector.Attributes;
using WellEngineered.CruiseControl.Remote;

namespace WellEngineered.CruiseControl.Demos.Plugin.State
{
    [ReflectorType("plainTextState")]
    public class PlainTextStateManager
        : IStateManager
    {
        private readonly IFileSystem fileSystem;

        public PlainTextStateManager()
            : this(new SystemIoFileSystem())
        {
        }

        public PlainTextStateManager(IFileSystem fileSystem)
        {
            this.fileSystem = fileSystem;
        }

        public IIntegrationResult LoadState(string project)
        {
            var path = this.GeneratePath(project);
            using (var reader = new StreamReader(
                this.fileSystem.OpenInputStream(path)))
            {
                var status = (IntegrationStatus)
                    Enum.Parse(typeof(IntegrationStatus), reader.ReadLine());
                var lastSummary = new IntegrationSummary(
                    status,
                    reader.ReadLine(),
                    reader.ReadLine(),
                    DateTime.Parse(reader.ReadLine()));
                var result = new IntegrationResult(
                    project,
                    reader.ReadLine(),
                    reader.ReadLine(),
                    null,
                    lastSummary);
                return result;
            }
        }

        public void SaveState(IIntegrationResult result)
        {
            var path = this.GeneratePath(result.ProjectName);
            using (var writer = new StreamWriter(
                this.fileSystem.OpenOutputStream(path)))
            {
                writer.WriteLine(result.Status);
                writer.WriteLine(result.Label);
                writer.WriteLine(result.LastSuccessfulIntegrationLabel);
                writer.WriteLine(result.StartTime.ToString("o"));
                writer.WriteLine(result.WorkingDirectory);
                writer.WriteLine(result.ArtifactDirectory);
            }
        }

        public bool HasPreviousState(string project)
        {
            var path = this.GeneratePath(project);
            return this.fileSystem.FileExists(path);
        }

        private string GeneratePath(string project)
        {
            var path = Path.Combine(Environment.CurrentDirectory,
                project + ".txt");
            return path;
        }
    }
}
