using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CliArgs;

namespace CliArgsTest
{
    class Program
    {
        static void MainDef(string[] args)
        {
            CliArgDescr d = new CliArgDescr(CliArgDescr.Default2000);
            d.allowCombineShortKey = true;
            ArgsParser p = new ArgsParser();
            if (args.Length == 0)
            {
                d.logicalKeys = new CliArgKey[]
                {
                    new CliArgKey{ 
                        shortKeys = new string[] { "b" }
                    }
                };
                p.SetArgs(new string[] { "-abc" });
            }
            else
                p.SetArgs(args);

            p.SetDescr(d);
            var r = p.FindNext();
            while (r != null)
            {
                if ((r.isKey)&&(r.hasValue))
                    Console.WriteLine($"[{r.rawKeyName}]: {r.value}");
                else if (r.isKey)
                    Console.WriteLine($"[{r.rawKeyName}]");
                else
                    Console.WriteLine($"{r.value}");
                r = p.FindNext();
            }  
        }

        [CliFull("debug")]
        [CliShort("d")]
        static bool debug;

        [CliFull("name")]
        [CliShort("n")]
        [CliShort("name")]
        static string name = "abc";

        [CliValue(0)]
        static string vals1;
        [CliValue(1)]
        static string vals2;
        [CliValue]
        static string[] rest;

        static void Main(string[] args)
        {
            bool res;
            if (args.Length > 0)
                res = CliArgRef.ParseArgs(args);
            else
                //res = CliArgRef.ParseArgs( new string[] { "--name=3333"});
                res = CliArgRef.ParseArgs(new string[] { "aa","bb","cc","dd","ee" });
            Console.WriteLine($"res: {res}");
            Console.WriteLine($"debug: {debug}");
            Console.WriteLine($"name: {name}");
            Console.WriteLine($"vals: {vals1} {vals2}");

            Console.WriteLine($"{rest.GetType().Name}");
            if (rest != null)
            {
                foreach (var v in rest)
                {
                    Console.WriteLine($"* {v}");
                }
            }
        }
    }

    // class OtherProgram
    // {
    //     static void Main(string[] args)
    //     {
    //         bool res = CliArgRef.ParseArgs(args);
    //         Console.WriteLine($"res2: {res}");
    //     }
    // }
}
