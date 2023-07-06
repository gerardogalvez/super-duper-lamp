using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine.UI;
using UnityEngine;

public class BuildMenu : MonoBehaviour {

    [SerializeField]
    private GameObject towerSelection;

    [SerializeField]
    private TextMeshProUGUI selectedTowerName;

    [SerializeField]
    private TextMeshProUGUI selectedTowerDescription;

    private Dictionary<Type, TowerInformation> towerInfoDictionary;

    public static BuildMenu instance;

    public Type SelectedTowerType { get; private set; }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        this.PopulateTowerSelectionMenu();
    }

    private void PopulateTowerSelectionMenu()
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        Type[] types = assembly.GetTypes();
        IEnumerable<Type> towerTypes = types.Where((t) => t.BaseType == typeof(BaseTower)).OrderBy(t => ((BaseTower)Activator.CreateInstance(t)).Index);

        this.towerInfoDictionary = new Dictionary<Type, TowerInformation>();
        foreach (var type in towerTypes)
        {
            BaseTower tower = Activator.CreateInstance(type) as BaseTower;
            string towerName = type.GetProperty("Name").GetValue(tower) as string;
            string towerDescription = type.GetProperty("Description").GetValue(tower) as string;
            Sprite towerSprite = type.GetProperty("Sprite").GetValue(tower) as Sprite;
            Sprite pressedTowerSprite = type.GetProperty("PressedSprite").GetValue(tower) as Sprite;
            Sprite highlightedTowerSprite = type.GetProperty("HighlightedSprite").GetValue(tower) as Sprite;
            Sprite disabledTowerSprite = type.GetProperty("DisabledSprite").GetValue(tower) as Sprite;
            var towerInformation = ScriptableObject.CreateInstance(typeof(TowerInformation)) as TowerInformation;
            towerInformation.Init(towerName, towerDescription);
            towerInfoDictionary.Add(tower.GetType(), towerInformation);

            this.AddTowerImage(tower, towerSprite, pressedTowerSprite, highlightedTowerSprite, disabledTowerSprite);
        }

        this.towerSelection.transform.GetChild(0).GetComponent<Toggle>().isOn = true;
        this.towerSelection.transform.GetChild(0).GetComponent<Toggle>().Select();
    }

    private void AddTowerImage(BaseTower tower, Sprite towerSprite, Sprite pressedSprite, Sprite highlightedSprite, Sprite disabledSprite)
    {
        Type towerType = tower.GetType();
        GameObject newObject = new GameObject();

        Image image = newObject.AddComponent<Image>();
        image.sprite = towerSprite;

        Toggle toggle = newObject.AddComponent<Toggle>();
        toggle.group = this.towerSelection.GetComponent<ToggleGroup>();
        toggle.transition = Selectable.Transition.SpriteSwap;
        toggle.spriteState = new SpriteState
        {
            pressedSprite = pressedSprite,
            highlightedSprite = highlightedSprite,
            disabledSprite = disabledSprite,
        };

        toggle.onValueChanged.AddListener((bool on) =>
        {
            if (on)
            {
                this.selectedTowerName.text = this.towerInfoDictionary[towerType].Name;
                this.selectedTowerDescription.text = this.towerInfoDictionary[towerType].Description;
                this.SelectedTowerType = towerType;
                toggle.GetComponent<Image>().sprite = highlightedSprite;
            }
            else
            {
                toggle.GetComponent<Image>().sprite = towerSprite;
            }
        });

        newObject.GetComponent<RectTransform>().SetParent(towerSelection.transform);
        newObject.SetActive(true);
    }
}
