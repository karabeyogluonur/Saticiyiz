
using System.Collections.Generic;

namespace ST.Application.Common.Paging;

public interface IPagedList<T>
{
    int IndexFrom { get; }
    int PageIndex { get; }
    int PageSize { get; }
    int TotalCount { get; }
    int TotalPages { get; }
    IList<T> Items { get; }
    bool HasPreviousPage { get; }

    bool HasNextPage { get; }
}
