namespace CodeChops.MagicEnums.Attributes;

/// <summary>
/// When applied to a class, this attribute will source generate a clone of this class. 
/// This clone has the accessibility modifier changed from 'public' to 'internal'.
/// The namespace will also be changed to the provided target namespace. 
/// If not provided, the new namespace will be {CurrentNamespace}.Internal.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class CloneAsInternal : Attribute
{
	public string? TargetNamespace { get; }

	/// <param name="targetNamespace">
	/// The namespace of the cloned class. 
	/// If not provided, the new namespace will be {CurrentNamespace}.Internal.
	/// </param>
	public CloneAsInternal(string? targetNamespace = null)
	{
		this.TargetNamespace = targetNamespace;
	}
}