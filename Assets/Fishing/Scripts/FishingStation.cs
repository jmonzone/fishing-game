using UnityEngine;

public class FishingStation : JobStation
{
    [SerializeField] private FishingRod fishingRod;

    public override string ActionText => "Fish";

    protected override void OnBackButtonClicked()
    {
        EndAction();
    }

    public override void SetFungal(FungalController fungal)
    {
    }

    protected override void OnJobStarted()
    {
    }

    private void Update()
    {
        if (IsActive && Input.GetMouseButtonDown(0) && !Utility.IsPointerOverUI) fishingRod.Use();
    }
}
