﻿using bsn.GoldParser.Semantic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Vasm;
using Vasm.x86;
namespace VTC.Core
{
 
    public class Preprocessor
    {

        public static Stack<Location> PreprocessorStack = new Stack<Location>();
      
         
        static Preprocessor()
        {
           Symbols.Add("DATE", DateTime.Now.ToString());
           Symbols.Add("USER", Environment.UserName);
           Symbols.Add("VERSION", Assembly.GetExecutingAssembly().GetName().Version.ToString());
           Symbols.Add("OSVERSION",Environment.OSVersion.ToString());
           Symbols.Add("COMPILER","Arsslensoft Vatu Compiler");
    
        }
        public static Dictionary<string, object> Symbols = new Dictionary<string, object>();
        public static Dictionary<string, object> TempSymbols = new Dictionary<string, object>();
        public static void DefineSymbol(string name, object val)
        {
            if (!Symbols.ContainsKey(name))
                Symbols.Add(name, val);
            else ResolveContext.Report.Error(0, Location.Null, "Duplicate preprocessor symbol definition");
        }
        public static void UndefineSymbol(string name)
        {
            if (Symbols.ContainsKey(name))
                Symbols.Remove(name);
            else ResolveContext.Report.Error(0, Location.Null, "Preprocessor symbol doesn't exist");
        }
        public static object IsDefined(string name)
        {
            return Symbols.ContainsKey(name);
        }
        public static object GetDefine(string name)
        {
            return Symbols[name];
        }
        static Regex lnregex = new Regex("\\s*#line\\s*\"(?<include>.*?)\"\\s*(?<line>[0-9]+)|\\s*#include\\s*([<\"])(?<include>.*?)([>\"])");
        static  Regex incregex = new Regex("");

     public static string FixInclude(string file,string currentdir)
        {
            if (!Directory.Exists(currentdir))
                currentdir = "";
            string tryout = Path.GetFullPath(Path.Combine(currentdir, file));
            if (File.Exists(tryout))
                return tryout;
          foreach (string path in Paths)
          {
               tryout = Path.Combine(path, file);
              if (File.Exists(tryout))
                  return tryout;
          }
          return file;
      }
     internal static List<string> Paths = new List<string>();

      static List<string> pfiles = new List<string>();
    
        public static string GetLine(int line, string file)
        {
            string ln = "";
            if (!File.Exists(file))
            {
                ResolveContext.Report.Error(0, Location.Null, "Preprocessor file not found "+file);
                return "";
            }
            using (StreamReader st = new StreamReader(file))
            {
                while (line > 0)
                {
                    ln = st.ReadLine();
            
                    
                        line--;
                }
            }
            return ln;
        }
        public static StreamReader GetInclude(string file)
        {
            if (!File.Exists(file))
            {
                ResolveContext.Report.Error(0, Location.Null, "Preprocessor file not found " + file);
                return null;
            }
            else return new StreamReader(file);
        }
    }
    public class DiagnosticPreprocessor : PreprocessorDeclaration
    {
  
        [Rule(@"<PP Diag>     ::= ~'#'warn StringLiteral")]
        [Rule(@"<PP Diag>     ::= ~'#'error StringLiteral")]
      public DiagnosticPreprocessor(SimpleToken tok, StringLiteral msg)
      {
            if(tok.Name == "error")
          ResolveContext.Report.Error(0, Location, msg.Value.GetValue().ToString());
            else ResolveContext.Report.Warning( Location, msg.Value.GetValue().ToString());
      }

     
    }
    

  public class RegionPreprocessor : PreprocessorDeclaration
  {
   
      [Rule(@"<PP Region>    ::= ~'#'~region StringLiteral")]
      public RegionPreprocessor(StringLiteral namel)
      {
      
      }

      [Rule(@"<PP ERegion>    ::=  ~'#'~endregion")]
      public RegionPreprocessor()
      {
      
      }
      public override bool Preprocess(CompilerContext ctx, ref DeclarationSequence<Declaration> decl)
      {
       
          return true;
      }



  }
  public class IncludePreprocessor : PreprocessorDeclaration
  {
     
