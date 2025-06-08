using Generation.Data;

namespace Generation
{
    public interface IGeneratorPass
    {
        public void Apply(World world);
    }
}
