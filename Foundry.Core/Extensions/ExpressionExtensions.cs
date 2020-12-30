using System;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Microsoft.Toolkit.Diagnostics;

namespace Foundry
{
    public static class ExpressionExtensions
    {
        /// <summary>
        /// Gets the body of an expression as text.
        /// </summary>
        /// <param name="expression">The expression to get the body of.</param>
        /// <exception cref="ArgumentNullException"><paramref name="expression"/> is <see langword="null"/></exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetBody<T>(this Expression<Func<T>> expression)
        {
            Guard.IsNotNull(expression, nameof(expression));

            return GetBody(expression.Body);
        }

        /// <summary>
        /// Gets the body of an expression as text.
        /// </summary>
        /// <param name="expression">The expression to get the body of.</param>
        public static string GetBody(this Expression expression)
        {
            Guard.IsNotNull(expression, nameof(expression));

            string body;
            switch (expression)
            {
                case MemberExpression memberBody:
                    body = memberBody.Member.Name;
                    break;
                case ConstantExpression constantBody:
                    body = constantBody.Value!.ToString()!;
                    break;
                case UnaryExpression unaryBody:
                    return GetBody(unaryBody.Operand);
                case MethodCallExpression methodBody:
                    string c = methodBody.Method.IsStatic && methodBody.Method.DeclaringType is not null ? $"{methodBody.Method.DeclaringType.Name}." : string.Empty;
                    body = $"{c}{methodBody.Method.Name}({methodBody.Arguments.Aggregate(string.Empty, (prev, next) => $"{prev}, {GetBody(next)}")[(methodBody.Arguments.Count > 0 ? 2 : 0)..]})";
                    break;
                case BinaryExpression simpleBinaryExpression:
                    string op = Expression.MakeBinary(simpleBinaryExpression.NodeType, Expression.Constant(0), Expression.Constant(0)).ToString().Replace("0", "").Replace("(", "").Replace(")", "");
                    body = $"{GetBody(simpleBinaryExpression.Left)} {op} {GetBody(simpleBinaryExpression.Right)}";
                    break;
                default:
                    body = expression.ToString();
                    break;
            }

            return body;
        }
    }
}
