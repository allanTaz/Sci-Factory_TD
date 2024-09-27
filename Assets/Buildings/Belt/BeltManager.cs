using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BeltManager : MonoBehaviour
{
    private static BeltManager _instance;
    public static BeltManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("BeltManager");
                _instance = go.AddComponent<BeltManager>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    [SerializeField] private float tickInterval = 0.1f;

    [SerializeField] private List<Belt> registeredBelts = new List<Belt>();
    private Coroutine tickCoroutine;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        StartTickSystem();
    }

    public void RegisterBelt(Belt belt)
    {
        if (!registeredBelts.Contains(belt))
        {
            registeredBelts.Add(belt);
        }
    }

    public void UnregisterBelt(Belt belt)
    {
        registeredBelts.Remove(belt);
    }

    private void StartTickSystem()
    {
        if (tickCoroutine != null)
        {
            StopCoroutine(tickCoroutine);
        }
        tickCoroutine = StartCoroutine(TickRoutine());
    }

    private IEnumerator TickRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(tickInterval);
            UpdateBelts();
        }
    }

    private void UpdateBelts()
    {
        for (int i = registeredBelts.Count - 1; i >= 0; i--)
        {
            if (registeredBelts[i] != null)
            {
                registeredBelts[i].Tick();
            }
            else
            {
                registeredBelts.RemoveAt(i);
            }
        }
    }
}