using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Common.Models
{
    public class PaginationParams
    {
        private int _pageSize = 10;
        private int _pageNumber = 1;

        const int maxPageSize = 50;

        public int PageNumber
        {
            get
            {
                return _pageNumber;
            }
            set
            {
                _pageNumber = value <= 0 ? 1 : value;
            }
        }
        public int PageSize
        {
            get
            {
                return _pageSize;
            }
            set
            {
                if (value > maxPageSize) _pageSize = maxPageSize;
                else if (value <= 0) _pageSize = 10;
                else _pageSize = value;

                //_pageSize = (value > maxPageSize) ? maxPageSize : value;
            }
        }
    }
}
