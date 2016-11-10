using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace AutoMapper.EquivilencyExpression
{
    public static class EquivilentExpressions
    {
        private static readonly UserDefinedEquivilentExpressions UserDefinedEquivilentExpressions = new UserDefinedEquivilentExpressions();
        private static readonly IList<IGenerateEquivilentExpressions> GenerateEquivilentExpressions = new List<IGenerateEquivilentExpressions>{UserDefinedEquivilentExpressions};
        
        /// <summary>
        /// Equality List for Generating Equality Comparisons between two types
        /// </summary>
        public static ICollection<IGenerateEquivilentExpressions> GenerateEquality => GenerateEquivilentExpressions;

        internal static IEquivilentExpression GetEquivilentExpression(Type sourceType, Type destinationType)
        {
            var generate = GenerateEquality.FirstOrDefault(g => g.CanGenerateEquivilentExpression(sourceType, destinationType));
            return generate?.GeneratEquivilentExpression(sourceType, destinationType);
        }

        /// <summary>
        /// Make Comparison between <typeparamref name="TSource"/> and <typeparamref name="TDestination"/>
        /// </summary>
        /// <typeparam name="TSource">Compared type</typeparam>
        /// <typeparam name="TDestination">Type being compared to</typeparam>
        /// <param name="mappingExpression">Base Mapping Expression</param>
        /// <param name="equivilentExpression">Equivilent Expression between <typeparamref name="TSource"/> and <typeparamref name="TDestination"/></param>
        /// <returns></returns>
        public static IMappingExpression<TSource, TDestination> EqualityComparision<TSource, TDestination>(this IMappingExpression<TSource, TDestination> mappingExpression, Expression<Func<TSource, TDestination, bool>> equivilentExpression) 
            where TSource : class 
            where TDestination : class
        {
            UserDefinedEquivilentExpressions.AddEquivilencyExpression(equivilentExpression);
            return mappingExpression;
        }


        /// <summary>
        /// Make Comparison between <typeparamref name="TSource"/> and <typeparamref name="TDestination"/>
        /// </summary>
        /// <typeparam name="TSource">Compared type</typeparam>
        /// <typeparam name="TDestination">Type being compared to</typeparam>
        /// <param name="mappingExpression">Base Mapping Expression</param>
        /// <param name="equivilentExpression">Equivilent Expression between <typeparamref name="TSource"/> and <typeparamref name="TDestination"/></param>
        /// <param name="softDeletePropertyExpression">Property that will be set to True when a element has been deleted</param>
        /// <returns></returns>
        public static IMappingExpression<TSource, TDestination> EqualityComparision<TSource, TDestination>(this IMappingExpression<TSource, TDestination> mappingExpression, Expression<Func<TSource, TDestination, bool>> equivilentExpression,
            Expression<Func<TDestination, bool>> softDeletePropertyExpression)
            where TSource : class
            where TDestination : class
        {
            UserDefinedEquivilentExpressions.AddEquivilencyExpression(equivilentExpression, softDeletePropertyExpression.GetSetter());
            return mappingExpression;
        }

        /// <summary>
        /// Make Comparison between <typeparamref name="TSource"/> and <typeparamref name="TDestination"/>
        /// </summary>
        /// <typeparam name="TSource">Compared type</typeparam>
        /// <typeparam name="TDestination">Type being compared to</typeparam>
        /// <param name="mappingExpression">Base Mapping Expression</param>
        /// <param name="equivilentExpression">Equivilent Expression between <typeparamref name="TSource"/> and <typeparamref name="TDestination"/></param>
        /// <param name="softDeleteAction">Action that will be called when a element has been deleted</param>
        /// <returns></returns>
        public static IMappingExpression<TSource, TDestination> EqualityComparision<TSource, TDestination>(this IMappingExpression<TSource, TDestination> mappingExpression, Expression<Func<TSource, TDestination, bool>> equivilentExpression,
           Action<TDestination,bool> softDeleteAction)
           where TSource : class
           where TDestination : class
        {
            UserDefinedEquivilentExpressions.AddEquivilencyExpression(equivilentExpression, softDeleteAction);
            return mappingExpression;
        }

        /// <summary>
        /// Convert a lambda expression for a getter into a setter
        /// </summary>
        private static Action<T, TProperty> GetSetter<T, TProperty>(this Expression<Func<T, TProperty>> expression)
        {
            var memberExpression = (MemberExpression)expression.Body;
            var property = (System.Reflection.PropertyInfo)memberExpression.Member;
            var setMethod = property.SetMethod;

            var parameterT = Expression.Parameter(typeof(T), "x");
            var parameterTProperty = Expression.Parameter(typeof(TProperty), "y");

            var newExpression =
                Expression.Lambda<Action<T, TProperty>>(
                    Expression.Call(parameterT, setMethod, parameterTProperty),
                    parameterT,
                    parameterTProperty
                );

            return newExpression.Compile();
        }
    }
}