namespace Admiral.Booking.AppCore.Domain.Base.Rules.Localizations;

public readonly record struct LocalizationKey
{
    public string Key { get; init; }
    public object[]? Args { get; init; }

    public LocalizationKey(string key, params object[] args)
    {
        Key = key;
        Args = args.Length > 0 ? args : null;
    }

    public static implicit operator LocalizationKey(string key) => new(key);

    public override string ToString()
        => Args is { Length: > 0 }
            ? string.Format(Key, Args)
            : Key;
}
