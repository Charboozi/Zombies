using System.Collections.Generic;
using UnityEngine;

public class AffectableDoorsList : MonoBehaviour
{
    public static AffectableDoorsList Instance { get; private set; }

    [Header("Manually Assigned Doors")]
    [SerializeField] private List<SlidingDoor> doors = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public SlidingDoor GetRandomDoor()
    {
        if (doors.Count == 0) return null;
        return doors[Random.Range(0, doors.Count)];
    }

    public List<SlidingDoor> GetAllDoors() => doors;

    public void AddDoor(SlidingDoor door)
    {
        if (!doors.Contains(door))
            doors.Add(door);
    }

    public void RemoveDoor(SlidingDoor door)
    {
        if (doors.Contains(door))
            doors.Remove(door);
    }
}
