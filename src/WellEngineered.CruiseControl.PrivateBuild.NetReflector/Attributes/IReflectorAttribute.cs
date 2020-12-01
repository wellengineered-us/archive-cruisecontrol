namespace WellEngineered.CruiseControl.PrivateBuild.NetReflector.Attributes
{
	public interface IReflectorAttribute
	{
		string Name { get; }
		string Description { get; }
        bool HasCustomFactory { get; }
	}
}
