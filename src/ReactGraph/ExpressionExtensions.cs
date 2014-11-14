using System.Linq.Expressions;
using System.Reflection;

namespace ReactGraph
{
    static class ExpressionExtensions
    {
        public static bool IsWritable(this LambdaExpression expression)
        {
            var memberExpression = expression.Body as MemberExpression;
            return IsWritable(memberExpression);
        }

        public static bool IsWritable(this MemberExpression memberExpression)
        {
            if (memberExpression == null) return false;
            var propertyInfo = memberExpression.Member as PropertyInfo;
            var fieldInfo = memberExpression.Member as FieldInfo;

            if (propertyInfo == null && fieldInfo == null) return false;
            if (propertyInfo != null && !propertyInfo.CanWrite) return false;
            if (fieldInfo != null && fieldInfo.IsInitOnly) return false;

            return true;
        }

        public static bool IsRoot(this LambdaExpression expression)
        {
            var memberExpression = expression.Body as MemberExpression;
            return IsRoot(memberExpression);
        }

        public static bool IsRoot(this MemberExpression memberExpression)
        {
            if (memberExpression == null) return false;
            return memberExpression.Expression is ConstantExpression;
        }
    }
}