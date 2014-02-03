using System;
using System.IO;
using System.Reflection;
using FubuCore;
using FubuCsProjFile.Templating.Graph;

namespace Fubu.Generation.Templates
{
    public class FileTemplate
    {
        public static FileTemplate Embedded(string name)
        {
            var text = Assembly.GetExecutingAssembly().GetManifestResourceStream(typeof (FileTemplate), name)
                .ReadAllText();

            return new FileTemplate
            {
                Extension = Path.GetExtension(name),
                RawText = text
            };
        }

        public static FileTemplate FromFile(string path)
        {
            return new FileTemplate
            {
                RawText = new FileSystem().ReadStringFromFile(path),
                Extension = Path.GetExtension(path)
            };
        }

        public string Contents(Substitutions substitutions)
        {
            return substitutions.ApplySubstitutions(RawText);
        }

        public string RawText { get; set; }
        public string Extension { get; set; }
    }
}