      StringLiteral id;
      public override bool Preprocess(CompilerContext ctx,ref DeclarationSequence<Declaration> decl)
      {
          DependencyParsing dep = new DependencyParsing();
          dep.File = id.Value.GetValue().ToString();
          
          dep.File = Preprocessor.FixInclude(dep.File,Path.GetDirectoryName(ResolveContext.Report.FilePath));
          string path = Path.GetDirectoryName(dep.File);
          if (!Preprocessor.Paths.Contains(path))
              Preprocessor.Paths.Add(path);
          if (ctx.InputSources.Contains(dep))
          {
              dep = ctx.InputSources[ctx.InputSources.IndexOf(dep)];
              return ctx.ResolveDependency(ctx.InputSources.IndexOf(dep));
          }
          else
          {
              dep.InputStream = new StreamReader(File.OpenRead(dep.File));
              ctx.InputSources.Add(dep);
              return ctx.ResolveDependency(ctx.InputSources.Count -1);
          }
          return false;
      }
      [Rule(@"<PP Include>    ::= ~'#'~include StringLiteral")]
      public IncludePreprocessor(StringLiteral src)
      {

          id = src;
      }
    


  }
  public class DefinePreprocessor : PreprocessorDeclaration
  {
      PreprocessorExpr ppexpr;
      Identifier id;
      public override bool Preprocess(CompilerContext ctx, ref DeclarationSequence<Declaration> decl)
      {
          if (ppexpr == null)
              Preprocessor.UndefineSymbol(id.Name);
          else
          {
              object val = ppexpr.Evaluate();
              Preprocessor.DefineSymbol(id.Name, val);
          }
          return true;
      }
      [Rule(@"<PP Define>    ::= ~'#'~define Id <PP Expr>")]
      public DefinePreprocessor(Identifier id, PreprocessorExpr expr)
      {

          object val = expr.Evaluate();
          Preprocessor.TempSymbols.Add(id.Name, val);

          this.id = id;
          ppexpr = expr;
      }
      [Rule(@"<PP Define>     ::= ~'#'~undef Id")]
      public DefinePreprocessor(Identifier id)
      {
          this.id = id;
       
      }


  }
  enum CondtionalPP
  {
      If,
      IfElif,
      IfElifElse,
      IfElse,
      None
  }
  public class ConditionalPreprocessor : PreprocessorDeclaration
  {
      ConditionalPreprocessor CPP;
    public  DeclarationSequence<Declaration> Decls;
     public PreprocessorExpr PPExpr;
      ElifSequence<ConditionalPreprocessor> Elifs;
      DeclarationSequence<Declaration> Else;
      CondtionalPP _cond = CondtionalPP.None;
      [Rule(@"<PP If>    ::= ~'#'if <PP Expr> ~'{' <Decls>  ~'}' <PP Elif List>")]
      public ConditionalPreprocessor(SimpleToken tok, PreprocessorExpr expr,DeclarationSequence<Declaration> DECL, ElifSequence<ConditionalPreprocessor> cpp)
      {
          Decls = DECL;
          PPExpr = expr;
          _cond = CondtionalPP.IfElif;
          Elifs = cpp;
      }

      [Rule(@"<PP If>    ::= ~'#'if <PP Expr> ~'{' <Decls> ~'}' <PP Elif List> ~else ~'{' <Decls>  ~'}'         ")]
      public ConditionalPreprocessor(SimpleToken tok, PreprocessorExpr expr, DeclarationSequence<Declaration> DECL, ElifSequence<ConditionalPreprocessor> cpp, DeclarationSequence<Declaration> els)
      {
          Decls = DECL;
          PPExpr = expr;
          Else = els;
          Elifs = cpp;
          _cond = CondtionalPP.IfElifElse;
      }

      [Rule(@"<PP If>    ::= ~'#'if <PP Expr> ~'{' <Decls> ~'}' ~else ~'{' <Decls>  ~'}'     ")]
      public ConditionalPreprocessor(SimpleToken tok, PreprocessorExpr expr, DeclarationSequence<Declaration> DECL,  DeclarationSequence<Declaration> els)
      {
          Decls = DECL;
          PPExpr = expr;
          Else = els;
          _cond = CondtionalPP.IfElse;
      }

