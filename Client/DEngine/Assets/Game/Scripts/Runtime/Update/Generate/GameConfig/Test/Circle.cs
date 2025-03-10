
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using Luban;


namespace Game.Update.DataTable.Test
{
/// <summary>
/// 这是一个圆
/// </summary>
public sealed partial class Circle : Shape
{
    public Circle(ByteBuf _buf)  : base(_buf) 
    {
        Radius = _buf.ReadFloat();
    }

    public static Circle DeserializeCircle(ByteBuf _buf)
    {
        return new Test.Circle(_buf);
    }

    /// <summary>
    /// 半径
    /// </summary>
    public readonly float Radius;
   
    public const int __ID__ = -495269460;
    public override int GetTypeId() => __ID__;

    public override void ResolveRef(Tables tables)
    {
        base.ResolveRef(tables);
    }

    public override string ToString()
    {
        return "{ "
        + "radius:" + Radius + ","
        + "}";
    }
}

}

