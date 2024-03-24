using System.ComponentModel;

namespace Domain;

public enum ECardSuit
{
    [Description("R")]
    Red,
    
    [Description("Y")]
    Yellow,
    
    [Description("G")]
    Green,
    
    [Description("B")]
    Blue,
    
    [Description("")]
    Wild
}