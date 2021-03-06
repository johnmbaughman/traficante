﻿using System;
using Traficante.TSQL.Evaluator.Visitors;

namespace Traficante.TSQL.Parser.Nodes
{
    public class IntegerNode : ConstantValueNode
    {
        public IntegerNode(string value)
        {
            if (int.TryParse(value, out var value1))
            {
                ObjValue = value1;
            }
            else if (long.TryParse(value, out var value2))
            {
                ObjValue = value2;
            }

            Id = $"{nameof(IntegerNode)}{value}{ReturnType.Name}";
        }

        public override object ObjValue { get; }

        public override Type ReturnType => typeof(int?);

        public override string Id { get; }

        public override string ToString()
        {
            return ObjValue.ToString();
        }

        public override void Accept(IExpressionVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}