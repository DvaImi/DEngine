
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using Luban;
using Game.LubanTable;

namespace Game.LubanTable.Test
{
public abstract partial class Shape : Luban.BeanBase
{
    public Shape(ByteBuf _buf) 
    {
        PostInit();
    }

    public static Shape DeserializeShape(ByteBuf _buf)
    {
        switch (_buf.ReadInt())
        {
            case Test.Circle.__ID__: return new Test.Circle(_buf);
            case Test.Rectangle.__ID__: return new Test.Rectangle(_buf);
            default: throw new SerializationException();
        }
    }


    public virtual void ResolveRef(Tables tables)
    {
        PostResolveRef();
    }

    public override string ToString()
    {
        return "{ "
        + "}";
    }

    partial void PostInit();
    partial void PostResolveRef();
}
}
