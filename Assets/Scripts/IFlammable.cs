using Unity.VisualScripting;
using UnityEngine;

public interface IFlammable
{
    Transform ObjectTransform();
    void Burn();
    void Burnt();
    bool IsBurnt();
    void Heatup(float _damage);
    bool ShouldBurn();
}
