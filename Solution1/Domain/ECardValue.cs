using System.ComponentModel;

namespace Domain;

public enum ECardValue
{
    [Description("0")]
    Value0,
    
    [Description("1")]
    Value1,
    
    [Description("2")]
    Value2,
    
    [Description("3")]
    Value3,
    
    [Description("4")]
    Value4,
    
    [Description("5")]
    Value5,
    
    [Description("6")]
    Value6,
    
    [Description("7")]
    Value7,
    
    [Description("8")]
    Value8,
    
    [Description("9")]
    Value9,
    
    [Description("<->")]
    ActionReverse,
    
    [Description("Ø")]
    ActionSkip,
    
    [Description("+2")]
    ActionDrawTwo,
    
    [Description("?")]
    WildChangeColor,
    
    [Description("+4")]
    WildDrawFour,
    
}