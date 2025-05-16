using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TravelAgencyBackend.Helpers 
{
    public class PaginatedList<T> : List<T>
    {
        public int PageIndex { get; private set; }
        public int PageSize { get; private set; }
        public int TotalCount { get; private set; }
        public int TotalPages { get; private set; }

        public PaginatedList(List<T> items, int count, int pageIndex, int pageSize)
        {
            PageIndex = pageIndex;
            PageSize = pageSize;
            TotalCount = count;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);

            AddRange(items);
        }

        public bool HasPreviousPage => PageIndex > 1;

        public bool HasNextPage => PageIndex < TotalPages;

        public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> source, int pageIndex, int pageSize)
        {
            var count = await source.CountAsync();
            // 確保 pageIndex 在有效範圍內
            if (pageIndex < 1) pageIndex = 1;
            // 計算總頁數後，再次檢查 pageIndex 是否超出範圍 (例如刪除最後一頁的資料後)
            int totalPages = (int)Math.Ceiling(count / (double)pageSize);
            if (pageIndex > totalPages && totalPages > 0) pageIndex = totalPages;


            var items = await source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
            return new PaginatedList<T>(items, count, pageIndex, pageSize);
        }

        public static PaginatedList<T> Create(IEnumerable<T> source, int pageIndex, int pageSize)
        {
            var count = source.Count();
            // 確保 pageIndex 在有效範圍內
            if (pageIndex < 1) pageIndex = 1;
            // 計算總頁數後，再次檢查 pageIndex 是否超出範圍
            int totalPages = (int)Math.Ceiling(count / (double)pageSize);
            if (pageIndex > totalPages && totalPages > 0) pageIndex = totalPages;

            var items = source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
            return new PaginatedList<T>(items, count, pageIndex, pageSize);
        }
    }
}
