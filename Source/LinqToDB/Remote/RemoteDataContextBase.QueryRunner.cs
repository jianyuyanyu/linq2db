﻿using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

using LinqToDB.Data;
using LinqToDB.Internal.SqlQuery;
using LinqToDB.Internal.Common;
using LinqToDB.Internal.SqlProvider;
using LinqToDB.Internal.Linq;
using LinqToDB.Internal.DataProvider;
using LinqToDB.Internal;
using LinqToDB.Internal.Remote;

namespace LinqToDB.Remote
{
	public abstract partial class RemoteDataContextBase
	{
		IQueryRunner IDataContext.GetQueryRunner(Query query, IDataContext parametersContext, int queryNumber, IQueryExpressions expressions, object?[]? parameters, object?[]? preambles)
		{
			ThrowOnDisposed();

			return new QueryRunner(query, queryNumber, this, parametersContext, expressions, parameters, preambles);
		}

		sealed class QueryRunner : QueryRunnerBase
		{
			public QueryRunner(Query query, int queryNumber, RemoteDataContextBase dataContext, IDataContext parametersContext, IQueryExpressions expressions, object?[]? parameters, object?[]? preambles)
				: base(query, queryNumber, dataContext, parametersContext, expressions, parameters, preambles)
			{
				_dataContext = dataContext;
			}

			readonly RemoteDataContextBase _dataContext;

			ILinqService?     _client;
			EvaluationContext _evaluationContext = null!;

			public override Expression? MapperExpression { get; set; }

			protected override void SetQuery(IReadOnlyParameterValues parameterValues, bool forGetSqlText)
			{
				_evaluationContext = new EvaluationContext(parameterValues);
			}

			#region GetSqlText

			public override IReadOnlyList<QuerySql> GetSqlText()
			{
				SetCommand(true);
				return GetSqlTextImpl();
			}

			private IReadOnlyList<QuerySql> GetSqlTextImpl()
			{
				var query        = Query.Queries[QueryNumber];
				var sqlBuilder   = DataContext.CreateSqlBuilder();
				var sqlOptimizer = DataContext.GetSqlOptimizer(DataContext.Options);
				var factory      = sqlOptimizer.CreateSqlExpressionFactory(DataContext.MappingSchema, DataContext.Options);
				var commandCount = sqlBuilder.CommandCount(query.Statement);

				using var sqlStringBuilder = Pools.StringBuilder.Allocate();

				var queries = new QuerySql[commandCount];

				for (var i = 0; i < commandCount; i++)
				{
					AliasesHelper.PrepareQueryAndAliases(new IdentifierServiceSimple(128), query.Statement, query.Aliases, out var aliases);

					var optimizationContext = new OptimizationContext(
						_evaluationContext,
						DataContext.Options,
						DataContext.SqlProviderFlags,
						DataContext.MappingSchema,
						sqlOptimizer.CreateOptimizerVisitor(false),
						sqlOptimizer.CreateConvertVisitor(false),
						factory,
						isParameterOrderDepended : DataContext.SqlProviderFlags.IsParameterOrderDependent,
						isAlreadyOptimizedAndConverted : true,
						parametersNormalizerFactory : static () => NoopQueryParametersNormalizer.Instance);

					var statement = query.Statement.PrepareStatementForSql(optimizationContext);

					sqlBuilder.BuildSql(i, statement, sqlStringBuilder.Value, optimizationContext, aliases, null);

					if (i == 0)
					{
						var queryHints = DataContext.GetNextCommandHints(false);
						if (queryHints != null)
						{
							var querySql = sqlStringBuilder.Value.ToString();

							querySql = sqlBuilder.ApplyQueryHints(querySql, queryHints);

							sqlStringBuilder.Value.Append(querySql);
						}
					}

					DataParameter[]? parameters = null;
					var sql                     = sqlStringBuilder.Value.ToString();

					sqlStringBuilder.Value.Length = 0;

					if (optimizationContext.HasParameters())
					{
						var sqlParameters = optimizationContext.GetParameters();
						parameters = new DataParameter[sqlParameters.Count];

						for (var pIdx = 0; pIdx < sqlParameters.Count; pIdx++)
						{
							var p              = sqlParameters[pIdx];
							var parameterValue = p.GetParameterValue(_evaluationContext.ParameterValues);
							parameters[pIdx] = new DataParameter(p.Name, parameterValue.ProviderValue, parameterValue.DbDataType);
						}
					}

					queries[i] = new QuerySql(sql, parameters ?? Array.Empty<DataParameter>());
				}

				return queries;
			}

