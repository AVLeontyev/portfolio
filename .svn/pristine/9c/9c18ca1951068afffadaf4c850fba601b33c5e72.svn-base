using System;
using System.Text.RegularExpressions;

namespace TestFunc
{
    class Program
    {
        static void Main(string[] args)
        {
            object test = null;

            try {
                //test = new DomainName ();
                //test = new ImportCSV ();

                //test = new HOracleConnection();
                ////test = new HOleDbOracleConnection ();
                //(test as HDbOracle).Open ();
                //(test as HDbOracle).Query();
                //(test as HDbOracle).Close ();
                //(test as HDbOracle).OutResult ();

                //test = new DbSources();

                //test = new GenericSorted();

                test = new SetQuery ();
            } catch (Exception e) {
                Console.Write(e.Message + Environment.NewLine);
            }

            Console.Write(@"Для завершения работы нажмите любую клавишу..."); Console.ReadKey();
        }

        private class HRegEx
        {
            public HRegEx ()
            {
                string reg = @"[\\/]";
                string[] paths = { "sd\fdg", "asd/fger", "adf\\sefwes", "as//fdsfsw", "a\\sfs/aefwef", "jhgd\\sw\\ueg/rw" };
                Regex rx = new Regex(reg);
                foreach (string path in paths)
                    Console.WriteLine(string.Format("в строке: '{0}' {1} вхождений из '{2}'", path, rx.Matches(path).Count, reg));
            }
        }
    }
}
