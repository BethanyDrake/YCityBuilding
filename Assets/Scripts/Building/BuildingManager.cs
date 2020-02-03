﻿using System.Collections.Generic;
using UnityEngine;

public class BuildingManager : MonoBehaviour
{
    public Building[,] _buildings;
    public GameObject buildingParent;

    [SerializeField] private ParticleSystem place;
    private GameManager _gameManager;

    private Ground _ground;

    // Start is called before the first frame update
    void Start()
    {
        _gameManager = GetComponent<GameManager>();
        _ground = GameObject.Find("Ground").GetComponent<Ground>();
        _buildings = new Building[_gameManager.mapWidth, _gameManager.mapHeight];
        _ground.InitTrees();
    }

    // Update is called once per frame

    public void addBuilding(Building building, Vector3 position,
        Building.DIRECTION direction = Building.DIRECTION.Bottom)
    {
        DestoryExistNature(position);

        var addedBuilding = Instantiate(building, position, Quaternion.identity, buildingParent.transform);
        addedBuilding.SetDirection(direction);
        _buildings[(int) position.x, (int) position.z] = addedBuilding;
        Instantiate(place, position, Quaternion.identity, buildingParent.transform);
    }

    public void addRoad(Road road, Vector3 position)
    {
        var x = (int) position.x;
        var z = (int) position.z;

        DestoryExistNature(position);

        var addedRoad = Instantiate(road, position, Quaternion.identity);
        _buildings[x, z] = addedRoad;

        CorrectionRoad(x, z, true);
    }

    private void CorrectionRoad(int x, int z, bool needFixAround = false)
    {
        if (x < 0 || z < 0 || x >= _gameManager.mapWidth || z >= _gameManager.mapWidth) return;

        var building =  _buildings[x, z];
        
        if (building?.type != Building.BuildingType.Road) return;

        var road = (Road) building;        

        var around = new List<Building>();

        if (x < _gameManager.mapWidth - 1) around.Add((_buildings[x + 1, z]));
        if (z > 0) around.Add(_buildings[x, z - 1]);
        if (x > 0) around.Add(_buildings[x - 1, z]);
        if (z < _gameManager.mapHeight - 1) around.Add(_buildings[x, z + 1]);

        var TypeRoad = Building.BuildingType.Road;
        var aroundRoad = around.FindAll((r => r && r.type == TypeRoad));
        var aroundRoadCount = aroundRoad.Count;


        if (aroundRoadCount == 4)
        {
            road.SetTypeAndDirection(Road.TYPE.Crossing);
        }

        if (aroundRoadCount == 3)
        {
            if (around[0]?.type == TypeRoad && around[1]?.type == TypeRoad && around[2]?.type == TypeRoad)
            {
                road.SetTypeAndDirection(Road.TYPE.CrossingT, 270);
            }

            if (around[1]?.type == TypeRoad && around[2]?.type == TypeRoad && around[3]?.type == TypeRoad)
            {
                road.SetTypeAndDirection(Road.TYPE.CrossingT, 0);
            }

            if (around[2]?.type == TypeRoad && around[3]?.type == TypeRoad && around[0]?.type == TypeRoad)
            {
                road.SetTypeAndDirection(Road.TYPE.CrossingT, 90);
            }

            if (around[3]?.type == TypeRoad && around[0]?.type == TypeRoad && around[1]?.type == TypeRoad)
            {
                road.SetTypeAndDirection(Road.TYPE.CrossingT, 180);
            }
        }

        if (aroundRoadCount == 2)
        {
            if (around[0]?.type == TypeRoad && around[2]?.type == TypeRoad)
            {
                road.SetTypeAndDirection(Road.TYPE.Straight, 90);
            }
            else if (around[1]?.type == TypeRoad && around[3]?.type == TypeRoad)
            {
                road.SetTypeAndDirection(Road.TYPE.Straight);
            }
            else
            {
                if (around[0]?.type == TypeRoad && around[1]?.type == TypeRoad)
                {
                    road.SetTypeAndDirection(Road.TYPE.Turn, 180);
                }

                if (around[0]?.type == TypeRoad && around[3]?.type == TypeRoad)
                {
                    road.SetTypeAndDirection(Road.TYPE.Turn, 90);
                }

                if (around[1]?.type == TypeRoad && around[2]?.type == TypeRoad)
                {
                    road.SetTypeAndDirection(Road.TYPE.Turn, 270);
                }

                if (around[2]?.type == TypeRoad && around[3]?.type == TypeRoad)
                {
                    road.SetTypeAndDirection(Road.TYPE.Turn, 0);
                }
            }
        }

        if (aroundRoadCount == 1)
        {
            if (around[0]?.type == TypeRoad || around[2]?.type == TypeRoad)
            {
                road.SetTypeAndDirection(Road.TYPE.Straight, 90);
            }
            else
            {
                road.SetTypeAndDirection(Road.TYPE.Straight);
            }
        }

        if (needFixAround)
        {
            CorrectionRoad(x + 1, z);
            CorrectionRoad(x, z - 1);
            CorrectionRoad(x - 1, z);
            CorrectionRoad(x, z + 1);
        }
    }

    private void DestoryExistNature(Vector3 position)
    {
        var x = (int) position.x;
        var z= (int) position.z;
        var build = _buildings[x, z];
        if (build && build.type == Building.BuildingType.Tree)
        {
            Destroy(build.gameObject);
            _buildings[x, z] = null;
        }
    }

    public bool IsHaveBuilding(Vector3 position)
    {
        if (position.x < 0 || position.x >= _gameManager.mapWidth || position.z < 0 ||
            position.z >= _gameManager.mapHeight)
        {
            return true;
        }

        return _buildings[(int) position.x, (int) position.z] &&
               _buildings[(int) position.x, (int) position.z].type != Building.BuildingType.Tree;
    }

    public Vector3 CalculateGridPosition(Vector3 position)
    {
        return new Vector3(Mathf.Floor(position.x), 0, Mathf.Floor(position.z));
    }
}