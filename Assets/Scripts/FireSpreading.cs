using UnityEngine;
using UnityEngine.InputSystem;

public class FireSpreading : MonoBehaviour
{
    Camera mainCam;
    Ray rayFromMainCam;
    private void Awake()
    {
        mainCam = Camera.main;
    }
    private void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            rayFromMainCam = mainCam.ScreenPointToRay(Mouse.current.position.ReadValue());
            Debug.Log(Mouse.current.position.ReadValue());
            if (Physics.Raycast(rayFromMainCam, out RaycastHit hit))
            {
                if (hit.transform.CompareTag("Flammable"))
                {
                    StartFire(hit.transform.GetComponent<IFlammable>());
                }
            }
        }
    }

    void StartFire(IFlammable target)
    {
        if (target.IsBurning())
            return;
        target.Burn();
    }
}
