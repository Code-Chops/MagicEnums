using CodeChops.Contracts;

namespace CodeChops.MagicEnums.Json.UnitTests;

public class EntityWithMagicEnumMock : Entity<SingletonId<EntityWithMagicEnumMock>>
{
	public int Age { get; init; }
	public MagicEnumWrapperContractMock Wrapper { get; init; } = null!;

	public EntityWithMagicEnumMock(SingletonId<EntityWithMagicEnumMock> id) 
		: base(id)
	{
	}
}

public record EntityContract(int Age, MagicEnumWrapperContractMock Wrapper) : Contract;

public record EntityAdapter : Adapter<EntityWithMagicEnumMock, EntityContract>
{
	protected override Type GetDomainObjectType() => typeof(EntityWithMagicEnumMock);
	protected override EntityWithMagicEnumMock ConvertContractToDomainObject(Contract contract)
		=> contract is EntityContract(var age, var wrapper) ? new EntityWithMagicEnumMock(SingletonId<EntityWithMagicEnumMock>.Instance) { Age = age, Wrapper = wrapper} : null!;

	protected override EntityContract ConvertDomainObjectToContract(IDomainObject domainObject)
		=> domainObject is EntityWithMagicEnumMock entity ? new EntityContract(entity.Age, entity.Wrapper) : null!;
}