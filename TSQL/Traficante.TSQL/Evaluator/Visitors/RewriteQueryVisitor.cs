﻿using System;
using System.Collections.Generic;
using System.Linq;
using Traficante.TSQL.Evaluator.Utils;
using Traficante.TSQL.Parser.Nodes;

namespace Traficante.TSQL.Evaluator.Visitors
{
    public class RewriteQueryVisitor : IScopeAwareExpressionVisitor
    {
        private Scope _scope;

        protected Stack<Node> Nodes { get; } = new Stack<Node>();

        public RootNode RootScript { get; private set; }

        public void Visit(Node node)
        {
        }

        public void Visit(DescNode node)
        {
            var from = (FromFunctionNode) Nodes.Pop();
            Nodes.Push(new DescNode(from, node.Type));
        }

        public void Visit(StarNode node)
        {
            var right = Nodes.Pop();
            var left = Nodes.Pop();
            Nodes.Push(new StarNode(left, right));
        }

        public void Visit(FSlashNode node)
        {
            var right = Nodes.Pop();
            var left = Nodes.Pop();
            Nodes.Push(new FSlashNode(left, right));
        }

        public void Visit(ModuloNode node)
        {
            var right = Nodes.Pop();
            var left = Nodes.Pop();
            Nodes.Push(new ModuloNode(left, right));
        }

        public void Visit(AddNode node)
        {
            var right = Nodes.Pop();
            var left = Nodes.Pop();
            Nodes.Push(new AddNode(left, right));
        }

        public void Visit(HyphenNode node)
        {
            var right = Nodes.Pop();
            var left = Nodes.Pop();
            Nodes.Push(new HyphenNode(left, right));
        }

        public void Visit(AndNode node)
        {
            var right = Nodes.Pop();
            var left = Nodes.Pop();
            Nodes.Push(new AndNode(left, right));
        }

        public void Visit(OrNode node)
        {
            var right = Nodes.Pop();
            var left = Nodes.Pop();
            Nodes.Push(new OrNode(left, right));
        }

        public void Visit(EqualityNode node)
        {
            var right = Nodes.Pop();
            var left = Nodes.Pop();
            Nodes.Push(new EqualityNode(left, right));
        }

        public void Visit(GreaterOrEqualNode node)
        {
            var right = Nodes.Pop();
            var left = Nodes.Pop();
            Nodes.Push(new GreaterOrEqualNode(left, right));
        }

        public void Visit(LessOrEqualNode node)
        {
            var right = Nodes.Pop();
            var left = Nodes.Pop();
            Nodes.Push(new LessOrEqualNode(left, right));
        }

        public void Visit(GreaterNode node)
        {
            var right = Nodes.Pop();
            var left = Nodes.Pop();
            Nodes.Push(new GreaterNode(left, right));
        }

        public void Visit(LessNode node)
        {
            var right = Nodes.Pop();
            var left = Nodes.Pop();
            Nodes.Push(new LessNode(left, right));
        }

        public void Visit(DiffNode node)
        {
            var right = Nodes.Pop();
            var left = Nodes.Pop();
            Nodes.Push(new DiffNode(left, right));
        }

        public void Visit(NotNode node)
        {
            Nodes.Push(new NotNode(Nodes.Pop()));
        }

        public void Visit(LikeNode node)
        {
            var right = Nodes.Pop();
            var left = Nodes.Pop();
            Nodes.Push(new LikeNode(left, right));
        }

        public void Visit(RLikeNode node)
        {
            var right = Nodes.Pop();
            var left = Nodes.Pop();
            Nodes.Push(new RLikeNode(left, right));
        }

        public void Visit(InNode node)
        {
            var right = (ArgsListNode)Nodes.Pop();
            var left = Nodes.Pop();

            Node exp = new EqualityNode(left, right.Args[0]);

            for (var i = 1; i < right.Args.Length; i++)
            {
                exp = new OrNode(exp, new EqualityNode(left, right.Args[i]));
            }

            Nodes.Push(exp);
        }

