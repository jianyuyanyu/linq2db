﻿using LinqToDB.Interceptors;

using LinqToDB.Internal.Interceptors;

namespace LinqToDB.Remote
{
	public abstract partial class RemoteDataContextBase :
		IInterceptable<IDataContextInterceptor>,
		IInterceptable<IEntityServiceInterceptor>,
		IInterceptable<IUnwrapDataObjectInterceptor>,
		IInterceptable<IEntityBindingInterceptor>,
		IInterceptable<IQueryExpressionInterceptor>,
		IInterceptable<IExceptionInterceptor>
	{
		// remote context interceptors support is quite limited and supports only IDataContextInterceptor
		// interceptors, but not other interceptors, including AggregatedInterceptor<T>
		IDataContextInterceptor?      IInterceptable<IDataContextInterceptor>.     Interceptor { get; set; }
		IEntityServiceInterceptor?    IInterceptable<IEntityServiceInterceptor>.   Interceptor { get; set; }
		IUnwrapDataObjectInterceptor? IInterceptable<IUnwrapDataObjectInterceptor>.Interceptor { get; set; }
		IEntityBindingInterceptor?    IInterceptable<IEntityBindingInterceptor>.   Interceptor { get; set; }
		IQueryExpressionInterceptor?  IInterceptable<IQueryExpressionInterceptor>. Interceptor { get; set; }
		IExceptionInterceptor?        IInterceptable<IExceptionInterceptor>.       Interceptor { get; set; }

		/// <inheritdoc cref="IDataContext.AddInterceptor(IInterceptor)"/>
		public void AddInterceptor(IInterceptor interceptor)
		{
			ThrowOnDisposed();

			this.AddInterceptorImpl(interceptor);
		}

		/// <inheritdoc cref="IDataContext.RemoveInterceptor(IInterceptor)"/>
		public void RemoveInterceptor(IInterceptor interceptor)
		{
			ThrowOnDisposed();

			this.RemoveInterceptorImpl(interceptor);
		}
	}
}
