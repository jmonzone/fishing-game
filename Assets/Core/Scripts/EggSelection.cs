using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class EggSelection : MonoBehaviour
{
    public event UnityAction<Pet> OnEggSelected;

    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    public void SetPets(List<Pet> pets)
    {
        var eggControllers = GetComponentsInChildren<EggController>().ToList();

        for (var i = 0; i < eggControllers.Count && i < pets.Count; i++)
        {
            eggControllers[i].Initialize(pets[i]);
            eggControllers[i].OnHatch += pet => OnEggSelected?.Invoke(pet);
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            var ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 50f))
            {
                var egg = hit.transform.GetComponentInParent<EggController>();
                if (egg)
                {
                    egg.Hatch();
                    enabled = false;
                }
            }
        }
    }
}
