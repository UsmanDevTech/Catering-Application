using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Application.Common.Models
{
    public class PagedResponse<T>
    {
        public PagedResponse(IEnumerable<T> items, int count, int pageNumber, int pageSize)
        {
            Items = items;
            TotalItems = count;
            PageSize = pageSize;
            TotalPages = (int)Math.Ceiling(TotalItems / (double)pageSize);
            CurrentPage = pageNumber;
        }

        public IEnumerable<T> Items { get; private set; }

        public int CurrentPage { get; private set; }
        public int TotalPages { get; private set; }
        public int PageSize { get; private set; }
        public int TotalItems { get; private set; }

        public bool HasPrevious => CurrentPage > 1;
        public bool HasNext => CurrentPage < TotalPages;
    }
}
