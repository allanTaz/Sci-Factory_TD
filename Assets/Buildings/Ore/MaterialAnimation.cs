using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.Collections.AllocatorManager;

public class MaterialAnimation : MonoBehaviour
{
    public GameObject prefab;
    private Renderer objectRenderer;
    private MaterialPropertyBlock propBlock;
    public GameObject[] gameObjects = new GameObject[4];

    // Example properties - adjust these based on your shader properties
    public float transition = 0f;

    void Awake()
    {
        propBlock = new MaterialPropertyBlock();
    }

    void Start()
    {
        for (int i = 0; i < 3; i++)
        {
            gameObjects[i] = Instantiate(prefab, new Vector3(i, 0, 0), Quaternion.identity);
            if (i == 0)
            {
                transition = -30f;
            }
            else if (i == 1)
            {
                transition = -20f;
            }
            else
            {
                transition = -10f;
            }
            Renderer _renderer = gameObjects[i].GetComponent<Renderer>();
            //_renderer.GetPropertyBlock(block);
            propBlock.SetFloat("_Transition", transition);
            _renderer.SetPropertyBlock(propBlock);
        }
    }

}
