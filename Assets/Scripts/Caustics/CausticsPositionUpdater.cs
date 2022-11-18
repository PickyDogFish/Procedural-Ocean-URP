using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CausticsPositionUpdater : MonoBehaviour
{
    [SerializeField] private float topHeight = 0;
    [SerializeField] private Transform viewer;


    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(viewer.position.x, topHeight - transform.localScale.y / 2, viewer.position.z);

    }
}
