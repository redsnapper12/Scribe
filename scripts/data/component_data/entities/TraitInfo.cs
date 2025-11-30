using Godot;
using System;

namespace Scribe.Scripts.Data.ComponentData;

[GlobalClass]
public partial class TraitInfo : Resource
{
    [Export] public string Name { get; set; } = "";
    [Export(PropertyHint.MultilineText)] public string Description { get; set; } = "";

    public TraitInfo() : base()
    {
    }
}
