﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Remoting.Messaging;
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

        static double Sum(Table t)
        {
            // LINQ create a query
            var nums = from v in t.Values
                where v.Type == DataType.Number
                select v.Number;
            return nums.Sum();
        }

        public static double TableTestReverseWithTable()
        {
            string scriptCode = @"
                return dosum {1, 2, 3, 4, 5, 6, 7, 8, 9, 10}
                ";
            Script script = new Script();
            script.Globals["dosum"] = (Func<Table, double>) Sum;
            DynValue res = script.DoString(scriptCode);

            Console.WriteLine(res);
            Console.ReadKey();
            return res.Number;
        }

        //2019-7-30-15:08
        [MoonSharpUserData]
        class MyClass1
        {
            // if here 'C' or 'c', the 'c' script can access both. Otherwise can not. because the rule of match.
            //public double calcHypotenuse(double a, double b)
            public double CalcHypotenuse(double a, double b)
            {
                return Math.Sqrt(a * a + b * b);
            }
        }

        public static double CallMyClass1()
        {
            string scriptCode = @"
                return MC.calcHypotenuse(3, 4)
            ";
            // Automatically register all MoonSharpUserData types
            UserData.RegisterAssembly();

            Script script = new Script();

            // Pass an instance of MyClass1 to the script in a global
            script.Globals["MC"] = new MyClass1();

            DynValue res = script.DoString(scriptCode);
            Console.WriteLine(res);  // => 5
            Console.ReadKey();
            return res.Number;
        }
        class MyClass2
        {
            public double calcHypotenuse(double a, double b)
            {
                return Math.Sqrt(a * a + b * b);
            }
        }
        public static double CallMyClass2()
        {
            string scriptCode = @"
                return MC.calcHypotenuse(3, 4)   
                -- return MC.CalcHypotenuse(3, 4) error
            ";

            // Register just MyClass1, explicitly
            UserData.RegisterType<MyClass2>();
            Script script = new Script();

            // create a userdata, explicitly
            DynValue obj = UserData.Create(new MyClass2());

            script.Globals.Set("MC", obj);

            DynValue res = script.DoString(scriptCode);

            Console.WriteLine(res); // => 5
            Console.ReadKey();
            return res.Number;
        }

        [MoonSharpUserData]
        class MyClassStatic1
        {
            public static double calcHypotenuse(double a, double b)
            {
                return Math.Sqrt(a * a + b * b);
            }
        }

        public static double MyClassStaticThroughInstance()
        {
            string scriptCode = @"
                return MCS.calcHypotenuse(6, 8);
            ";

            UserData.RegisterAssembly();

            Script script = new Script();

            script.Globals["MCS"] = new MyClassStatic1();

            DynValue res = script.DoString(scriptCode);

            Console.WriteLine(res); // => 10
            Console.ReadKey();
            return res.Number;
        }

        public static double MyClassStaticThroughPlaceholder()
        {
            string scriptCode = @"
                return MCS.calcHypotenuse(9, 12)
            ";

            UserData.RegisterAssembly();
            Script script = new Script();

            script.Globals["MCS"] = typeof(MyClassStatic1);

            DynValue res = script.DoString(scriptCode);

            Console.WriteLine(res); // => 15
            Console.ReadKey();
            return res.Number;
        }
        class MyClass3
        {
            public string ManipulateString(string input, ref string tobeconcat, out string lowercase)
            {
                tobeconcat = input + tobeconcat;
                lowercase = input.ToLower();
                return input.ToUpper();
            }
        }
        public static void MethodWithByRef()
        {
            string scriptCode = @"
                x, y, z = myobj:manipulateString('CiAo', 'hello');
                print(x) -- CIAO 
                print(y) -- CiAohello 
                print(z) -- ciao
            ";

            UserData.RegisterType<MyClass3>();
            Script script = new Script();

            script.Globals["myobj"] = new MyClass3();

            DynValue res = script.DoString(scriptCode);

            Console.WriteLine(res); // => void
            Console.ReadKey();
        }

        class IndexerTestClass
        {
            Dictionary<int, int> mymap = new Dictionary<int, int>();

            public int this[int idx]
            {
                get { return mymap[idx]; }
                set { mymap[idx] = value; }
            }

            public int this[int idx1, int idx2, int idx3]
            {
                get
                {
                    int idx = (idx1 + idx2) * idx3;
                    return mymap[idx];
                }
                set
                {
                    int idx = (idx1 + idx2) * idx3;
                    mymap[idx] = value;;
                }
            }
        }

        public static void MethodTestIndexer()
        {
            string scriptCode = @"
                o[5] = 19
                print(o[5]) -- 19
                x = 5 + o[5]
                print(x) -- 24
                o[1,2,3] = 19
                print(o[1,2,3]) -- 19
                x = 5 + o[1,2,3]
                print(x) -- 24

                m = {
                    __index = o,
                    __newindex = o
                    -- pretend this is some meaningful functions...
                    --__index = function(obj, idx) return o[idx] end,     
	                --__newindex = function(obj, idx, val) end
                    -- => :“cannot multi-index through metamethods. userdata expected”
                }
                t = { }
                
                setmetatable(t, m)
                t[10,11,12] = 1234
                return t[10,11,12]
            ";

            UserData.RegisterType<IndexerTestClass>();
            Script script = new Script();
            script.Globals["o"] = new IndexerTestClass();

            DynValue res = script.DoString(scriptCode);
            Console.WriteLine(res); // => 1234
            Console.ReadKey();
        }
        class ArithmOperatorsTestClass : IComparable, System.Collections.IEnumerable
        {
            public int Value { get; set; }

            public ArithmOperatorsTestClass()
            {
            }

            public ArithmOperatorsTestClass(int value)
            {
                Value = value;
            }

            public int Length { get { return 117; } }

            [MoonSharpUserDataMetamethod("__concat")]
            public static int Concat(ArithmOperatorsTestClass o, int v)
            {
                return o.Value + v;
            }

            [MoonSharpUserDataMetamethod("__concat")]
            public static int Concat(int v, ArithmOperatorsTestClass o)
            {
                return o.Value + v;
            }

            [MoonSharpUserDataMetamethod("__concat")]
            public static int Concat(ArithmOperatorsTestClass o1, ArithmOperatorsTestClass o2)
            {
                return o1.Value + o2.Value;
            }

            public static int operator +(ArithmOperatorsTestClass o, int v)
            {
                return o.Value + v;
            }

            public static int operator +(int v, ArithmOperatorsTestClass o)
            {
                return o.Value + v;
            }

            public static int operator +(ArithmOperatorsTestClass o1, ArithmOperatorsTestClass o2)
            {
                return o1.Value + o2.Value;
            }

            public override bool Equals(object obj)
            {
                if (obj is double)
                    return ((double)obj) == Value;

                ArithmOperatorsTestClass other = obj as ArithmOperatorsTestClass;
                if (other == null) return false;
                return Value == other.Value;
            }

            public override int GetHashCode()
            {
                return Value.GetHashCode();
            }

            public int CompareTo(object obj)
            {
                if (obj is double)
                    return Value.CompareTo((int)(double)obj);

                ArithmOperatorsTestClass other = obj as ArithmOperatorsTestClass;
                if (other == null) return 1;
                return Value.CompareTo(other.Value);
            }

            public System.Collections.IEnumerator GetEnumerator()
            {
                return (new List<int>() { 1, 2, 3 }).GetEnumerator();
            }

            [MoonSharpUserDataMetamethod("__call")]
            public int DefaultMethod()
            {
                return -Value;
            }

            [MoonSharpUserDataMetamethod("__pairs")]
            [MoonSharpUserDataMetamethod("__ipairs")]
            public System.Collections.IEnumerator Pairs()
            {
                return (new List<DynValue>() {
                    // key "a", value "A"
                    DynValue.NewTuple(DynValue.NewString("a"), DynValue.NewString("A")),
                    DynValue.NewTuple(DynValue.NewString("b"), DynValue.NewString("B")),
                    DynValue.NewTuple(DynValue.NewString("c"), DynValue.NewString("C")) }).GetEnumerator();
            }


        }

        public static void OperatorsAndMetaMethods_WithAttributes()
        {
            string scriptCode = @"    
				print( o .. 1 );
				print( 1 .. o );
				print( o .. o );
			";

            UserData.RegisterType<ArithmOperatorsTestClass>();
            Script script = new Script();
            script.Globals["o"] = new ArithmOperatorsTestClass(5);

            script.DoString(scriptCode);
            Console.ReadKey();
        }


        public static void OperatorsAndMetaMethods_WithOperatorsOverloads()
        {
            string scriptCode = @"    
				print( o + 1 );
				print( 1 + o );
				print( o + o );
			";

            UserData.RegisterType<ArithmOperatorsTestClass>();
            Script script = new Script();
            script.Globals["o"] = new ArithmOperatorsTestClass(5);

            script.DoString(scriptCode);
            Console.ReadKey();
        }


        public static void OperatorsAndMetaMethods_Comparisons()
        {
            string scriptCode = @"    
				print( 'o == 1 ?', o == 1 );
				print( '1 == o ?', 1 == o );
				print( 'o == 5 ?', o == 5 );
				print( 'o != 1 ?', o != 1 );
				print( 'o <  1 ?', o <  1 );
				print( 'o <= 1 ?', o <= 1 );
				print( 'o <  6 ?', o <  6 );
				print( 'o <= 6 ?', o <= 6 );
				print( 'o >  1 ?', o >  1 );
				print( 'o >= 1 ?', o >= 1 );
				print( 'o >  6 ?', o >  6 );
				print( 'o >= 6 ?', o >= 6 );
			";

            UserData.RegisterType<ArithmOperatorsTestClass>();
            Script script = new Script();
            script.Globals["o"] = new ArithmOperatorsTestClass(5);

            script.DoString(scriptCode);
            Console.ReadKey();
        }


        public static void OperatorsAndMetaMethods_Length()
        {
            string scriptCode = @"    
				print( '#o + o ?', #o + o); -- 122
			";

            UserData.RegisterType<ArithmOperatorsTestClass>();
            Script script = new Script();
            script.Globals["o"] = new ArithmOperatorsTestClass(5);

            script.DoString(scriptCode);
            Console.ReadKey();
        }


        public static void OperatorsAndMetaMethods_ForEach()
        {
            string scriptCode = @"    
				local sum = 0
				for i, k, v in ipairs(o) do
					--sum = sum + i
                    print(i) -- a b c
                    print(k)
                    print(v) -- A B C
				end

				print (sum); -- 6
			";

            UserData.RegisterType<ArithmOperatorsTestClass>();
            Script script = new Script();
            script.Globals["o"] = new ArithmOperatorsTestClass(5);

            script.DoString(scriptCode);
            Console.ReadKey();
        }

        [MoonSharpUserData]
        class MyClass4
        {
            public event EventHandler SomethingHappened;

            public void RaiseTheEvent()
            {
                if (SomethingHappened != null)
                    SomethingHappened(this, EventArgs.Empty);
            }
        }

        public static void Events()
        {
            string scriptCode = @"    
				function handler(o, a)
					print('handled!', o, a);
				end

				myobj.somethingHappened.add(handler);
				myobj.raiseTheEvent();
				myobj.somethingHappened.remove(handler);
				myobj.raiseTheEvent();
			";

            UserData.RegisterType<EventArgs>();
            UserData.RegisterType<MyClass4>();
            Script script = new Script();
            script.Globals["myobj"] = new MyClass4();
            script.DoString(scriptCode);
            Console.ReadKey();
        }
    }
}
