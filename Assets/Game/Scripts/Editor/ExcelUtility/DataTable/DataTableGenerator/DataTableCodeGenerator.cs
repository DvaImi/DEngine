using System.Text;

namespace Game.Editor.DataTableTools
{
    public delegate void DataTableCodeGenerator(DataTableProcessor dataTableProcessor, StringBuilder codeContent, object userData);
}