        public virtual void Visit(FieldNode node)
        {
            Nodes.Push(new FieldNode(Nodes.Pop(), node.FieldOrder, node.FieldName));
        }

        public void Visit(FieldOrderedNode node)
        {
            Nodes.Push(new FieldOrderedNode(Nodes.Pop(), node.FieldOrder, node.FieldName, node.Order));
        }

        public virtual void Visit(SelectNode node)
        {
            var fields = CreateFields(node.Fields);

            Nodes.Push(new SelectNode(fields.ToArray()));
        }

        public void Visit(StringNode node)
        {
            Nodes.Push(new StringNode(node.Value));
        }

        public void Visit(DecimalNode node)
        {
            Nodes.Push(new DecimalNode(node.Value));
        }

        public void Visit(IntegerNode node)
        {
            Nodes.Push(new IntegerNode(node.ObjValue.ToString()));
        }

        public void Visit(BooleanNode node)
        {
            Nodes.Push(new BooleanNode(node.Value));
        }

        public void Visit(WordNode node)
        {
            Nodes.Push(new WordNode(node.Value));
        }

        public void Visit(ContainsNode node)
        {
            var right = Nodes.Pop();
            var left = Nodes.Pop();
            Nodes.Push(new ContainsNode(left, right as ArgsListNode));
        }

        public virtual void Visit(FunctionNode node)
        {
            VisitAccessMethod(node);
        }

        public void Visit(IsNullNode node)
        {
            Nodes.Push(new IsNullNode(Nodes.Pop(), node.IsNegated));
        }

        public virtual void Visit(AccessColumnNode node)
        {
            Nodes.Push(new AccessColumnNode(node.Name, node.Alias, node.ReturnType, node.Span));
        }

        public void Visit(AllColumnsNode node)
        {
            Nodes.Push(new AllColumnsNode());
        }

        public void Visit(IdentifierNode node)
        {
            Nodes.Push(new IdentifierNode(node.Name));
        }

        public void Visit(AccessObjectArrayNode node)
        {
            Nodes.Push(new AccessObjectArrayNode(node.Token, node.PropertyInfo));
        }

        public void Visit(AccessObjectKeyNode node)
        {
            Nodes.Push(new AccessObjectKeyNode(node.Token, node.PropertyInfo));
        }

        public void Visit(PropertyValueNode node)
        {
            Nodes.Push(new PropertyValueNode(node.Name, node.PropertyInfo));
        }

        public void Visit(VariableNode node)
        {
            Nodes.Push(new VariableNode(node.Name, node.ReturnType));
        }

        public void Visit(DeclareNode node)
        {
            Nodes.Push(new DeclareNode(node.Variable, node.Type));
        }

        public void Visit(SetNode node)
        {
            var variable = (VariableNode)Nodes.Pop();
            var value = Nodes.Pop();
            Nodes.Push(new SetNode(variable, value));
        }

        public virtual void Visit(DotNode node)
        {
            var exp = Nodes.Pop();
            var root = Nodes.Pop();
            Nodes.Push(new DotNode(root, exp, node.IsOuter, node.Name, exp.ReturnType));
        }

        public void Visit(ArgsListNode node)
        {
            var args = new Node[node.Args.Length];

            for (var i = node.Args.Length - 1; i >= 0; --i)
                args[i] = Nodes.Pop();

            Nodes.Push(new ArgsListNode(args));
        }

        public virtual void Visit(WhereNode node)
        {
            Nodes.Push(new WhereNode(Nodes.Pop()));
        }

        public virtual void Visit(GroupByNode node)
        {
            var having = Nodes.Peek() as HavingNode;

            if (having != null)
                Nodes.Pop();

            var fields = new FieldNode[node.Fields.Length];

            for (var i = node.Fields.Length - 1; i >= 0; --i)
                fields[i] = Nodes.Pop() as FieldNode;


            Nodes.Push(new GroupByNode(fields, having));
        }

        public void Visit(HavingNode node)
        {
            Nodes.Push(new HavingNode(Nodes.Pop()));
        }

        public void Visit(SkipNode node)
        {
            Nodes.Push(new SkipNode((IntegerNode) node.Expression));
        }

