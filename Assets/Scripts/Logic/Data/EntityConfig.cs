using UnityEngine;

[CreateAssetMenu(menuName = "ARPG/EntityConfig")]
public class EntityConfig : ScriptableObject
{
    public string EntityName;
    public GameObject Prefab;
    public float MoveSpeed = 5f;
    public float RotationSpeed = 720f;
    public ComboTreeConfig ComboTree;
}