using CommandLine;

namespace RonConverter
{
    public class CommandLineOptions
    {
        [Value(index: 0, Required = true, HelpText = "input ron file is required")]
        public string Input { get; set; }

        [Value(index: 1, Required = false, HelpText = "output filename")]
        public string output { get; set; }

        [Option(Required = true)]
        public Gender Gender { get; set; }
    }

    public enum Gender
    {
        Either,
        Female,
        Male
    }
}

