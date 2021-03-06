﻿using ImmutableObjectGraph;
using ImmutableObjectGraph.CodeGeneration;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace LanguageService
{
    public abstract class SyntaxNodeOrToken
    {
        public virtual bool IsLeafNode => true;
        public virtual bool IsToken => true;
        public abstract ImmutableList<SyntaxNodeOrToken> Children { get; }
    }

    [GenerateImmutable(GenerateBuilder = true)]
    public abstract partial class SyntaxNode : SyntaxNodeOrToken
    {
        [Required]
        readonly SyntaxKind kind;
        [Required]
        readonly int startPosition;
        [Required]
        readonly int length;
        public override bool IsToken => false;
        public override bool IsLeafNode => this.Children.Count == 0;

        public IEnumerable<SyntaxNodeOrToken> Descendants()
        {
            if (this is SyntaxNode && ((SyntaxNode)this).Kind == SyntaxKind.ChunkNode)
            {
                yield return this;
            }

            foreach (var node in this.Children)
            {
                yield return node;

                var nodeAsSyntaxNode = node as SyntaxNode;

                if (nodeAsSyntaxNode != null)
                {
                    foreach (var nextNode in nodeAsSyntaxNode.Descendants())
                    {
                        yield return nextNode;
                    }
                }
            }
        }
    }

    [GenerateImmutable(GenerateBuilder = true)]
    public partial class ChunkNode : SyntaxNode
    {
        [Required]
        readonly BlockNode programBlock;
        [Required]
        readonly Token endOfFile;

        public override ImmutableList<SyntaxNodeOrToken> Children
        {
            get
            {
                return ImmutableList.Create<SyntaxNodeOrToken>(programBlock, endOfFile);
            }
        }
    }

    [GenerateImmutable(GenerateBuilder = true)]
    public partial class BlockNode : SyntaxNode
    {
        [Required, NotRecursive]
        readonly ImmutableList<StatementNode> statements;

        public override ImmutableList<SyntaxNodeOrToken> Children
        {
            get
            {
                return statements.Cast<SyntaxNodeOrToken>().ToImmutableList();
            }
        }
    }

    #region Simple Statement Nodes
    [GenerateImmutable(GenerateBuilder = true)]
    public abstract partial class StatementNode : SyntaxNode { }

    [GenerateImmutable(GenerateBuilder = true)]
    public partial class SemiColonStatementNode : StatementNode
    {
        [Required]
        readonly Token semiColon;

        public override ImmutableList<SyntaxNodeOrToken> Children
        {
            get
            {
                return ImmutableList.Create<SyntaxNodeOrToken>(semiColon);
            }
        }
    }

    [GenerateImmutable(GenerateBuilder = true)]
    public partial class FunctionCallStatementNode : StatementNode
    {
        [Required]
        readonly PrefixExp prefixExp;
        readonly Token colon;
        readonly Token name;
        [Required]
        readonly Args args;

        public override ImmutableList<SyntaxNodeOrToken> Children
        {
            get
            {
                var children = new List<SyntaxNodeOrToken>();
                children.Add(prefixExp);
                if (colon != null)
                {
                    children.Add(colon);
                    children.Add(name);
                }
                children.Add(args);
                return children.ToImmutableList();
            }
        }
    }

    [GenerateImmutable(GenerateBuilder = true)]
    public partial class ReturnStatementNode : StatementNode
    {
        [Required]
        readonly Token returnKeyword;
        [Required]
        readonly SeparatedList expList;
        readonly Token semiColon;

        public override ImmutableList<SyntaxNodeOrToken> Children
        {
            get
            {
                var children = new List<SyntaxNodeOrToken> { returnKeyword, expList };
                if (semiColon != null)
                    children.Add(semiColon);
                return children.ToImmutableList();
            }
        }
    }

    [GenerateImmutable(GenerateBuilder = true)]
    public partial class BreakStatementNode : StatementNode
    {
        [Required]
        readonly Token breakKeyword;

        public override ImmutableList<SyntaxNodeOrToken> Children
        {
            get
            {
                return ImmutableList.Create<SyntaxNodeOrToken>(breakKeyword);
            }
        }
    }

    [GenerateImmutable(GenerateBuilder = true)]
    public partial class GoToStatementNode : StatementNode
    {
        [Required]
        readonly Token goToKeyword;
        [Required]
        readonly Token name;

        public override ImmutableList<SyntaxNodeOrToken> Children
        {
            get
            {
                return ImmutableList.Create<SyntaxNodeOrToken>(goToKeyword, name);
            }
        }
    }

    [GenerateImmutable(GenerateBuilder = true)]
    public partial class DoStatementNode : StatementNode
    {
        [Required]
        readonly Token doKeyword;
        [Required]
        readonly BlockNode block;
        [Required]
        readonly Token endKeyword;

        public override ImmutableList<SyntaxNodeOrToken> Children
        {
            get
            {
                return ImmutableList.Create<SyntaxNodeOrToken>(doKeyword, block, endKeyword);
            }
        }
    }

    [GenerateImmutable(GenerateBuilder = true)]
    public partial class WhileStatementNode : StatementNode
    {
        [Required]
        readonly Token whileKeyword;
        [Required]
        readonly ExpressionNode exp;
        [Required]
        readonly Token doKeyword;
        [Required]
        readonly BlockNode block;
        [Required]
        readonly Token endKeyword;
        public override ImmutableList<SyntaxNodeOrToken> Children
        {
            get
            {
                return ImmutableList.Create<SyntaxNodeOrToken>(whileKeyword, exp, doKeyword, block, endKeyword);
            }
        }
    }

    [GenerateImmutable(GenerateBuilder = true)]
    public partial class RepeatStatementNode : StatementNode
    {
        [Required]
        readonly Token repeatKeyword;
        [Required]
        readonly BlockNode block;
        [Required]
        readonly Token untilKeyword;
        [Required]
        readonly ExpressionNode exp;

        public override ImmutableList<SyntaxNodeOrToken> Children
        {
            get
            {
                return ImmutableList.Create<SyntaxNodeOrToken>(repeatKeyword, block, untilKeyword, exp);
            }
        }
    }

    [GenerateImmutable(GenerateBuilder = true)]
    public partial class GlobalFunctionStatementNode : StatementNode
    {
        [Required]
        readonly Token functionKeyword;
        [Required]
        readonly FuncNameNode funcName;
        [Required]
        readonly FuncBodyNode funcBody;

        public override ImmutableList<SyntaxNodeOrToken> Children
        {
            get
            {
                return ImmutableList.Create<SyntaxNodeOrToken>(functionKeyword, funcName, funcBody);
            }
        }
    }

    [GenerateImmutable(GenerateBuilder = true)]
    public partial class LocalAssignmentStatementNode : StatementNode
    {
        [Required]
        readonly Token localKeyword;
        [Required]
        readonly SeparatedList nameList;
        readonly Token assignmentOperator;
        readonly SeparatedList expList;

        public override ImmutableList<SyntaxNodeOrToken> Children
        {
            get
            {
                var children = new List<SyntaxNodeOrToken>();
                children.Add(localKeyword);
                children.Add(nameList);
                if (assignmentOperator != null)
                {
                    children.Add(assignmentOperator);
                    children.Add(expList);
                }
                return children.ToImmutableList();
            }
        }
    }

    [GenerateImmutable(GenerateBuilder = true)]
    public partial class LocalFunctionStatementNode : StatementNode
    {
        [Required]
        readonly Token localKeyword;
        [Required]
        readonly Token functionKeyword;
        [Required]
        readonly Token name;
        [Required]
        readonly FuncBodyNode funcBody;

        public override ImmutableList<SyntaxNodeOrToken> Children
        {
            get
            {
                return ImmutableList.Create<SyntaxNodeOrToken>(localKeyword, functionKeyword, name, funcBody);
            }
        }
    }

    [GenerateImmutable(GenerateBuilder = true)]
    public partial class SimpleForStatementNode : StatementNode
    {
        [Required]
        readonly Token forKeyword;
        [Required]
        readonly Token name;
        [Required]
        readonly Token assignmentOperator;
        [Required]
        readonly ExpressionNode exp1;
        [Required]
        readonly Token comma;
        [Required]
        readonly ExpressionNode exp2;
        readonly Token optionalComma;
        readonly ExpressionNode optionalExp3;
        [Required]
        readonly Token doKeyword;
        [Required]
        readonly BlockNode block;
        [Required]
        readonly Token endKeyword;

        public override ImmutableList<SyntaxNodeOrToken> Children
        {
            get
            {
                var children = new List<SyntaxNodeOrToken> { forKeyword, name, assignmentOperator, exp1, comma, exp2 };
                if (optionalComma != null)
                {
                    children.Add(optionalComma);
                    children.Add(OptionalExp3);
                }
                children.Add(doKeyword);
                children.Add(block);
                children.Add(endKeyword);
                return children.ToImmutableList();
            }
        }
    }

    [GenerateImmutable(GenerateBuilder = true)]
    public partial class MultipleArgForStatementNode : StatementNode
    {
        [Required]
        readonly Token forKeyword;
        [Required]
        readonly SeparatedList nameList;
        [Required]
        readonly Token inKeyword;
        [Required]
        readonly SeparatedList expList;
        [Required]
        readonly Token doKeyword;
        [Required]
        readonly BlockNode block;
        [Required]
        readonly Token endKeyword;

        public override ImmutableList<SyntaxNodeOrToken> Children
        {
            get
            {
                return ImmutableList.Create<SyntaxNodeOrToken>(forKeyword, nameList, inKeyword, expList, doKeyword, block, endKeyword);
            }
        }
    }

    [GenerateImmutable(GenerateBuilder = true)]
    public partial class LabelStatementNode : StatementNode
    {
        [Required]
        readonly Token doubleColon1;
        [Required]
        readonly Token name;
        [Required]
        readonly Token doubleColon2;

        public override ImmutableList<SyntaxNodeOrToken> Children
        {
            get
            {
                return ImmutableList.Create<SyntaxNodeOrToken>(doubleColon1, name, doubleColon2);
            }
        }
    }

    [GenerateImmutable(GenerateBuilder = true)]
    public partial class AssignmentStatementNode : StatementNode
    {
        [Required]
        readonly SeparatedList varList;
        [Required]
        readonly Token assignmentOperator;
        [Required]
        readonly SeparatedList expList;

        public override ImmutableList<SyntaxNodeOrToken> Children
        {
            get
            {
                return ImmutableList.Create<SyntaxNodeOrToken>(varList, assignmentOperator, expList);
            }
        }
    }
    #endregion

    #region If Statement
    [GenerateImmutable(GenerateBuilder = true)]
    public partial class IfStatementNode : StatementNode
    {
        [Required]
        readonly Token ifKeyword;
        [Required]
        readonly ExpressionNode exp;
        [Required]
        readonly Token thenKeyword;
        [Required]
        readonly BlockNode ifBlock;
        [Required, NotRecursive]
        readonly ImmutableList<ElseIfBlockNode> elseIfList;
        readonly ElseBlockNode elseBlock;
        [Required]
        readonly Token endKeyword;

        public override ImmutableList<SyntaxNodeOrToken> Children
        {
            get
            {
                var children = new List<SyntaxNodeOrToken> { ifKeyword, exp, thenKeyword, ifBlock };

                //TODO remove temporary code:
                if (elseIfList != null)
                {
                    foreach (var node in elseIfList)
                    {
                        children.Add(node);
                    }
                }

                if (elseBlock != null)
                {
                    children.Add(elseBlock);
                }

                children.Add(endKeyword); //Why doesnt this do anything??!?!!
                return children.ToImmutableList();
            }
        }
    }

    [GenerateImmutable(GenerateBuilder = true)]
    public partial class ElseBlockNode : SyntaxNode
    {
        [Required]
        readonly Token elseKeyword;
        [Required]
        readonly BlockNode block;

        public override ImmutableList<SyntaxNodeOrToken> Children
        {
            get
            {
                return ImmutableList.Create<SyntaxNodeOrToken>(elseKeyword, block);
            }
        }
    }

    [GenerateImmutable(GenerateBuilder = true)]
    public partial class ElseIfBlockNode : SyntaxNode
    {
        [Required]
        readonly Token elseIfKeyword;
        [Required]
        readonly ExpressionNode exp;
        [Required]
        readonly Token thenKeyword;
        [Required]
        readonly BlockNode block;

        public override ImmutableList<SyntaxNodeOrToken> Children
        {
            get
            {
                return ImmutableList.Create<SyntaxNodeOrToken>(elseIfKeyword, exp, thenKeyword, block);
            }
        }
    }
    #endregion

    #region Expression nodes

    [GenerateImmutable(GenerateBuilder = true)]
    public abstract partial class ExpressionNode : SyntaxNode { }

    [GenerateImmutable(GenerateBuilder = true)]
    public partial class SimpleExpression : ExpressionNode
    {
        [Required]
        readonly Token expressionValue;

        public override ImmutableList<SyntaxNodeOrToken> Children
        {
            get
            {
                return ImmutableList.Create<SyntaxNodeOrToken>(expressionValue);
            }
        }
    }

    [GenerateImmutable(GenerateBuilder = true)]
    public partial class BinaryOperatorExpression : ExpressionNode
    {
        [Required]
        readonly ExpressionNode exp1;
        [Required]
        readonly Token binaryOperator;
        [Required]
        readonly ExpressionNode exp2;

        public override ImmutableList<SyntaxNodeOrToken> Children
        {
            get
            {
                return ImmutableList.Create<SyntaxNodeOrToken>(exp1, binaryOperator, exp2);
            }
        }
    }

    [GenerateImmutable(GenerateBuilder = true)]
    public partial class UnaryOperatorExpression : ExpressionNode
    {
        [Required]
        readonly Token unaryOperator;
        [Required]
        readonly ExpressionNode exp;

        public override ImmutableList<SyntaxNodeOrToken> Children
        {
            get
            {
                return ImmutableList.Create<SyntaxNodeOrToken>(unaryOperator, exp);
            }
        }
    }

    [GenerateImmutable(GenerateBuilder = true)]
    public partial class TableConstructorExp : ExpressionNode
    {
        [Required]
        readonly Token openCurly;
        [Required]
        readonly SeparatedList fieldList;
        [Required]
        readonly Token closeCurly;

        public override ImmutableList<SyntaxNodeOrToken> Children
        {
            get
            {
                return ImmutableList.Create<SyntaxNodeOrToken>(openCurly, fieldList, closeCurly);
            }
        }
    }

    [GenerateImmutable(GenerateBuilder = true)]
    public partial class FunctionDef : ExpressionNode
    {
        [Required]
        readonly Token functionKeyword;
        [Required]
        readonly FuncBodyNode functionBody;

        public override ImmutableList<SyntaxNodeOrToken> Children
        {
            get
            {
                return ImmutableList.Create<SyntaxNodeOrToken>(functionKeyword, functionBody);
            }
        }
    }

    #region FieldNodes
    [GenerateImmutable(GenerateBuilder = true)]
    public abstract partial class FieldNode : ExpressionNode { } //TODO: is this inheritance okay?

    [GenerateImmutable(GenerateBuilder = true)]
    public partial class BracketField : FieldNode
    {
        [Required]
        readonly Token openBracket;
        [Required]
        readonly ExpressionNode identifierExp;
        [Required]
        readonly Token closeBracket;
        [Required]
        readonly Token assignmentOperator;
        [Required]
        readonly ExpressionNode assignedExp;

        public override ImmutableList<SyntaxNodeOrToken> Children
        {
            get
            {
                return ImmutableList.Create<SyntaxNodeOrToken>(openBracket, identifierExp, closeBracket, assignmentOperator, assignedExp);
            }
        }
    }

    [GenerateImmutable(GenerateBuilder = true)]
    public partial class AssignmentField : FieldNode
    {
        [Required]
        readonly Token name;
        [Required]
        readonly Token assignmentOperator;
        [Required]
        readonly ExpressionNode exp;

        public override ImmutableList<SyntaxNodeOrToken> Children
        {
            get
            {
                return ImmutableList.Create<SyntaxNodeOrToken>(name, assignmentOperator, exp);
            }
        }
    }

    [GenerateImmutable(GenerateBuilder = true)]
    public partial class ExpField : FieldNode
    {
        [Required]
        readonly ExpressionNode exp;

        public override ImmutableList<SyntaxNodeOrToken> Children
        {
            get
            {
                return ImmutableList.Create<SyntaxNodeOrToken>(exp);
            }
        }
    }
    #endregion

    #region PrefixExp Expression
    [GenerateImmutable(GenerateBuilder = true)]
    public abstract partial class PrefixExp : ExpressionNode { }

    [GenerateImmutable(GenerateBuilder = true)]
    public abstract partial class Var : PrefixExp { }

    [GenerateImmutable(GenerateBuilder = true)]
    public partial class NameVar : Var
    {
        [Required]
        readonly Token name;

        public override ImmutableList<SyntaxNodeOrToken> Children
        {
            get
            {
                return ImmutableList.Create<SyntaxNodeOrToken>(name);
            }
        }
    }

    [GenerateImmutable(GenerateBuilder = true)]
    public partial class SquareBracketVar : Var
    {
        [Required]
        readonly PrefixExp prefixExp;
        [Required]
        readonly Token openBracket;
        [Required]
        readonly ExpressionNode exp;
        [Required]
        readonly Token closeBracket;

        public override ImmutableList<SyntaxNodeOrToken> Children
        {
            get
            {
                return ImmutableList.Create<SyntaxNodeOrToken>(prefixExp, openBracket, exp, closeBracket);
            }
        }
    }

    [GenerateImmutable(GenerateBuilder = true)]
    public partial class DotVar : Var
    {
        [Required]
        readonly PrefixExp prefixExp;
        [Required]
        readonly Token dotOperator;
        [Required]
        readonly Token nameIdentifier;

        public override ImmutableList<SyntaxNodeOrToken> Children
        {
            get
            {
                return ImmutableList.Create<SyntaxNodeOrToken>(prefixExp, dotOperator, nameIdentifier);
            }
        }
    }

    [GenerateImmutable(GenerateBuilder = true)]
    public partial class FunctionCallPrefixexp : PrefixExp
    {
        [Required]
        readonly PrefixExp prefixExp;
        readonly Token colon;
        readonly Token name;
        [Required]
        readonly Args args;

        public override ImmutableList<SyntaxNodeOrToken> Children
        {
            get
            {
                var children = new List<SyntaxNodeOrToken> { prefixExp };
                if (colon != null)
                {
                    children.Add(colon);
                    children.Add(name);
                }
                children.Add(args);
                return children.ToImmutableList();
            }
        }
    }

    [GenerateImmutable(GenerateBuilder = true)]
    public partial class ParenPrefixExp : PrefixExp
    {
        [Required]
        readonly Token openParen;
        [Required]
        readonly ExpressionNode exp;
        [Required]
        readonly Token closeParen;

        public override ImmutableList<SyntaxNodeOrToken> Children
        {
            get
            {
                return ImmutableList.Create<SyntaxNodeOrToken>(openParen, exp, closeParen);
            }
        }
    }
    #endregion

    #endregion

    #region Args Nodes
    [GenerateImmutable(GenerateBuilder = true)]
    public abstract partial class Args : SyntaxNode { }

    [GenerateImmutable(GenerateBuilder = true)]
    public partial class TableContructorArg : Args
    {
        [Required]
        readonly Token openCurly;
        [Required]
        readonly SeparatedList fieldList;
        [Required]
        readonly Token closeCurly;

        public override ImmutableList<SyntaxNodeOrToken> Children
        {
            get
            {
                return ImmutableList.Create<SyntaxNodeOrToken>(openCurly, fieldList, closeCurly);
            }
        }
    }

    [GenerateImmutable(GenerateBuilder = true)]
    public partial class ParenArg : Args
    {
        [Required]
        readonly Token openParen;
        [Required]
        readonly SeparatedList expList;
        [Required]
        readonly Token closeParen;

        public override ImmutableList<SyntaxNodeOrToken> Children
        {
            get
            {
                return ImmutableList.Create<SyntaxNodeOrToken>(openParen, expList, closeParen);
            }
        }
    }

    [GenerateImmutable(GenerateBuilder = true)]
    public partial class StringArg : Args
    {
        [Required]
        readonly Token stringLiteral;

        public override ImmutableList<SyntaxNodeOrToken> Children
        {
            get
            {
                return ImmutableList.Create<SyntaxNodeOrToken>(stringLiteral);
            }
        }
    }
    #endregion

    #region List nodes

    [GenerateImmutable(GenerateBuilder = true)]
    public partial class SeparatedList : SyntaxNode
    {
        [Required, NotRecursive]
        readonly ImmutableList<SeparatedListElement> syntaxList;

        public override ImmutableList<SyntaxNodeOrToken> Children
        {
            get
            {
                var children = new List<SyntaxNodeOrToken>();

                foreach (var listItem in syntaxList)
                {
                    foreach (var child in listItem.Children)
                    {
                        children.Add(child);
                    }
                }

                return children.ToImmutableList();
            }
        }
    }

    [GenerateImmutable(GenerateBuilder = true)]
    public partial class SeparatedListElement : SyntaxNode
    {
        [Required]
        readonly SyntaxNodeOrToken element;
        [Required]
        readonly Token seperator;

        public override ImmutableList<SyntaxNodeOrToken> Children
        {
            get
            {
                if (seperator != null)
                {
                    return ImmutableList.Create<SyntaxNodeOrToken>(element, seperator);
                }
                else
                {
                    return ImmutableList.Create<SyntaxNodeOrToken>(element);
                }
            }
        }
    }

    [GenerateImmutable(GenerateBuilder = true)]
    public abstract partial class ParList : SyntaxNode { }

    [GenerateImmutable(GenerateBuilder = true)]
    public partial class VarArgParList : ParList
    {
        [Required]
        readonly Token varargOperator;

        public override ImmutableList<SyntaxNodeOrToken> Children
        {
            get
            {
                return ImmutableList.Create<SyntaxNodeOrToken>(varargOperator);
            }
        }
    }

    [GenerateImmutable(GenerateBuilder = true)]
    public partial class NameListPar : ParList
    {
        [Required]
        readonly SeparatedList namesList;
        readonly Token comma;
        readonly Token vararg;

        public override ImmutableList<SyntaxNodeOrToken> Children
        {
            get
            {
                var children = new List<SyntaxNodeOrToken> { namesList };
                if (comma != null)
                {
                    children.Add(comma);
                    children.Add(vararg);
                }
                return children.ToImmutableList();
            }
        }
    }

    #endregion

    [GenerateImmutable(GenerateBuilder = true)]
    public partial class TableConstructorNode : SyntaxNode
    {
        [Required]
        readonly Token openCurly;
        [Required]
        readonly SeparatedList fieldList;
        [Required]
        readonly Token closeCurly;

        public override ImmutableList<SyntaxNodeOrToken> Children
        {
            get
            {
                return ImmutableList.Create<SyntaxNodeOrToken>(openCurly, fieldList, closeCurly);
            }
        }
    }

    [GenerateImmutable(GenerateBuilder = true)]
    public partial class FuncBodyNode : SyntaxNode
    {
        [Required]
        readonly Token openParen;
        [Required]
        readonly ParList parameterList;
        [Required]
        readonly Token closeParen;
        [Required]
        readonly BlockNode block;
        [Required]
        readonly Token endKeyword;

        public override ImmutableList<SyntaxNodeOrToken> Children
        {
            get
            {
                return ImmutableList.Create<SyntaxNodeOrToken>(openParen, parameterList, closeParen, block, endKeyword);
            }
        }
    }

    [GenerateImmutable(GenerateBuilder = true)]
    public partial class FuncNameNode : SyntaxNode
    {
        [Required]
        readonly Token name;
        [Required, NotRecursive]
        readonly SeparatedList funcNameList;
        readonly Token optionalColon;
        readonly Token optionalName;

        public override ImmutableList<SyntaxNodeOrToken> Children
        {
            get
            {
                var children = new List<SyntaxNodeOrToken> { name, funcNameList };
                if (optionalColon != null)
                {
                    children.Add(optionalColon);
                    children.Add(optionalName);
                }
                return children.ToImmutableList();
            }
        }
    }
}
