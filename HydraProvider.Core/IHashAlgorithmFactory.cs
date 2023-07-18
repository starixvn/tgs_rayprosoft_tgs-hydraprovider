namespace Tgs.HydraProvider;

public interface IHashAlgorithmFactory
{
	IHashAlgorithm GetHashAlgorithm(string algorithm);
}
