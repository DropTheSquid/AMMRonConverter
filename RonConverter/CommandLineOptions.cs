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

        [Option(Required = false, Default = Operation.Morph)]
        public Operation Operation { get; set; }

        [Option(Required = false)]
        public string DiffFrom { get; set; }

        [Option(Required = false)]
        public string CustomMorphTargetName { get; set; }
    }

    public enum Gender
    {
        Either,
        Female,
        Male
    }

    public enum Operation
    {
        Overrides,
        Morph,
        Full,
        MorphDiff
    }
}

