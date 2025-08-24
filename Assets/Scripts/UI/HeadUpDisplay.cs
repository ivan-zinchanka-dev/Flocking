using Management;
using Models;
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
        private Flocking _flocking;
        [SerializeField]
        private FlockingConfig _flockingConfig;
        [SerializeField] 
        private InterestsManager _interestsManager;
        
        private const string ReproductionRateFormat = "Reproduction rate: {0:f2}";
        private const string VelocityLimitFormat = "Max entity speed: {0:f2}";
        private const string TotalEntitiesFormat = "Total entities: {0}";
        
        private void Awake()
        {
            UpdateTotalEntitiesText(_flocking.EntitiesCount);

            _velocityLimitSlider.value = _flockingConfig.EntityVelocityLimit;
            _velocityLimitTextMesh.text = string.Format(VelocityLimitFormat, _flockingConfig.EntityVelocityLimit);
            
            _reproductionRateSlider.value = _flockingConfig.ReproductionRate;
            _reproductionRateTextMesh.text = string.Format(ReproductionRateFormat, _flockingConfig.ReproductionRate);
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
            _totalEntitiesTextMesh.text = string.Format(TotalEntitiesFormat, count);
        }

        private void AddFood()
        {
            _interestsManager.SpawnPointOfInterest();
        }
        
        private void UpdateVelocityLimit(float limit)
        {
            _flockingConfig.EntityVelocityLimit = limit;
            _velocityLimitTextMesh.text = string.Format(VelocityLimitFormat, limit);
        }
        
        private void UpdateReproductionRate(float rate)
        {
            _flockingConfig.ReproductionRate = rate;
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