			#endregion

			public override void Dispose()
			{
				if (_client != null)
					DisposeClient(_client);

				base.Dispose();
			}

			public override async ValueTask DisposeAsync()
			{
				if (_client != null)
					await DisposeClientAsync(_client).ConfigureAwait(false);

				await base.DisposeAsync().ConfigureAwait(false);
			}

			public override int ExecuteNonQuery()
			{
				SetCommand(false);

				var queryContext = Query.Queries[QueryNumber];

				var q = _dataContext.GetSqlOptimizer(_dataContext.Options).PrepareStatementForRemoting(
					queryContext.Statement, ((IDataContext)_dataContext).SqlProviderFlags, _dataContext.MappingSchema, _dataContext.Options, _evaluationContext);

				var data = LinqServiceSerializer.Serialize(
					_dataContext.SerializationMappingSchema,
					q,
					_evaluationContext.ParameterValues,
					_dataContext.GetNextCommandHints(true),
					_dataContext.Options);

				if (_dataContext._batchCounter > 0)
				{
					_dataContext._queryBatch!.Add(data);
					return -1;
				}

				_client = _dataContext.GetClient();

				return _client.ExecuteNonQuery(_dataContext.ConfigurationString, data);
			}

			public override object? ExecuteScalar()
			{
				if (_dataContext._batchCounter > 0)
					throw new LinqToDBException("Incompatible batch operation.");

				SetCommand(false);

				var queryContext = Query.Queries[QueryNumber];

				var sqlOptimizer = _dataContext.GetSqlOptimizer(_dataContext.Options);
				var q            = sqlOptimizer.PrepareStatementForRemoting(queryContext.Statement, ((IDataContext)_dataContext).SqlProviderFlags, _dataContext.MappingSchema, _dataContext.Options, _evaluationContext);

				var data = LinqServiceSerializer.Serialize(
					_dataContext.SerializationMappingSchema,
					q,
					_evaluationContext.ParameterValues,
					_dataContext.GetNextCommandHints(true),
					_dataContext.Options);

				_client = _dataContext.GetClient();

				var ret = _client.ExecuteScalar(_dataContext.ConfigurationString, data);

				object? result = null;
				if (ret != null)
				{
					var lsr = LinqServiceSerializer.DeserializeResult(_dataContext.SerializationMappingSchema, _dataContext.MappingSchema, _dataContext.Options, ret);
					var value = lsr.Data[0][0];

					if (!string.IsNullOrEmpty(value))
					{
						result = SerializationConverter.Deserialize(_dataContext.SerializationMappingSchema, lsr.FieldTypes[0], value);
					}
				}

				return result;
			}

			public override DataReaderWrapper ExecuteReader()
			{
				_dataContext.ThrowOnDisposed();

				if (_dataContext._batchCounter > 0)
					throw new LinqToDBException("Incompatible batch operation.");

				SetCommand(false);

				var queryContext = Query.Queries[QueryNumber];

				var q = _dataContext.GetSqlOptimizer(_dataContext.Options).PrepareStatementForRemoting(queryContext.Statement, ((IDataContext)_dataContext).SqlProviderFlags, _dataContext.MappingSchema, _dataContext.Options, _evaluationContext);

				var data = LinqServiceSerializer.Serialize(
					_dataContext.SerializationMappingSchema,
					q,
					_evaluationContext.ParameterValues,
					_dataContext.GetNextCommandHints(true),
					_dataContext.Options);

				_client = _dataContext.GetClient();

				var ret = _client.ExecuteReader(_dataContext.ConfigurationString, data);

				var result = LinqServiceSerializer.DeserializeResult(_dataContext.SerializationMappingSchema, _dataContext.MappingSchema, _dataContext.Options, ret);

				return new DataReaderWrapper(new RemoteDataReader(_dataContext.SerializationMappingSchema, result));
			}

			sealed class DataReaderAsync : IDataReaderAsync
			{
				public DataReaderAsync(RemoteDataReader dataReader)
				{
					DataReader = dataReader;
				}

				public DbDataReader DataReader { get; }

				public Task<bool> ReadAsync(CancellationToken cancellationToken)
				{
					return DataReader.ReadAsync(cancellationToken);
				}

