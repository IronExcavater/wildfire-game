using UnityEngine;

public class FlammableObjectSmall: MonoBehaviour, IFlammable
{
    public enum State
    {
        Dry,
        Wet,
        Heating,
        Burning,
        Burnt
    }

    [SerializeField]
    State BurnState;

    MeshRenderer meshRenderer;
    [SerializeField]
    float burnDuration = 2f;
    float burnTimer = 0f;
    float hitpoint = 10f;



    private void Awake()
    {
        BurnState = State.Dry;
        meshRenderer = GetComponent<MeshRenderer>();
    }

    private void Update()
    {
        if (BurnState.Equals(State.Burning))
        {
            if (burnTimer > 0)
            {
                burnTimer -= Time.deltaTime;
            }
            else
            {
                burnTimer = 0;
                Burnt();
            }
        }
    }

    public void Burn()
    {
        BurnState = State.Burning;
        burnTimer = burnDuration;
        //change colour
        meshRenderer.material.color = Color.red;
        //fire particles
    }
    public void Burnt()
    {
        BurnState = State.Burnt;
        //change colour
        meshRenderer.material.color = Color.black;
        //smoke particles
    }
    public void SetBurnDuration(float _duration)
    {
        burnDuration = _duration;
    }
    public bool IsBurning()
    {
        return BurnState.Equals(State.Burning) || BurnState.Equals(State.Burnt);
    }
    public void SetObjectWet()
    {
        BurnState = State.Wet;
    }

    public Transform ObjectTransform()
    {
        return transform;
    }
    public void Heatup(float _damage)
    {
        if(hitpoint > 0)
        {
            hitpoint -= _damage;

        }
    }
    public bool ShouldBurn()
    {
        return hitpoint <= 0;
    }
}
