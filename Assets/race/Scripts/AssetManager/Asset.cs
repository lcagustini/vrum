using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AssetReference : UnityEngine.AddressableAssets.AssetReference
{
    public AssetReference() : base() { }
    public AssetReference(string guid) : base(guid) { }

    public override int GetHashCode()
    {
        return AssetGUID.GetHashCode();
    }

    public override bool Equals(object obj)
    {
        return AssetGUID == (obj as AssetReference)?.AssetGUID;
    }

    public static bool operator ==(AssetReference a, AssetReference b)
    {
        if (a is null) return b is null;
        if (b is null) return a is null;

        return a.Equals(b);
    }

    public static bool operator !=(AssetReference a, AssetReference b)
    {
        if (a is null) return !(b is null);
        if (b is null) return !(a is null);

        return !a.Equals(b);
    }
}

public class Asset
{
    public string assetID;

    public override int GetHashCode()
    {
        return assetID.GetHashCode();
    }

    public override bool Equals(object obj)
    {
        return assetID == (obj as Asset)?.assetID;
    }

    public static bool operator ==(Asset a, Asset b)
    {
        if (a is null) return b is null;
        if (b is null) return a is null;

        return a.Equals(b);
    }

    public static bool operator !=(Asset a, Asset b)
    {
        if (a is null) return !(b is null);
        if (b is null) return !(a is null);

        return !a.Equals(b);
    }
}

[System.Serializable]
public class CarAsset : Asset
{
    public AssetReference carModel;
    public CarConfig carConfig;
}

[System.Serializable]
public class TrackAsset : Asset
{
    public AssetReference trackModel;
    public AssetReference trackData;
}
