using System;
using UnityEngine;

[Serializable]
public class YouTubeSearchResult
{
    public Item[] items;
}

[Serializable]
public class Item
{
    public Id id;
    public Snippet snippet;
}

[Serializable]
public class Id
{
    public string videoId;
}

[Serializable]
public class Snippet
{
    public string title;
    public Thumbnails thumbnails;
}

[Serializable]
public class Thumbnails
{
    public ThumbnailInfo high; // 고해상도 썸네일을 사용
}

[Serializable]
public class ThumbnailInfo
{
    public string url;
}