using System.Collections;

public class EasyScriptList : IEnumerable<object>
{
    private object[] elements;
    private int count;

    public EasyScriptList()
    {
        elements = new object[4];
        count = 0;
    }

    public int Count => count;

    public void Add(object element)
    {
        if (count == elements.Length)
        {
            Resize();
        }
        elements[count++] = element;
    }

    public object this[int index]
    {
        get
        {
            if (index < 0 || index >= count)
            {
                throw new IndexOutOfRangeException();
            }
            return elements[index];
        }
        set
        {
            if (index < 0 || index >= count)
            {
                throw new IndexOutOfRangeException();
            }
            elements[index] = value;
        }
    }
    public void Clear()
    {
        elements = new object[4];
        count = 0;
    }

    private void Resize()
    {
        int newSize = elements.Length * 2;
        object[] newElements = new object[newSize];
        Array.Copy(elements, newElements, elements.Length);
        elements = newElements;
    }

    public override string ToString()
    {
        return "[" + string.Join(", ", elements.Where(e => e != null).Select(e => e is string ? $"\"{e}\"" : e?.ToString() ?? "null")) + "]";
    }

    public IEnumerator<object> GetEnumerator()
    {
        for (int i = 0; i < count; i++)
        {
            yield return elements[i];
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}