        public void Visit(TakeNode node)
        {
            Nodes.Push(new TakeNode((IntegerNode) node.Expression));
        }

        public void Visit(FromFunctionNode node)
        {
            Nodes.Push(new FromFunctionNode((FunctionNode)Nodes.Pop(), node.Alias));
        }

        public void Visit(FromTableNode node)
        {
            Nodes.Push(new FromTableNode(node.Database, node.Schema, node.TableOrView, node.Alias));
        }

        public void Visit(JoinFromNode node)
        {
            var exp = Nodes.Pop();
            var right = (FromNode) Nodes.Pop();
            var left = (FromNode) Nodes.Pop();
            Nodes.Push(new JoinFromNode(left, right, exp, node.JoinType));
        }

        public void Visit(ExpressionFromNode node)
        {
            Nodes.Push(new ExpressionFromNode((FromNode) Nodes.Pop()));
        }

        public void Visit(InMemoryTableFromNode node)
        {
            Nodes.Push(new InMemoryTableFromNode(node.VariableName, node.Alias));
        }

        public void Visit(IntoNode node)
        {
            Nodes.Push(new IntoNode(node.Name));
        }

        public void Visit(QueryScope node)
        {
        }

        public void Visit(QueryNode node)
        {
            var orderBy = node.OrderBy != null ? Nodes.Pop() as OrderByNode : null;
            var groupBy = node.GroupBy != null ? Nodes.Pop() as GroupByNode : null;

            var skip = node.Skip != null ? Nodes.Pop() as SkipNode : null;
            var take = node.Take != null ? Nodes.Pop() as TakeNode : null;

            var select = Nodes.Pop() as SelectNode;
            var where = node.Where != null ? Nodes.Pop() as WhereNode : null;
            var from = node.From != null ? Nodes.Pop() as FromNode : null;

            if (groupBy == null)
            {
                var split = SplitBetweenAggreateAndNonAggreagate(select.Fields);
                if (split.NotAggregateFields.Length == 0)
                {
                    select.ReturnsSingleRow = true;
                }
            }

            if (from == null)
            {
                select.ReturnsSingleRow = true;
            }

            Nodes.Push(new QueryNode(select, from, where, groupBy, orderBy, skip, take));
        }

        public void Visit(RootNode node)
        {
            RootScript = new RootNode(Nodes.Pop());
        }

        public void Visit(SingleSetNode node)
        {

        }

        public void Visit(UnionNode node)
        {
            var right = Nodes.Pop();
            var left = Nodes.Pop();
            Nodes.Push(new UnionNode(node.ResultTableName, node.Keys, left, right, node.IsNested, node.IsTheLastOne));
        }

        public void Visit(UnionAllNode node)
        {
            var right = Nodes.Pop();
            var left = Nodes.Pop();
            Nodes.Push(new UnionAllNode(node.ResultTableName, node.Keys, left, right, node.IsNested,
                node.IsTheLastOne));
        }

        public void Visit(ExceptNode node)
        {
            var right = Nodes.Pop();
            var left = Nodes.Pop();
            Nodes.Push(new ExceptNode(node.ResultTableName, node.Keys, left, right, node.IsNested, node.IsTheLastOne));
        }

        public void Visit(IntersectNode node)
        {
            var right = Nodes.Pop();
            var left = Nodes.Pop();
            Nodes.Push(
                new IntersectNode(node.ResultTableName, node.Keys, left, right, node.IsNested, node.IsTheLastOne));
        }

        public void Visit(PutTrueNode node)
        {
            Nodes.Push(new PutTrueNode());
        }

        public void Visit(MultiStatementNode node)
        {
            var items = new Node[node.Nodes.Length];

            for (var i = node.Nodes.Length - 1; i >= 0; --i)
                items[i] = Nodes.Pop();

            Nodes.Push(new MultiStatementNode(items, node.ReturnType));
        }

        public void Visit(CteExpressionNode node)
        {
            var sets = new CteInnerExpressionNode[node.InnerExpression.Length];

            var set = Nodes.Pop();

            for (var i = node.InnerExpression.Length - 1; i >= 0; --i)
                sets[i] = (CteInnerExpressionNode) Nodes.Pop();

            Nodes.Push(new CteExpressionNode(sets, set));
        }

