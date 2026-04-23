using System.Collections.Generic;

namespace EcommerceDemoV1.Application.Common;

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new List<T>();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int Size { get; set; }
}