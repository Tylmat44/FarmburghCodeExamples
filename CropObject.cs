using cakeslice;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

//The game world implementation of the Crop object.
//Attatched to the crop model.
//Handles the growing and harvesting of the crop.
public class CropObject : MonoBehaviour
{
    private Crop crop;
    private Dictionary<int, Vector2> tileGroup;
    private bool isHover = false;
    private double growthStep;
    private int step;

    public Crop Crop { get => crop; set => crop = value; }
    public Dictionary<int, Vector2> TileGroup { get => tileGroup; set => tileGroup = value; }

    // Start is called before the first frame update
    void Start()
    {
        this.gameObject.GetComponent<OutlineMaster>().SetOutlinesEnabled(false, 2);
        if (this.Crop.GrowthTimeLeft <= TimeSpan.Zero)
        {
            this.readyToHarvest();
            step = 10;
        } else
        {
            //Calculates how tall the crop model should be (No individual stage sprites yet)
            growthStep = this.Crop.GrowthTime.TotalMinutes / 10;
            step = 10;
            while ((this.Crop.GrowthTime.TotalMinutes - this.Crop.GrowthTimeLeft.TotalMinutes) < growthStep * step)
            {
                step -= 1;
            }
        }


    }

    //Takes off time remaining every frame.
    void Update()
    {
        if (this.Crop.GrowthTimeLeft > TimeSpan.Zero)
        {
            this.Crop.GrowthTimeLeft -= TimeSpan.FromSeconds(Time.deltaTime * AlwaysRunning.speedMultiplier);
            this.transform.localScale = new Vector3(10f, step, 10f);
            if (this.Crop.GrowthTime.TotalMinutes - this.Crop.GrowthTimeLeft.TotalMinutes > growthStep * step)
            {
                step += 1;
            }
        }

        if (isHover)
        {
            HoverPopup._instance.setAndShowPopup(this.Crop.Name + "\n" + this.Crop.GrowthTimeLeft.ToString(@"hh\:mm\:ss"));
        }


    }

    //Creates and outline effect on the model when mouse if over it.
    //Opens popup to show crop name and time left.
    void OnMouseEnter()
    {
        this.gameObject.GetComponent<OutlineMaster>().SetOutlinesEnabled(true, 2);
        HoverPopup._instance.setAndShowPopup(this.Crop.Name + "\n" + this.Crop.GrowthTimeLeft.ToString(@"hh\:mm\:ss"));
        isHover = true;

    }

    private void OnMouseExit()
    {
        this.gameObject.GetComponent<OutlineMaster>().SetOutlinesEnabled(false, 2);
        if (this.Crop.GrowthTimeLeft <= TimeSpan.Zero)
        {
            this.readyToHarvest();
        }
        isHover = false;
        HoverPopup._instance.hidePopup();
    }

    //Harvests the crop if ready.
    //Adds resources and xp and deletes the Crop object.
    void OnMouseDown()
    {
        if (this.Crop.GrowthTimeLeft <= TimeSpan.Zero)
        {
            Inventory.addItem(this.Crop.Resource, this.Crop.HarvestQuantity + LevelManager.CropProductionAdder);
            AccountManager.gainXP(this.Crop.HarvestXp);
            ActiveTileableObjects.activeTileableObjects.Remove(this.Crop);
            TileManager.map[(int)this.TileGroup[0].x][(int)this.TileGroup[0].y].hasObject = false;
            TileManager.map[(int)this.TileGroup[1].x][(int)this.TileGroup[1].y].hasObject = false;
            TileManager.map[(int)this.TileGroup[2].x][(int)this.TileGroup[2].y].hasObject = false;
            TileManager.map[(int)this.TileGroup[3].x][(int)this.TileGroup[3].y].hasObject = false;
            HoverPopup._instance.hidePopup();
            GameObject harvestPopup = Instantiate(DataManager.prefabDB["HARVEST_POPUP_PREFAB"], Input.mousePosition, Quaternion.identity);
            harvestPopup.transform.SetParent(FindObjectOfType<Canvas>().transform);
            harvestPopup.GetComponent<HarvestPopup>().setAndShowPopup(this.Crop.HarvestQuantity + LevelManager.CropProductionAdder, this.Crop.HarvestXp, DataManager.getSprite(this.Crop.Key));
            AudioManager._instance.playSoundEffect("HARVEST_CROP");
            Destroy(this.gameObject);

        }
    }

    private void readyToHarvest()
    {
        this.gameObject.GetComponent<OutlineMaster>().SetOutlinesEnabled(true, 1);
    }
}