				public void Dispose()
				{
					DataReader.Dispose();
				}

				public ValueTask DisposeAsync()
				{
					return DataReader.DisposeAsync();
				}
			}

			public override async Task<IDataReaderAsync> ExecuteReaderAsync(CancellationToken cancellationToken)
			{
				if (_dataContext._batchCounter > 0)
					throw new LinqToDBException("Incompatible batch operation.");

				// preload _configurationInfo asynchronously if needed
				await _dataContext.GetConfigurationInfoAsync(cancellationToken).ConfigureAwait(false);

				SetCommand(false);

				var queryContext = Query.Queries[QueryNumber];

				var q = _dataContext.GetSqlOptimizer(_dataContext.Options).PrepareStatementForRemoting(queryContext.Statement, ((IDataContext)_dataContext).SqlProviderFlags, _dataContext.MappingSchema, _dataContext.Options, _evaluationContext);

				var data = LinqServiceSerializer.Serialize(
					_dataContext.SerializationMappingSchema,
					q,
					_evaluationContext.ParameterValues,
					_dataContext.GetNextCommandHints(true),
					_dataContext.Options);

				_client = _dataContext.GetClient();

				var ret = await _client.ExecuteReaderAsync(_dataContext.ConfigurationString, data, cancellationToken).ConfigureAwait(false);

				var result = LinqServiceSerializer.DeserializeResult(_dataContext.SerializationMappingSchema, _dataContext.MappingSchema, _dataContext.Options, ret);
				var reader = new RemoteDataReader(_dataContext.SerializationMappingSchema, result);

				return new DataReaderAsync(reader);
			}

			public override async Task<object?> ExecuteScalarAsync(CancellationToken cancellationToken)
			{
				if (_dataContext._batchCounter > 0)
					throw new LinqToDBException("Incompatible batch operation.");

				// preload _configurationInfo asynchronously if needed
				await _dataContext.GetConfigurationInfoAsync(cancellationToken).ConfigureAwait(false);

				SetCommand(false);

				var queryContext = Query.Queries[QueryNumber];

				var q = _dataContext.GetSqlOptimizer(_dataContext.Options).PrepareStatementForRemoting(queryContext.Statement, ((IDataContext)_dataContext).SqlProviderFlags, _dataContext.MappingSchema, _dataContext.Options, _evaluationContext);

				var data = LinqServiceSerializer.Serialize(
					_dataContext.SerializationMappingSchema,
					q,
					_evaluationContext.ParameterValues,
					_dataContext.GetNextCommandHints(true),
					_dataContext.Options);

				_client = _dataContext.GetClient();

				var ret = await _client.ExecuteScalarAsync(_dataContext.ConfigurationString, data, cancellationToken)
					.ConfigureAwait(false);

				object? result = null;
				if (ret != null)
				{
					var lsr = LinqServiceSerializer.DeserializeResult(_dataContext.SerializationMappingSchema, _dataContext.MappingSchema, _dataContext.Options, ret);
					var value = lsr.Data[0][0];
					result = SerializationConverter.Deserialize(_dataContext.SerializationMappingSchema, lsr.FieldTypes[0], value);
				}

				return result;
			}

			public override async Task<int> ExecuteNonQueryAsync(CancellationToken cancellationToken)
			{
				// preload _configurationInfo asynchronously if needed
				await _dataContext.GetConfigurationInfoAsync(cancellationToken).ConfigureAwait(false);

				SetCommand(false);

				var queryContext = Query.Queries[QueryNumber];

				var q = _dataContext.GetSqlOptimizer(_dataContext.Options).PrepareStatementForRemoting(queryContext.Statement, ((IDataContext)_dataContext).SqlProviderFlags, _dataContext.MappingSchema, _dataContext.Options, _evaluationContext);

				var data = LinqServiceSerializer.Serialize(
					_dataContext.SerializationMappingSchema,
					q,
					_evaluationContext.ParameterValues,
					_dataContext.GetNextCommandHints(true),
					_dataContext.Options);

				if (_dataContext._batchCounter > 0)
				{
					_dataContext._queryBatch!.Add(data);
					return -1;
				}

				_client = _dataContext.GetClient();

				return await _client.ExecuteNonQueryAsync(_dataContext.ConfigurationString, data, cancellationToken)
					.ConfigureAwait(false);
			}
		}
	}
}