      [Rule(@"<PP Elif>    ::= elif <PP Expr> ~'{' <Decls> ~'}'")]
      [Rule(@"<PP If>    ::= ~'#'if <PP Expr> ~'{' <Decls>  ~'}'   ")]
      public ConditionalPreprocessor(SimpleToken tok, PreprocessorExpr expr, DeclarationSequence<Declaration> DECL)
      {
          Decls = DECL;
          PPExpr = expr;
          if(tok.Name == "if")
             _cond = CondtionalPP.If;
      }
         


            
   
    
   

     [Rule(@"<PP Conditional> ::= <PP If>")]
     public ConditionalPreprocessor(ConditionalPreprocessor tok)
     {
         CPP = tok;
     }


     public override bool Preprocess(CompilerContext ctx,ref DeclarationSequence<Declaration> decl)
     {
         if (CPP != null)
             return CPP.Preprocess(ctx,ref decl);
         else if (PPExpr != null)
         {
             object eval = PPExpr.Evaluate();
             if (!(eval is bool))
                 return false;

             bool v = (bool)eval;
             if (_cond != CondtionalPP.None)
             {
               
                 // if
                 if (_cond == CondtionalPP.If && v)
                     decl = Decls;
                 else if (_cond == CondtionalPP.IfElse)
                     decl = v ? Decls : Else;
                 else if (_cond == CondtionalPP.IfElif)
                 {
                     if (v) decl = Decls;
                     else
                     {

                         foreach (ConditionalPreprocessor cp in Elifs)
                         {
                             if (cp != null)
                             {
                                 eval = cp.PPExpr.Evaluate();
                                 if (!(eval is bool))
                                     continue;

                                 v = (bool)eval;
                                 if (v)
                                 {
                                     decl = Decls;
                                     break;
                                 }

                             }
                         }
                     }

                 }
                 else if (_cond == CondtionalPP.IfElifElse)
                 {
                     if (v) decl = Decls;
                     else
                     {
                         decl = null;
                         foreach (ConditionalPreprocessor cp in Elifs)
                         {
                             if (cp != null)
                             {
                                 eval = cp.PPExpr.Evaluate();
                                 if (!(eval is bool))
                                     continue;

                                 v = (bool)eval;
                                 if (v)
                                 {
                                     decl = Decls;
                                     break;
                                 }

                             }
                         }

                         if (decl == null)
                                     decl = Else;
                             
                    
                     }

                 }
             }
       
         }
         return true;
     }

  }
  public class PreprocessorDeclaration : Declaration
  {
      public virtual bool Preprocess(CompilerContext ctx, ref DeclarationSequence<Declaration> decl)
      {
          if(PPDecl != null)
                 return PPDecl.Preprocess(ctx,ref decl);
          return false;
      }

      public PreprocessorDeclaration() { }

      public PreprocessorDeclaration PPDecl { get; set; }

      [Rule(@"<Preproc Decl> ::= <PP Define> ")]
      [Rule(@"<Preproc Decl> ::= <PP Conditional> ")]
      [Rule(@"<Preproc Decl> ::= <PP Diag> ")]
      [Rule(@"<Preproc Decl> ::= <PP Region> ")]
      [Rule(@"<Preproc Decl> ::= <PP Include> ")]
      public PreprocessorDeclaration(PreprocessorDeclaration decl)
      {
          PPDecl = decl;
          PPDecl.position = position;
      }
  }

  public class PreprocessorExpr : SimpleToken
  {
      
      public virtual object Evaluate()
      {
          return null;
      }
      public virtual bool Preprocess()
      {
          return true;
      }
      public PreprocessorExpr()
      {

      }
  }
  public class DefinitionExpr : PrimaryExpressionPreprocessor
  {
      string id;
      public override object Evaluate()
      {
          if (Preprocessor.Symbols.ContainsKey(id))
              return Preprocessor.Symbols[id];
          else ResolveContext.Report.Error(0, Location, "Unresolved preprocessor symbol");

          return null;
      }
        [Rule(@"<PP Primary Expr> ::= Id ")]
      public DefinitionExpr(Identifier expr)
        {
            id = expr.Name;
        }
  }
  public class PrimaryExpressionPreprocessor : PreprocessorExpr
  {
      Literal ce;
      PreprocessorExpr pexpr;

