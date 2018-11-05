using Sharpmake;

namespace CSProjTemplate
{
    static class Common
    {
        public static DevEnv @devEnv = DevEnv.vs2017;
      //  public static string @libRoot = "[sharpmake.libroot]";
        public static string libRoot = "Lib";
        public static string @projName = "[sharpmake.projname]";
        public static string @srcRoot = "[sharpmake.srcroot]";
        public static string @defines = "[sharpmake.defines]";
        public static string genfolder = "generated";
        public static ITarget[] GetTargets()
        {
            return new[]
        {
            new Target(

                // Building for amd64 and x86. Note that Any CPU is also an
                // option here
                Platform.anycpu,
                devEnv,
                Optimization.Debug | Optimization.Release,
                OutputType.Dll,
            //    Blob.Blob,
                // When building for the .NET framework, you can specify a .NET
                // Framework to target. As always, you can use the bit OR
                // operator to specify many at once.
                framework: DotNetFramework.[sharpmake.framework]
            )
        };
        }

        [Main]
        public static void SharpmakeMain(Arguments args)
        {
            args.Generate<DotNetSolution>();
        }
    }

    [Sharpmake.Generate]
    public class CSharpDll : CSharpProject
    {
        public CSharpDll()
        {
         //   SourceFilesExcludeRegex.Add("*.cpp");
            Name = Common.projName;
            RootPath = @"[project.SharpmakeCsPath]";
            AddTargets(Common.GetTargets());
            //SourceRootPath = @"[project.RootPath]\[project.Name]";
            SourceRootPath = System.IO.Path.Combine("[project.RootPath]", Common.srcRoot) ;
        }
        [Configure()]
        public virtual void ConfigureAll(Configuration conf, Target target)
        {
            conf.Output = Configuration.OutputType.DotNetClassLibrary;
            conf.ReferencesByName.Add(
                "System",
                "System.Core",
                "mscorlib",
                "System.Runtime.Serialization",
                "System.Xml.Linq",
                "System.XML",
                "System.Data.DataSetExtensions",
                "System.Data"
                );
            string libFolder = System.IO.Path.Combine(conf.Project.SharpmakeCsPath, Common.libRoot);
            if (System.IO.Directory.Exists(libFolder))
            {
                string[] dlls = System.IO.Directory.GetFiles(libFolder, "*.dll", System.IO.SearchOption.AllDirectories);
                conf.ReferencesByPath.AddRange(dlls);
            }
            conf.Defines.AddRange(Common.defines.Split(';'));
            conf.ProjectFileName = "[project.Name].[target.DevEnv].[target.Framework]";
            conf.ProjectPath = @"[project.SharpmakeCsPath]\" + Common.genfolder;
        }
    }

    [Sharpmake.Generate]
    public class DotNetSolution : CSharpSolution
    {
        public DotNetSolution()
        {
            Name = Common.projName;
            AddTargets(Common.GetTargets());
        }

        [Configure()]
        public void ConfigureAll(Configuration conf, Target target)
        {
            conf.SolutionFileName = @"[solution.Name]_[target.Framework]";
            conf.SolutionPath = @"[solution.SharpmakeCsPath]\" + Common.genfolder;

            conf.AddProject<CSharpDll>(target);
        }

    }
}
