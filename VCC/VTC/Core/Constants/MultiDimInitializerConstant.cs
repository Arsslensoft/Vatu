using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;
using VTC.Base.GoldParser.Semantic;

namespace VTC.Core
{
 
    public class MultiDimInitializerConstant : Expr
    {
        public int Size { get; set; }
        TypeSpec roottype = null;
        MultiDimInitializerSequence _initseq = null;
        [Rule("<NDim Initializer>    ::=  ~'{' <NDim Initializers>  ~'}'   ")]
        public MultiDimInitializerConstant(MultiDimInitializerSequence seq)
        {
            _initseq = seq;
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

            bool ok = true;
            
            foreach(Expr e in _initseq.Expressions)
            {
                if(e is InitializerConstant)
                    ok &= (e as InitializerConstant).MatchType(ts.BaseType);
                else   if(e is MultiDimInitializerConstant)
                    ok &= (e as MultiDimInitializerConstant).MatchType(ts.BaseType);
            }

            return arr.BaseType.Equals(this.type.BaseType) && ok;
        }
        public bool CollectDataReccursive(Expr exp, EmitContext ec , List<object> dat)
        {
            if (exp is InitializerConstant && (exp as InitializerConstant)._initseq.Expressions[0] is ConstantExpression)
            {
                if (roottype.Equals(BuiltinTypeSpec.String) && (exp as InitializerConstant)._initseq.Expressions[0] is StringConstant)
                {
                    foreach (StringConstant str in (exp as InitializerConstant)._initseq.Expressions)
                    {
                        DataMember dm = new DataMember(str.ConstVar.Signature.ToString(), (string)str.GetValue(), true, str.Verbatim);
                        ec.EmitData(dm, str.ConstVar, true);
                        dat.Add(str.ConstVar.Signature.ToString());

                    }
                    return true;
                }
                else
                {
                    if (roottype.Size == 2)
                    {

                        foreach (ConstantExpression str in (exp as InitializerConstant)._initseq.Expressions)
                        {
                            if (roottype.IsSigned)
                                dat.Add((ushort)((short)str.GetValue()));
                            else dat.Add((ushort)str.GetValue());
                        }

                        return true;
                    }
                    else if (roottype.Size == 1)
                    {

                        foreach (ConstantExpression str in (exp as InitializerConstant)._initseq.Expressions)
                        {
                            if (roottype.IsSigned)
                                dat.Add((byte)((sbyte)str.GetValue()));
                            else if (roottype.Equals(BuiltinTypeSpec.Bool))
                                dat.Add(((bool)str.GetValue()) ? (byte)1 : (byte)0);
                            else dat.Add((byte)str.GetValue());
                        }

                        return true;
                    }
                    else if (roottype.IsFloat && !roottype.IsPointer)
                    {
                        foreach (ConstantExpression str in (exp as InitializerConstant)._initseq.Expressions)
                        {
                            foreach (byte b in BitConverter.GetBytes((float)str.GetValue()))
                                dat.Add(b);
                        }
                      
                        return true;
                    }
                    else return false;
                }
            }
            else if (exp is InitializerConstant)
            {
                bool ok = true;
                foreach (Expr e in (exp as InitializerConstant)._initseq.Expressions)
                    ok &= CollectDataReccursive(e, ec,  dat);
                return ok;
            }
            else if (exp is MultiDimInitializerConstant)
            {
                bool ok = true;
                foreach (Expr e in (exp as MultiDimInitializerConstant)._initseq.Expressions)
                    ok &= CollectDataReccursive(e, ec, dat);
                return ok;
            }
            else return false;

        }
        public DataMember GetData(EmitContext ec, string name)
        {       
            List<object> lb = new List<object>();
            if (roottype.Equals(BuiltinTypeSpec.String))
            {
                
                bool ok = true;
                foreach (Expr e in _initseq.Expressions)
                    ok &= CollectDataReccursive(e, ec, lb);

                return new DataMember(name, lb.Cast<string>().ToArray());
            }
            else
            {
                if (roottype.Size == 2)
                {
              
             
                    bool ok = true;
                    foreach (Expr e in _initseq.Expressions)
                        ok &= CollectDataReccursive(e, ec, lb);


                    if (roottype.IsSigned)
                    return new DataMember(name, lb.Cast<short>().ToArray());
                  else return new DataMember(name, lb.Cast<ushort>().ToArray());
                  
                }
                else if (roottype.Size == 1)
                {


                    bool ok = true;
                    foreach (Expr e in _initseq.Expressions)
                        ok &= CollectDataReccursive(e, ec, lb);


                    if (roottype.IsSigned)
                        return new DataMember(name, lb.Cast<sbyte>().ToArray());
                    else return new DataMember(name, lb.Cast<byte>().ToArray());
                }
                else if (roottype.IsFloat && !roottype.IsPointer)
                {
                    bool ok = true;
                    foreach (Expr e in _initseq.Expressions)
                        ok &= CollectDataReccursive(e, ec, lb);


                    return new DataMember(name, lb.Cast<byte>().ToArray());
                }
                else return null;
            }

        }

        public override SimpleToken DoResolve(ResolveContext rc)
        {
            _initseq = (MultiDimInitializerSequence)_initseq.DoResolve(rc);
          
            Type = new ArrayTypeSpec(Namespace.Default, _initseq.Expressions[0].Type, _initseq.Expressions.Count);
            Type.GetBase(Type, ref roottype); 
            Size = _initseq.Expressions.Count;

            return this;
        }
    }
}
