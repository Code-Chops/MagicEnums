using CodeChops.DomainDrivenDesign.Contracts;

namespace CodeChops.MagicEnums.Json.UnitTests;

public class EntityWithMagicEnumMock : Entity
{
	public override SingletonId<EntityWithMagicEnumMock> Id => SingletonId<EntityWithMagicEnumMock>.Instance;
	
	public int Age { get; init; }
	public MagicEnumWrapperContractMock Wrapper { get; init; } = null!;
}

public record EntityContract(int Age, MagicEnumWrapperContractMock Wrapper) : Contract;


public record EntityAdapter : Adapter<EntityContract, EntityWithMagicEnumMock>
{
	protected override Type GetDomainObjectType() => typeof(EntityWithMagicEnumMock);
	protected override EntityWithMagicEnumMock ConvertContractToDomainObject(Contract contract)
		=> contract is EntityContract entityContract ? new EntityWithMagicEnumMock() { Age = entityContract.Age, Wrapper = entityContract.Wrapper} : null!;

	protected override EntityContract ConvertDomainObjectToContract(IDomainObject domainObject)
		=> domainObject is EntityWithMagicEnumMock entity ? new EntityContract(entity.Age, entity.Wrapper) : null!;
}