      public override object Evaluate()
      {
          if (ce != null)
          {
              if (ce is DecLiteral || ce is HexLiteral || ce is OctLiteral || ce is BinaryLiteral)
                  return Convert.ToDecimal(ce.Value.GetValue().ToString());
              else if (ce is StringLiteral)
                  return Convert.ToString(ce.Value.GetValue().ToString());
              else if (ce is BooleanLiteral)
                  return Convert.ToBoolean(ce.Value.GetValue().ToString());
              else if (ce is MacroLiteral)
              {
                  if (ce.Value is UIntConstant)
                      return Convert.ToDecimal(ce.Value.GetValue().ToString());
                  else if (ce.Value is StringConstant)
                      return Convert.ToString(ce.Value.GetValue().ToString());
                  else if (ce.Value is BoolConstant)
                      return Convert.ToBoolean(ce.Value.GetValue().ToString());
              }
              else
              {
                  ResolveContext.Report.Error(0, Location, "Preprocessor expression error only strings,numbers and boolean are allowed");
                  return null;

              }
          }
          return pexpr.Evaluate();
      }
      public PrimaryExpressionPreprocessor() { }
          [Rule(@"<PP Primary Expr> ::= <CONSTANT> ")]
      public PrimaryExpressionPreprocessor(Literal expr)
      {
          ce = expr;
      }

        [Rule(@"<PP Primary Expr> ::= ~'(' <PP Expr> ~')' ")]
          public PrimaryExpressionPreprocessor(PreprocessorExpr expr)
          {
              pexpr = expr;
          }
      
       
  }
  public class UnaryExpressionPreprocessor : PreprocessorExpr
  {
      PrimaryExpressionPreprocessor pep;
      PreprocessorExpr right;
      public object GetValue()
      {

          object lStart = right.Evaluate();
       

          object lFinal;
   

          // this attempts to determine the Greatest Common Type
          PreprocessorTypeChecker.GreatestCommonType gct = PreprocessorTypeChecker.GCT(lStart, lStart);

          // convert the values to the Greatest Common Type
          switch (gct)
          {
         
              case PreprocessorTypeChecker.GreatestCommonType.BooleanType:
                  lFinal = Convert.ToBoolean(lStart);
        
                  break;
      
              default:
                  throw new ArgumentOutOfRangeException();
          }

          // execute on the converted values
          return !(bool)(lFinal);


      }
      public override object Evaluate()
      {
          if (right != null)
              return GetValue();
          return pep.Evaluate();
      }
      [Rule(@"<PP Unary Expr> ::= ~'!'    <PP Unary Expr> ")]
      public UnaryExpressionPreprocessor(PreprocessorExpr expr)
      {
          right = expr;
      }

      [Rule(@"<PP Unary Expr> ::=  <PP Primary Expr>")]
      public UnaryExpressionPreprocessor(PrimaryExpressionPreprocessor expr)
      {
          pep = expr;
      }
  }
  static class PreprocessorTypeChecker
  {
      public static bool IsBoolean(object obj)
      {
          return (Type.GetTypeCode(obj.GetType()) == TypeCode.Boolean);
      }

      public static bool IsNumeric(object obj)
      {
          switch (Type.GetTypeCode(obj.GetType()))
          {
              case TypeCode.Empty:
              case TypeCode.Object:
              case TypeCode.DBNull:
              case TypeCode.Boolean:
              case TypeCode.Char:
              case TypeCode.SByte:
              case TypeCode.Byte:
                  return false;
              case TypeCode.Int16:
              case TypeCode.UInt16:
              case TypeCode.Int32:
              case TypeCode.UInt32:
              case TypeCode.Int64:
              case TypeCode.UInt64:
              case TypeCode.Single:
              case TypeCode.Double:
              case TypeCode.Decimal:
                  return true;
              case TypeCode.DateTime:
              case TypeCode.String:
                  return false;
              default:
                  throw new ArgumentOutOfRangeException();
          }
      }

      public enum GreatestCommonType
      {
          StringType,
          BooleanType,
          NumericType
      }

