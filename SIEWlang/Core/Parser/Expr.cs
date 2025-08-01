using SIEWlang.Core.Lexer;
namespace SIEWlang.Core.Parser;

// ***************************************************
// This code is autogenerated by the GenerateAst Tool.
// ***************************************************

public abstract class Expr{
    public abstract T Accept<T>(IVisitor<T> visitor);

    public interface IVisitor<R>
    {
        R VisitAssignExpr(Assign expr);
        R VisitBinaryExpr(Binary expr);
        R VisitCallExpr(Call expr);
        R VisitGroupingExpr(Grouping expr);
        R VisitLiteralExpr(Literal expr);
        R VisitLogicalExpr(Logical expr);
        R VisitUnaryExpr(Unary expr);
        R VisitVariableExpr(Variable expr);
    }

   public class Assign : Expr
   {
        public Token Name { get; }
        public Expr Value { get; }

        public Assign(Token Name, Expr Value)
        {
            this.Name = Name;
            this.Value = Value;
        }

        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitAssignExpr(this);
        }
   }

   public class Binary : Expr
   {
        public Expr Left { get; }
        public Token Operator { get; }
        public Expr Right { get; }

        public Binary(Expr Left, Token Operator, Expr Right)
        {
            this.Left = Left;
            this.Operator = Operator;
            this.Right = Right;
        }

        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitBinaryExpr(this);
        }
   }

   public class Call : Expr
   {
        public Expr Callee { get; }
        public Token Paren { get; }
        public List<Expr> Arguments { get; }

        public Call(Expr Callee, Token Paren, List<Expr> Arguments)
        {
            this.Callee = Callee;
            this.Paren = Paren;
            this.Arguments = Arguments;
        }

        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitCallExpr(this);
        }
   }

   public class Grouping : Expr
   {
        public Expr Expression { get; }

        public Grouping(Expr Expression)
        {
            this.Expression = Expression;
        }

        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitGroupingExpr(this);
        }
   }

   public class Literal : Expr
   {
        public Object Value { get; }

        public Literal(Object Value)
        {
            this.Value = Value;
        }

        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitLiteralExpr(this);
        }
   }

   public class Logical : Expr
   {
        public Expr Left { get; }
        public Token Operator { get; }
        public Expr Right { get; }

        public Logical(Expr Left, Token Operator, Expr Right)
        {
            this.Left = Left;
            this.Operator = Operator;
            this.Right = Right;
        }

        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitLogicalExpr(this);
        }
   }

   public class Unary : Expr
   {
        public Token Operator { get; }
        public Expr Right { get; }

        public Unary(Token Operator, Expr Right)
        {
            this.Operator = Operator;
            this.Right = Right;
        }

        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitUnaryExpr(this);
        }
   }

   public class Variable : Expr
   {
        public Token Name { get; }

        public Variable(Token Name)
        {
            this.Name = Name;
        }

        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitVariableExpr(this);
        }
   }

}
