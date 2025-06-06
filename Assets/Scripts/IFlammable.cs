using Unity.VisualScripting;
using UnityEngine;

public interface IFlammable
{
    void Burn();
    void Burnt();
    bool IsBurning();
}
