using Microsoft.EntityFrameworkCore;
using System.Drawing.Printing;

namespace aspnetMCVBSUser1
{
    public class PaginatedList<T> : List<T>
    {
        public int PageSize { get; private set; }
        public int PageIndex { get; private set; }
        public int TotalPages { get; private set; }

        public int ShowIndexCount { get; private set; } = 5;



        public PaginatedList(List<T> items, int count, int pageIndex, int
       pageSize)
        {
            PageIndex = pageIndex;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
            PageSize = pageSize;
            AddRange(items);
        }

        /// <summary>
        /// 启用或禁用“上一页”
        /// </summary>
        public bool HasPreviousPage => PageIndex > 1;

        /// <summary>
        /// 启用或禁用“下一页”
        /// </summary>
        public bool HasNextPage => PageIndex < TotalPages;
    

        /// <summary>
        /// CreateAsync 方法将提取页面大小和页码，并将相应的 Skip 和 Take 语句应
        ///用于 IQueryable 。 当在 IQueryable 上调用 ToListAsync 时，它将返回仅包含请求页的
        //列表
        /// </summary>
        /// <param name="source">经过筛选排序后的所有要显示的所有数据</param>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">页面大小</param>
        /// <returns></returns>
        public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T>
       source, int pageIndex, int pageSize)
        {
            var count = await source.CountAsync();
            var items = await source.Skip((pageIndex - 1) *
           pageSize).Take(pageSize).ToListAsync();
            return new PaginatedList<T>(items, count, pageIndex, pageSize);
        }

        public static  PaginatedList<T> CreateAsync(List<T>
      source, int pageIndex, int pageSize)
        {
            var count =  source.Count();
            var items =  source.Skip((pageIndex - 1) *
           pageSize).Take(pageSize).ToList();
            return new PaginatedList<T>(items, count, pageIndex, pageSize);
        }
    }

}
