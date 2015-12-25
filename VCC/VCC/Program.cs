using bsn.GoldParser.Grammar;
using bsn.GoldParser.Parser;
using bsn.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Vasm;


namespace VCC.Core
{
    public class ConsoleUtils
    {
        public static void WriteGreen(string txt)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(txt);
            Console.ForegroundColor = ConsoleColor.White;
        }
        public static void WriteRed(string txt)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(txt);
            Console.ForegroundColor = ConsoleColor.White;
        }
        public static void WriteYellow(string txt)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(txt);
            Console.ForegroundColor = ConsoleColor.White;
        }
        
    }
    public class Test
    {
        public bool Success { get; set; }
        public string TestString { get; set; }
        public string TestName { get; set; }
        public Test(string tst,string name)
        {
            TestName = name;
            TestString = tst;
        }
    }
    class Program
    {
        static void RunTest(ref Test t)
        {
            Console.Clear();
            Console.Title = t.TestName;
            try
            {

                // grab an execution context to be used for the entire 'session'
                using (AssemblyWriter asmw = new AssemblyWriter(Console.OpenStandardOutput(), Encoding.ASCII))
                {
                    using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("VCC.VATU.cgt"))
                    {


                        // compile/run each statement one entry at a time
                        CompiledGrammar grammar = CompiledGrammar.Load(stream);
                        var actions = new SemanticTypeActions<SimpleToken>(grammar);


                        var processor = new SemanticProcessor<SimpleToken>(new StringReader(t.TestString), actions);

                        ParseMessage parseMessage = processor.ParseAll();
                        if (parseMessage == ParseMessage.Accept)
                        {

                            Console.Write("Syntax : "); ConsoleUtils.WriteGreen("OK");

                            var globals = processor.CurrentToken as GlobalSequence<Global>;
                            ResolveContext RootCtx = null;
                            List<Declaration> Resolved = new List<Declaration>();
                            List<ResolveContext> ResolveCtx = new List<ResolveContext>();
                            foreach (Global stmts in globals)
                            {

                                ResolveContext old_ctx = RootCtx;


                                RootCtx = ResolveContext.CreateRootContext(stmts.Used, stmts.Namespace, stmts.Declarations);
                                if (old_ctx != null)
                                    RootCtx.FillKnownByKnown(old_ctx._known);
                                if (stmts != null)
                                {

                                    foreach (Declaration stmt in stmts.Declarations)
                                    {

                                        if (stmt.BaseDeclaration is MethodDeclaration)
                                        {
                                            MethodDeclaration md = (MethodDeclaration)stmt.BaseDeclaration;
                                            ResolveContext childctx = RootCtx.CreateAsChild(stmts.Used, stmts.Namespace, md);
                                            stmt.Resolve(childctx);
                                            MethodDeclaration d = (MethodDeclaration)md.DoResolve(childctx);
                                            // RootCtx.UpdateChildContext("<method-decl>", childctx);
                                            RootCtx.UpdateFather(childctx);
                                            Resolved.Add(d);
                                            ResolveCtx.Add(childctx);
                                        }
                                        else if (stmt.BaseDeclaration is StructDeclaration)
                                        {
                                            StructDeclaration md = (StructDeclaration)stmt.BaseDeclaration;
                                            ResolveContext childctx = RootCtx.CreateAsChild(stmts.Used, stmts.Namespace, md);
                                            stmt.Resolve(childctx);
                                            StructDeclaration d = (StructDeclaration)md.DoResolve(childctx);
                                            // RootCtx.UpdateChildContext("<method-decl>", childctx);
                                            RootCtx.UpdateFather(childctx);
                                            Resolved.Add(d);
                                            ResolveCtx.Add(childctx);
                                        }
                                        else if (stmt.BaseDeclaration is EnumDeclaration)
                                        {
                                            EnumDeclaration md = (EnumDeclaration)stmt.BaseDeclaration;
                                            ResolveContext childctx = RootCtx.CreateAsChild(stmts.Used, stmts.Namespace, md);
                                            stmt.Resolve(childctx);
                                            EnumDeclaration d = (EnumDeclaration)md.DoResolve(childctx);
                                            // RootCtx.UpdateChildContext("<method-decl>", childctx);
                                            RootCtx.UpdateFather(childctx);
                                            Resolved.Add(d);
                                            ResolveCtx.Add(childctx);
                                        }
                                        else
                                        {
                                            RootCtx.IsInTypeDef = stmt.IsTypeDef;
                                            RootCtx.IsInStruct = stmt.IsStruct;
                                            stmt.Resolve(RootCtx);
                                            ResolveCtx.Add(RootCtx);
                                            Declaration d = (Declaration)stmt.DoResolve(RootCtx);
                                            Resolved.Add(d);
                                        }
                                    }

                                }

                            }
                            if (ResolveContext.Report.ErrorCount == 0)
                            {
                                Console.Write("Resolve : ");
                                ConsoleUtils.WriteGreen("OK");

                                int i = 0;
                                EmitContext ec = new EmitContext(asmw);
                                foreach (Declaration stmt in Resolved)
                                {
                                    ec.SetCurrentResolve(ResolveCtx[i]);
                                    stmt.Emit(ec);
                                    i++;
                                } Console.Write("Emit : ");
                                ConsoleUtils.WriteGreen("OK");
                                ConsoleUtils.WriteYellow("Asm Code : ");
                                ec.Emit();
                           
                            }
                        }
                        else
                        {
                            Console.Write("Resolve : ");
                            ConsoleUtils.WriteRed("ERROR");

                            IToken token = processor.CurrentToken;
                            Console.WriteLine("At index: {0} [{1}] {2},{3}", token.Position.Index, parseMessage, token.Position.Line, token.Position.Column);
                            //Console.WriteLine(string.Format("{0} {1}", "^".PadLeft(token.Position.Index + 1), parseMessage));
                        }

                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("Failure  ");
                ConsoleUtils.WriteRed("Error: "+ex.Message);
            }
            Console.Write("Success ? "); 
            if (Console.ReadLine() == "Y")
                t.Success = true;
            else
                t.Success = false;
   

         
        }
        static void Run(string input)
        {

        
            // grab an execution context to be used for the entire 'session'
            using (AssemblyWriter asmw = new AssemblyWriter(Console.OpenStandardOutput(), Encoding.ASCII))
            {
                using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("VCC.VATU.cgt"))
                {


                    // compile/run each statement one entry at a time
                    CompiledGrammar grammar = CompiledGrammar.Load(stream);
                    var actions = new SemanticTypeActions<SimpleToken>(grammar);


                    var processor = new SemanticProcessor<SimpleToken>(new StringReader(input), actions);

                    ParseMessage parseMessage = processor.ParseAll();
                    if (parseMessage == ParseMessage.Accept)
                    {

                        Console.WriteLine("Syntax : "); ConsoleUtils.WriteGreen("OK");

                        var globals = processor.CurrentToken as GlobalSequence<Global>;
                           ResolveContext RootCtx=null ;
                           List<Declaration> Resolved = new List<Declaration>();
                           List<ResolveContext> ResolveCtx = new List<ResolveContext>();
                        foreach (Global stmts in globals)
                        {

                            ResolveContext old_ctx = RootCtx;
                               

                        RootCtx = ResolveContext.CreateRootContext(stmts.Used,stmts.Namespace, stmts.Declarations);
                        if (old_ctx != null)
                            RootCtx.FillKnownByKnown(old_ctx._known);
                        if (stmts != null)
                        {
                        
                            foreach (Declaration stmt in stmts.Declarations)
                            {

                                if (stmt.BaseDeclaration is MethodDeclaration)
                                {
                                    MethodDeclaration md = (MethodDeclaration)stmt.BaseDeclaration;
                                    ResolveContext childctx = RootCtx.CreateAsChild(stmts.Used,stmts.Namespace,md);
                                    stmt.Resolve(childctx);
                                    MethodDeclaration d = (MethodDeclaration)md.DoResolve(childctx);
                                   // RootCtx.UpdateChildContext("<method-decl>", childctx);
                                    RootCtx.UpdateFather(childctx);
                                    Resolved.Add(d);
                                    ResolveCtx.Add(childctx);
                                }
                                else if (stmt.BaseDeclaration is StructDeclaration)
                                {
                                    StructDeclaration md = (StructDeclaration)stmt.BaseDeclaration;
                                    ResolveContext childctx = RootCtx.CreateAsChild(stmts.Used,stmts.Namespace, md);
                                    stmt.Resolve(childctx);
                                    StructDeclaration d = (StructDeclaration)md.DoResolve(childctx);
                                    // RootCtx.UpdateChildContext("<method-decl>", childctx);
                                    RootCtx.UpdateFather(childctx);
                                    Resolved.Add(d);
                                    ResolveCtx.Add(childctx);
                                }
                                else if (stmt.BaseDeclaration is EnumDeclaration)
                                {
                                    EnumDeclaration md = (EnumDeclaration)stmt.BaseDeclaration;
                                    ResolveContext childctx = RootCtx.CreateAsChild(stmts.Used,stmts.Namespace, md);
                                    stmt.Resolve(childctx);
                                    EnumDeclaration d = (EnumDeclaration)md.DoResolve(childctx);
                                    // RootCtx.UpdateChildContext("<method-decl>", childctx);
                                    RootCtx.UpdateFather(childctx);
                                    Resolved.Add(d);
                                    ResolveCtx.Add(childctx);
                                }
                                else
                                {
                                    RootCtx.IsInTypeDef = stmt.IsTypeDef;
                                    RootCtx.IsInStruct = stmt.IsStruct;
                                    stmt.Resolve(RootCtx);
                                    ResolveCtx.Add(RootCtx);
                                    Declaration d = (Declaration)stmt.DoResolve(RootCtx);
                                    Resolved.Add(d);
                                }
                            }
                         
                        }

                    }

                        
                        int i = 0;
                        EmitContext ec = new EmitContext(asmw);
                        foreach (Declaration stmt in Resolved)
                        {
                            ec.SetCurrentResolve(ResolveCtx[i]);
                            stmt.Emit(ec);
                            i++;
                        }
                        Console.WriteLine("Asm Code : ");
                        ec.Emit();
                    }
                    else
                    {


                        IToken token = processor.CurrentToken;
                        Console.WriteLine("At index: {0} [{1}] {2},{3}", token.Position.Index, parseMessage,token.Position.Line, token.Position.Column);
                        //Console.WriteLine(string.Format("{0} {1}", "^".PadLeft(token.Position.Index + 1), parseMessage));
                    }

                }
            }
        }
        static void Init()
        {
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("VCC.VATU.cgt"))
            {
                CompiledGrammar grammar = CompiledGrammar.Load(stream);
                var actions = new SemanticTypeActions<SimpleToken>(grammar);
                try
                {
                    actions.Initialize(true);
                  
                }
                catch (InvalidOperationException ex)
                {
                    Console.Write(ex.Message);
                    Console.ReadKey(true);
                    return;
                }



            }
        }
        static void Main(string[] args)
        {
            Console.WriteLine("Copyright (c) 2015 Arsslensoft Research. All rights reserved");
            ResolveContext.Report = new ConsoleReporter();
          //  Init();
            string sample = "";
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("VCC.Sample.vt"))
            {
                sample = new StreamReader(stream).ReadToEnd();
                Run(sample);
            }
   List<Test> t = new List<Test>();
            t.Add(new Test("int GLOBALVAR = 789; void main() {int a = 45; \nuint v = 3762u; bool x = false; byte c = 5;}","BASIC TEST"));
            t.Add(new Test(" namespace STD; void printf(string); int ABC; namespace Hello;  const uint ABC=9; namespace MYNS; use STD;  enum VALS {A,B,C,D = 1, E = 4};  string* ABC; entry void main(){MYNS::ABC = 5; Hello::ABC = (int)(VALS.B); }","NAMESPACE TEST & Prototype"));
            t.Add(new Test("namespace MYNS;struct XA{int a;int b;}; struct XA x;  entry void main(struct XA b){byte a = 8;x.b = b.b; }", "SIMPLE STRUCT TEST"));
            t.Add(new Test( "entry void main(){uint a; uint b;uint c;  a = a / b;a = a % b; a = a * b; a = a + c; a = a- c; a = a ^ c;}","BASIC ARITHMETIC TEST"));
            t.Add(new Test("entry void main(){bool a=true; bool b=true; int x; int y; a = a || b; a = a&&b;  x = x|y; x = x^y; x = x&y; b = x > y; y = x >> 3; }","BITWISE AND BOOLEAN TEST"));
            t.Add(new Test("entry void main(){bool a=true; bool b=true;int x;int y;  b = !a; y = ~x;}","BOOL TEST 2"));
            t.Add(new Test("struct XA{int a;int b;}; typedef bool BOOL; entry void main(){byte x = 8; uint y = 52;sbyte a = 7; x = (byte)y; y =(uint)x; y = sizeof(@BOOL); }", "CAST & SIZEOF TEST"));
            t.Add(new Test("int ABC = 7; entry void main(){ uint y = 0;  AX=  BX-DX; AX = AX + CX; AX = SI ^DI; BX = $CX && £DX; CS++; DS--;}","REGISTER OPERATION TEST"));
            t.Add(new Test("entry void main(){int i = 1; int j = 2; bool b;}", "BASIC 2"));
            t.Add(new Test("entry void main(){ uint x = 7u; bool b; b = true && false; asm { pushad; mov AX,\"[0x10]\";  }}","ASM STATEMENT TEST"));
            t.Add(new Test("entry void main(){ bool b = true; bool c; b = true != b ; int x = 8; x *= 4; c<>b;}", "SWAP TEST"));
            t.Add(new Test("entry void main(){ int x = 78i; x = (7+(8)) + 4 + (x + 3) - (x / 3);}", "PARENTHESIS EXPRESSION TEST"));
            t.Add(new Test("entry void main(){int k ;bool x;  while(x == true) {k++; if(k < 5) next; else if( k > 5) break;  }}","BASIC IF-WHILE TEST"));
            t.Add(new Test("entry void main(){int k ;switch(k) {case 0: k++; break; case 1: k--; break; default: k = 1i; }}","SWITCH TEST"));
            t.Add(new Test("struct KAE{int a; int b;};entry void main(const int v){int k ;} \n int Test(struct KAE k, int v,int i){struct KAE d;\n k.b = v; d.a = v;} \nvoid KEL(){int a[5];} ","STRUCT ACCESS TEST"));
            t.Add(new Test("namespace MYNS; use STD; struct ZA { int mp; int hj;}; struct YA {int o; byte h;  struct ZA z;}; struct XA {int v;struct YA l;};   struct VALS { int a; int b;struct XA j; int k; };  entry void main(){struct VALS x; x.a = 7i; x.j.l.o = 5i; x.j.l.h = 6b; x.j.l.z.mp = 7i; }","HARD-STRUCT ACCESS STAT TEST VAR"));
            t.Add(new Test("namespace HELLO; int ABC; struct XC{int b;}; namespace TYPES; use HELLO; typedef struct XC MYXC; namespace MYNS; use STD;use TYPES; struct ZA { int mp; int hj; @MYXC _xc;}; struct YA {int o; byte h;  struct ZA z;}; struct XA {int v;struct YA l;};   struct VALS { int a; int b;struct XA j; int k; };  struct VALS x;@MYXC v; entry void main(){x.j.l.z._xc.b = x.k; }","STAT FIELD"));
            t.Add(new Test("namespace HELLO; int ABC; struct XC{int b;}; namespace TYPES; use HELLO; typedef struct XC MYXC; namespace MYNS; use STD;use TYPES; struct ZA { int mp; int hj; @MYXC _xc;}; struct YA {int o; byte h;  struct ZA z;}; struct XA {int v;struct YA l;};   struct VALS { int a; int b;struct XA j; int k; }; struct GH {int a;int b; @MYXC c;};  @MYXC v; \nentry void main(struct VALS x){x.j.l.z._xc.b = x.k; /*x.c.b = 7;*/ } \n struct VALS x;struct VALS y; void method() { main(x); }","STAT PARAM"));
            t.Add(new Test("const byte KELL[] = 65534585548a; int ABC = 0; entry void main(int *h){int k[3];int *v;int d; /*byte** K = \"HEL\";*/ k[1] = 3; *v = 3;}","ARRAY LITERAL TEST"));
            t.Add(new Test("entry void main(int* a){int **b; int c; c = (*a);}","POINTER TEST"));
            t.Add(new Test("entry void main(){int i = 0;  loop { i++; } switch(i) {case 1:  break; case 2:  goto case 1; case 3: goto case 2; case 4: goto default; case 5: break; \ndefault:  }}","LOOP-GOTO [CASE/DEFAULT] TEST"));
            Test k;
            foreach (Test ts in t)
            {
                k = ts;
                RunTest(ref k);
            }



            Console.Read();

        }
    }
}
