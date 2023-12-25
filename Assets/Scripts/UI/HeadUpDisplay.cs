using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class HeadUpDisplay : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _totalEntitiesTextMesh;
        [SerializeField] private Button _addFoodButton;
        
        [Space]
        [SerializeField] private InterestsManager _interestsManager;
        [SerializeField] private Flocking _flocking;

        private void Awake()
        {
            UpdateTotalEntitiesText(_flocking.EntitiesCount);
        }

        private void OnEnable()
        {
            _flocking.OnEntitiesCountChanged += UpdateTotalEntitiesText;
            _addFoodButton.onClick.AddListener(AddFood);
        }

        private void UpdateTotalEntitiesText(int count)
        {
            _totalEntitiesTextMesh.text = "Total entities: " + count;
        }

        private void AddFood()
        {
            _interestsManager.SpawnPointOfInterest();
        }
        
        private void OnDisable()
        {
            _addFoodButton.onClick.RemoveListener(AddFood);
            _flocking.OnEntitiesCountChanged -= UpdateTotalEntitiesText;
        }
    }
}