using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class NotSoSimpleTower : BaseTower {

    protected override string TowerPrefabName => "NotSoSimpleTowerPrefab";

    public override string Name => "Not So Simple Tower";

    public override string Description => "Description for Not So Simple Tower";

    public override int Index => 1;

    public override Sprite Sprite => Resources.Load<Sprite>(this.TowerDirectory + this.Name);

    public override Sprite PressedSprite => Resources.Load<Sprite>(this.TowerDirectory + this.Name + "Pressed");

    public override Sprite HighlightedSprite => Resources.Load<Sprite>(this.TowerDirectory + this.Name + "Highlighted");

    public override Sprite DisabledSprite => Resources.Load<Sprite>(this.TowerDirectory + this.Name + "Disabled");

    protected override float BaseFireRate => 0.5f;

    protected override float BaseRange => 2.0f;

    private static GameObject TowerPrefab;

    public override GameObject BuildTower(Vector3 position)
    {
        if (NotSoSimpleTower.TowerPrefab == null)
        {
            NotSoSimpleTower.TowerPrefab = Resources.Load(this.TowerPrefabName, typeof(GameObject)) as GameObject;
        }

        GameObject tower = Instantiate(NotSoSimpleTower.TowerPrefab, position, Quaternion.identity);
        tower.AddComponent<NotSoSimpleTower>();
        return tower;
    }

    // Use this for initialization
    protected override void Awake () {
        base.Awake();
    }
    
    // Update is called once per frame
    void Update () {
        
    }
}
