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
                        await Converter.Convert(opts.Input, opts.output, opts.Gender);

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
