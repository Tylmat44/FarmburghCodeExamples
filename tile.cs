using cakeslice;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{

    private Vector2 gridPosition = Vector2.zero;
    public bool hasObject;
    public bool isPlowed;
    public Color color = new Color(134f / 255f, 229f / 255f, 25f / 255f);

    public bool IsPlowed { get => isPlowed; set => isPlowed = value; }
    public Vector2 GridPosition { get => gridPosition; set => gridPosition = value; }


    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    //When a tile is selected, this checks what the tool or TileableObject the user has selected and does something accordingly.
    void OnMouseDown()
    {
        // Plows a group of 4 tiles (standard crop size) using this tile as a base.
        if (ToolChest.PlowSelected)
        {
            Dictionary<int, Vector2> tileGroup = this.calculateTileGroup(AccountManager.ToolUpgrades["PLOW"]*2, AccountManager.ToolUpgrades["PLOW"]*2);
            bool hasObject = true;

            for(int i = (int)this.GridPosition.x; i < ((int)this.GridPosition.x + AccountManager.ToolUpgrades["PLOW"] * 2) - 1; i++)
            {
                for (int j = (int)this.GridPosition.y; j < ((int)this.GridPosition.y + AccountManager.ToolUpgrades["PLOW"] * 2) - 1; j++)
                {
                    try
                    {
                        AudioManager._instance.playSoundEffect("PLOW_GRASS");
                        TileManager._instance.plowTiles(TileManager.map[i][j].calculateTileGroup(2, 2));
                    } catch
                    {

                    }
                }
            }
        }

        // Clears a group of 4 tiles (standard crop size) using this tile as a base.
        if (ToolChest.ClearSelected)
        {
            AudioManager._instance.playSoundEffect("PLOW_GRASS");
            TileManager._instance.clearTiles(this.calculateTileGroup(AccountManager.ToolUpgrades["PLOW"]*2, AccountManager.ToolUpgrades["PLOW"]*2));
        }

        // If the user has an object selected and wants to place.
        if (ToolChest.SelectionInfo != null)
        {
            switch (ToolChest.SelectionInfo.Type)
            {
                case (ResourceType.seed):
                    if (TileManager.map[(int)this.calculateTileGroup(2, 2)[0].x][(int)this.calculateTileGroup(2, 2)[0].y].isPlowed && !TileManager.map[(int)this.calculateTileGroup(2, 2)[0].x][(int)this.calculateTileGroup(2, 2)[0].y].hasObject)
                    {

                        if (ToolChest.SelectionInfo.FromStorage)
                        {
                            if(Inventory.getItemAmount(ToolChest.SelectionInfo.Key) > 0)
                            {
                                Crop tempCrop = DataManager.cropDB[ToolChest.SelectionInfo.Key].Copy();
                                tempCrop.GrowthTimeLeft = TimeSpan.FromSeconds(tempCrop.GrowthTime.TotalSeconds - (tempCrop.GrowthTime.TotalSeconds * LevelManager.CropAnimalSpeedMultiplier));
                                addTileableObject(tempCrop);
                                AudioManager._instance.playSoundEffect("PLANT_CROP");
                                Inventory.removeItem(ToolChest.SelectionInfo.Key + "_SEEDS", 1);
                            }
                        }
                        else
                        {
                            if (AccountManager.Coins >= DataManager.masterDB[ToolChest.SelectionInfo.Key].BuyPrice)
                            {
                                Crop tempCrop = DataManager.cropDB[ToolChest.SelectionInfo.Key].Copy();
                                tempCrop.GrowthTimeLeft = TimeSpan.FromSeconds(tempCrop.GrowthTime.TotalSeconds - (tempCrop.GrowthTime.TotalSeconds * LevelManager.CropAnimalSpeedMultiplier));
                                addTileableObject(tempCrop);
                                AudioManager._instance.playSoundEffect("PLANT_CROP");
                                AccountManager.Coins -= DataManager.masterDB[ToolChest.SelectionInfo.Key + "_SEEDS"].BuyPrice;
                            }
                        }
                    }
                    break;
                case (ResourceType.animal):
                    int x;
                    int y;
                    if (!ToolChest.MoveSelected || (ToolChest.SelectionInfo.AnimalSelected.Rotation == 0 || ToolChest.SelectionInfo.AnimalSelected.Rotation == 180))
                    {
                        x = (int)DataManager.animalDB[ToolChest.SelectionInfo.Key].Size.x;
                        y = (int)DataManager.animalDB[ToolChest.SelectionInfo.Key].Size.y;
                    }
                    else
                    {
                        x = (int)DataManager.animalDB[ToolChest.SelectionInfo.Key].Size.y;
                        y = (int)DataManager.animalDB[ToolChest.SelectionInfo.Key].Size.x;
                    }

                    Dictionary<int, Vector2> group = this.calculateTileGroup(x, y);
                    bool isOccupied = false;

                    foreach (Vector2 tile in group.Values)
                    {
                        if (TileManager.map[(int)tile.x][(int)tile.y].hasObject || TileManager.map[(int)tile.x][(int)tile.y].IsPlowed)
                        {
                            isOccupied = true;
                        }
                    }
                    if (!isOccupied)
                    {
                        if (!ToolChest.MoveSelected)
                        {
                            if (ToolChest.SelectionInfo.FromStorage)
                            {
                                if (Inventory.getItemAmount(ToolChest.SelectionInfo.Key) > 0)
                                {
                                    AudioManager._instance.playSoundEffect("HARVEST_ANIMAL");
                                    addTileableObject(DataManager.animalDB[ToolChest.SelectionInfo.Key].Copy());
                                    Inventory.removeItem(ToolChest.SelectionInfo.Key, 1);
                                }
                            }
                            else
                            {
                                if (AccountManager.Coins >= DataManager.animalDB[ToolChest.SelectionInfo.Key].Cost)
                                {
                                    AudioManager._instance.playSoundEffect("HARVEST_ANIMAL");
                                    addTileableObject(DataManager.animalDB[ToolChest.SelectionInfo.Key].Copy());
                                    AccountManager.Coins -= DataManager.animalDB[ToolChest.SelectionInfo.Key].Cost;
                                }
                            }
                           
                        }
                        else
                        {
                            Animal tempAnimal = ToolChest.SelectionInfo.AnimalSelected;
                            tempAnimal.GrowthTimeLeft = TimeSpan.FromSeconds(tempAnimal.GrowthTime.TotalSeconds - (tempAnimal.GrowthTime.TotalSeconds * LevelManager.CropAnimalSpeedMultiplier));
                            ToolChest.SelectionInfo = null;
                            ToolChest.MoveSelected = false;
                            addTileableObject(tempAnimal);
                            AudioManager._instance.playSoundEffect("HARVEST_ANIMAL");
                            ToolChest.MoveGameObject.GetComponent<AnimalObject>().destroyObject();
                        }
                    }
                    break;
                case (ResourceType.useableBuilding):
                    if (!ToolChest.MoveSelected || (ToolChest.SelectionInfo.UseableBuildingSelected.Rotation == 0 || ToolChest.SelectionInfo.UseableBuildingSelected.Rotation == 180))
                    {
                        x = (int)DataManager.useableBuildingDB[ToolChest.SelectionInfo.Key].Size.x;
                        y = (int)DataManager.useableBuildingDB[ToolChest.SelectionInfo.Key].Size.y;
                    }
                    else
                    {
                        x = (int)DataManager.useableBuildingDB[ToolChest.SelectionInfo.Key].Size.y;
                        y = (int)DataManager.useableBuildingDB[ToolChest.SelectionInfo.Key].Size.x;
                    }

                    group = this.calculateTileGroup(x, y);
                    isOccupied = false;

                    foreach (Vector2 tile in group.Values)
                    {
                        if (TileManager.map[(int)tile.x][(int)tile.y].hasObject || TileManager.map[(int)tile.x][(int)tile.y].IsPlowed)
                        {
                            isOccupied = true;
                        }
                    }
                    if (!isOccupied)
                    {
                        if (!ToolChest.MoveSelected)
                        {
                            if (AccountManager.Coins >= DataManager.useableBuildingDB[ToolChest.SelectionInfo.Key].Cost)
                            {
                                AudioManager._instance.playSoundEffect("BUILD_USEABLE_BUILDING");
                                addTileableObject(DataManager.useableBuildingDB[ToolChest.SelectionInfo.Key].Copy());
                                AccountManager.Coins -= DataManager.useableBuildingDB[ToolChest.SelectionInfo.Key].Cost;
                            }
                        }
                        else
                        {
                            UseableBuilding tempUB = ToolChest.SelectionInfo.UseableBuildingSelected;
                            ToolChest.SelectionInfo = null;
                            ToolChest.MoveSelected = false;
                            AudioManager._instance.playSoundEffect("BUILD_USEABLE_BUILDING");
                            addTileableObject(tempUB);
                            ToolChest.MoveGameObject.GetComponent<UseableBuildingObject>().destroyObject();
                        }
                    }
                    break;
                case (ResourceType.decor):
                    if (!ToolChest.MoveSelected || (ToolChest.SelectionInfo.DecorSelected.Rotation == 0 || ToolChest.SelectionInfo.DecorSelected.Rotation == 180))
                    {
                        x = (int)DataManager.decorDB[ToolChest.SelectionInfo.Key].Size.x;
                        y = (int)DataManager.decorDB[ToolChest.SelectionInfo.Key].Size.y;
                    }
                    else
                    {
                        x = (int)DataManager.decorDB[ToolChest.SelectionInfo.Key].Size.y;
                        y = (int)DataManager.decorDB[ToolChest.SelectionInfo.Key].Size.x;
                    }

                    group = this.calculateTileGroup(x, y);
                    isOccupied = false;

                    foreach (Vector2 tile in group.Values)
                    {
                        if (TileManager.map[(int)tile.x][(int)tile.y].hasObject || TileManager.map[(int)tile.x][(int)tile.y].IsPlowed)
                        {
                            isOccupied = true;
                        }
                    }
                    if (!isOccupied)
                    {
                        if (!ToolChest.MoveSelected)
                        {
                            if (ToolChest.SelectionInfo.FromStorage)
                            {
                                if (Inventory.getItemAmount(ToolChest.SelectionInfo.Key) > 0)
                                {
                                    AudioManager._instance.playSoundEffect("HARVEST_ANIMAL");
                                    addTileableObject(DataManager.decorDB[ToolChest.SelectionInfo.Key].Copy());
                                    Inventory.removeItem(ToolChest.SelectionInfo.Key, 1);
                                }
                            }
                            else
                            {
                                if (AccountManager.Coins >= DataManager.decorDB[ToolChest.SelectionInfo.Key].Cost)
                                {
                                    AudioManager._instance.playSoundEffect("HARVEST_ANIMAL");
                                    addTileableObject(DataManager.decorDB[ToolChest.SelectionInfo.Key].Copy());
                                    AccountManager.Coins -= DataManager.decorDB[ToolChest.SelectionInfo.Key].Cost;
                                }
                            }
                        }
                        else
                        {
                            Decor tempDecor = ToolChest.SelectionInfo.DecorSelected;
                            ToolChest.SelectionInfo = null;
                            ToolChest.MoveSelected = false;
                            AudioManager._instance.playSoundEffect("HARVEST_ANIMAL");
                            addTileableObject(tempDecor);
                            ToolChest.MoveGameObject.GetComponent<DecorObject>().destroyObject();
                        }
                    }
                    break;
            }

        }

    }

    // Highights the tile and others in it's group based on the size of the tool/resource selected.
    void OnMouseEnter()
    {
        if (ToolChest.PlowSelected || ToolChest.ClearSelected || ToolChest.MoveSelected || ToolChest.SelectionInfo != null)
        {
            if (ToolChest.SelectionInfo != null)
            {
                if(ToolChest.SelectionInfo.Type == ResourceType.seed)
                {
                    foreach (Vector2 tile in this.calculateTileGroup(2, 2).Values)
                    {
                        TileManager.map[(int)tile.x][(int)tile.y].gameObject.GetComponent<Outline>().enabled = true;
                        TileManager.map[(int)tile.x][(int)tile.y].gameObject.GetComponent<Outline>().color = (ToolChest.MoveSelected ? 0 : 1);
                    } 
                }
                else if (ToolChest.SelectionInfo.Type == ResourceType.animal)
                {
                    foreach (Vector2 tile in this.calculateTileGroup((int)DataManager.animalDB[ToolChest.SelectionInfo.Key].Size.x, (int)DataManager.animalDB[ToolChest.SelectionInfo.Key].Size.y).Values)
                    {
                        TileManager.map[(int)tile.x][(int)tile.y].gameObject.GetComponent<Outline>().enabled = true;
                        TileManager.map[(int)tile.x][(int)tile.y].gameObject.GetComponent<Outline>().color = (ToolChest.MoveSelected ? 0 : 1);
                    }
                }
                else if (ToolChest.SelectionInfo.Type == ResourceType.useableBuilding)
                {
                    foreach (Vector2 tile in this.calculateTileGroup((int)DataManager.useableBuildingDB[ToolChest.SelectionInfo.Key].Size.x, (int)DataManager.useableBuildingDB[ToolChest.SelectionInfo.Key].Size.y).Values)
                    {
                        TileManager.map[(int)tile.x][(int)tile.y].gameObject.GetComponent<Outline>().enabled = true;
                        TileManager.map[(int)tile.x][(int)tile.y].gameObject.GetComponent<Outline>().color = (ToolChest.MoveSelected ? 0 : 1);
                    }
                }
                else if (ToolChest.SelectionInfo.Type == ResourceType.decor)
                {
                    foreach (Vector2 tile in this.calculateTileGroup((int)DataManager.decorDB[ToolChest.SelectionInfo.Key].Size.x, (int)DataManager.decorDB[ToolChest.SelectionInfo.Key].Size.y).Values)
                    {
                        TileManager.map[(int)tile.x][(int)tile.y].gameObject.GetComponent<Outline>().enabled = true;
                        TileManager.map[(int)tile.x][(int)tile.y].gameObject.GetComponent<Outline>().color = (ToolChest.MoveSelected ? 0 : 1);
                    }
                }
            }
            else
            {
                foreach (Vector2 tile in this.calculateTileGroup(AccountManager.ToolUpgrades["PLOW"] * 2, AccountManager.ToolUpgrades["PLOW"] * 2).Values)
                {
                    TileManager.map[(int)tile.x][(int)tile.y].gameObject.GetComponent<Outline>().enabled = true;
                    TileManager.map[(int)tile.x][(int)tile.y].gameObject.GetComponent<Outline>().color = (ToolChest.MoveSelected ? 0 : 1);
                }
            }
        } else
        {

        }
    }

    void OnMouseExit()

    {
        if (ToolChest.PlowSelected || ToolChest.ClearSelected || ToolChest.MoveSelected || ToolChest.SelectionInfo != null)
        {
            if (ToolChest.SelectionInfo != null)
            {
                if (ToolChest.SelectionInfo.Type == ResourceType.seed)
                {
                    foreach (Vector2 tile in this.calculateTileGroup(2, 2).Values)
                    {
                        TileManager.map[(int)tile.x][(int)tile.y].gameObject.GetComponent<Outline>().enabled = false;
                    }
                }
                if (ToolChest.SelectionInfo.Type == ResourceType.animal)
                {
                    foreach (Vector2 tile in this.calculateTileGroup((int)DataManager.animalDB[ToolChest.SelectionInfo.Key].Size.x, (int)DataManager.animalDB[ToolChest.SelectionInfo.Key].Size.y).Values)
                    {
                        TileManager.map[(int)tile.x][(int)tile.y].gameObject.GetComponent<Outline>().enabled = false;
                    }
                }
                else if (ToolChest.SelectionInfo.Type == ResourceType.useableBuilding)
                {
                    foreach (Vector2 tile in this.calculateTileGroup((int)DataManager.useableBuildingDB[ToolChest.SelectionInfo.Key].Size.x, (int)DataManager.useableBuildingDB[ToolChest.SelectionInfo.Key].Size.y).Values)
                    {
                        TileManager.map[(int)tile.x][(int)tile.y].gameObject.GetComponent<Outline>().enabled = false;
                    }
                }
                else if (ToolChest.SelectionInfo.Type == ResourceType.decor)
                {
                    foreach (Vector2 tile in this.calculateTileGroup((int)DataManager.decorDB[ToolChest.SelectionInfo.Key].Size.x, (int)DataManager.decorDB[ToolChest.SelectionInfo.Key].Size.y).Values)
                    {
                        TileManager.map[(int)tile.x][(int)tile.y].gameObject.GetComponent<Outline>().enabled = false;
                    }
                }
            }
            else
            {
                foreach (Vector2 tile in this.calculateTileGroup(AccountManager.ToolUpgrades["PLOW"] * 2, AccountManager.ToolUpgrades["PLOW"] * 2).Values)
                {
                    TileManager.map[(int)tile.x][(int)tile.y].gameObject.GetComponent<Outline>().enabled = false;
                }
            }
        }
    }

    //Adds a TileableObject to this tile, and the others around it based on the objects size.
    public void addTileableObject(TileableObjects addObject)
    {
        Dictionary<int, Vector2> group = null;

        if (addObject.Type == TileableObjectType.crop)
        {
            group = this.calculateTileGroup(2, 2); ;
        } else if (addObject.Type == TileableObjectType.animal)
        {
            group = this.calculateTileGroup((int)((Animal)addObject).Size.x, (int)((Animal)addObject).Size.y);
        } else if (addObject.Type == TileableObjectType.useableBuilding)
        {
            group = this.calculateTileGroup((int)((UseableBuilding)addObject).Size.x, (int)((UseableBuilding)addObject).Size.y);
        }
        else if (addObject.Type == TileableObjectType.decor)
        {
            group = this.calculateTileGroup((int)((Decor)addObject).Size.x, (int)((Decor)addObject).Size.y);
        }

        bool objectInPlace = false;

        foreach (Vector2 tile in group.Values) { 
            if (TileManager.map[(int)tile.x][(int)tile.y].hasObject)
            {
                objectInPlace = true;
            }
        }

        if (!objectInPlace)
        {
            foreach (Vector2 tile in group.Values)
            {
                TileManager.map[(int)tile.x][(int)tile.y].hasObject = true;
                TileManager.map[(int)tile.x][(int)tile.y].GetComponent<Outline>().enabled = false;
            }

            if (addObject.Type == TileableObjectType.crop)
            {
                //Instantiates the actual object for the crop so that it can be interacted with in the world.
                GameObject gameObjectSlot = Instantiate(DataManager.prefabDB[addObject.Key], TileManager.map[(int)group[0].x][(int)group[0].y].gameObject.transform.position, Quaternion.identity, (TileManager.map[(int)group[0].x][(int)group[0].y]).transform);
                gameObject.tag = "TileableObject";

                //Gives the instantiated object some properties from its class.
                gameObjectSlot.GetComponent<CropObject>().Crop = (Crop)addObject;
                gameObjectSlot.GetComponent<CropObject>().TileGroup = group;
                gameObjectSlot.GetComponent<CropObject>().Crop.TileGroup = group;

                //Adds the new object to a list, so that it can be accessed and saved later.
                ActiveTileableObjects.activeTileableObjects.Add(gameObjectSlot.GetComponent<CropObject>().Crop);

            } else if (addObject.Type == TileableObjectType.animal)
            {
                GameObject gameObjectSlot = Instantiate(DataManager.prefabDB[addObject.Key], TileManager.map[(int)group[0].x][(int)group[0].y].gameObject.transform.position, Quaternion.identity, (TileManager.map[(int)group[0].x][(int)group[0].y]).transform);
                gameObject.tag = "TileableObject";
                gameObjectSlot.GetComponent<AnimalObject>().Animal = (Animal)addObject;
                gameObjectSlot.GetComponent<AnimalObject>().TileGroup = group;
                gameObjectSlot.GetComponent<AnimalObject>().Animal.TileGroup = group;
                ActiveTileableObjects.activeTileableObjects.Add(gameObjectSlot.GetComponent<AnimalObject>().Animal);

                if (gameObjectSlot.GetComponent<AnimalObject>().Animal.IsChild)
                {
                    ActiveTileableObjects.animalCount[gameObjectSlot.GetComponent<AnimalObject>().Animal.Resource] += 1;
                }
                else
                {
                    ActiveTileableObjects.animalCount[gameObjectSlot.GetComponent<AnimalObject>().Animal.Key] += 1;
                }

            } else if (addObject.Type == TileableObjectType.useableBuilding)
            {
                GameObject gameObjectSlot = Instantiate(DataManager.prefabDB[addObject.Key], TileManager.map[(int)group[0].x][(int)group[0].y].gameObject.transform.position, Quaternion.identity, (TileManager.map[(int)group[0].x][(int)group[0].y]).transform);
                gameObject.tag = "TileableObject";
                gameObjectSlot.GetComponent<UseableBuildingObject>().UseableBuilding = (UseableBuilding)addObject;
                gameObjectSlot.GetComponent<UseableBuildingObject>().TileGroup = group;
                gameObjectSlot.GetComponent<UseableBuildingObject>().UseableBuilding.TileGroup = group;
                ActiveTileableObjects.activeTileableObjects.Add(gameObjectSlot.GetComponent<UseableBuildingObject>().UseableBuilding);
            }
            else if (addObject.Type == TileableObjectType.decor)
            {
                GameObject gameObjectSlot = Instantiate(DataManager.prefabDB[addObject.Key], TileManager.map[(int)group[0].x][(int)group[0].y].gameObject.transform.position, Quaternion.identity, (TileManager.map[(int)group[0].x][(int)group[0].y]).transform);
                gameObject.tag = "TileableObject";
                gameObjectSlot.GetComponent<DecorObject>().Decor = (Decor)addObject;
                gameObjectSlot.GetComponent<DecorObject>().TileGroup = group;
                gameObjectSlot.GetComponent<DecorObject>().Decor.TileGroup = group;
                ActiveTileableObjects.activeTileableObjects.Add(gameObjectSlot.GetComponent<DecorObject>().Decor);
            }

        }
    }

    public void plowTile()
    {
        this.IsPlowed = true;
        this.color = new Color(165f / 255f, 42f / 255f, 42f / 255f);
        this.transform.GetComponent<Renderer>().material.color = color;

    }

    public void clearTile()
    {
        this.IsPlowed = false;
        this.color = new Color(134f / 255f, 229f / 255f, 25f / 255f);
        transform.GetComponent<Renderer>().material.color = color;
    }

    //Calculates a tile group, which is the tiles required to place an object based on its size and starts in the tile that is currently selected/hovered.
    //Size is based on number of tiles for length (x) and widgth (y)
    //x and y can never be odd, except for 1 (Object can be 1x1, 2x1, 4x1, etc)
    public Dictionary<int, Vector2> calculateTileGroup(int x, int y)
    {
        //If an object's data gives and odd x or y that is not equal to one, it lets me know I've made a mistake.
        if ((x % 2 != 0) || (y % 2 != 0 ))
        {
            if (x != 1 && y != 1)
            {
                Debug.Log("Odd Tile Dummy");
                return null;
            }
        }

        Dictionary<int, Vector2> list = new Dictionary<int, Vector2>();
        Vector2 baseT = new Vector2();
        int position = 0;

        //Special case for when the x and y are both 2 and/or 1
        if (x <= 2 && y <= 2)
        {
            if ((this.GridPosition.x % x) == 0)
            {
                baseT.x = this.GridPosition.x - (x - 1);

            }
            else
            {
                if (((this.GridPosition.x + (x - 1)) % x) == 0)
                {
                    baseT.x = this.GridPosition.x;
                }
                else
                {
                    for (int i = 1; i < x; i++)
                    {
                        if (((this.GridPosition.x + i) % x) == 0)
                        {
                            baseT.x = (this.GridPosition.x + i) - (x - 1);
                        }
                    }
                }
            }

            if ((this.GridPosition.y % y) == 0)
            {
                baseT.y = this.GridPosition.y - (y - 1);

            }
            else
            {
                if (((this.GridPosition.y + (y - 1)) % y) == 0)
                {
                    baseT.y = this.GridPosition.y;
                }
                else
                {
                    for (int i = 1; i < y; i++)
                    {
                        if (((this.GridPosition.y + i) % y) == 0)
                        {
                            baseT.y = (this.GridPosition.y + i) - (y - 1);
                        }
                    }
                }
            }

            position = 0;
            for (int j = 0; j < x; j++)
            {
                for (int k = 0; k < y; k++)
                {
                    list.Add(position, new Vector2(baseT.x + j, baseT.y + k));
                    position++;
                }
            }

            return list;
        }

        if ((this.GridPosition.x % 2) == 0 )
        {
            baseT.x = this.GridPosition.x - 1;
        } else
        {
            baseT.x = this.GridPosition.x;
        }

        if ((this.GridPosition.y % 2) == 0)
        {
            baseT.y = this.GridPosition.y - 1;
        }
        else
        {
            baseT.y = this.GridPosition.y;
        }

        //Makes sure the tile selected doesn't cause the group to go outside of the map range.
        //If it does, it will set the base tile to be at least far enough away to allow that object to be placed.
        if (baseT.x + x > TileManager.mapSize)
        {
            baseT.x = TileManager.mapSize - x + 1;
        }

        if (baseT.y + y > TileManager.mapSize)
        {
            baseT.y = TileManager.mapSize - y + 1;
        }

        //Main algorithm for creating a larger tile group.
        position = 0;
        for (int j = 0; j < x; j++)
        {
            for (int k = 0; k < y; k++)
            {
                list.Add(position, new Vector2(baseT.x + j, baseT.y + k));
                position++;
            }
        }

        return list;
    }
}
