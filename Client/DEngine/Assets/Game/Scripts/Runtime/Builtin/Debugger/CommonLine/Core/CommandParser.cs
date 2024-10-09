using System.Collections.Generic;
using DEngine;
namespace Game.CommandLine
{
    public class CommandParser
    {
        const char EOL = '\0';
        const char SPACE = ' ';
        const char DBL_QUOTATION = '"';
        const char SGL_QUOTATION = '\'';

        int index;
        string input;
        bool HasMore => index < input.Length;

        public bool Parse(string input, out string[] result)
        {
            result = null;
            if (input == null || input.Length == 0)
            {
                return false;
            }

            this.input = input.Trim() + EOL;
            index = 0;
            try
            {
                List<string> list = new();
                foreach (var part in Walk())
                {
                    list.Add(part);
                }
                result = list.ToArray();
                return list.Count > 0;
            }
            catch (DEngineException)
            {
                result = null;
                return false;
            }
        }

        private IEnumerable<string> Walk()
        {

            while (HasMore)
            {
                char c = input[index++];
                System.Console.WriteLine(c);
                switch (c)
                {
                    case DBL_QUOTATION:
                        yield return NextString(index, DBL_QUOTATION);
                        if (HasMore && input[index] == SPACE) index++;
                        break;

                    case SGL_QUOTATION:
                        yield return NextString(index, SGL_QUOTATION);
                        if (HasMore && input[index] == SPACE) index++;
                        break;

                    case EOL:
                        break;

                    default:
                        yield return NextId(index - 1);
                        break;
                }
            }
        }

        private string NextId(int start, int length = 0)
        {
            while (HasMore)
            {
                char c = input[index++];
                if (char.IsWhiteSpace(c))
                {
                    return input.Substring(start, length + 1);
                }
                if (c == DBL_QUOTATION || c == SGL_QUOTATION)
                {
                    index--;
                    return input.Substring(start, length + 1);
                }
                length++;
            }
            return input.Substring(start, length);
        }

        private string NextString(int start, char quotation, int length = 0)
        {

            while (HasMore)
            {
                char c = input[index++];
                if (c == quotation)
                {
                    return input.Substring(start, length);
                }
                length++;
            }
            throw new DEngineException("string parser error");
        }
    }
}