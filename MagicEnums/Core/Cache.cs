namespace CodeChops.MagicEnums.Core;

[GenerateStronglyTypedId<SingletonId<Cache>>]
public partial class Cache : MutableDictionaryEntity<string, IMagicEnum>
{
	protected override IReadOnlyDictionary<string, IMagicEnum> Dictionary => _dictionary;
	private static readonly Dictionary<string, IMagicEnum> _dictionary = new();

	/// <summary>
	/// Tries to add the magic enum to the cache.
	/// </summary>
	internal static void AddEnum(IMagicEnum magicEnum) 
		=> _dictionary.Add(magicEnum.Name, magicEnum);
}