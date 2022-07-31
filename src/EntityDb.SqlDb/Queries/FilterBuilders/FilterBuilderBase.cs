using EntityDb.Abstractions.Queries.FilterBuilders;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Envelopes;
using EntityDb.SqlDb.Documents;
using EntityDb.SqlDb.Queries.Definitions.Filter;
using System;
using System.Collections.Generic;

namespace EntityDb.SqlDb.Queries.FilterBuilders;

internal abstract class FilterBuilderBase : IFilterBuilder<IFilterDefinition>
{
    public IFilterDefinition TransactionTimeStampGte(TimeStamp timeStamp)
    {
        return Gte(nameof(ITransactionDocument.TransactionTimeStamp), timeStamp);
    }

    public IFilterDefinition TransactionTimeStampLte(TimeStamp timeStamp)
    {
        return Lte(nameof(ITransactionDocument.TransactionTimeStamp), timeStamp);
    }

    public IFilterDefinition TransactionIdIn(params Id[] transactionIds)
    {
        return In(nameof(ITransactionDocument.TransactionId), transactionIds);
    }

    protected static IFilterDefinition DataTypeIn(params Type[] dataTypes)
    {
        var typeNames = dataTypes.GetTypeHeaderValues();

        return In(nameof(ITransactionDocument.DataType), typeNames);
    }

    public IFilterDefinition Not(IFilterDefinition filter)
    {
        return new NotFilterDefinition(filter);
    }

    public IFilterDefinition And(params IFilterDefinition[] filters)
    {
        return new AndFilterDefinition(filters);
    }

    public IFilterDefinition Or(params IFilterDefinition[] filters)
    {
        return new OrFilterDefinition(filters);
    }

    protected static IFilterDefinition Eq<TValue>(string fieldName, TValue value)
        where TValue : notnull
    {
        return new EqFilterDefinition(fieldName, value);
    }

    protected static IFilterDefinition In<TValue>(string fieldName, IEnumerable<TValue> values)
        where TValue : notnull
    {
        return new InFilterDefinition(fieldName, values);
    }

    protected static IFilterDefinition AnyIn<TValue>(string fieldName, IEnumerable<TValue> values)
        where TValue : notnull
    {
        return new AnyInFilterDefinition(fieldName, values);
    }

    protected static IFilterDefinition Gte<TValue>(string fieldName, TValue value)
        where TValue : notnull
    {
        return new GteFilterDefinition(fieldName, value);
    }

    protected static IFilterDefinition Lte<TValue>(string fieldName, TValue value)
        where TValue : notnull
    {
        return new LteFilterDefinition(fieldName, value);
    }
}
