namespace GitLurker.Core.Models;

using System;

public class Snippet
{
    public Snippet()
    {
        Id = Guid.NewGuid();
        Hotkey = new Hotkey();
    }

    public Guid Id { get; set; }

    public string Name { get; set; }

    public string Value { get; set; }

    public Hotkey Hotkey { get; set; }
}