      public static GreatestCommonType GCT(params object[] toCheck)
      {
          int boolCount = 0;
          int stringCount = 0;
          int numCount = 0;

          foreach (var obj in toCheck)
          {
              if (IsBoolean(obj))
                  boolCount++;
              else if (IsNumeric(obj))
                  numCount++;
              else
              {
                  stringCount++;
              }
          }

          if (boolCount == toCheck.Length)
          {
              return GreatestCommonType.BooleanType;
          }

          if (numCount == toCheck.Length)
          {
              return GreatestCommonType.NumericType;
          }

          if (numCount > 0 && boolCount == 0)
          {
              return GreatestCommonType.NumericType;
          }

          return GreatestCommonType.StringType;

      }

  }
  public class BinaryExpressionPreprocessor : PreprocessorExpr
  {
      PreprocessorExpr expr;
     PreprocessorExpr _left;
     BinaryOp _op;
     PreprocessorExpr _right;



      [Rule(@"<PP Compare Expr> ::= <PP Compare Expr> '==' <PP Unary Expr> ")]
      [Rule(@"<PP Compare Expr> ::= <PP Compare Expr> '!=' <PP Unary Expr> ")]
      [Rule(@"<PP And Expr>::= <PP And Expr>    '&&' <PP Compare Expr>")]
      [Rule(@"<PP Or Expr> ::= <PP Or Expr> '||' <PP And Expr>  ")]
      public BinaryExpressionPreprocessor(PreprocessorExpr left, BinaryOp op, PreprocessorExpr right)
      {
          this._left = left;
          this._op = op;
          this._right = right;
      }

      [Rule(@"<PP Expr> ::= <PP Or Expr>")]
      public BinaryExpressionPreprocessor(PreprocessorExpr expr)
      {
          this.expr = expr;
      }


      public object GetValue()
      {

          object lStart = _left.Evaluate();
          object rStart = _right.Evaluate();

          object lFinal;
          object rFinal;

          // this attempts to determine the Greatest Common Type
          PreprocessorTypeChecker.GreatestCommonType gct = PreprocessorTypeChecker.GCT(lStart, rStart);

          // convert the values to the Greatest Common Type
          switch (gct)
          {
              case PreprocessorTypeChecker.GreatestCommonType.StringType:
                  lFinal = Convert.ToString(lStart);
                  rFinal = Convert.ToString(rStart);
                  break;
              case PreprocessorTypeChecker.GreatestCommonType.BooleanType:
                  lFinal = Convert.ToBoolean(lStart);
                  rFinal = Convert.ToBoolean(rStart);
                  break;
              case PreprocessorTypeChecker.GreatestCommonType.NumericType:
                  lFinal = Convert.ToDecimal(lStart);
                  rFinal = Convert.ToDecimal(rStart);
                  break;
              default:
                  throw new ArgumentOutOfRangeException();
          }

          // execute on the converted values
          if (lFinal is bool && rFinal is bool)
              return EvaluateOper((bool)lFinal, (bool)rFinal);
          else if (lFinal is string && rFinal is string)
              return EvaluateOper((string)lFinal, (string)rFinal);
          else
              return EvaluateOper((Decimal)lFinal, (Decimal)rFinal);

      
      }
      bool EvaluateOper(string a, string b)
      {
          if (_op.Operator == BinaryOperator.Equality)
              return a == b;
          else if (_op.Operator == BinaryOperator.Inequality)
              return a != b;

          return false;
      }
      bool EvaluateOper(Decimal a,Decimal b)
      {
          if (_op.Operator == BinaryOperator.Equality)
              return a == b;
          else if (_op.Operator == BinaryOperator.Inequality)
              return a != b;

          return false;
      }
      bool EvaluateOper(bool a, bool b)
      {
          if (_op.Operator == BinaryOperator.LogicalAnd)
              return a && b;
          else if (_op.Operator == BinaryOperator.LogicalOr)
              return a || b;
         else if (_op.Operator == BinaryOperator.Equality)
              return a == b;
          else if (_op.Operator == BinaryOperator.Inequality)
              return a != b;
          return false;
      }


      public override object Evaluate()
      {
          if (expr != null)
              return expr.Evaluate();

          return GetValue();
      }
  }

}
