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

    [SerializeField]
    LayerMask flammableLayer;
    [SerializeField]
    float fireRange;
    [SerializeField]
    float fireStrength;    
    [SerializeField]
    float fireDamageInterval;

    bool isGameStarted = false;

    private void Awake()
    {
        mainCam = Camera.main;
        burningObjs = new List<IFlammable>();
    }
    private void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame && !isGameStarted)
        {
            isGameStarted = true;
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
        burningObjs.Add(target);

        target.Burn();

        StartCoroutine(SpreadFire());
    }

    IEnumerator SpreadFire()
    {
        while (burningObjs.Count > 0)
        {
            List<IFlammable> ignitedObjs = new List<IFlammable>();
            List<IFlammable> burntObjs = new List<IFlammable>();

            for (int i = 0; i < burningObjs.Count; i++)
            {
                Collider[] hits = Physics.OverlapSphere(burningObjs[i].ObjectTransform().position, fireRange, flammableLayer);
                Debug.DrawLine(burningObjs[i].ObjectTransform().position, burningObjs[i].ObjectTransform().position + Vector3.forward * fireRange, Color.red, fireDamageInterval);
                Debug.DrawLine(burningObjs[i].ObjectTransform().position, burningObjs[i].ObjectTransform().position + Vector3.back * fireRange, Color.red, fireDamageInterval);
                Debug.DrawLine(burningObjs[i].ObjectTransform().position, burningObjs[i].ObjectTransform().position + Vector3.right * fireRange, Color.red, fireDamageInterval);
                Debug.DrawLine(burningObjs[i].ObjectTransform().position, burningObjs[i].ObjectTransform().position + Vector3.left * fireRange, Color.red, fireDamageInterval);

                if (hits.Length > 0)
                {
                    foreach (var hit in hits)
                    {
                        float damage = ((fireRange - Vector3.Distance(burningObjs[i].ObjectTransform().position, hit.ClosestPoint(burningObjs[i].ObjectTransform().position))) / fireRange) * fireStrength;
                        IFlammable flammableObj = hit.transform.GetComponent<IFlammable>();
                        flammableObj.Heatup(damage);
                        if (flammableObj.ShouldBurn())
                        {
                            flammableObj.Burn();
                            ignitedObjs.Add(flammableObj);
                        }
                    }
                }
                if (burningObjs[i].IsBurnt())
                    burntObjs.Add(burningObjs[i]);
            }
                
            foreach (var obj in ignitedObjs)
            {
                burningObjs.Add(obj);
            }
            yield return new WaitForSeconds(fireDamageInterval);

            foreach (var obj in burntObjs)
            {
                burningObjs.Remove(obj);
                if (burningObjs.Count <= 0)
                    break;
            }
        }
        Debug.Log("Lose Game");
    }
}
