using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ManagerUI : MonoBehaviour {

    [SerializeField]
    private TextMeshProUGUI dimensions;

    public void OnDimensionsSliderValueChange(Slider slider)
    {
        this.dimensions.text = $"Dimensions: {(int)slider.value}x{(int)slider.value}";
    }
}
