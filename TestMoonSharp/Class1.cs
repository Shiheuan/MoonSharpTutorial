﻿using System;
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
    }
}
