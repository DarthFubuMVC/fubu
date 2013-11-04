using FubuCore.CommandLine;

namespace Fubu.Generation
{
    public class QuickstartInput : NewSolutionInput
    {
        public NewCommandInput ToInput()
        {
            return new NewCommandInput
            {
                SolutionName = SolutionName,
                PreviewFlag = PreviewFlag,
                RippleFlag = RippleFlag,
                ShortNameFlag = ShortNameFlag,

                OptionsFlag = new string[]{"spark"}, // needs to get smarter here
                EmptyFlag = false,
                TestsFlag = true
            };
        }
    }

    [CommandDescription("Stand up a full FubuMVC application structure with the full idiomatic stack")]
    public class QuickstartCommand : FubuCommand<QuickstartInput>
    {
        public override bool Execute(QuickstartInput input)
        {
            return new NewCommand().Execute(input.ToInput());
        }
    }
}