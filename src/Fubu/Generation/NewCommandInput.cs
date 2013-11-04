using System.Collections.Generic;
using System.ComponentModel;

namespace Fubu.Generation
{
    public class NewCommandInput : NewSolutionInput
    {

        public NewCommandInput()
        {
            RippleFlag = FeedChoice.PublicOnly;
        }


        // If this is blank, use this folder as the solution name too

        [Description("If chosen, fubu new will only create an empty solution.")]
        public bool EmptyFlag { get; set; }

        [Description("Add a testing library for the project using the default FubuTestingSupport w/ NUnit")]
        public bool TestsFlag { get; set; }

        [Description("Ignore the presence of existing files")]
        public bool IgnoreFlag { get; set; }


        [Description("Extra options for the new application")]
        public IEnumerable<string> OptionsFlag { get; set; }
    }

    public enum FeedChoice
    {
        FloatingEdge,
        Edge,
        PublicOnly
    }
}