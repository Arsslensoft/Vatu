using VTC.Base.GoldParser.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vasm;

namespace VTC.Core
{

    // a.b | a->b | a[5]
    /// <summary>
    /// Access Op
    /// </summary>
    public class AccessOperation : Expr
    {

        public int Offset { get; set; }
        public MemberSpec Member { get; set; }
        private AccessOp _op;



        [Rule(@"<Op Pointer> ::= <Op Pointer> '.' <Declared Expression>")]
        [Rule(@"<Op Pointer> ::= <Op Pointer> '->' <Declared Expression>")]
        public AccessOperation(Expr left, AccessOp op, Expr target)
        {
            _op = op;


            _op.Left = left;
            _op.Right = target;


        }

        [Rule(@"<Op Pointer> ::= <Op Pointer> '.' <VALUE POS>")]
        public AccessOperation(Expr left, AccessOp op, ValuePosIdentifier target)
        {
            _op = op;
            _op.Left = left;
            _op.Right = target;


        }

        [Rule(@"<Op Pointer> ::= <Name> '::' <Declared Expression>")]
        public AccessOperation(NameIdentifier id, AccessOp op, Expr target)
        {
            _op = op;
            _op.Namespace = new Namespace(id.Name);
            _op.Right = target;


        }


        [Rule(@"<Op Pointer> ::= <Type> '::' <Declared Expression>")]
        public AccessOperation(TypeToken id, AccessOp op, Expr target)
        {
            _op = op;
            _op.LeftType = id;
            _op.Right = target;


        }

        [Rule(@"<Op Pointer> ::= <Op Pointer> ~'[' <Expression> ~']'")]
        public AccessOperation(Expr left, Expr target)
        {
            _op = new ByIndexOperator();
            _op.Left = left;
            _op.Right = target;


        }


       public override bool Resolve(ResolveContext rc)
        {
            if (_op._op != AccessOperator.ByName)
            {
                bool ok = _op.Left.Resolve(rc);
                ok &= _op.Right.Resolve(rc);
                return ok;
            }
            else return _op.Right.Resolve(rc);

        }
 public override SimpleToken DoResolve(ResolveContext rc)
        {
           
            AcceptStatement = (_op._op == AccessOperator.ByName);
            if (_op._op != AccessOperator.ByName)
            {

                if ((_op.Right is DeclaredExpression) && (_op.Right  as DeclaredExpression).Expression is MethodExpression)
                {
                    _op.Left = (Expr)_op.Left.DoResolve(rc);
                    // back up 
                    rc.CreateNewState();
                    rc.CurrentGlobalScope |= ResolveScopes.MethodExtensionAccess;

                    if(_op is ByValueOperator)
                    rc.CurrentExtensionLookup = _op.Left.Type;
                    else rc.CurrentExtensionLookup = _op.Left.Type.BaseType;

                    rc.StaticExtensionLookup = false;
                    rc.ExtensionVar = _op.Left;

                    _op.Right = (Expr)_op.Right.DoResolve(rc);

                    // restore
                    rc.RestoreOldState();
                    return _op.Right;
                }
                else
                {
                    rc.CreateNewState();
                    rc.CurrentGlobalScope |= ResolveScopes.VariableExtensionAccess; // disable false variable resolve errors
                    _op.Right = (Expr)_op.Right.DoResolve(rc);
                    _op.Left = (Expr)_op.Left.DoResolve(rc);
                    rc.RestoreOldState();

                    SimpleToken s = _op.DoResolve(rc);
                    
                    return s;
                }
            }
            else
            {
                SimpleToken s = _op.DoResolve(rc);
               
                return s;
            }


        }

        public override FlowState DoFlowAnalysis(FlowAnalysisContext fc)
        {
            if (_op._op != AccessOperator.ByName)
            {
                FlowState ok = _op.Left.DoFlowAnalysis(fc);
                ok &= _op.Right.DoFlowAnalysis(fc);
                return _op.DoFlowAnalysis(fc);
            }
            else return _op.Right.DoFlowAnalysis(fc) & _op.DoFlowAnalysis(fc);
        }
       
        public override bool Emit(EmitContext ec)
        {
            return _op.Emit(ec);
        }
        public override bool EmitToStack(EmitContext ec)
        {


            return _op.EmitToStack(ec);
        }
        public override bool EmitFromStack(EmitContext ec)
        {

            return _op.EmitFromStack(ec);
        }
        public override bool EmitBranchable(EmitContext ec, Label truecase, bool v)
        {
            return _op.EmitBranchable(ec, truecase, v);
        }
    }
}
