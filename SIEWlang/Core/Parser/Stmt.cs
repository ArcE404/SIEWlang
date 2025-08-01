using SIEWlang.Core.Lexer;
namespace SIEWlang.Core.Parser;

// ***************************************************
// This code is autogenerated by the GenerateAst Tool.
// ***************************************************

public abstract class Stmt{
    public abstract T Accept<T>(IVisitor<T> visitor);

    public interface IVisitor<R>
    {
        R VisitBlockStmt(Block stmt);
        R VisitClassStmt(Class stmt);
        R VisitExpressionStmt(Expression stmt);
        R VisitFunctionStmt(Function stmt);
        R VisitIfStmt(If stmt);
        R VisitPrintStmt(Print stmt);
        R VisitReturnStmt(Return stmt);
        R VisitVarStmt(Var stmt);
        R VisitWhileStmt(While stmt);
    }

   public class Block : Stmt
   {
        public List<Stmt> Statements { get; }

        public Block(List<Stmt> Statements)
        {
            this.Statements = Statements;
        }

        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitBlockStmt(this);
        }
   }

   public class Class : Stmt
   {
        public Token Name { get; }
        public List<Stmt.Function> Methods { get; }

        public Class(Token Name, List<Stmt.Function> Methods)
        {
            this.Name = Name;
            this.Methods = Methods;
        }

        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitClassStmt(this);
        }
   }

   public class Expression : Stmt
   {
        public Expr expression { get; }

        public Expression(Expr expression)
        {
            this.expression = expression;
        }

        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitExpressionStmt(this);
        }
   }

   public class Function : Stmt
   {
        public Token Name { get; }
        public List<Token> Params { get; }
        public List<Stmt> Body { get; }

        public Function(Token Name, List<Token> Params, List<Stmt> Body)
        {
            this.Name = Name;
            this.Params = Params;
            this.Body = Body;
        }

        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitFunctionStmt(this);
        }
   }

   public class If : Stmt
   {
        public Expr Condition { get; }
        public Stmt ThenBranch { get; }
        public Stmt? ElseBranch { get; }

        public If(Expr Condition, Stmt ThenBranch, Stmt? ElseBranch)
        {
            this.Condition = Condition;
            this.ThenBranch = ThenBranch;
            this.ElseBranch = ElseBranch;
        }

        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitIfStmt(this);
        }
   }

   public class Print : Stmt
   {
        public Token PrintType { get; }
        public Expr Expression { get; }

        public Print(Token PrintType, Expr Expression)
        {
            this.PrintType = PrintType;
            this.Expression = Expression;
        }

        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitPrintStmt(this);
        }
   }

   public class Return : Stmt
   {
        public Token Keyword { get; }
        public Expr? Value { get; }

        public Return(Token Keyword, Expr? Value)
        {
            this.Keyword = Keyword;
            this.Value = Value;
        }

        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitReturnStmt(this);
        }
   }

   public class Var : Stmt
   {
        public Token Name { get; }
        public Expr Initializer { get; }

        public Var(Token Name, Expr Initializer)
        {
            this.Name = Name;
            this.Initializer = Initializer;
        }

        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitVarStmt(this);
        }
   }

   public class While : Stmt
   {
        public Expr Condition { get; }
        public Stmt Body { get; }

        public While(Expr Condition, Stmt Body)
        {
            this.Condition = Condition;
            this.Body = Body;
        }

        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitWhileStmt(this);
        }
   }

}
