using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PngWatermarker;
using PngWatermarker.Watermarks;

namespace CLI
{
    class Program
    {
        static List<TextWatermark> textMarks = new List<TextWatermark>();
        static List<BinaryWatermark> binMarks = new List<BinaryWatermark>();
        static List<FileWatermark> fileMarks = new List<FileWatermark>();

        private static int totalMarks
        {
            get
            {
                return textMarks.Count + binMarks.Count + fileMarks.Count;
            }
        }

        static void Main(string[] args)
        {
            var options = new Options();
            args = new string[] { "-i=a", "-k=nut", "-o=b", "-k \"coco\":\"nut\"", "-p=3" };

            Watermark finalMark = null;

            if (CommandLine.Parser.Default.ParseArguments(args, options))
            {
                // process any text watermarks
                foreach (var x in options.TextMarks)
                {
                    TextWatermark t = new TextWatermark(x.Substring(1, x.Length - 2)); // new TextWatermark w/o beginning and ending quotes
                    textMarks.Add(t);
                }

                // process any file watermarks
                foreach(var x in options.FileMarks)
                {
                    FileWatermark f = new FileWatermark(x);
                    fileMarks.Add(f);
                }

                // process any binary watermarks
                foreach(var x in options.BinaryMarks)
                {
                    string[] parts = x.Replace("{", "").Replace("}", "").Split(',');
                    byte[] data = new byte[parts.Length];

                    for(var y = 0; y < parts.Length; y++)
                    {
                        data[y] = Convert.ToByte(parts[y], 16);
                    }

                    BinaryWatermark b = new BinaryWatermark(data);
                    binMarks.Add(b);
                }



                // Values are available here
                int i = 1;
                i++;
            }
            else
            {
                var b = CommandLine.Text.HelpText.AutoBuild(options);

                Console.WriteLine(b.ToString());
                Console.ReadKey();
            }
        }
    }
}
