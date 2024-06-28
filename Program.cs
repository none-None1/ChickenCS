using System;
using System.Numerics;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
/*
* Chicken interpreter in C#
* 
* Chicken is an esoteric, dynamically typed language, its types behave like JavaScript
* Add:
*      NUM STR BOOL UND NAN
* NUM  NUM STR NUM  NAN NAN
* STR  STR STR STR  STR STR
* BOOL NUM STR NUM  NAN NAN
* UND  NAN STR NAN  NAN NAN
* NAN  NAN STR NAN  NAN NAN
* 
* Sub:
*      NUM STR BOOL UND NAN
* NUM  NUM NUM NUM  NAN NAN
* STR  NUM NUM NUM  NAN NAN
* BOOL NUM NUM NUM  NAN NAN
* UND  NAN NAN NAN  NAN NAN
* NAN  NAN NAN NAN  NAN NAN
* 
* Mul:
*      NUM STR BOOL UND NAN
* NUM  NUM NUM NUM  NAN NAN
* STR  NUM NUM NUM  NAN NAN
* BOOL NUM NUM NUM  NAN NAN
* UND  NAN NAN NAN  NAN NAN
* NAN  NAN NAN NAN  NAN NAN
*/
namespace ChickenCS
{
    internal class Program
    {
        const int NUM = 0; // number
        const int STR = 1; // string
        const int BOOL = 2; // bool
        const int UND = 3; // undefined
        const int NAN = 4; // nan
        const int STACK = 5; // dummy, refers to stack
        class Chicken
        {
            public string str;
            public int type;
            public Chicken() {
                str = "undefined";
                type = UND;
            }
            public Chicken(string s,int t)
            {
                str = s;
                type = t;
            }
            static string add(string x, string y)
            {
                try
                {
                    return (BigInteger.Parse(x) + BigInteger.Parse(y)).ToString();
                }
                catch (FormatException)
                {
                    return "NaN";
                }
            }
            static string sub(string x, string y)
            {
                try
                {
                    return (BigInteger.Parse(x) - BigInteger.Parse(y)).ToString();
                }
                catch (FormatException)
                {
                    return "NaN";
                }
            }
            static string mul(string x, string y)
            {
                try
                {
                    return (BigInteger.Parse(x) * BigInteger.Parse(y)).ToString();
                }
                catch (FormatException)
                {
                    return "NaN";
                }
            }
            public static Chicken operator +(Chicken x, Chicken y)
            {
                if (x.type == STR || y.type == STR)
                {
                    return new Chicken(x.str + y.str, STR);
                }
                if (x.type > y.type)
                {
                    Chicken t = x;
                    x = y;
                    y = t;
                }
                switch (x.type)
                {
                    case NUM:
                        {
                            switch(y.type)
                            {
                                case NUM:
                                    {
                                        return new Chicken(add(x.str, y.str), NUM);
                                    }
                                case BOOL:
                                    {
                                        return new Chicken(add(x.str,(y.str == "true")?"1":"0"),NUM);
                                    }
                                case UND:
                                    {
                                        return new Chicken("NaN", NAN);
                                    }
                                case NAN:
                                    {
                                        return new Chicken("NaN", NAN);
                                    }
                            }
                            break;
                        }
                    case BOOL:
                        {
                            switch (y.type)
                            {
                                case NUM:
                                    {
                                        return new Chicken(add((x.str == "true") ? "1" : "0", y.str), NUM);
                                    }
                                case BOOL:
                                    {
                                        return new Chicken(add((x.str == "true") ? "1" : "0", (y.str == "true") ? "1" : "0"), NUM);
                                    }
                                case UND:
                                    {
                                        return new Chicken("NaN", NAN);
                                    }
                                case NAN:
                                    {
                                        return new Chicken("NaN", NAN);
                                    }
                            }
                            break;
                        }
                    case UND:
                        {
                            return new Chicken("NaN", NAN);
                        }
                    case NAN:
                        {
                            return new Chicken("NaN", NAN);
                        }
                }
                return new Chicken("NaN", NAN);
            }
            public static Chicken operator -(Chicken x, Chicken y)
            {
                if (x.type == UND || x.type == NAN || y.type == UND || y.type == NAN) return new Chicken("NaN", NAN);
                string xstr = x.str, ystr = y.str;
                if (x.type == BOOL) xstr = ((xstr == "true") ? "1" : "0");
                if (y.type == BOOL) ystr = ((xstr == "true") ? "1" : "0");
                string r = sub(xstr, ystr);
                if(r!="NaN") return new Chicken(r, NUM);
                return new Chicken("NaN", NAN);
            }

