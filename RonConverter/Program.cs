using CommandLine;
using System;
using System.Threading.Tasks;

namespace RonConverter
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            return await Parser.Default.ParseArguments<CommandLineOptions>(args)
                .MapResult(async (CommandLineOptions opts) =>
                {
                    try
                    {
                        if (opts.Operation == Operation.Morph)
                        {
                            await Converter.GenerateMorph(opts.Input, opts.output, opts.Gender);
                        }
                        else if (opts.Operation == Operation.Overrides)
                        {
                             await Converter.Convert(opts.Input, opts.output, opts.Gender);
                        }
                        else if (opts.Operation == Operation.Full)
                        {
                            await Converter.FullCustomHead(opts.Input, opts.output, opts.Gender, opts.CustomMorphTargetName);
                        }

                        return 0;
                    }
                    catch
                    {
                        Console.WriteLine("Error!");
                        return -3; // Unhandled error
                    }
                },
                errs => Task.FromResult(-1)); // Invalid arguments
        }
    }
}
