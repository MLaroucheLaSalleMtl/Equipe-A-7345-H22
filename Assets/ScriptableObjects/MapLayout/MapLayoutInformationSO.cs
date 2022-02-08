using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable/MapLayout/Information", fileName = "MapLayoutInformationSO")]
public class MapLayoutInformationSO : ScriptableObject
{
    [SerializeField] private List<Vector2> roomPositions;
    [SerializeField] private List<Room> rooms;

    public List<Vector2> RoomPositions { get => roomPositions; set => roomPositions = value; }
    public List<Room> Rooms { get => rooms; set => rooms = value; }
}
