using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using Mono.Options;
using PngWatermarker;
using PngWatermarker.Watermarks;

namespace pngw
{
    class Program
    {
        static List<Watermark> marks = new List<Watermark>();
        static string InputFile;

        static string OutputFile;

        static string Password;
        static bool Encrypt;
        static string Algorithm;
        static string EncKey;
        static bool ReedSolomon;
        static bool help;
        static string Mode;
        static bool OnlyConsole;

        static void Main(string[] args)
        {
            //bool goodArgs = parseArgs(new string[]{"-m=EMBED", "-i=TestImage.png", "-o=Output.png", "-p=password", "-t=Hello", "-t=World", "-f=TestFile.txt", "-f=TestFile.txt", "-b=03,2A,4C,BD", "-e", "-a=AES", "-k=supersecret"});
            //bool goodArgs = parseArgs(new string[] { "-m=EXTRACT","-c","-i=Output.png", "-p=password", "-t=Hello", "-t=World", "-f=TestFile.txt", "-f=TestFile.txt", "-b=03,2A,4C,BD", "-e", "-a=AES", "-k=supersecret" });
            bool goodArgs = parseArgs(new string[] { "-m=EXTRACT", "-i=Output.png", "-o=.","-p=password", "-t=Hello", "-t=World", "-f=TestFile.txt", "-f=TestFile.txt", "-b=03,2A,4C,BD", "-e", "-a=AES", "-k=supersecret" });
            if (goodArgs)
            {
                Watermarker.ReedSolomonProtection = ReedSolomon;

                if (Encrypt)
                {
                    SymmetricAlgorithm algo = null;
                    switch (Algorithm)
                    {
                        case "AES":
                            algo = new RijndaelManaged();
                            break;
                    }
                    if (algo != null)
                    {
                        algo.Padding = PaddingMode.Zeros;
                    }
                    else
                    {
                        Console.WriteLine("Invalid Algorithm specified, valid options are [\"AES\"]");
                        return;
                    }

                    EncryptedWatermark.Algorithm = algo;
                }

                if (Mode.Equals("EMBED"))
                {
                    Watermark finalMark;

                    if (marks.Count > 1)
                    {
                        CompositeWatermark comp = new CompositeWatermark();

                        foreach (Watermark m in marks)
                        {
                            comp.AddWatermark(m);
                        }
                        finalMark = comp;
                    }
                    else
                    {
                        finalMark = marks[0];
                    }

                    if (Encrypt)
                    {
                        finalMark = new EncryptedWatermark(finalMark, EncKey);
                    }

                    PNGFile fileIn = new PNGFile(InputFile);

                    Watermarker.EmbedWatermark(fileIn, finalMark, Password, OutputFile);
                }
                else
                {
                    PNGFile fileIn = new PNGFile(InputFile);

                    Watermark mark = Watermarker.ExtractWatermark(fileIn, Password);
                    List<Watermark> marks = new List<Watermark>();

                    // check to see if encrypted
                    if (mark.GetType().Equals(typeof(EncryptedWatermark)) )
                    {
                        Console.WriteLine("Detected an encrypted watermark.");
                        if (Encrypt)
                        {
                            Console.WriteLine("Attempting to decrypt the watermark...");
                            mark = ((EncryptedWatermark)mark).Decrypt(EncKey);
                        } else
                        {
                            Console.WriteLine("Encrypt flag was not set, unable to continue. Please try again with the -e flag and appropriate -a and -k values");
                            return;
                        }
                    }

                    // check to see if it is a composite
                    if (mark.GetType().Equals(typeof(CompositeWatermark)))
                    {
                        Console.WriteLine("Composite Watermark detected.");

                        CompositeWatermark comp = (CompositeWatermark)mark;

                        Console.WriteLine("Total of " + comp.Watermarks.Count + " Watermarks found.");

                        foreach(Watermark m in comp.Watermarks)
                        {
                            marks.Add(m);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Found 1 watermark.");
                        marks.Add(mark);
                    }

                    // check to see if only console output
                    if (OnlyConsole)
                    {
                        foreach(Watermark m in marks)
                        {
                            Console.WriteLine(m);
                        }
                    }
                    else
                    {
                        for(var x = 0; x < marks.Count; x++)
                        {
                            marks[x].Save(OutputFile + "\\" + "Watermark" + (x + 1));
                        }
                    }
                }

            }
            Console.ReadKey();
        }

        private static bool parseArgs( string[] args)
        {
            var p = new OptionSet() {
                { "m|mode=", "Mode of operation, either EMBED or EXTRACT", v => Mode = v},
                { "i|in=", "The input file to process.", v => InputFile = v },
                { "o|out=", "In EMBED mode this is the output file that will be created, when in EXTRACT this specifies the folder to dump watermarks to.", v => OutputFile = v },
                { "p|pass=", "The storage password.", v => Password = v },
                { "e|enc", "Perform encryption on the watermark.", v => Encrypt = v != null },
                { "a|algo=", "The encryption algorithm to use (only in -e/--enc was specified)",v => Algorithm = v},
                { "k|key=","The encryption password to use with the algorithm specified in -a/--algo.",v => EncKey = v},
                { "r|rs","Sets the use of ReedSolomon protection.",v => ReedSolomon = v != null},
                { "t|text=", "Text string to use as a watermark", v => marks.Add(new TextWatermark(v))},
                { "f|file=", "File to use as a watermark", v => marks.Add(new FileWatermark(v))},
                { "b|binary=", "Binary data to use as watermark, specified as 03,2A,4C,BD", v =>
                    marks.Add(new BinaryWatermark(v.Split(',').Select( s => Convert.ToByte(s,16)).ToArray()))
                },
                { "h|help","Shows this message.",v => help = (v != null)},
                {"c|console", "Console output only, this will only show Textual watermarks in EXTRACT mode, invalid with EMBED mode.",v => OnlyConsole = (v != null)}
            };
            List<string> extra = p.Parse(args);
            bool badArgs = false;

            // help
            if (help)
            {
                badArgs = true;
            }

            // no mode
            if (Mode == null || ((Mode.Equals("EMBED") || Mode.Equals("EXTRACT")) == false))
            {
                badArgs=true;
            }

            // no input file, or mode is EMBED and no output file, or no password
            if (InputFile == null || (Mode.Equals("EMBED") && OutputFile == null) || Password == null)
            {    
                badArgs = true;
            }

            // Encrypt is a yes but either missing algorithm or key
            if (Encrypt && (Algorithm == null || EncKey == null))
            {
                badArgs = true;
            }

            // Mode is EMBED but no marks were specified
            if (Mode.Equals("EMBED") && marks.Count == 0)
            {
                badArgs = true;
            }

            // Mode is extract, they dont want console only output, but didn't specify a directory to store marks.
            if (Mode.Equals("EXTRACT") && OutputFile == null && (OnlyConsole == false))
            {
                badArgs = true;
            }

            if (badArgs) {
                ShowHelp(p);
                return false;
            }

            return true;
        }

        static void ShowHelp(OptionSet p)
        {
            Console.WriteLine("Usage: pngw -m [EMBED|EXTRACT] -i=File.png -o=File2.png -p=Password [OPTIONS] WATERMARKS");
            Console.WriteLine("Embeds or extracts watermark(s) using the file specified with -i");
            Console.WriteLine("WATERMARKS above is a place for 1 to many watermarks specified with either -t, -f, or -b");
            Console.WriteLine();
            Console.WriteLine("Options:");
            p.WriteOptionDescriptions(Console.Out);
        }
    }
}
