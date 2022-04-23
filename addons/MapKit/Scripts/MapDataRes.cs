namespace YYZ.MapKit
{
// A "reference" implementation is provided here, while MapShower/MapView themself are not required to use them.

using Godot;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

public interface IRegion<TRegion>
{
    Color baseColor{get; set;}
    Color remapColor{get; set;}
    HashSet<TRegion> neighbors{get;set;}
    Vector2 center{get; set;}
}

public interface IRegionData
{
    int[] BaseColor{get; set;}
    int[] RemapColor{get; set;}
    int Points{get; set;}
    float X{get; set;}
    float Y{get; set;}
    int[][] Neighbors{get; set;}
}


public class RegionMapFactory<TRegionData, TRegion> where TRegionData : IRegionData where TRegion : IRegion<TRegion>, new()
{
    static Color Int4ToColor(int[] arr)
    {
        return Color.Color8((byte)arr[0], (byte)arr[1], (byte)arr[2], (byte)arr[3]);
    }

    class RegionDataResult
    {
        public TRegionData[] Areas;
    }

    /// <summary>
    /// Get a dictionary which maps baseColor to Region.
    /// </summary>
    public Dictionary<Color, TRegion> Get(string jsonString) // TODO: reduce to dictionary
    {
        var regionMap = new Dictionary<Color, TRegion>();
        var neighborsMap = new Dictionary<TRegion, int[][]>();

        TRegionData[] regionDataList = JsonConvert.DeserializeObject<RegionDataResult>(jsonString).Areas;
        // RegionData[] regionDataList = RegionDataResult.CreateFromJSON(jsonString).Areas;
        // Debug.Log($"regionDataList.Length={regionDataList.Length}");
        foreach(var regionData in regionDataList)
        {
            var region = new TRegion();

            Extract(regionData, region);

            region.neighbors = new HashSet<TRegion>();
            neighborsMap[region] = regionData.Neighbors;
            regionMap[region.baseColor] = region;
        }

        foreach(var KV in regionMap)
        {
            var region = KV.Value;
            foreach(int[] colorInt4 in neighborsMap[region]){
                region.neighbors.Add(regionMap[Int4ToColor(colorInt4)]);
            }
        }

        return regionMap;
    }

    protected virtual void Extract(TRegionData regionData, TRegion region)
    {
        region.baseColor = Int4ToColor(regionData.BaseColor);
        region.remapColor = Int4ToColor(regionData.RemapColor);
        region.center = new Vector2(regionData.X, regionData.Y);
    }
}

public class MapData<TData, TRegion> : IMapData<TRegion> where TData : IRegionData where TRegion : IRegion<TRegion>, new()
{
    public Image baseImage;
    public int width{get; set;}
    public int height{get; set;}
    protected Dictionary<Color, TRegion> areaMap = new Dictionary<Color, TRegion>();

    protected virtual RegionMapFactory<TData, TRegion> regionMapFactory{get => new RegionMapFactory<TData, TRegion>();}

    public MapData(Texture baseTexture, string path)
    {
        var regionJsonString = Utils.ReadText(path);

        baseImage = baseTexture.GetData();
        baseImage.Lock();

        width = baseImage.GetWidth();
        height = baseImage.GetHeight();

        areaMap = regionMapFactory.Get(regionJsonString);
    }

    protected Vector2 WorldToMap(Vector2 worldPos)
    {
        return new Vector2(worldPos.x + width / 2, worldPos.y + height / 2);
    }
    protected Vector2 MapToWorld(Vector2 mapPos)
    {
        return new Vector2(mapPos.x - width / 2, mapPos.y - height / 2);
    }
    
    public Color? Pos2Color(Vector2 worldPos)
    {
        // {0, 0} is assumed to be "center"
        var mapPos = WorldToMap(worldPos);
        int x = (int)Mathf.Floor(mapPos.x);
        int y = (int)Mathf.Floor(mapPos.y);

        if(x <= 0 || x >= width-1 || y<=0 || y >= height-1){
            return null;
        }

        return baseImage.GetPixel(x, y);
    }
    
    public TRegion ColorToArea(Color color)
    {
        return areaMap[color];
    }

    public IEnumerable<TRegion> GetAllAreas() => areaMap.Values;
}

// Reference implementations

public class RegionData : IRegionData
{
    public int[] BaseColor{get; set;}
    public int[] RemapColor{get; set;}
    public int Points{get; set;}
    public float X{get; set;}
    public float Y{get; set;}
    public int[][] Neighbors{get; set;}
}

public class Region : IArea, IRegion<Region>
{
    public Color baseColor{get; set;}
    public Color remapColor{get; set;}
    public HashSet<Region> neighbors{get; set;}
    public Vector2 center{get; set;}

    public override string ToString()
    {
        return $"Region({baseColor}, {center})";
    }

    int ToId()
    {
        return remapColor.g8 * 256 + remapColor.r8;
    }
}

public class MapDataRes : Resource, IMapDataRes<Region>
{
    [Export] protected Texture baseTexture;
    [Export(PropertyHint.File)] protected string regionDataPath;

    static MapData<RegionData, Region> instance;

    public MapData<RegionData, Region> GetInstance() => instance != null ? instance : instance = new MapData<RegionData, Region>(baseTexture, regionDataPath);
    IMapData<Region> IMapDataRes<Region>.GetInstance() => GetInstance();
}


}