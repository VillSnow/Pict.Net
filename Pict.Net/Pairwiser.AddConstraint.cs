using System;
using System.Linq.Expressions;
namespace Pict.Net
{
	public partial class Pairwiser
	{
		public Pairwiser AddConstraint<T1>(string parameterName1, Expression<Func<T1, bool>> constraint) 
			=> AddConstraint(new [] { parameterName1 }, constraint.Compile());

		public Pairwiser AddConstraint<T1>(Expression<Func<T1, bool>> constraint)
			=> AddConstraint((LambdaExpression)constraint);

		public Pairwiser AddConstraint<T1, T2>(string parameterName1, string parameterName2, Expression<Func<T1, T2, bool>> constraint) 
			=> AddConstraint(new [] { parameterName1, parameterName2 }, constraint.Compile());

		public Pairwiser AddConstraint<T1, T2>(Expression<Func<T1, T2, bool>> constraint)
			=> AddConstraint((LambdaExpression)constraint);

		public Pairwiser AddConstraint<T1, T2, T3>(string parameterName1, string parameterName2, string parameterName3, Expression<Func<T1, T2, T3, bool>> constraint) 
			=> AddConstraint(new [] { parameterName1, parameterName2, parameterName3 }, constraint.Compile());

		public Pairwiser AddConstraint<T1, T2, T3>(Expression<Func<T1, T2, T3, bool>> constraint)
			=> AddConstraint((LambdaExpression)constraint);

		public Pairwiser AddConstraint<T1, T2, T3, T4>(string parameterName1, string parameterName2, string parameterName3, string parameterName4, Expression<Func<T1, T2, T3, T4, bool>> constraint) 
			=> AddConstraint(new [] { parameterName1, parameterName2, parameterName3, parameterName4 }, constraint.Compile());

		public Pairwiser AddConstraint<T1, T2, T3, T4>(Expression<Func<T1, T2, T3, T4, bool>> constraint)
			=> AddConstraint((LambdaExpression)constraint);

		public Pairwiser AddConstraint<T1, T2, T3, T4, T5>(string parameterName1, string parameterName2, string parameterName3, string parameterName4, string parameterName5, Expression<Func<T1, T2, T3, T4, T5, bool>> constraint) 
			=> AddConstraint(new [] { parameterName1, parameterName2, parameterName3, parameterName4, parameterName5 }, constraint.Compile());

		public Pairwiser AddConstraint<T1, T2, T3, T4, T5>(Expression<Func<T1, T2, T3, T4, T5, bool>> constraint)
			=> AddConstraint((LambdaExpression)constraint);

		public Pairwiser AddConstraint<T1, T2, T3, T4, T5, T6>(string parameterName1, string parameterName2, string parameterName3, string parameterName4, string parameterName5, string parameterName6, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> constraint) 
			=> AddConstraint(new [] { parameterName1, parameterName2, parameterName3, parameterName4, parameterName5, parameterName6 }, constraint.Compile());

		public Pairwiser AddConstraint<T1, T2, T3, T4, T5, T6>(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> constraint)
			=> AddConstraint((LambdaExpression)constraint);

		public Pairwiser AddConstraint<T1, T2, T3, T4, T5, T6, T7>(string parameterName1, string parameterName2, string parameterName3, string parameterName4, string parameterName5, string parameterName6, string parameterName7, Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> constraint) 
			=> AddConstraint(new [] { parameterName1, parameterName2, parameterName3, parameterName4, parameterName5, parameterName6, parameterName7 }, constraint.Compile());

		public Pairwiser AddConstraint<T1, T2, T3, T4, T5, T6, T7>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> constraint)
			=> AddConstraint((LambdaExpression)constraint);

		public Pairwiser AddConstraint<T1, T2, T3, T4, T5, T6, T7, T8>(string parameterName1, string parameterName2, string parameterName3, string parameterName4, string parameterName5, string parameterName6, string parameterName7, string parameterName8, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> constraint) 
			=> AddConstraint(new [] { parameterName1, parameterName2, parameterName3, parameterName4, parameterName5, parameterName6, parameterName7, parameterName8 }, constraint.Compile());

		public Pairwiser AddConstraint<T1, T2, T3, T4, T5, T6, T7, T8>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> constraint)
			=> AddConstraint((LambdaExpression)constraint);

	}
}