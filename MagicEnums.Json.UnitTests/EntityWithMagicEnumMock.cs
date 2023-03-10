using CodeChops.Contracts;

namespace CodeChops.MagicEnums.Json.UnitTests;

public class EntityWithMagicEnumMock : Entity<SingletonId<EntityWithMagicEnumMock>>
{
	public int Age { get; init; }
	public MagicEnumWrapperContractMock Wrapper { get; init; } = null!;
}

public record EntityContract(int Age, MagicEnumWrapperContractMock Wrapper) : Contract;

public record EntityAdapter : Adapter<EntityWithMagicEnumMock, EntityContract>
{
	public override EntityWithMagicEnumMock ConvertToObject(EntityContract contract)
		=> new() { Id = SingletonId<EntityWithMagicEnumMock>.Instance, Age = contract.Age, Wrapper = contract.Wrapper};

	public override EntityContract ConvertToContract(EntityWithMagicEnumMock entity) 
		=> new(entity.Age, entity.Wrapper);
}