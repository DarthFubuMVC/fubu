using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using FubuCore.CommandLine;
using FubuCsProjFile.Templating;
using FubuCsProjFile.Templating.Graph;
using FubuCore;
using System.Linq;
using FubuCsProjFile.Templating.Planning;
using FubuCsProjFile.Templating.Runtime;

namespace Fubu.Generation
{
    [CommandDescription("Lays out and creates a new code tree with the idiomatic fubu project layout", Name = "new")]
    public class NewCommand : FubuCommand<NewCommandInput>
    {
        private static readonly IDictionary<FeedChoice, string> _rippleTemplates = new Dictionary<FeedChoice, string>{{FeedChoice.Edge, "edge-ripple"}, {FeedChoice.FloatingEdge, "floating-ripple"}, {FeedChoice.PublicOnly, "public-ripple"}};

        public NewCommand()
        {
            Usage("default").Arguments(x => x.SolutionName);
        }

        public override bool Execute(NewCommandInput input)
        {
            var request = BuildTemplateRequest(input);
            var plan = Templating.BuildPlan(request);

            if (input.PreviewFlag)
            {
                Console.WriteLine("To solution directory " + input.SolutionDirectory());
                Console.WriteLine();

                plan.WritePreview();
            }
            else
            {
                prepareTargetDirectory(input, request);

                Templating.ExecutePlan(plan);
                // TODO -- open the solution if you can
            }


            return true;
        }



        private static void prepareTargetDirectory(NewCommandInput input, TemplateRequest request)
        {
            if (input.CleanFlag)
            {
                new FileSystem().ForceClean(request.RootDirectory);
            }
            else if (!input.IgnoreFlag)
            {
                AssertEmpty(request.RootDirectory);
            }
        }

 
        public static void AssertEmpty(string directory)
        {
            if (Directory.Exists(directory))
            {
                var files = new FileSystem().FindFiles(directory, FileSet.Everything());
                var notBaseline = files.Where(x => !IsBaselineFile(x)).ToArray();
                if (notBaseline.Any())
                {
                    throw new InvalidOperationException("Directory {0} is not empty!  Use the --clean flag to override this validation check to overwrite the contents of the solution".ToFormat(directory));
                }
            }
        }

        public static bool IsBaselineFile(string file)
        {
            if (file.StartsWith(".git")) return true;

            if (Path.GetFileNameWithoutExtension(file).StartsWith("readme", StringComparison.OrdinalIgnoreCase)) return true;

            if (file.Contains(".git")) return true;

            if (Path.GetFileName(file).EqualsIgnoreCase("license.txt")) return true;

            return false;
        }

        public static TemplateRequest BuildTemplateRequest(NewCommandInput input)
        {
            var request = new TemplateRequest
            {
                RootDirectory = input.SolutionDirectory(),
                SolutionName = input.SolutionName
            };

            request.AddTemplate("baseline");

            request.AddTemplate(_rippleTemplates[input.RippleFlag]);

            if (input.AppFlag)
            {
                var projectRequest = addApplicationProject(input, request);
                if (input.TestsFlag)
                {
                    request.AddMatchingTestingProject(projectRequest);
                }
            }

            return request;
        }

        private static ProjectRequest addApplicationProject(NewCommandInput input, TemplateRequest request)
        {
            var project = new ProjectRequest(input.SolutionName, "baseline");
            project.Alterations.Add("structuremap");
            project.Alterations.Add("fubumvc-empty");

            if (input.OptionsFlag != null)
            {
                input.OptionsFlag.Each(x => project.Alterations.Add(x));
            }

            // TODO -- duplication!
            if (input.ShortNameFlag.IsNotEmpty())
            {
                project.Substitutions.Set(ProjectPlan.SHORT_NAME, input.ShortNameFlag);
            }

            // TODO -- Will need to check for razor too!
            if (!project.Alterations.Contains("spark"))
            {
                project.Alterations.Add("no-views");
            }

            request.AddProjectRequest(project);

            return project;
        }
    }
}