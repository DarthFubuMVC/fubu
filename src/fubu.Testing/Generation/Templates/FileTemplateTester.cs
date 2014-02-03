using Fubu.Generation.Templates;
using FubuCore;
using FubuCsProjFile.Templating.Graph;
using FubuTestingSupport;
using NUnit.Framework;

namespace fubu.Testing.Generation.Templates
{
    [TestFixture]
    public class FileTemplateTester
    {
        [Test]
        public void create_from_embedded_resource()
        {
            var template = FileTemplate.Embedded("view.spark");
            template.RawText.ShouldContain("<viewdata model=\"%MODEL%\" />");
            template.Extension.ShouldEqual(".spark");
        }

        [Test]
        public void load_from_file()
        {
            new FileSystem().WriteStringToFile("foo.spark", "<h1>Rock on!</h1>");

            var template = FileTemplate.FromFile("foo.spark");

            template.RawText.ShouldEqual("<h1>Rock on!</h1>");
            template.Extension.ShouldEqual(".spark");
        }

        [Test]
        public void get_content()
        {
            var substitutions = new Substitutions();
            substitutions.Set("%MODEL%", "Foo.Bar");

            var template = FileTemplate.Embedded("view.spark");
            template.Contents(substitutions).ShouldContain("<viewdata model=\"Foo.Bar\" />");
        }
    }
}