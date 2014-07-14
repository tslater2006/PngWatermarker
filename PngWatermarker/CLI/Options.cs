using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
namespace CLI
{
    class Options
    {
        [Option('i', "input", Required = true, HelpText = "Input file.")]
        public string InputFile { get; set; }

        [Option('o', "output", Required = true, HelpText = "Output file.")]
        public string OutputFile { get; set; }

        [Option('p',"password", Required= true,HelpText = "Storage Password.")]
        public string Password { get; set; }

        [Option('e',"encrypt",Required=false, HelpText= "Enable Encryption, if this is selected you MUST provide a value for -k/--key.")]
        public bool Encrypt { get; set; }

        [Option('a',"algo",Required=true,HelpText="Symmetric Algorithm to use, valid options are AES")]
        public string Algorithm {get;set;}
        [Option('k',"key", Required= false,HelpText = "Encryption Key.")]
        public string EncKey { get; set; }

        [Option('r',"reedsolomon",Required=false,HelpText="Enables ReedSolomon protection.")]
        public bool ReedSolomon { get; set; }

        [OptionArray('t',"text",HelpText="Text watermark(s), each string should be wrapped in quotes and seperated by a space.")]
        public string[]TextMarks {get;set;}

        [OptionArray('f',"file", HelpText = "File watermark(s), each file should be wrapped in quotes and seperated by a space.")]
        public string[] FileMarks { get; set; }

        [OptionArray('b', "binary", HelpText = "Binary watermark(s), each one should be formatted like {34,AE,2F,4B} and should be seperated by a space.")]
        public string[] BinaryMarks { get; set; }
    }
}
