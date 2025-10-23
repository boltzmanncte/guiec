using System.Collections.Concurrent;

namespace vfv.Services;

public class FileCache
{
    private readonly ConcurrentDictionary<string, CachedFile> _cache;
    private readonly int _maxCacheSize;
    private readonly TimeSpan _cacheExpiration;

    public FileCache(int maxCacheSize = 50, int cacheExpirationMinutes = 30)
    {
        _cache = new ConcurrentDictionary<string, CachedFile>();
        _maxCacheSize = maxCacheSize;
        _cacheExpiration = TimeSpan.FromMinutes(cacheExpirationMinutes);
    }

    public void AddOrUpdate(string filePath, string content)
    {
        // Clean up expired entries before adding new ones
        CleanupExpiredEntries();

        // If cache is full, remove oldest entry
        if (_cache.Count >= _maxCacheSize)
        {
            RemoveOldestEntry();
        }

        var cachedFile = new CachedFile
        {
            FilePath = filePath,
            Content = content,
            LoadedAt = DateTime.Now,
            LastAccessedAt = DateTime.Now,
            FileSize = content.Length
        };

        _cache.AddOrUpdate(filePath, cachedFile, (key, existing) => cachedFile);
    }

    public bool TryGet(string filePath, out string? content)
    {
        if (_cache.TryGetValue(filePath, out var cachedFile))
        {
            // Check if cache entry has expired
            if (DateTime.Now - cachedFile.LoadedAt > _cacheExpiration)
            {
                _cache.TryRemove(filePath, out _);
                content = null;
                return false;
            }

            // Update last accessed time
            cachedFile.LastAccessedAt = DateTime.Now;
            content = cachedFile.Content;
            return true;
        }

        content = null;
        return false;
    }

    public bool Contains(string filePath)
    {
        return _cache.ContainsKey(filePath);
    }

    public void Remove(string filePath)
    {
        _cache.TryRemove(filePath, out _);
    }

    public void Clear()
    {
        _cache.Clear();
    }

    public int Count => _cache.Count;

    public long TotalCacheSize => _cache.Values.Sum(f => (long)f.FileSize);

    public IEnumerable<CachedFileInfo> GetCachedFiles()
    {
        return _cache.Values.Select(f => new CachedFileInfo
        {
            FilePath = f.FilePath,
            LoadedAt = f.LoadedAt,
            LastAccessedAt = f.LastAccessedAt,
            FileSize = f.FileSize
        }).OrderByDescending(f => f.LastAccessedAt);
    }

    private void CleanupExpiredEntries()
    {
        var expiredKeys = _cache
            .Where(kvp => DateTime.Now - kvp.Value.LoadedAt > _cacheExpiration)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in expiredKeys)
        {
            _cache.TryRemove(key, out _);
        }
    }

    private void RemoveOldestEntry()
    {
        var oldestEntry = _cache
            .OrderBy(kvp => kvp.Value.LastAccessedAt)
            .FirstOrDefault();

        if (!string.IsNullOrEmpty(oldestEntry.Key))
        {
            _cache.TryRemove(oldestEntry.Key, out _);
        }
    }

    private class CachedFile
    {
        public string FilePath { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime LoadedAt { get; set; }
        public DateTime LastAccessedAt { get; set; }
        public int FileSize { get; set; }
    }
}

public class CachedFileInfo
{
    public string FilePath { get; set; } = string.Empty;
    public DateTime LoadedAt { get; set; }
    public DateTime LastAccessedAt { get; set; }
    public int FileSize { get; set; }
}
