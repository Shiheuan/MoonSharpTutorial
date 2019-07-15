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

                return fact(mynumber)";
            Script script = new Script();
            script.Globals["mynumber"] = 7;
            DynValue res = script.DoString(scriptCode);
            Console.WriteLine(res.Number);
            Console.ReadKey();
            return res.Number;
        }
    }
}
