using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Loaders;
using MoonSharp.Interpreter.Platforms;
using MoonSharp.VsCodeDebugger;
using MoonSharp.RemoteDebugger;
using Microsoft.VisualStudio.TestTools.UnitTesting;


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
                print(Mul) -- function: 00000097
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
            Console.WriteLine(script.Globals["Mul"]);
            Console.WriteLine(script.Globals.Get("Mul"));
            Console.WriteLine(script.Globals["fact"]);
            Console.WriteLine(script.Globals.Get("fact"));

            // -> MoonSharp.Interpreter.CallbackFunction
            // -> (Function CLR)
            // -> MoonSharp.Interpreter.Closure
            // -> (Function 00000067)
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
                print(tbl) --
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

        public static void TableTest3()
        {

            Script script = new Script();
            Table tbl = new Table(script);

            for (int i = 1; i <= 10; i++)
            {
                tbl[i] = i;
            }

            tbl["name"] = "a table you know.";

            script.Globals["t"] = tbl;
            var scriptCode = @"
                print(t)
                print(t.name)
                for k, v in ipairs(t) do
                    print(k, v)
                end
            ";
            script.DoString(scriptCode);
            Console.ReadKey();
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

        class MyProxy
        {
            private MyClass1 target;

            [MoonSharpHidden]
            public MyProxy(MyClass1 p)
            {
                this.target = p;
            }

            public double GetValue(double a, double b)
            {
                return target.CalcHypotenuse(a, b);
            }
        }

        public static double TestProxyObjects()
        {
            string scriptCode = @"
                return mytarget.GetValue(3, 4);
            ";
            
            UserData.RegisterProxyType<MyProxy, MyClass1>(r => new MyProxy(r));
            Script script = new Script();

            script.Globals["mytarget"] = new MyClass1();
            DynValue res = script.DoString(scriptCode);
            Console.WriteLine(res);
            Console.ReadKey();
            return res.Number;
        }
        public static void ErrorHandling()
        {
            try
            {
                string scriptCode = @"    
					return obj.calcHypotenuse(3, 4);
				";

                Script script = new Script();
                DynValue res = script.DoString(scriptCode);
                Console.WriteLine(res);
            }
            catch (ScriptRuntimeException ex)
            {
                Console.WriteLine("Doh! An error occured! {0}", ex.DecoratedMessage);
            }
            Console.ReadKey();
        }

        static void DoError()
        {
            throw new ScriptRuntimeException("This is an exceptional message, no pun intended.");
        }

        public static string ErrorGen()
        {
            string scriptCode = @"    
				local _, msg = pcall(DoError);
				return msg;
			";

            Script script = new Script();
            script.Globals["DoError"] = (Action)DoError;
            DynValue res = script.DoString(scriptCode);
            Console.WriteLine(res);
            Console.ReadKey();
            return res.String;
        }
        public static void EmbeddedResourceScriptLoader()
        {
            Script script = new Script();
            script.Options.ScriptLoader = new EmbeddedResourcesScriptLoader();
            script.DoFile("Scripts/Test.lua");
            Console.ReadKey();
        }

        private class MyCustomScriptLoader : ScriptLoaderBase
        {
            public override object LoadFile(string file, Table globalContext)
            {
                return string.Format("print ([[A request to load '{0}' has been made]])", file);
            }

            public override bool ScriptFileExists(string name)
            {
                return true;
            }
        }

        public static void CustomScriptLoader()
        {
            Script script = new Script();

            script.Options.ScriptLoader = new MyCustomScriptLoader()
            {
                ModulePaths = new string[] { "?_module.lua" }
            };

            script.DoString(@"
		        require 'somemodule'
		        f = loadfile 'someothermodule.lua'
		        f()
	        ");
            Console.ReadKey();
        }

        public static void ChangePlatform()
        {
            // This prints "function"
            Console.WriteLine(Script.RunString("return type(os.exit);").ToPrintString());

            // Save the old platform
            var oldplatform = Script.GlobalOptions.Platform;

            // Changing platform after a script has been created is not recommended.. do not do it.
            // We are doing it for the purpose of the walkthrough..


            //Script.GlobalOptions.Platform = new LimitedPlatformAccessor();
            // => type is function
            Script.GlobalOptions.Platform = new LimitedPlatformAccessor();

            // This time, this prints "nil"
            Console.WriteLine(Script.RunString("return type(os.exit);").ToPrintString());

            // Restore the old platform
            Script.GlobalOptions.Platform = oldplatform;
            Console.ReadKey();
        }

        public static void OverriddenPrint()
        {
            // redefine print to print in lowercase, for all new scripts
            Script.DefaultOptions.DebugPrint = s => Console.WriteLine(s.ToLower());

            Script script = new Script();

            DynValue fn = script.LoadString("print 'Hello, World!'");

            fn.Function.Call(); // this prints "hello, world!"

            // redefine print to print in UPPERCASE, for this script only
            script.Options.DebugPrint = s => Console.WriteLine(s.ToUpper());

            fn.Function.Call(); // this prints "HELLO, WORLD!"
            Console.ReadKey();
        }

        public static void TestVsDebugger()
        {
            var script = new Script();
            script.Globals["print"] = new Func<string, int>(text =>
            {
                Console.WriteLine(text);
                return text.Length;
            });
            //debug
            MoonSharpVsCodeDebugServer server = new MoonSharpVsCodeDebugServer();
            server.Start();
            server.AttachToScript(script, "DebugScript");
            /*
            string scriptCode = @"    
				function main()
                    local chars = """"
                    chars = print(""Hello world"")
                end
            ";
            /**/
            //Console.WriteLine(scriptCode);

            string scriptCode = File.ReadAllText(@"X:\gitdir\TestMoonSharp\TestMoonSharp\Scripts\debug.lua");
            //Console.WriteLine(scriptCode);

            script.DoString(scriptCode, null, @"X:\gitdir\TestMoonSharp\TestMoonSharp\Scripts\debug.lua");
            // wait for debugger to attach
            bool attached = AwaitDebuggerAttach(server);
            if (!attached)
            {
                Console.WriteLine("VS Code debugger did not attach. Running the script.");
            }

            
            script.Call(script.Globals["main"]);

            Console.ReadKey();
        }
        private static bool AwaitDebuggerAttach(MoonSharpVsCodeDebugServer server)
        {
            // as soon as a client has attached, 'm_Client__' field of 'm_Current' isn't null anymore
            // 
            // we wait for ~60 seconds for a client to attach

            BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.NonPublic;
            FieldInfo field = server.GetType().GetField("m_Current", bindFlags);
            object current = field.GetValue(server);

            FieldInfo property = current.GetType().GetField("m_Client__", bindFlags);

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            Console.WriteLine("Waiting for VS Code debugger to attach");
            while (property.GetValue(current) == null)
            {
                Thread.Sleep(500);
                if (stopwatch.Elapsed.TotalSeconds > 60) return false;
            }
            stopwatch.Stop();
            Console.WriteLine("VS Code debugger attached");
            return true;
        }

        static RemoteDebuggerService remoteDebugger;

        static void ActivateRemoteDebugger(Script script)
        {
            if (remoteDebugger == null)
            {
                remoteDebugger = new RemoteDebuggerService();

                // the last boolean is to specify if the script is free to run 
                // after attachment, defaults to false
                remoteDebugger.Attach(script, "Description of the script", false);
            }

            // start the web-browser at the correct url. Replace this or just
            // pass the url to the user in some way.
            Process.Start(remoteDebugger.HttpUrlStringLocalHost);
        }

        public static void DebuggerDemo()
        {
            Script script = new Script();

            ActivateRemoteDebugger(script);

            script.DoString(@"

				function accum(n, f)
					if (n == 0) then
						return 1;
					else
						return n * f(n);
					end
				end


				local sum = 0;

				for i = 1, 5 do
					-- let's use a lambda to spice things up
					sum = sum + accum(i, | x | x * 2);
				end
				");

            Console.WriteLine("The script has ended..");
            Console.ReadKey();
        }

        public static void CoroutinesFromCSharp()
        {
            string code = @"
				return function()
					local x = 0
					while true do
						x = x + 1
						coroutine.yield(x)
					end
				end
			";

            // Load the code and get the returned function
            Script script = new Script();
            DynValue function = script.DoString(code);

            // Create the coroutine in C#
            DynValue coroutine = script.CreateCoroutine(function);

            // Resume the coroutine forever and ever.. 
            while (true)
            {
                DynValue x = coroutine.Coroutine.Resume();
                Console.WriteLine("{0}", x);
            }
        }

        public static void CoroutinesAsCSharpIterator()
        {
            string code = @"
				return function()
					local x = 0
					while true do
						x = x + 1
						coroutine.yield(x)
						if (x > 5) then
							return 7
						end
					end
				end
				";

            // Load the code and get the returned function
            Script script = new Script();
            DynValue function = script.DoString(code);

            // Create the coroutine in C#
            DynValue coroutine = script.CreateCoroutine(function);

            // Loop the coroutine 
            string ret = "";

            foreach (DynValue x in coroutine.Coroutine.AsTypedEnumerable())
            {
                ret = ret + x.ToString();
            }
            Debug.Assert(ret == "1234567", "HA?");
            Assert.AreEqual("1234567", ret);
            Console.WriteLine(ret);
            Console.ReadKey();
        }

        public static void PreemptiveCoroutines()
        {
            string code = @"
	            function fib(n)
		            if (n == 0 or n == 1) then
			            return 1;
		            else
			            return fib(n - 1) + fib(n - 2);
		            end
	            end
	        ";

            // Load the code and get the returned function
            Script script = new Script(CoreModules.None);
            script.DoString(code);

            // get the function
            DynValue function = script.Globals.Get("fib");

            // Create the coroutine in C#
            DynValue coroutine = script.CreateCoroutine(function);

            // Set the automatic yield counter every 10 instructions. 
            // 10 is likely too small! Use a much bigger value in your code to avoid interrupting too often!
            coroutine.Coroutine.AutoYieldCounter = 10;

            int cycles = 0;
            DynValue result = null;

            // Cycle until we get that the coroutine has returned something useful and not an automatic yield..
            for (result = coroutine.Coroutine.Resume(8);
                result.Type == DataType.YieldRequest;
                result = coroutine.Coroutine.Resume())
            {
                cycles += 1;
            }
            Console.WriteLine(cycles);
            Console.WriteLine(result.Number);
            // Check the values of the operation
            Assert.AreEqual(DataType.Number, result.Type);
            Assert.AreEqual(34, result.Number);
            Console.ReadKey();
        }

        private delegate void CastDelegate(string str);

        private Delegate Dely;

        public static void testDelegate(Class1 ins)
        {
            CastDelegate cast;
            cast = testCastStatic;
            cast("type1");
            cast = new CastDelegate(ins.testCast);
            cast = ins.testCast;
            cast("tooStupid");
            ins.Dely = (CastDelegate)testCastStatic;
            ((CastDelegate)ins.Dely)("Whaaaaat");

            Console.ReadKey();
        }

        public void testDelegate2()
        {
            CastDelegate cast2;
            cast2 = testCastStatic;
            cast2("type1");
            cast2 = new CastDelegate(testCast);
            cast2 = testCast;
            cast2("tooStupid");
            Dely = (CastDelegate)testCastStatic;
            ((CastDelegate)Dely)("Whaaaaat");

            Console.ReadKey();
        }

        private static void testCastStatic(string name)
        {
            Console.WriteLine($"In function 'testCastStatic', say {name} !");
        }

        private void testCast(string name)
        {
            Console.WriteLine($"In function 'testCast', say {name} !");
        }

        public void testLoopWait()
        {
            const string test = @"
        do
            print('can I?')
            local timer = 0
            while timer< 10000000 do
                timer = timer + 10
            end
            print('You can.')
        end";

            Script script = new Script();
            script.DoString(test);

            Console.ReadKey();
        }

        public void testCoroutine()
        {
            const string test = @"
                co = function()
                    local count = 0
                    while count < 4 do
                        count = count + 1
                        coroutine.yield()
                    end
                end
            ";

            Script script = new Script();
            script.DoString(test);
            var c = script.CreateCoroutine(script.Globals.Get("co"));

            for (int i = 0; i < 6; i++)
            {
                Console.WriteLine(c.Coroutine.State);
                if (c.Coroutine.State != CoroutineState.Dead)
                    c.Coroutine.Resume();
            }

            Console.ReadKey();
        }

    }
}
