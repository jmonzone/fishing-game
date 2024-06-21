using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum InitialGameState
{
    GARDEN,
    FISHING,
    COOKING
}

public class GardenManager : MonoBehaviour
{
    [Header("Developer Options")]
    [SerializeField] private InitialGameState initialGameState;

    [Header("References")]
    [SerializeField] private FungalManager fungalManager;
    [SerializeField] private ControlPanel controlPanel;
    [SerializeField] private InventoryList inventoryUI;
    [SerializeField] private InventoryList feedUI;
    [SerializeField] private FishingStation fishingStation;

    private List<JobStation> jobStations = new List<JobStation>();
    private List<ItemInstance> Inventory => GameManager.Instance.Inventory;

    private enum GameState
    {
        GAMEPLAY,
        PET_INFO,
    }

    private void Start()
    {
        InitializeInventory();
        InitializeControlPanel();
        InitializeJobStations();

        if (Application.isEditor)
        {
            StartCoroutine(WaitForInitialization());
        }
    }

    private IEnumerator WaitForInitialization()
    {
        yield return new WaitForFixedUpdate();
        controlPanel.gameObject.SetActive(initialGameState == InitialGameState.GARDEN);
        switch (initialGameState)
        {
            case InitialGameState.FISHING:
                fishingStation.SetFungal(fungalManager.FungalControllers[0]);
                fishingStation.UseAction();
                break;
        }
    }

    private void InitializeInventory()
    {
        void UpdateInventory()
        {
            inventoryUI.SetInventory(Inventory);
            feedUI.SetInventory(Inventory);
        }

        GameManager.Instance.OnInventoryChanged += UpdateInventory;
        UpdateInventory();
    }

    private void InitializeControlPanel()
    {
        fungalManager.OnFungalTalkStart += () => controlPanel.SetFungal(fungalManager.TalkingFungal);
        controlPanel.OnFungalInteractionEnd += fungalManager.EndFungalTalk;
        controlPanel.OnEscortButtonClicked += () =>
        {
            if (fungalManager.EscortedFungal)
            {
                fungalManager.UnescortFungal();
            }
            else
            {
                fungalManager.EscortFungal();
            }
        };

    }

    private void InitializeJobStations()
    {
        void Station_OnJobEnd()
        {
            if (fungalManager.EscortedFungal) fungalManager.UnescortFungal();
        }

        void Station_OnJobStart(JobStation station)
        {
            var fungal = fungalManager.EscortedFungal;
            if (fungal)
            {
                fungalManager.UnescortFungal();
                station.SetFungal(fungal);
            }
        }

        jobStations = FindObjectsOfType<JobStation>().ToList();
        foreach (var station in jobStations)
        {
            station.OnJobStart += () => Station_OnJobStart(station);
            station.OnJobEnd += Station_OnJobEnd;
        }
    }

    
}
