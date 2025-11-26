using EFCoreTest.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text.Json;
using System.Threading.Tasks;
using static EFCoreTest.Services.CodingTestService;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace EFCoreTest.Services;

public class CodingTestService(AppDbContext db, ILogger<CodingTestService> logger) : ICodingTestService
{
    private readonly AppDbContext _db = db;
    private readonly ILogger<CodingTestService> _logger = logger;

   
       
    

    public async Task GeneratePostSummaryReportAsync(int maxItems)
    {
        // Task placeholder:
        // - Emit REPORT_START, then up to `maxItems` lines prefixed with "POST_SUMMARY|" and
        //   finally REPORT_END. Each summary line must include PostId|AuthorName|CommentCount|LatestCommentAuthor.
        // - Method must be read-only and efficient for large datasets;
        // Implement the method body in the assessment; do not change the signature.
        try
        {
            Console.WriteLine("REPORT_START");

            var posts = await _db.Posts .AsNoTracking().OrderByDescending(p => p.CreatedAt).Take(maxItems)
                .Select(p => new
                {
                    p.Id, AuthorName = p.Author.Name, CommentCount = p.Comments.Count(), LatestCommentAuthor = p.Comments
                        .OrderByDescending(c => c.CreatedAt)
                        .Select(c => c.Author.Name)
                        .FirstOrDefault()
                }).ToListAsync();

            foreach (var p in posts)
                Console.WriteLine( $"POST_SUMMARY|{p.Id}|{p.AuthorName}|{p.CommentCount}|{p.LatestCommentAuthor}");
            

            Console.WriteLine("REPORT_END");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GeneratePostSummaryReport - ", JsonSerializer.Serialize(ex.StackTrace));
            throw;  
        }
    }

    public async Task<IList<PostDto>> SearchPostSummariesAsync(string query, int maxResults = 50)
    {
        // Task placeholder:
        // - Return at most `maxResults` PostDto entries.
        // - Treat null/empty/whitespace query as no filter (return unfiltered results up to maxResults).
        // - Matching: case-insensitive substring in Title OR Content.
        // - Order by CreatedAt descending, project to PostDto, and avoid materializing full entities.
        // Implement the method body in the assessment; do not change the signature.
        try
        {
            var posts = QueryAll(query.Trim());

            return await posts.OrderByDescending(p => p.CreatedAt).Take(maxResults)
                .Select(p => new PostDto
                {
                    Id = p.Id,Title = p.Title,
                    AuthorName = p.Author.Name, CommentCount = p.Comments.Count(),
                    CreatedAt = p.CreatedAt
                }).ToListAsync();
         }
         catch (Exception ex)
          {
                _logger.LogError(ex, "SearchPostSummaries - ", JsonSerializer.Serialize(ex.StackTrace));
                throw;
          }
    }

    public async Task<IList<PostDto>> SearchPostSummariesAsync<TKey>(string query, int skip, int take, Expression<Func<PostDto, TKey>> orderBySelector, bool descending)
    {
        // Task placeholder:
        // - Server-side filter by `query` (null/empty => no filter), server-side ordering based on
        //   the provided DTO selector, then Skip/Take for paging. Project to PostDto and avoid
        //   per-row queries or client-side paging.
        // - Implementations may choose which selectors to support; unsupported selectors may
        //   be rejected by the grader.
        // Implement the method body in the assessment; do not change the signature.
        try
        {

            var posts =  QueryAll(query.Trim());
            var postdisplay = posts.Select(p => new PostDto
            {
                Id = p.Id,
                Title = p.Title,
                AuthorName = p.Author.Name,
                CommentCount = p.Comments.Count(),
                CreatedAt = p.CreatedAt
            });

             postdisplay = descending ? postdisplay.OrderByDescending(orderBySelector): postdisplay.OrderBy(orderBySelector);
             return await postdisplay.Skip(skip).Take(take).ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SearchPostSummaries - ", JsonSerializer.Serialize(ex.StackTrace));
            throw;
        }
    }
    private IQueryable<Post> QueryAll(string textFilter = null)
    {
        var posts = _db.Posts.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(textFilter))
        {
            var qLower = textFilter.ToLower();
            posts = posts.Where(p =>
                p.Title.ToLower().Contains(qLower) ||
                p.Content.ToLower().Contains(qLower)
            );
        }
        return posts;
    }

}
