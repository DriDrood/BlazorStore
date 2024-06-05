using System.Collections;
using System.Linq.Expressions;
using System.Reflection;

namespace Dumba.BlazorStore.Extensions;
static class ExpressionExtension
{
    public static TOut? GetValueOrNull<TIn, TOut>(this Expression<Func<TIn, TOut>> expression, TIn value)
    {
        (object? result, _) = GetValueOrNullInner(value, expression.Body);
        return (TOut?)result;
    }

    public static IEnumerable<object> GetPath<TIn, TOut>(this Expression<Func<TIn, TOut>> expression)
    {
        (_, IEnumerable<object> path) = GetValueOrNullInner(null, expression.Body);
        return path;
    }

    private static (object? outValue, IEnumerable<object> path) GetValueOrNullInner(object? value, Expression expr)
    {
        // constant
        if (expr is ConstantExpression conExpr)
        {
            return (conExpr.Value, Enumerable.Empty<object>());
        }

        // parameter
        if (expr is ParameterExpression)
        {
            return (value, Enumerable.Empty<object>());
        }

        // property
        if (expr is MemberExpression memExpr)
        {
            (object? innerValue, IEnumerable<object> prevPath) = GetValueOrNullInner(value, memExpr.Expression!);
            string propertyName = memExpr.Member.Name;

            object? outValue = memExpr.Member is FieldInfo fieldInfo
                ? fieldInfo.GetValue(innerValue) // local variables
                : innerValue?.GetType().GetProperty(propertyName)!.GetValue(innerValue);

            return (outValue, prevPath.Append(propertyName));
        }

        // []
        if (expr is MethodCallExpression callExpr && callExpr.Method.Name == "get_Item")
        {
            (object? innerValue, IEnumerable<object> prevPath) = GetValueOrNullInner(value, callExpr.Object!);
            (object? key, _) = GetValueOrNullInner(value, callExpr.Arguments[0]);

            if (innerValue is null || key is null)
            {
                if (innerValue is null && key is null)
                    return (null, prevPath);
                if (key is not null)
                    return (null, prevPath.Append(key));
                        
                return (innerValue, prevPath);
            }

            if (innerValue is IDictionary dict)
                return (dict[key], prevPath.Append(key));

            if (innerValue is IList list)
                return (list[(int)key], prevPath.Append(key));

            throw new Exception("Not supported collection type");
        }

        // System.Linq.Expressions.BlockExpression
        // System.Linq.Expressions.DebugInfoExpression
        // System.Linq.Expressions.DefaultExpression
        // System.Linq.Expressions.DynamicExpression
        // System.Linq.Expressions.GotoExpression
        // System.Linq.Expressions.IndexExpression
        // System.Linq.Expressions.InvocationExpression
        // System.Linq.Expressions.LabelExpression
        // System.Linq.Expressions.LambdaExpression
        // System.Linq.Expressions.ListInitExpression
        // System.Linq.Expressions.LoopExpression
        // System.Linq.Expressions.MemberInitExpression
        // System.Linq.Expressions.MethodCallExpression
        // System.Linq.Expressions.NewArrayExpression
        // System.Linq.Expressions.NewExpression
        // System.Linq.Expressions.RuntimeVariablesExpression
        // System.Linq.Expressions.SwitchExpression
        // System.Linq.Expressions.TryExpression
        // System.Linq.Expressions.TypeBinaryExpression
        // System.Linq.Expressions.UnaryExpression
        throw new Exception("Not supported expression type");
    }
}