using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;

public class FireSpreading : MonoBehaviour
{
    Camera mainCam;
    Ray rayFromMainCam;

    List<IFlammable> burningObjs;
    List<Ray[]> heatRays;

    [SerializeField]
    LayerMask flammableLayer;
    [SerializeField]
    float fireRange;
    [SerializeField]
    float fireStrength;    
    [SerializeField]
    float fireDamageInterval;

    private void Awake()
    {
        mainCam = Camera.main;
        burningObjs = new List<IFlammable>();
        heatRays = new List<Ray[]>();
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
        burningObjs.Add(target);
        if (burningObjs.Count.Equals(1))
            StartCoroutine(SpreadHeat());
        target.Burn();
    }

    IEnumerator SpreadHeat()
    {
        while (burningObjs.Count > 0)
        {
            for (int i = 0; i < burningObjs.Count; i++)
            {
                if(Physics.Raycast(burningObjs[i].ObjectTransform().position, Vector3.forward, out RaycastHit hit, fireRange, flammableLayer.value)){
                    float damage = ((fireRange - Vector3.Distance(burningObjs[i].ObjectTransform().position, hit.transform.position)) / fireRange) * fireStrength;
                    hit.transform.GetComponent<IFlammable>().Heatup(damage);
                }
                Debug.DrawLine(burningObjs[i].ObjectTransform().position, burningObjs[i].ObjectTransform().position + Vector3.forward * fireRange, Color.red, fireDamageInterval);
            }
            yield return new WaitForSeconds(fireDamageInterval);
        }
    }
}
