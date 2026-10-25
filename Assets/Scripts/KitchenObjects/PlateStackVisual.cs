using System.Collections.Generic;
using UnityEngine;

namespace NomadChef.KitchenObjects
{
    /// <summary>Shows one plate model per plate in any IPlateStack on the target object.</summary>
    public class PlateStackVisual : MonoBehaviour
    {
        [SerializeField] private GameObject plateStackObject;
        [SerializeField] private Transform stackRoot;
        [SerializeField] private GameObject plateVisualPrefab;
        [SerializeField] private float plateHeight = 0.035f;

        private readonly List<GameObject> plates = new List<GameObject>();
        private IPlateStack stack;

        private void Awake()
        {
            stack = plateStackObject.GetComponent<IPlateStack>();
            if (stack == null)
                Debug.LogError($"{plateStackObject.name} has no IPlateStack component", this);
        }

        private void OnEnable()
        {
            if (stack == null)
                return;

            stack.PlateCountChanged += Refresh;
            Refresh(stack.PlateCount);
        }

        private void OnDisable()
        {
            if (stack != null)
                stack.PlateCountChanged -= Refresh;
        }

        private void Refresh(int count)
        {
            while (plates.Count < count)
            {
                GameObject plate = Instantiate(plateVisualPrefab, stackRoot);
                plate.transform.localPosition = Vector3.up * (plateHeight * plates.Count);
                plates.Add(plate);
            }

            while (plates.Count > count)
            {
                int last = plates.Count - 1;
                Destroy(plates[last]);
                plates.RemoveAt(last);
            }
        }
    }
}
