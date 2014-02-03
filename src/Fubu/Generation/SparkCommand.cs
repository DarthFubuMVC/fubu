using System;
using System.ComponentModel;
using Fubu.Generation.Templates;
using FubuCore;
using FubuCore.CommandLine;
using FubuCsProjFile;

namespace Fubu.Generation
{
    public class ViewInput
    {
        [Description("Name of the view and matching model without file extension")]
        public string Name { get; set; }

        [Description("If specified, will make this actionless view applied to the given url pattern")]
        public string UrlFlag { get; set; }

        [Description("open the view model and view after generation")]
        public bool OpenFlag { get; set; }


    }

    [CommandDescription("Creates and attaches a Spark view model/view pair to the project in the current folder")]
    public class SparkCommand : FubuCommand<ViewInput>
    {
        public override bool Execute(ViewInput input)
        {
            // TODO -- look for the template in /src/templates
            var template = FileTemplate.Embedded("view.spark");


            MvcBuilder.BuildView(input, template);

            return true;
        }
    }

    [CommandDescription("Creates and attaches a Spark view model/view pair to the project in the current folder")]
    public class RazorCommand : FubuCommand<ViewInput>
    {
        public override bool Execute(ViewInput input)
        {
            // TODO -- look for the template in /src/templates
            var template = FileTemplate.Embedded("view.cshtml");


            MvcBuilder.BuildView(input, template);

            return true;
        }
    }

    public static class MvcBuilder
    {
        public static void BuildView(ViewInput input, FileTemplate template)
        {
            Location location = ProjectFinder.DetermineLocation(Environment.CurrentDirectory);

            ViewModelBuilder.BuildCodeFile(input, location);

            var modelName = location.Namespace + "." + input.Name;


            var path = ViewBuilder.Write(template, location, modelName);

            location.Project.Add(new Content(path.PathRelativeTo(location.Project.FileName)));

            if (input.OpenFlag)
            {
                EditorLauncher.LaunchFile(path);
            }
        }
    }

}