using UnityEngine;

public class TowerInformation : ScriptableObject
{
    public string Name { get; set; }
    public string Description { get; set; }

    public void Init(string name, string description)
    {
        this.Name = name;
        this.Description = description;
    }
}
