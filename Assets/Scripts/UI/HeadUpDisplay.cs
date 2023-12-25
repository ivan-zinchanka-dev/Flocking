using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class HeadUpDisplay : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _totalEntitiesTextMesh;
        [SerializeField] private Button _addFoodButton;
        
        [SerializeField] private TextMeshProUGUI _reproductionRateTextMesh;
        [SerializeField] private Slider _reproductionRateSlider;
        
        [Space]
        [SerializeField] private InterestsManager _interestsManager;
        [SerializeField] private Flocking _flocking;

        private void Awake()
        {
            UpdateTotalEntitiesText(_flocking.EntitiesCount);
            UpdateReproductionRate(_flocking.ReproductionRate);
            _reproductionRateSlider.value = _flocking.ReproductionRate;
        }

        private void OnEnable()
        {
            _flocking.OnEntitiesCountChanged += UpdateTotalEntitiesText;
            _addFoodButton.onClick.AddListener(AddFood);
            _reproductionRateSlider.onValueChanged.AddListener(UpdateReproductionRate);
        }

        private void UpdateTotalEntitiesText(int count)
        {
            _totalEntitiesTextMesh.text = "Total entities: " + count;
        }

        private void AddFood()
        {
            _interestsManager.SpawnPointOfInterest();
        }
        
        private void UpdateReproductionRate(float rate)
        {
            _flocking.ReproductionRate = rate;
            _reproductionRateTextMesh.text = $"Reproduction rate: {rate:f2}";
        }
        
        private void OnDisable()
        {
            _reproductionRateSlider.onValueChanged.RemoveListener(UpdateReproductionRate);
            _addFoodButton.onClick.RemoveListener(AddFood);
            _flocking.OnEntitiesCountChanged -= UpdateTotalEntitiesText;
        }
    }
}