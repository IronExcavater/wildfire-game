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


    [SerializeField]
    LayerMask flammableLayer;       
    [SerializeField]
    LayerMask burningLayer;    
    [SerializeField]
    LayerMask burntLayer;

    Color originalColor;

    private void Awake()
    {
        BurnState = State.Dry;
        meshRenderer = GetComponent<MeshRenderer>();
        originalColor = meshRenderer.material.color;
        this.gameObject.layer = (int) Mathf.Log(flammableLayer.value, 2);
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
        this.gameObject.layer = (int) Mathf.Log(burningLayer.value, 2);

        //change colour
        meshRenderer.material.color = Color.red;
        //fire particles
    }
    public void Burnt()
    {
        BurnState = State.Burnt;
        this.gameObject.layer = (int) Mathf.Log(burntLayer.value, 2);

        //change colour
        meshRenderer.material.color = Color.black;
        //smoke particles
    }
    public void SetBurnDuration(float _duration)
    {
        burnDuration = _duration;
    }
    public bool IsBurnt()
    {
        return BurnState.Equals(State.Burnt);
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
            BurnState = State.Heating;
            hitpoint -= _damage;

            Color newColour = Color.Lerp(Color.red, originalColor, hitpoint / 10f);
            meshRenderer.material.color = newColour;
        }
    }
    public bool ShouldBurn()
    {
        return hitpoint <= 0;
    }
}
