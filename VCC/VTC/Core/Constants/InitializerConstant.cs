using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using VTC.Base.GoldParser.Semantic;

namespace VTC.Core
{
  public  class InitializerConstant : Expr
    {
      public int Size { get; set; }

   internal   InitializerSequence _initseq = null;
      [Rule("<Initializer> ::= ~'{' <Initializers>  ~'}'   ")]
      public InitializerConstant(InitializerSequence seq){
          _initseq = seq;
      }
      public override SimpleToken DoResolve(ResolveContext rc)
      {
          _initseq = (InitializerSequence)_initseq.DoResolve(rc);
          
          Type = new ArrayTypeSpec(Namespace.Default, _initseq.Expressions[0].Type, _initseq.Expressions.Count);
          Size = _initseq.Expressions.Count;

          return this;
      }
      public bool MatchType(TypeSpec ts)
      {
          if (!ts.IsArray || !ts.Equals(Type))
          {
              ResolveContext.Report.Error(0, Location, "Initializers type mismatch");
              return false;

          }

          ArrayTypeSpec arr = (ts as ArrayTypeSpec);


          if (Size != arr.ArrayCount)
          {
              ResolveContext.Report.Error(0, Location, "Initializers size mismatch");
              return false;
          }



          return arr.BaseType.Equals(this.type.BaseType);
      }
      public DataMember GetData(EmitContext ec,string name)
      {
          if (type.BaseType.Equals(BuiltinTypeSpec.String))
          {
              List<string> lb = new List<string>();
              foreach (StringConstant str in _initseq.Expressions)
              {
                  DataMember dm = new DataMember(str.ConstVar.Signature.ToString(), (string)str.GetValue(), true, str.Verbatim);
                  ec.EmitData(dm, str.ConstVar, true);
                  lb.Add(str.ConstVar.Signature.ToString());

              }

              return new DataMember(name, lb.ToArray());
          }
          else
          {
              if (type.BaseType.Size == 2)
              {
                  List<ushort> us = new List<ushort>();
                  foreach (ConstantExpression str in _initseq.Expressions)
                  {
                      if (type.BaseType.IsSigned)
                          us.Add((ushort)((short)str.GetValue()));
                      else us.Add((ushort)str.GetValue());
                  }

                  return new DataMember(name, us.ToArray());
              }
              else if (type.BaseType.Size == 1)
              {

                  List<byte> us = new List<byte>();

                  foreach (ConstantExpression str in _initseq.Expressions)
                  {
                      if (type.BaseType.IsSigned)
                              us.Add((byte)((sbyte)str.GetValue()));
                      else us.Add((byte)str.GetValue());
                  }

                  return new DataMember(name, us.ToArray());
              }
              else if (type.BaseType.IsFloat && !type.BaseType.IsPointer)
              {
                  List<byte> us = new List<byte>();
                  foreach (ConstantExpression str in _initseq.Expressions)
                      us.AddRange(BitConverter.GetBytes((float)str.GetValue()));

                  return new DataMember(name, us.ToArray());
              }
              else return null;
          }
         
      }
    }
}
