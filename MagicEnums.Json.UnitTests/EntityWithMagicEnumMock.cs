using CodeChops.Contracts;
using CodeChops.Contracts.Adapters;

namespace CodeChops.MagicEnums.Json.UnitTests;

public class EntityWithMagicEnumMock(
    SingletonId<EntityWithMagicEnumMock> id,
    int age,
    MagicEnumWrapperContractMock wrapper)
    : Entity<SingletonId<EntityWithMagicEnumMock>>(id)
{
    public int Age { get; } = age;
    public MagicEnumWrapperContractMock Wrapper { get; } = wrapper;
}

public record EntityContract(int Age, MagicEnumWrapperContractMock Wrapper) : Contract;

public record EntityAdapter : Adapter<EntityWithMagicEnumMock, EntityContract>
{
    public override EntityWithMagicEnumMock ConvertToObject(EntityContract contract)
        => new(id: SingletonId<EntityWithMagicEnumMock>.Default, age: contract.Age, wrapper: contract.Wrapper);

    public override EntityContract ConvertToContract(EntityWithMagicEnumMock entity)
        => new(entity.Age, entity.Wrapper);
}