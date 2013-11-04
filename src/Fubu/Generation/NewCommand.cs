using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using FubuCore.CommandLine;
using FubuCsProjFile.Templating;
using FubuCore;
using System.Linq;

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
            var plan = BuildTemplatePlan(request);

            if (input.PreviewFlag)
            {
                Console.WriteLine("To solution directory " + input.SolutionDirectory());
                Console.WriteLine();

                plan.WritePreview();
            }
            else
            {
                prepareTargetDirectory(input, request);

                ExecutePlan(plan);
            }


            return true;
        }

        public static void ExecutePlan(TemplatePlan plan, Action beforeRake = null)
        {
            plan.Execute();

            if (beforeRake != null) beforeRake();
            new RakeStep().Alter(plan);

            plan.WriteInstructions();
        }

        public static TemplatePlan BuildTemplatePlan(TemplateRequest request)
        {
            var library = LoadTemplates();
            var planner = new TemplatePlanBuilder(library);

            var plan = planner.BuildPlan(request);
            if (plan.Steps.OfType<GemReference>().Any())
            {
                plan.Add(new BundlerStep());
            }

            return plan;
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

            request.AddTemplate(_rippleTemplates[input.RippleFlag]);

            if (!input.EmptyFlag)
            {
                var projectRequest = addApplicationProject(input, request);
                if (input.TestsFlag)
                {
                    var testing = new TestProjectRequest(projectRequest.Name + ".Testing", "baseline",
                                                         projectRequest.Name);

                    testing.AddAlteration("unit-testing");

                    request.AddTestingRequest(testing);
                }

            }

            request.AddTemplate("baseline");

            return request;
        }

        private static ProjectRequest addApplicationProject(NewCommandInput input, TemplateRequest request)
        {
            var project = new ProjectRequest(input.SolutionName, "baseline");
            project.AddAlteration("structuremap");
            project.AddAlteration("fubumvc-empty");

            if (input.OptionsFlag != null)
            {
                input.OptionsFlag.Each(x => project.AddAlteration(x));
            }

            // TODO -- duplication!
            if (input.ShortNameFlag.IsNotEmpty())
            {
                project.Substitutions.Set(ProjectPlan.SHORT_NAME, input.ShortNameFlag);
            }

            // TODO -- Will need to check for razor too!
            if (!project.Alterations.Contains("spark"))
            {
                project.AddAlteration("no-views");
            }

            request.AddProjectRequest(project);

            return project;
        }

        public static TemplateLibrary LoadTemplates()
        {
            var path = AppDomain.CurrentDomain.BaseDirectory.AppendPath("templates");
            if (Directory.Exists(path))
            {
                return new TemplateLibrary(path);
            }

            // Testing mode.
            path = Assembly.GetExecutingAssembly().Location.ToFullPath()
                          .ParentDirectory().ParentDirectory().ParentDirectory()
                          .ParentDirectory().ParentDirectory()
                          .AppendPath("templates");

            if (!Directory.Exists(path))
            {
                path = AppDomain.CurrentDomain.BaseDirectory.ParentDirectory()
                                .ParentDirectory()
                                .ParentDirectory()
                                .ParentDirectory()
                                .AppendPath("templates");
            }

            return new TemplateLibrary(path);
        }


    }
}