            public static Chicken operator *(Chicken x, Chicken y)
            {
                if (x.type == UND || x.type == NAN || y.type == UND || y.type == NAN) return new Chicken("NaN", NAN);
                string xstr = x.str, ystr = y.str;
                if (x.type == BOOL) xstr = ((xstr == "true") ? "1" : "0");
                if (y.type == BOOL) ystr = ((xstr == "true") ? "1" : "0");
                string r = mul(xstr, ystr);
                if (r != "NaN") return new Chicken(r, NUM);
                return new Chicken("NaN", NAN);
            }

            public bool truthy()
            {
                switch (type)
                {
                    case BOOL: return str == "true";
                    case STR: return str != "";
                    case NUM: return str != "0";
                    case UND: return false;
                    case NAN: return false;
                }
                return true;
            }
        }
        static List<Chicken> stack = new List<Chicken>();
        static void Main(string[] args)
        {
            // Step 1: Input
            bool minichicken = false;
            string program = "";
            string[] typenames = { "NUM", "STR", "BOOL", "UND", "NAN" };
            foreach(string i in args){
                if (i == "-h")
                {
                    Console.WriteLine("ChickenCS: Chicken interpreter");
                    Console.WriteLine("ChickenCS [filename] [-m]");
                    Console.WriteLine("filename    The filename to interpret, will read from standard input if not given");
                    Console.WriteLine("-m          Interpret using MiniChicken instead of standard Chicken");
                    Console.WriteLine("Note: When inputting program or input from stdin, end your input with '@'");
                    Console.WriteLine("Tips: set environment variable CHICKEN_DEBUG to display debug info");
                    return;
                }
            }
            if(args.Length == 0)
            {
                int c;
                while ((c = Console.Read()) != 64)
                {
                    program += Convert.ToChar(c);
                }
            }
            else
            {
                if (args.Length == 1)
                {
                    if (args[0] == "-m")
                    {
                        minichicken = true;
                        int c;
                        while ((c = Console.Read()) != 64)
                        {
                            program += Convert.ToChar(c);
                        }
                    }
                    else
                    {
                        program = File.ReadAllText(args[0]);
                    }
                }
                else
                {
                    if (args.Length == 2)
                    {
                        minichicken = true;
                        if (args[0] == "-m")
                        {
                            program = File.ReadAllText(args[1]);
                        }
                        else
                        {
                            program = File.ReadAllText(args[0]);
                        }
                    }
                    else
                    {
                        Console.WriteLine("ChickenCS: Chicken interpreter");
                        Console.WriteLine("ChickenCS [filename] [-m]");
                        Console.WriteLine("filename    The filename to interpret, will read from standard input if not given");
                        Console.WriteLine("-m          Interpret using MiniChicken instead of standard Chicken");
                        Console.WriteLine("Note: When inputting program or input from stdin, end your input with '@'");
                        Console.WriteLine("Tips: set environment variable CHICKEN_DEBUG to display debug info");
                        return;
                    }
                }
            }
            stack.Add(new Chicken("", STACK));
            string input="";
            int C;
            while ((C = Console.Read()) != 64)
            {
                input += Convert.ToChar(C);
            }
            input = input.Replace("\r\n", "\n");
            stack.Add(new Chicken(input, STR));
            // Step 2: Parse
            if (minichicken)
            {
                program = program.Replace("\r\n", "\n");
                program = program.Replace("\n", " ");
                foreach (string i in program.Split(' ',StringSplitOptions.RemoveEmptyEntries))
                {
                    try
                    {
                        BigInteger.Parse(i);
                    }catch (Exception)
                    {
                        Console.WriteLine("Error: Number expected");
                        return;
                    }
                    stack.Add(new Chicken(i, NUM));
                }
            }
            else
            {
                program = program.Replace("\r\n","\n");
                foreach (string i in program.Split('\n'))
                {
                    BigInteger cnt = BigInteger.Zero;
                    foreach(string j in i.Split(" ", StringSplitOptions.RemoveEmptyEntries))
                    {
                        if (j != "chicken")
                        {
                            Console.WriteLine("Error: 'chicken' expected, found '" + j + "'");
                            return;
                        }
                        cnt++;
                    }
                    stack.Add(new Chicken(cnt.ToString(), NUM));
                }
            }
            stack.Add(new Chicken("0", NUM));
            // Step 3: Run
            int ip = 2;
            bool running = true;
            while (ip < stack.Count)
            {
                if (!running) break;
                if (Environment.GetEnvironmentVariable("CHICKEN_DEBUG") != null)
                {
                    Console.Write("Stack size: ");
                    Console.WriteLine(stack.Count);
                    foreach (Chicken i in stack)
                    {
                        if (i.type == STACK) Console.WriteLine("<STACK>");
                        else
                        {
                            Console.Write(i.str);
                            Console.Write(':');
                            Console.WriteLine(typenames[i.type]);
                        }
                    }
                    Console.WriteLine("---");
                    Console.WriteLine("IP: " + Convert.ToString(ip));
                    Console.WriteLine("Current command: " + stack[ip].str);
                    Console.ReadKey();
                }
                switch (stack[ip].str)
                {
                    case "0":
                        {
                            running = false; break;
                        }
                    case "1":
                        {
                            stack.Add(new Chicken("chicken", STR)); // We still push 'chicken' in minichicken mode because most Chicken programs retrieve letters from the 'chicken' string
                            break;
                        }
                    case "2":
                        {
                            Chicken a = stack[stack.Count - 1];
                            stack.RemoveAt(stack.Count - 1);
                            Chicken b = stack[stack.Count - 1];
                            stack.RemoveAt(stack.Count - 1);
                            stack.Add(b + a);
                            break;
                        }
                    case "3":
                        {
                            Chicken a = stack[stack.Count - 1];
                            stack.RemoveAt(stack.Count - 1);
                            Chicken b = stack[stack.Count - 1];
                            stack.RemoveAt(stack.Count - 1);
                            stack.Add(b - a);
                            break;
                        }
                    case "4":
                        {
                            Chicken a = stack[stack.Count - 1];
                            stack.RemoveAt(stack.Count - 1);
                            Chicken b = stack[stack.Count - 1];
                            stack.RemoveAt(stack.Count - 1);
                            stack.Add(b * a);
                            break;
                        }
                    case "5":
                        {
                            Chicken a = stack[stack.Count - 1];
                            stack.RemoveAt(stack.Count - 1);
                            Chicken b = stack[stack.Count - 1];
                            stack.RemoveAt(stack.Count - 1);
                            string res = "";
                            if (a.type == NAN || b.type == NAN) res = "false";
                            else
                            {
                                if (a.type == UND || b.type == UND) res = (a.type == UND && b.type == UND) ? "true" : "false";
                                else
                                {
                                    string astr=a.str,bstr=b.str;
                                    if (a.type == BOOL)
                                    {
                                        astr = (astr == "true") ? "1" : "0";
                                    }
                                    if (b.type == BOOL)
                                    {
                                        bstr = (bstr == "true") ? "1" : "0";
                                    }
                                    if ((bstr == "0" && astr == "" && b.type != NUM) || (astr == "0" && bstr == "") && b.type != NUM) res = "true"; // Special case: 0==''
                                    else res = (astr == bstr) ? "true" : "false";
                                }
                            }
                            stack.Add(new Chicken(res, BOOL));
                            break;
                        }
                    case "6":
                        {
                            int a = Convert.ToInt32(stack[stack.Count - 1].str);
                            stack.RemoveAt(stack.Count - 1);
                            int b = Convert.ToInt32(stack[ip+1].str);
                            if (stack[b].type==STACK)
                            {
                                if (a<0||a >= stack.Count)
                                {
                                    stack.Add(new Chicken("undefined", UND));
                                }
                                else
                                {
                                    stack.Add(stack[a]);
                                }
                            }
                            else if(stack[b].type == STR)
                            {
                                if (a<0||a >= stack[b].str.Length)
                                {
                                    stack.Add(new Chicken("undefined", UND));
                                }
                                else
                                {
                                    stack.Add(new Chicken(Convert.ToString(stack[b].str[a]),STR));
                                }
                            }
                            else
                            {
                                stack.Add(new Chicken("undefined", UND));
                            }
                            ip++;
                            break;
                        }
                    case "7":
                        {
                            int a = Convert.ToInt32(stack[stack.Count - 1].str);
                            stack.RemoveAt(stack.Count - 1);
                            Chicken b = stack[stack.Count - 1];
                            stack.RemoveAt(stack.Count - 1);
                            stack[a] = b;
                            break;
                        }
                    case "8":
                        {
                            int a = Convert.ToInt32(stack[stack.Count - 1].str);
                            stack.RemoveAt(stack.Count - 1);
                            Chicken b = stack[stack.Count - 1];
                            stack.RemoveAt(stack.Count - 1);
                            if (b.truthy())
                            {
                                ip += a;
                            }
                            break;
                        }
                    case "9":
                        {
                            int a = Convert.ToInt32(stack[stack.Count - 1].str);
                            stack.RemoveAt(stack.Count - 1);
                            stack.Add(new Chicken("&#" + Convert.ToString(a) + ";", STR));
                            break;
                        }
                    default:
                        {
                            stack.Add(new Chicken((BigInteger.Parse(stack[ip].str) - 10).ToString(),NUM)); break;
                        }
                }
                ip++;
            }
            Console.WriteLine(Regex.Replace(stack[stack.Count - 1].str, @"\&\#\d+\;", new MatchEvaluator(unescape)));
        }
        static string unescape(Match str)
        {
            return Convert.ToString(Convert.ToChar(Convert.ToInt32(str.ToString().Substring(2, str.ToString().Length - 3))));
        }
    }
}