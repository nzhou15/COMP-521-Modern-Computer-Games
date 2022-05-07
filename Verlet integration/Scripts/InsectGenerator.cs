using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InsectGenerator : MonoBehaviour
{
    public GameObject insectPrefab;

    public void CreateNewInsect()
    {
        GameObject temp = Instantiate(insectPrefab, transform.position, transform.rotation);
        temp.transform.parent = this.transform;
    }
}
