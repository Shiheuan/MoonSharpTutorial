using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MoonSharp.Interpreter;

namespace TestMoonSharp
{
    class Class1
    {
        public double MoonSharpFactorial()
        {
            string scriptCode = @"
                -- defines a factorial function
                function fact (n)
                    if (n == 0) then
                        return 1
                    else
                        return n*fact(n-1)
                    end
                end

                --return fact(mynumber)";
            Script script = new Script();
            script.DoString(scriptCode);
            DynValue luaFactFunction = script.Globals.Get("fact");
            DynValue res = script.Call(luaFactFunction, 5);
            Console.WriteLine(res.Number);
            Console.ReadKey();
            return res.Number;
        }

        public void DynValueTest()
        {
            DynValue v1 = DynValue.NewNumber(1);
            DynValue v2 = DynValue.NewString("ciao");
            DynValue v3 = DynValue.FromObject(new Script(), "hello");

            Console.WriteLine("{0} - {1} - {2}", v1.Type, v2.Type, v3.Type);
            Console.WriteLine("{0} - {1} - {2}", v1.Number, v2.String, v3.Number);
            Console.ReadKey();
        }

        public void TupleTest()
        {
            DynValue ret = Script.RunString("return true, 'ciao', 2*3");

            Console.WriteLine("{0}", ret.Type);

            for (int i = 0; i < ret.Tuple.Length; i++)
            {
                Console.WriteLine("{0} = {1}", ret.Tuple[i].Type, ret.Tuple[i]);
            }
            Console.ReadKey();

        }

        private static int Mul(int a, int b)
        {
            return a * b;
        }

        public double CallbackTest()
        {
            string scriptCode = @"    
                -- defines a factorial function
                function fact (n)
                    if (n == 0) then
                        return 1
                    else
                        return Mul(n, fact(n - 1))
                    end
                end";

            Script script = new Script();
            script.Globals["Mul"] = (Func<int, int, int>) Mul;
            script.DoString(scriptCode);

            DynValue res = script.Call(script.Globals["fact"], 4);

            Console.WriteLine(res.Number);
            Console.ReadKey();
            return res.Number;
        }

        private static IEnumerable<int> GetNumbers()
        {
            for (int i = 1; i <= 10; i++)
                yield return i;
        }

        public static double EnumerableTest()
        {
            string scriptCode = @"    
                total = 0;
                
                for i in getNumbers() do
                    total = total + i;
                end

                return total;
                ";

            Script script = new Script();

            script.Globals["getNumbers"] = (Func<IEnumerable<int>>)GetNumbers;

            DynValue res = script.DoString(scriptCode);
            Console.WriteLine(res);
            Console.ReadKey();
            return res.Number;
        }

        private static List<int> GetNumberList()
        {
            List<int> lst = new List<int>();
            for (int i = 1; i <= 10; i++)
            {
                lst.Add(i);
            }

            return lst;
        }

        public static double TableTest1()
        {
            string scriptCode = @"
                total = 0
                tbl = getNumbers()
                for _, i in ipairs(tbl) do
                    total = total + i
                end

                return total";
            Script script = new Script();
            script.Globals["getNumbers"] = (Func<List<int>>)GetNumberList;

            DynValue res = script.DoString(scriptCode);
            Console.WriteLine(res);
            Console.ReadKey();
            return res.Number;
        }

        private static Table GetNumberTable(Script script)
        {
            Table tbl = new Table(script);

            for (int i = 1; i <= 10; i++)
            {
                tbl[i] = i;
            }

            return tbl;
        }

        public static double TableTest2()
        {
            string scriptCode = @"    
                total = 0;

                tbl = getNumbers()

                for _, i in ipairs(tbl) do
                    total = total + i;
                end

                return total;
                ";
            Script script = new Script();
            script.Globals["getNumbers"] = (Func<Script, Table>) (GetNumberTable);
            DynValue res = script.DoString(scriptCode);
            Console.WriteLine(res);
            Console.ReadKey();
            return res.Number;
        }

        public static double TableTestReverse()
        {
            string scriptCode = @"
                return dosum {1, 2, 3, 4, 5, 6, 7, 8, 9, 10}
                ";
            Script script = new Script();
            script.Globals["dosum"] = (Func<List<int>, int>)(l => l.Sum());
            DynValue res = script.DoString(scriptCode);

            Console.WriteLine(res);
            Console.ReadKey();
            return res.Number;
        }

        // why return "0"? because the type is wrong, not int but double.
        public static double TableTestReverseSafer()
        {
            string scriptCode = @"
                doum = {1, 2, 3, 4, 5, 6, 7, 8, 9, 10}
                print(type(doum))
                return dosum {1, 2, 3, 4, 5, 6, 7, 8, 9, 10}
                ";
            Script script = new Script();
            // this "(Func<List<object>, int>)(l=>l.OfType<int>().Sum())" make some mistakes.
            script.Globals["dosum"] = (Func<List<object>, int>) (l =>
            {
                Console.WriteLine(l[0].GetType());
                return (int)l.OfType<double>().Sum();
            });
            //script.Globals["dosum"] = (Func<List<int>, int>)(l => l.Sum());
            DynValue res = script.DoString(scriptCode);

            Console.WriteLine(res);
            Console.ReadKey();
            return res.Number;
        }
    }
}
