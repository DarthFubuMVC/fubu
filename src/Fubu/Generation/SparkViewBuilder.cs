using FubuCore;
using System.Linq;

namespace Fubu.Generation
{
    // TODO -- make this fancier later.
    // TODO -- detect Spark or Razor
    // TODO -- let you define view template by codebase
    public static class SparkViewBuilder
    {
        public static readonly string Template = "<viewdata model=\"%MODEL%\" />";

        public static string Write(Location location, string inputModel)
        {
            var path = location.CurrentFolder.AppendPath(inputModel.Split('.').Last() + ".spark");
            var contents = Template.Replace("%MODEL%", inputModel);

            new FileSystem().WriteStringToFile(path, contents);

            return path;
        }
    }
}