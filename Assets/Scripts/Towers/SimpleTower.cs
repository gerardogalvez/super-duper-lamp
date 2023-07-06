﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class SimpleTower : BaseTower {

    protected override string TowerPrefabName => "SimpleTowerPrefab";

    public override string Name => "Simple Tower";

    public override string Description => "Description for Simple Tower";

    public override int Index => 0;

    public override Sprite Sprite => Resources.Load<Sprite>(this.TowerDirectory + this.Name);

    public override Sprite PressedSprite => Resources.Load<Sprite>(this.TowerDirectory + this.Name + "Pressed");

    public override Sprite HighlightedSprite => Resources.Load<Sprite>(this.TowerDirectory + this.Name + "Highlighted");

    public override Sprite DisabledSprite => Resources.Load<Sprite>(this.TowerDirectory + this.Name + "Disabled");

    protected override float BaseFireRate => 0.5f;

    protected override float BaseRange => 1.0f;

    private static GameObject TowerPrefab;

    public override GameObject BuildTower(Vector3 position)
    {
        if (SimpleTower.TowerPrefab == null)
        {
            SimpleTower.TowerPrefab = Resources.Load(this.TowerPrefabName, typeof(GameObject)) as GameObject;
        }

        GameObject tower = Instantiate(SimpleTower.TowerPrefab, position, Quaternion.identity);
        tower.AddComponent<SimpleTower>();
        return tower;
    }

    // Use this for initialization
    protected override void Awake () {
        base.Awake();
        Debug.Log("SimpleTower::Start called by " + this.gameObject.name);
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
