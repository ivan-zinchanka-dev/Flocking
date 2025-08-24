using Management;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class HeadUpDisplay : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] 
        private TextMeshProUGUI _totalEntitiesTextMesh;
        [SerializeField] 
        private Button _addFoodButton;
        
        [Space]
        [SerializeField] 
        private TextMeshProUGUI _velocityLimitTextMesh;
        [SerializeField] 
        private Slider _velocityLimitSlider;
        
        [Space]
        [SerializeField] 
        private TextMeshProUGUI _reproductionRateTextMesh;
        [SerializeField] 
        private Slider _reproductionRateSlider;
        
        [Header("Dependencies")]
        [SerializeField] 
        private InterestsManager _interestsManager;
        [SerializeField] 
        private Flocking _flocking;

        private const string ReproductionRateFormat = "Reproduction rate: {0:f2}";
        private const string VelocityLimitFormat = "Max entity speed: {0:f2}";
        
        private void Awake()
        {
            UpdateTotalEntitiesText(_flocking.EntitiesCount);

            _velocityLimitSlider.value = _flocking.EntityVelocityLimit;
            _velocityLimitTextMesh.text = string.Format(VelocityLimitFormat, _flocking.EntityVelocityLimit);
            
            _reproductionRateSlider.value = _flocking.ReproductionRate;
            _reproductionRateTextMesh.text = string.Format(ReproductionRateFormat, _flocking.ReproductionRate);
        }

        private void OnEnable()
        {
            _flocking.OnEntitiesCountChanged += UpdateTotalEntitiesText;
            _addFoodButton.onClick.AddListener(AddFood);
            _velocityLimitSlider.onValueChanged.AddListener(UpdateVelocityLimit);
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
        
        private void UpdateVelocityLimit(float limit)
        {
            _flocking.EntityVelocityLimit = limit;
            _velocityLimitTextMesh.text = string.Format(VelocityLimitFormat, limit);
        }
        
        private void UpdateReproductionRate(float rate)
        {
            _flocking.ReproductionRate = rate;
            _reproductionRateTextMesh.text = string.Format(ReproductionRateFormat, rate);
        }
        
        private void OnDisable()
        {
            _reproductionRateSlider.onValueChanged.RemoveListener(UpdateReproductionRate);
            _velocityLimitSlider.onValueChanged.RemoveListener(UpdateVelocityLimit);
            _addFoodButton.onClick.RemoveListener(AddFood);
            _flocking.OnEntitiesCountChanged -= UpdateTotalEntitiesText;
        }
    }
}