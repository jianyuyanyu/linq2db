﻿using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace LinqToDB.Data
{
	using Async;
	using Common;
	using Interceptors;
	using RetryPolicy;
	using Tools;

	public partial class DataConnection
	{
#if NET6_0_OR_GREATER
		/// <summary>
		/// This is internal API and is not intended for use by Linq To DB applications.
		/// </summary>
		public async ValueTask DisposeCommandAsync()
		{
			if (_command != null)
			{
				await DataProvider.DisposeCommandAsync(_command).ConfigureAwait(Common.Configuration.ContinueOnCapturedContext);
				_command = null;
			}
		}
#endif

		/// <summary>
		/// Starts new transaction asynchronously for current connection with default isolation level. If connection already has transaction, it will be rolled back.
		/// </summary>
		/// <param name="cancellationToken">Asynchronous operation cancellation token.</param>
		/// <returns>Database transaction object.</returns>
		public virtual async Task<DataConnectionTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
		{
			if (!DataProvider.TransactionsSupported)
				return new(this);

			await EnsureConnectionAsync(cancellationToken).ConfigureAwait(Common.Configuration.ContinueOnCapturedContext);

			// If transaction is open, we dispose it, it will rollback all changes.
			//
			if (TransactionAsync != null) await TransactionAsync.DisposeAsync().ConfigureAwait(Common.Configuration.ContinueOnCapturedContext);

			var dataConnectionTransaction = await TraceActionAsync(
				this,
				TraceOperation.BeginTransaction,
				static _ => "BeginTransactionAsync",
				default(object?),
				static async (dataConnection, _, cancellationToken) =>
				{
			// Create new transaction object.
			//
					dataConnection.TransactionAsync = await dataConnection._connection!.BeginTransactionAsync(cancellationToken).ConfigureAwait(Common.Configuration.ContinueOnCapturedContext);

					dataConnection._closeTransaction = true;

			// If the active command exists.
			//
					if (dataConnection._command != null)
						dataConnection._command.Transaction = dataConnection.Transaction;

					return new DataConnectionTransaction(dataConnection);
				}, cancellationToken)
				.ConfigureAwait(Common.Configuration.ContinueOnCapturedContext);

			return dataConnectionTransaction;
		}

		/// <summary>
		/// Starts new transaction asynchronously for current connection with specified isolation level. If connection already have transaction, it will be rolled back.
		/// </summary>
		/// <param name="isolationLevel">Transaction isolation level.</param>
		/// <param name="cancellationToken">Asynchronous operation cancellation token.</param>
		/// <returns>Database transaction object.</returns>
		public virtual async Task<DataConnectionTransaction> BeginTransactionAsync(IsolationLevel isolationLevel, CancellationToken cancellationToken = default)
		{
			if (!DataProvider.TransactionsSupported)
				return new(this);

			await EnsureConnectionAsync(cancellationToken).ConfigureAwait(Common.Configuration.ContinueOnCapturedContext);

			// If transaction is open, we dispose it, it will rollback all changes.
			//
			if (TransactionAsync != null) await TransactionAsync.DisposeAsync().ConfigureAwait(Common.Configuration.ContinueOnCapturedContext);

			var dataConnectionTransaction = await TraceActionAsync(
				this,
				TraceOperation.BeginTransaction,
				static il => $"BeginTransactionAsync({il})",
				isolationLevel,
				static async (dataConnection, isolationLevel, cancellationToken) =>
				{
			// Create new transaction object.
			//
					dataConnection.TransactionAsync = await dataConnection._connection!.BeginTransactionAsync(isolationLevel, cancellationToken).ConfigureAwait(Common.Configuration.ContinueOnCapturedContext);

					dataConnection._closeTransaction = true;

			// If the active command exists.
			//
					if (dataConnection._command != null)
						dataConnection._command.Transaction = dataConnection.Transaction;

					return new DataConnectionTransaction(dataConnection);
				}, cancellationToken)
				.ConfigureAwait(Common.Configuration.ContinueOnCapturedContext);

			return dataConnectionTransaction;
		}

		/// <summary>
		/// Ensure that database connection opened. If opened connection missing, it will be opened asynchronously.
		/// </summary>
		/// <param name="cancellationToken">Asynchronous operation cancellation token.</param>
		/// <returns>Async operation task.</returns>
		public async Task EnsureConnectionAsync(CancellationToken cancellationToken = default)
		{
			CheckAndThrowOnDisposed();

			try
			{
				if (_connection == null)
				{
					DbConnection connection;
					if (_connectionFactory != null)
						connection = _connectionFactory(Options);
					else
						connection = DataProvider.CreateConnection(ConnectionString!);

					_connection = AsyncFactory.Create(connection);

					if (RetryPolicy != null)
						_connection = new RetryingDbConnection(this, _connection, RetryPolicy);
				}

				if (_connection.State == ConnectionState.Closed)
				{
					var interceptor = ((IInterceptable<IConnectionInterceptor>)this).Interceptor;
					if (interceptor is not null)
					{
						await using (ActivityService.StartAndConfigureAwait(ActivityID.ConnectionInterceptorConnectionOpeningAsync))
							await interceptor.ConnectionOpeningAsync(new(this), _connection.Connection, cancellationToken)
								.ConfigureAwait(Common.Configuration.ContinueOnCapturedContext);
					}

					await _connection.OpenAsync(cancellationToken).ConfigureAwait(Common.Configuration.ContinueOnCapturedContext);

					_closeConnection = true;

					if (interceptor is not null)
					{
						await using (ActivityService.StartAndConfigureAwait(ActivityID.ConnectionInterceptorConnectionOpenedAsync))
							await interceptor.ConnectionOpenedAsync(new (this), _connection.Connection, cancellationToken)
								.ConfigureAwait(Common.Configuration.ContinueOnCapturedContext);
					}
				}
			}
			catch (Exception ex)
			{
				if (TraceSwitchConnection.TraceError)
				{
					OnTraceConnection(new TraceInfo(this, TraceInfoStep.Error, TraceOperation.Open, true)
					{
						TraceLevel = TraceLevel.Error,
						StartTime  = DateTime.UtcNow,
						Exception  = ex,
					});
				}

				throw;
			}
		}

		/// <summary>
		/// Commits started (if any) transaction, associated with connection.
		/// If underlying provider doesn't support asynchronous commit, it will be performed synchronously.
		/// </summary>
		/// <param name="cancellationToken">Asynchronous operation cancellation token.</param>
		/// <returns>Asynchronous operation completion task.</returns>
		public virtual async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
		{
			if (TransactionAsync != null)
			{
				await TraceActionAsync(
					this,
					TraceOperation.CommitTransaction,
					static _ => "CommitTransactionAsync",
					default(object?),
					static async (dataConnection, _, cancellationToken) =>
					{
						await dataConnection.TransactionAsync!.CommitAsync(cancellationToken).ConfigureAwait(Common.Configuration.ContinueOnCapturedContext);

						if (dataConnection._closeTransaction)
						{
							await dataConnection.TransactionAsync.DisposeAsync().ConfigureAwait(Common.Configuration.ContinueOnCapturedContext);
							dataConnection.TransactionAsync = null;

							if (dataConnection._command != null)
								dataConnection._command.Transaction = null;
						}
						return _;
					}, cancellationToken)
					.ConfigureAwait(Common.Configuration.ContinueOnCapturedContext);
			}
		}

		/// <summary>
		/// Rollbacks started (if any) transaction, associated with connection.
		/// If underlying provider doesn't support asynchonous commit, it will be performed synchonously.
		/// </summary>
		/// <param name="cancellationToken">Asynchronous operation cancellation token.</param>
		/// <returns>Asynchronous operation completion task.</returns>
		public virtual async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
		{
			if (TransactionAsync != null)
			{
				await TraceActionAsync(
					this,
					TraceOperation.RollbackTransaction,
					static _ => "RollbackTransactionAsync",
					default(object?),
					static async (dataConnection, _, cancellationToken) =>
					{
						await dataConnection.TransactionAsync!.RollbackAsync(cancellationToken).ConfigureAwait(Common.Configuration.ContinueOnCapturedContext);

						if (dataConnection._closeTransaction)
						{
							await dataConnection.TransactionAsync.DisposeAsync().ConfigureAwait(Common.Configuration.ContinueOnCapturedContext);
							dataConnection.TransactionAsync = null;

							if (dataConnection._command != null)
								dataConnection._command.Transaction = null;
						}
						return _;
					}, cancellationToken)
					.ConfigureAwait(Common.Configuration.ContinueOnCapturedContext);
			}
		}

		/// <summary>
		/// Dispose started (if any) transaction, associated with connection.
		/// If underlying provider doesn't support asynchonous disposal, it will be performed synchonously.
		/// </summary>
		/// <returns>Asynchronous operation completion task.</returns>
		public virtual async Task DisposeTransactionAsync()
		{
			if (TransactionAsync != null)
			{
				await TraceActionAsync(
					this,
					TraceOperation.DisposeTransaction,
					static _ => "DisposeTransactionAsync",
					default(object?),
					static async (dataConnection, _, cancellationToken) =>
					{
						await dataConnection.TransactionAsync!.DisposeAsync().ConfigureAwait(Common.Configuration.ContinueOnCapturedContext);
						dataConnection.TransactionAsync = null;

						if (dataConnection._command != null)
							dataConnection._command.Transaction = null;

						return _;
					}, default)
					.ConfigureAwait(Common.Configuration.ContinueOnCapturedContext);
			}
		}

		/// <summary>
		/// Closes and dispose associated underlying database transaction/connection asynchronously.
		/// </summary>
		/// <returns>Asynchronous operation completion task.</returns>
		public virtual async Task CloseAsync()
		{
			var interceptor = ((IInterceptable<IDataContextInterceptor>)this).Interceptor;
			if (interceptor != null)
			{
				await using (ActivityService.StartAndConfigureAwait(ActivityID.DataContextInterceptorOnClosingAsync))
					await interceptor.OnClosingAsync(new (this)).ConfigureAwait(Common.Configuration.ContinueOnCapturedContext);
			}

#if NET6_0_OR_GREATER
			await DisposeCommandAsync().ConfigureAwait(Common.Configuration.ContinueOnCapturedContext);
#else
			DisposeCommand();
#endif

			if (TransactionAsync != null && _closeTransaction)
			{
				await TransactionAsync.DisposeAsync().ConfigureAwait(Common.Configuration.ContinueOnCapturedContext);
				TransactionAsync = null;
			}

			if (_connection != null)
			{
				if (_disposeConnection)
				{
					await _connection.DisposeAsync().ConfigureAwait(Common.Configuration.ContinueOnCapturedContext);
					_connection = null;
				}
				else if (_closeConnection)
					await _connection.CloseAsync().ConfigureAwait(Common.Configuration.ContinueOnCapturedContext);
			}

			if (interceptor != null)
			{
				await using (ActivityService.StartAndConfigureAwait(ActivityID.DataContextInterceptorOnClosedAsync))
					await interceptor.OnClosedAsync(new(this)).ConfigureAwait(Common.Configuration.ContinueOnCapturedContext);
			}
		}

		/// <summary>
		/// Disposes connection asynchronously.
		/// </summary>
		/// <returns>Asynchronous operation completion task.</returns>
		public async ValueTask DisposeAsync()
		{
			Disposed = true;
			await CloseAsync().ConfigureAwait(Common.Configuration.ContinueOnCapturedContext);
		}

		protected static async Task<TResult> TraceActionAsync<TContext, TResult>(
			DataConnection                                                   dataConnection,
			TraceOperation                                                   traceOperation,
			Func<TContext, string?>?                                         commandText,
			TContext                                                         context,
			Func<DataConnection, TContext, CancellationToken, Task<TResult>> action,
			CancellationToken                                                cancellationToken)
		{
			var now       = DateTime.UtcNow;
			Stopwatch? sw = null;
			var sql       = dataConnection.TraceSwitchConnection.TraceInfo ? commandText?.Invoke(context) : null;

			if (dataConnection.TraceSwitchConnection.TraceInfo)
			{
				sw = Stopwatch.StartNew();
				dataConnection.OnTraceConnection(new TraceInfo(dataConnection, TraceInfoStep.BeforeExecute, traceOperation, true)
				{
					TraceLevel  = TraceLevel.Info,
					CommandText = sql,
					StartTime   = now,
				});
			}

			try
			{
				var actionResult = await action(dataConnection, context, cancellationToken).ConfigureAwait(Common.Configuration.ContinueOnCapturedContext);

				if (dataConnection.TraceSwitchConnection.TraceInfo)
				{
					dataConnection.OnTraceConnection(new TraceInfo(dataConnection, TraceInfoStep.AfterExecute, traceOperation, true)
					{
						TraceLevel    = TraceLevel.Info,
						CommandText   = sql,
						StartTime     = now,
						ExecutionTime = sw?.Elapsed
					});
				}

				return actionResult;
			}
			catch (Exception ex)
			{
				if (dataConnection.TraceSwitchConnection.TraceError)
				{
					dataConnection.OnTraceConnection(new TraceInfo(dataConnection, TraceInfoStep.Error, traceOperation, true)
					{
						TraceLevel    = TraceLevel.Error,
						CommandText   = dataConnection.TraceSwitchConnection.TraceInfo ? sql : commandText?.Invoke(context),
						StartTime     = now,
						ExecutionTime = sw?.Elapsed,
						Exception     = ex,
					});
				}

				throw;
			}
		}

		#region ExecuteNonQueryAsync

		protected virtual async Task<int> ExecuteNonQueryAsync(CancellationToken cancellationToken)
		{
			var result = Option<int>.None;

			if (((IInterceptable<ICommandInterceptor>)this).Interceptor != null)
				await using (ActivityService.StartAndConfigureAwait(ActivityID.CommandInterceptorExecuteNonQueryAsync))
					result = await ((IInterceptable<ICommandInterceptor>)this).Interceptor.ExecuteNonQueryAsync(new (this), CurrentCommand!, result, cancellationToken)
						.ConfigureAwait(Common.Configuration.ContinueOnCapturedContext);

			await using (ActivityService.StartAndConfigureAwait(ActivityID.CommandExecuteNonQueryAsync))
			{
				return result.HasValue
					? result.Value
					: await CurrentCommand!.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(Common.Configuration.ContinueOnCapturedContext);
			}
		}

		internal async Task<int> ExecuteNonQueryDataAsync(CancellationToken cancellationToken)
		{
			if (TraceSwitchConnection.Level == TraceLevel.Off)
				await using ((DataProvider.ExecuteScope(this) ?? EmptyIAsyncDisposable.Instance).ConfigureAwait(Common.Configuration.ContinueOnCapturedContext))
					return await ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(Common.Configuration.ContinueOnCapturedContext);

			var now = DateTime.UtcNow;
			var sw  = Stopwatch.StartNew();

			if (TraceSwitchConnection.TraceInfo)
			{
				OnTraceConnection(new TraceInfo(this, TraceInfoStep.BeforeExecute, TraceOperation.ExecuteNonQuery, true)
				{
					TraceLevel     = TraceLevel.Info,
					StartTime      = now,
					Command        = CurrentCommand,
				});
			}

			try
			{
				int ret;
				await using ((DataProvider.ExecuteScope(this) ?? EmptyIAsyncDisposable.Instance).ConfigureAwait(Common.Configuration.ContinueOnCapturedContext))
					ret = await ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(Common.Configuration.ContinueOnCapturedContext);

				if (TraceSwitchConnection.TraceInfo)
				{
					OnTraceConnection(new TraceInfo(this, TraceInfoStep.AfterExecute, TraceOperation.ExecuteNonQuery, true)
					{
						TraceLevel      = TraceLevel.Info,
						Command         = CurrentCommand,
						StartTime       = now,
						ExecutionTime   = sw.Elapsed,
						RecordsAffected = ret,
					});
				}

				return ret;
			}
			catch (Exception ex)
			{
				if (TraceSwitchConnection.TraceError)
				{
					OnTraceConnection(new TraceInfo(this, TraceInfoStep.Error, TraceOperation.ExecuteNonQuery, true)
					{
						TraceLevel     = TraceLevel.Error,
						Command        = CurrentCommand,
						StartTime      = now,
						ExecutionTime  = sw.Elapsed,
						Exception      = ex,
					});
				}

				throw;
			}
		}

		#endregion

		#region ExecuteScalarAsync

		protected virtual async Task<object?> ExecuteScalarAsync(CancellationToken cancellationToken)
		{
			var result = Option<object?>.None;

			if (this is IInterceptable<ICommandInterceptor> { Interceptor: { } interceptor })
			{
				await using (ActivityService.StartAndConfigureAwait(ActivityID.CommandInterceptorExecuteScalarAsync))
					result = await interceptor.ExecuteScalarAsync(new (this), CurrentCommand!, result, cancellationToken)
						.ConfigureAwait(Common.Configuration.ContinueOnCapturedContext);
			}

			await using (ActivityService.StartAndConfigureAwait(ActivityID.CommandExecuteScalarAsync))
			{
				return result.HasValue
					? result.Value
					: await CurrentCommand!.ExecuteScalarAsync(cancellationToken).ConfigureAwait(Common.Configuration.ContinueOnCapturedContext);
			}
		}

		internal async Task<object?> ExecuteScalarDataAsync(CancellationToken cancellationToken)
		{
			if (TraceSwitchConnection.Level == TraceLevel.Off)
				await using ((DataProvider.ExecuteScope(this) ?? EmptyIAsyncDisposable.Instance).ConfigureAwait(Common.Configuration.ContinueOnCapturedContext))
					return await ExecuteScalarAsync(cancellationToken).ConfigureAwait(Common.Configuration.ContinueOnCapturedContext);

			var now = DateTime.UtcNow;
			var sw  = Stopwatch.StartNew();

			if (TraceSwitchConnection.TraceInfo)
			{
				OnTraceConnection(new TraceInfo(this, TraceInfoStep.BeforeExecute, TraceOperation.ExecuteScalar, true)
				{
					TraceLevel     = TraceLevel.Info,
					Command        = CurrentCommand,
					StartTime      = now,
				});
			}

			try
			{
				object? ret;
				await using ((DataProvider.ExecuteScope(this) ?? EmptyIAsyncDisposable.Instance).ConfigureAwait(Common.Configuration.ContinueOnCapturedContext))
					ret = await ExecuteScalarAsync(cancellationToken).ConfigureAwait(Common.Configuration.ContinueOnCapturedContext);

				if (TraceSwitchConnection.TraceInfo)
				{
					OnTraceConnection(new TraceInfo(this, TraceInfoStep.AfterExecute, TraceOperation.ExecuteScalar, true)
					{
						TraceLevel      = TraceLevel.Info,
						Command         = CurrentCommand,
						StartTime       = now,
						ExecutionTime   = sw.Elapsed,
					});
				}

				return ret;
			}
			catch (Exception ex)
			{
				if (TraceSwitchConnection.TraceError)
				{
					OnTraceConnection(new TraceInfo(this, TraceInfoStep.Error, TraceOperation.ExecuteScalar, true)
					{
						TraceLevel     = TraceLevel.Error,
						Command        = CurrentCommand,
						StartTime      = now,
						ExecutionTime  = sw.Elapsed,
						Exception      = ex,
					});
				}

				throw;
			}
		}

		#endregion

		#region ExecuteReaderAsync

		protected virtual async Task<DataReaderWrapper> ExecuteReaderAsync(
			CommandBehavior   commandBehavior,
			CancellationToken cancellationToken)
		{
			var result = Option<DbDataReader>.None;

			var interceptor = ((IInterceptable<ICommandInterceptor>)this).Interceptor;
			if (interceptor != null)
			{
				await using (ActivityService.StartAndConfigureAwait(ActivityID.CommandInterceptorExecuteReaderAsync))
					result = await interceptor.ExecuteReaderAsync(new (this), CurrentCommand!, commandBehavior, result, cancellationToken)
						.ConfigureAwait(Common.Configuration.ContinueOnCapturedContext);
			}

			DbDataReader? dr;

			await using (ActivityService.StartAndConfigureAwait(ActivityID.CommandExecuteReaderAsync))
			{
				dr = result.HasValue
					? result.Value
					: await CurrentCommand!.ExecuteReaderAsync(commandBehavior, cancellationToken).ConfigureAwait(Common.Configuration.ContinueOnCapturedContext);
			}

			if (interceptor != null)
			{
				using (ActivityService.Start(ActivityID.CommandInterceptorAfterExecuteReader))
					interceptor.AfterExecuteReader(new (this), _command!, commandBehavior, dr);
			}

			var wrapper = new DataReaderWrapper(this, dr, CurrentCommand);
			_command    = null;

			return wrapper;
		}

		internal async Task<DataReaderWrapper> ExecuteDataReaderAsync(
			CommandBehavior commandBehavior,
			CancellationToken cancellationToken)
		{
			if (TraceSwitchConnection.Level == TraceLevel.Off)
				await using ((DataProvider.ExecuteScope(this) ?? EmptyIAsyncDisposable.Instance).ConfigureAwait(Common.Configuration.ContinueOnCapturedContext))
					return await ExecuteReaderAsync(commandBehavior, cancellationToken)
						.ConfigureAwait(Common.Configuration.ContinueOnCapturedContext);

			var now = DateTime.UtcNow;
			var sw  = Stopwatch.StartNew();

			if (TraceSwitchConnection.TraceInfo)
			{
				OnTraceConnection(new TraceInfo(this, TraceInfoStep.BeforeExecute, TraceOperation.ExecuteReader, true)
				{
					TraceLevel     = TraceLevel.Info,
					Command        = CurrentCommand,
					StartTime      = now,
				});
			}

			try
			{
				DataReaderWrapper ret;

				await using ((DataProvider.ExecuteScope(this) ?? EmptyIAsyncDisposable.Instance).ConfigureAwait(Common.Configuration.ContinueOnCapturedContext))
					ret = await ExecuteReaderAsync(commandBehavior, cancellationToken)
						.ConfigureAwait(Common.Configuration.ContinueOnCapturedContext);

				if (TraceSwitchConnection.TraceInfo)
				{
					OnTraceConnection(new TraceInfo(this, TraceInfoStep.AfterExecute, TraceOperation.ExecuteReader, true)
					{
						TraceLevel     = TraceLevel.Info,
						Command        = ret.Command,
						StartTime      = now,
						ExecutionTime  = sw.Elapsed,
					});
				}

				return ret;
			}
			catch (Exception ex)
			{
				if (TraceSwitchConnection.TraceError)
				{
					OnTraceConnection(new TraceInfo(this, TraceInfoStep.Error, TraceOperation.ExecuteReader, true)
					{
						TraceLevel     = TraceLevel.Error,
						Command        = CurrentCommand,
						StartTime      = now,
						ExecutionTime  = sw.Elapsed,
						Exception      = ex,
					});
				}

				throw;
			}
		}

		#endregion
	}
}