        public void Visit(CteInnerExpressionNode node)
        {
            Nodes.Push(new CteInnerExpressionNode(Nodes.Pop(), node.Name));
        }

        public void Visit(JoinsNode node)
        {
            Nodes.Push(new JoinsNode((JoinFromNode) Nodes.Pop()));
        }

        public void Visit(JoinNode node)
        {
        }

        public void Visit(OrderByNode node)
        {
            var fields = new FieldOrderedNode[node.Fields.Length];

            for (var i = node.Fields.Length - 1; i >= 0; --i)
                fields[i] = (FieldOrderedNode)Nodes.Pop();

            Nodes.Push(new OrderByNode(fields));
        }

        public void SetScope(Scope scope)
        {
            _scope = scope;
        }

        private void VisitAccessMethod(FunctionNode node)
        {
            var args = Nodes.Pop() as ArgsListNode;

            Nodes.Push(new FunctionNode(node.Database, node.Schema, node.Name, node.Arguments, node.Method));
        }

        private FieldNode[] CreateFields(FieldNode[] oldFields)
        {
            var reorderedList = new FieldNode[oldFields.Length];
            var fields = new List<FieldNode>(reorderedList.Length);

            for (var i = reorderedList.Length - 1; i >= 0; i--) reorderedList[i] = Nodes.Pop() as FieldNode;


            for (int i = 0, j = reorderedList.Length, p = 0; i < j; ++i)
            {
                var field = reorderedList[i];

                if (field.Expression is AllColumnsNode)
                {
                    fields.AddRange(new FieldNode[0]);
                    continue;
                }

                fields.Add(new FieldNode(field.Expression, p++, field.FieldName));
            }

            return fields.ToArray();
        }

        public void Visit(CreateTableNode node)
        {
        }

        public void Visit(StatementsArrayNode node)
        {
        }

        public void Visit(StatementNode node)
        {
        }

        public void Visit(CaseNode node)
        {
            var whenThenPairs = new List<(Node When, Node Then)>();

            for (int i = 0; i < node.WhenThenPairs.Length; ++i)
            {
                var then = Nodes.Pop();
                var when = Nodes.Pop();
                whenThenPairs.Add((when, then));
            }

            var elseNode = Nodes.Pop();

            Nodes.Push(new CaseNode(whenThenPairs.ToArray(), elseNode, node.ReturnType));
        }

        private (FieldNode[] AggregateFields, FieldNode[] NotAggregateFields) SplitBetweenAggreateAndNonAggreagate(FieldNode[] fieldsToSplit)
        {
            var aggregateFields = new List<FieldNode>();
            var notAggregateFields = new List<FieldNode>();

            foreach (var root in fieldsToSplit)
            {
                var subNodes = new Stack<Node>();

                subNodes.Push(root.Expression);
                bool hasAggregateMethod = false;
                while (subNodes.Count > 0)
                {
                    var subNode = subNodes.Pop();

                    if (subNode is FunctionNode aggregateMethod && aggregateMethod.IsAggregateMethod)
                    {
                        hasAggregateMethod = true;
                        break;
                    }
                    else
                    if (subNode is FunctionNode method)
                    {
                        foreach (var arg in method.Arguments.Args)
                            subNodes.Push(arg);
                    }
                    else if (subNode is BinaryNode binary)
                    {
                        subNodes.Push(binary.Left);
                        subNodes.Push(binary.Right);
                    }
                }
                if (hasAggregateMethod)
                    aggregateFields.Add(root);
                else
                    notAggregateFields.Add(root);
            }

            return (aggregateFields.ToArray(), notAggregateFields.ToArray());
        }

        public void Visit(TypeNode node)
        {
            Nodes.Push(new TypeNode(node.Name, node.Size));
        }

        public void Visit(ExecuteNode node)
        {
            Nodes.Push(new ExecuteNode(node.VariableToSet, node.FunctionToRun));
        }
    }
}