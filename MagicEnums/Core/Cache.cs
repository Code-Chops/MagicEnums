namespace CodeChops.MagicEnums.Core;

/// <summary>
/// Holds each uninitialized (non-abstract) enum in cache by its name. 
/// </summary>
[GenerateStronglyTypedId<SingletonId<Cache>>]
public partial class Cache : MutableDictionaryEntity<string, IMagicEnum>
{
	public static Cache Instance { get; } = new(); 

	private Cache()
	{
	}
	
	protected override Dictionary<string, IMagicEnum> Dictionary { get; } = new();

	/// <summary>
	/// Tries to add the magic enum to the cache.
	/// </summary>
	internal void AddEnum(IMagicEnum magicEnum) 
		=> this.Dictionary.Add(magicEnum.Name, magicEnum);
}