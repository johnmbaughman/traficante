﻿using System;

namespace Traficante.TSQL.Parser.Nodes
{
    public class InternalQueryNode : QueryNode
    {
        public InternalQueryNode(SelectNode select, FromNode from, WhereNode where, GroupByNode groupBy,
            OrderByNode orderBy, SkipNode skip, TakeNode take)
            : base(select, from, where, groupBy, orderBy, skip, take)
        {
        }

        public override Type ReturnType => null;

        public override void Accept(IExpressionVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString()
        {
            return
                $"{Select.ToString()} {From.ToString()} {Where?.ToString()} {GroupBy?.ToString()} {OrderBy?.ToString()} {Skip?.ToString()} {Take?.ToString()}";
        }
    }
}