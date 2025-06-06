using UnityEngine;

public class Tree : MonoBehaviour, IFlammable
{
    public enum State
    {
        Original,
        Burning,
        Burnt
    }

    [SerializeField]
    State BurnState;

    MeshRenderer meshRenderer;
    [SerializeField]
    float burnDuration = 2f;
    float burnTimer = 0f;

    private void Awake()
    {
        BurnState = State.Original;
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
}
