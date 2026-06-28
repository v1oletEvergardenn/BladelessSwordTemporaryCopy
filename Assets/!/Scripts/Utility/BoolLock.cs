using System;
using System.Collections.Generic;

public sealed class BoolLock
{
    // lockName -> version (only one active entry per name)
    private readonly Dictionary<string, int> _namedLocks = new Dictionary<string, int>();

    public bool Check()
    {
        return _namedLocks.Count > 0;
    }

    public bool Check(string lockName)
    {
        if (string.IsNullOrEmpty(lockName)) return false;
        return _namedLocks.ContainsKey(lockName);
    }

    public IDisposable Add(string lockName)
    {
        if (string.IsNullOrEmpty(lockName)) return null;

        int nextVersion = 1;
        if (_namedLocks.TryGetValue(lockName, out int currentVersion))
            nextVersion = currentVersion + 1;

        // Replace old lock with the same name.
        _namedLocks[lockName] = nextVersion;
        return new Token(this, lockName, nextVersion);
    }

    public void Remove(string lockName)
    {
        if (string.IsNullOrEmpty(lockName)) return;
        _namedLocks.Remove(lockName);
    }

    public void Clear()
    {
        _namedLocks.Clear();
    }

    private void Remove(string lockName, int tokenVersion)
    {
        if (string.IsNullOrEmpty(lockName)) return;

        // Remove only if this token still owns the current lock version.
        if (_namedLocks.TryGetValue(lockName, out int currentVersion) && currentVersion == tokenVersion)
        {
            _namedLocks.Remove(lockName);
        }
    }

    private sealed class Token : IDisposable
    {
        private readonly BoolLock _owner;
        private readonly string _lockName;
        private readonly int _version;
        private bool _disposed;

        public Token(BoolLock owner, string lockName, int version)
        {
            _owner = owner;
            _lockName = lockName;
            _version = version;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _owner.Remove(_lockName, _version);
        }
    }
}