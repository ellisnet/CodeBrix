<sub>[CodeBrix](../../README.md) › [Libraries](README.md) › CodeBrix.NotionApi</sub>

# CodeBrix.NotionApi

**CodeBrix.NotionApi is a fully managed .NET client library for the Notion API.** It covers every Notion
endpoint group - pages, blocks, databases, data sources, views, users, comments, custom emojis, search,
file uploads and OAuth - plus the complete Notion object model of rich text, property values, property
items, block types, parents, icons, covers and files, and a set of high-level authoring helpers that
handle Notion's size limits for you. All JSON goes through `System.Text.Json`, there is no native code,
and you use it from any .NET 10 application or from a CodeBrix.Platform application.

| | |
| --- | --- |
| **Repository** | [ellisnet/CodeBrix.NotionApi](https://github.com/ellisnet/CodeBrix.NotionApi) |
| **Packages** | [`CodeBrix.NotionApi.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.NotionApi.MitLicenseForever) |
| **License** | MIT; see [License](#license) |
| **Requires** | .NET 10 or later. No native libraries and no operating-system restrictions; the only runtime requirement is outbound HTTPS to `https://api.notion.com` and a Notion integration token |
| **Use it from** | Any .NET 10 application, or a CodeBrix.Platform application |
| **Platforms** | Anywhere .NET 10 runs |

## What it does

- Covers every Notion API endpoint group through one sub-client each: blocks, pages, databases, data
  sources, views, users, comments, custom emojis, search, file uploads and OAuth authentication.
- Models the complete Notion object model - rich text, property values, property items, block types,
  parents, icons, covers, files - as typed C# classes.
- Deserializes polymorphically on the JSON discriminator, and falls back to a registered fallback type
  instead of throwing, so a Notion type this library does not model still round-trips.
- Applies a configurable, opt-in retry policy that understands Notion's rate limiting and the
  server-supplied `Retry-After` header.
- Registers into `Microsoft.Extensions.DependencyInjection` with one call and pools its `HttpClient`
  through `IHttpClientFactory`, and logs requests through a static opt-in hook.
- Ships authoring helpers - `NotionText`, `NotionBlocks` and `NotionAuthoringExtensions` - that split
  long rich-text runs and batch large appends so the API's per-request caps never reach your code.
- Does a Markdown round trip: create a page from Markdown, read a page back as Markdown, and edit page
  content through four Markdown update operations.
- Leaves an `IRestClient` escape hatch and an `ApiEndpoints` class of URL builders for anything the
  model layer does not cover.

## When to use it

Reach for CodeBrix.NotionApi whenever a .NET program has to read from or write to a Notion workspace:
publishing documents into a wiki, syncing a database with an internal system, generating reports from a
data source, building pages from local content, or driving a Notion-backed workflow from a service.

What it deliberately does not do:

- It does not reorder existing pages or blocks - Notion has no such endpoint. Positional control exists
  only at insert time.
- It does not reposition on move: `Pages.MoveAsync` re-parents only.
- It does not delete pages. Trash them with `PagesUpdateParameters.InTrash = true`.
- It does not parse or render Markdown itself; the Markdown endpoints hand strings to and from Notion,
  which does the conversion server-side.
- It does not cache, memoize or dedupe requests, and it does not rate-limit proactively. Only
  `AppendChildrenBatchedAsync` paces itself, and retry is opt-in.
- It does not manage OAuth flows beyond the four token endpoints: no browser redirect handling, no
  token storage.
- It does not provide LINQ-over-Notion, an ORM, or change tracking. You build request objects and read
  response objects.
- It does not ship the polymorphic-JSON attributes themselves; those come from
  [`CodeBrix.Json.Extensions.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.Json.Extensions.MitLicenseForever).

## Getting started

```bash
dotnet add package CodeBrix.NotionApi.MitLicenseForever
```

The package ID carries the `.MitLicenseForever` suffix; the assembly and the namespace do not. There is
no package named plain `CodeBrix.NotionApi`.

```csharp
using CodeBrix.NotionApi;   // the ENTIRE client and model surface —
                            // one large, flat namespace

using CodeBrix.Json.Extensions.Polymorphism;  // only if you declare your
                            // own [JsonDiscriminator]/[JsonKnownType]/
                            // [JsonFallbackType] hierarchies
```

`ServiceCollectionExtensions` - which is where `AddNotionClient` and `AddNotionClientFactory` live -
also sits in `CodeBrix.NotionApi`, not in `Microsoft.Extensions.DependencyInjection`, so the startup
file needs the same `using` line to see those extension methods. Beyond creating a client or calling one
of the two registration methods, no registration call is required.

Outside dependency injection, the shared factory instance creates a client you own and dispose.

```csharp
using var notion = NotionClientFactory.Instance.Create(new ClientOptions
{
    AuthToken = "ntn_your_integration_token",
    // BaseUrl, NotionVersion, RetryPolicy, HttpClient are optional
});
```

Here is a complete program: connect, prove the token works, create a child page and fill it.

```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CodeBrix.NotionApi;

internal static class Program
{
    private static async Task Main()
    {
        var token = Environment.GetEnvironmentVariable("NOTION_AUTH_TOKEN");
        var parentPageId =
            Environment.GetEnvironmentVariable("NOTION_PARENT_PAGE_ID");

        using var notion = NotionClientFactory.Instance.Create(
            new ClientOptions
            {
                AuthToken = token,
                RetryPolicy = new DefaultRetryPolicy(),
            });

        var me = await notion.Users.MeAsync();
        Console.WriteLine($"Connected as: {me.Name}");

        var page = await notion.CreateChildPageAsync(
            parentPageId, "Hello from CodeBrix.NotionApi");

        await notion.AppendChildrenBatchedAsync(page.Id,
            new List<IBlockObjectRequest>
            {
                NotionBlocks.Heading2("It works"),
                NotionBlocks.Paragraph("Created by a .NET console app."),
            });

        Console.WriteLine(page.Url);
    }
}
```

> [!IMPORTANT]
> Share the parent page with your integration in Notion first, or every call fails with
> `NotionAPIErrorCode.ObjectNotFound` - which reads like a wrong id.

For an ASP.NET Core host, replace the factory line with `services.AddNotionClientFactory();` and inject
`INotionClientFactory`.

## Key concepts

### The client and its sub-clients

`NotionClientFactory` implements `INotionClientFactory`. Use the process-wide, lazily created
`NotionClientFactory.Instance`, or construct one over an `IHttpClientFactory`; `SetHttpClientFactory` is
set-once and thread-safe and throws if a factory was already set or if this instance has already created
a client. `INotionClient` is `IDisposable` and exposes one sub-client per endpoint group: `Users`,
`Pages`, `Databases`, `DataSources`, `Views`, `Blocks`, `Search`, `Comments`, `Emojis`, `FileUploads`,
`AuthenticationClient` and `RestClient`. Note the property name is `AuthenticationClient`, not
`Authentication`. Every method on every sub-client takes a trailing
`CancellationToken cancellationToken = default`.

`ClientOptions` carries `BaseUrl` (defaulting to `https://api.notion.com/`), `NotionVersion`,
`AuthToken` (the integration token, sent as a bearer token), `RetryPolicy` (opt-in; null means no
retries) and `HttpClient` (caller-owned when supplied). The client sends a fixed Notion API version
header by default, and `NotionVersion` overrides it; the value it sends is stated in the repository's
`AGENT-README.txt`.

Disposing an `INotionClient` disposes its `IRestClient`, which releases only an `HttpClient` the library
created and owns. An `HttpClient` supplied through `ClientOptions.HttpClient`, or drawn from an
`IHttpClientFactory`, is left for its owner.

### The endpoint groups, in full

| Sub-client | Methods |
| --- | --- |
| `IPagesClient` | `CreateAsync`, `RetrieveAsync`, `UpdatePropertiesAsync`, `UpdateAsync`, `RetrievePagePropertyItemAsync`, `RetrieveAsMarkdownAsync`, `MoveAsync`, `UpdateMarkdownAsync` |
| `IDatabasesClient` | `RetrieveAsync`, `CreateAsync`, `UpdateAsync` |
| `IDataSourcesClient` | `RetrieveAsync`, `CreateAsync`, `UpdateAsync`, `ListDataSourceTemplatesAsync`, `QueryAsync` |
| `IBlocksClient` | `RetrieveAsync`, `UpdateAsync`, `RetrieveChildrenAsync`, `AppendChildrenAsync`, `DeleteAsync`, `QueryMeetingNotesAsync` |
| `IViewsClient` | `ListAsync`, `CreateAsync`, `RetrieveAsync`, `UpdateAsync`, `DeleteAsync`, `CreateQueryAsync`, `GetQueryResultsAsync`, `DeleteQueryAsync` |
| `IUsersClient` | `RetrieveAsync`, `ListAsync()`, `ListAsync(ListUsersRequest)`, `MeAsync` |
| `ISearchClient` | `SearchAsync` |
| `ICommentsClient` | `CreateAsync`, `RetrieveAsync`, `RetrieveSingleAsync`, `UpdateAsync`, `DeleteAsync` |
| `IEmojisClient` | `ListAsync` |
| `IFileUploadsClient` | `CreateAsync`, `SendAsync`, `CompleteAsync`, `ListAsync`, `RetrieveAsync` |
| `IAuthenticationClient` | `CreateTokenAsync`, `RevokeTokenAsync`, `IntrospectTokenAsync`, `RefreshTokenAsync` |

Those lists are complete: the interfaces have no other members. `IRestClient` is the escape hatch, with
`GetAsync<T>`, two `PostAsync<T>` overloads (a body or file-upload form data), `PatchAsync<T>`,
`DeleteAsync` and `DeleteAsync<T>`; `ApiEndpoints` supplies the URL builders to go with it, grouped as
`PagesApiUrls`, `BlocksApiUrls`, `UsersApiUrls`, `SearchApiUrls`, `CommentsApiUrls`, `DatabasesApiUrls`,
`FileUploadsApiUrls`, `AuthenticationUrls`, `DataSourcesApiUrls`, `ViewsApiUrls` and `EmojisApiUrls`.

### Dependency injection

```csharp
static IServiceCollection AddNotionClientFactory(this IServiceCollection services,
                                                 Action<HttpClient> configureClient = null)
static IServiceCollection AddNotionClient(this IServiceCollection services,
                                          Action<ClientOptions> configureOptions)
```

`AddNotionClientFactory` is the recommended registration: it registers `INotionClientFactory` as a
singleton backed by an `IHttpClientFactory`-managed named `HttpClient`. Inject the factory, then create
and dispose an `INotionClient` per unit of work. Because the `HttpClient` is resolved per request,
handler rotation - the protection against stale DNS and socket exhaustion - is preserved.
`AddNotionClient` instead registers a single application-wide `INotionClient` as a singleton, also
`IHttpClientFactory`-backed; the container disposes it at shutdown, so an injected client from that
registration must not be disposed by your code. Logging is opt-in and static:
`NotionClientLogging.ConfigureLogger(ILoggerFactory loggerFactory)`.

### Pagination

Every listing endpoint returns a `PaginatedList<T>` with `Results`, `HasMore`, `NextCursor` and `Type`,
and `IPaginationParameters` supplies `StartCursor` and `PageSize`, whose Notion maximum is 100. The loop
is the same everywhere.

```csharp
string cursor = null;
do
{
    var page = await notion.Blocks.RetrieveChildrenAsync(
        new BlockRetrieveChildrenRequest
        {
            BlockId = blockId, StartCursor = cursor, PageSize = 100,
        });

    foreach (var block in page.Results) { /* ... */ }

    cursor = page.HasMore ? page.NextCursor : null;
}
while (cursor != null);
```

### Errors and retry

A non-success response throws `NotionApiException`, carrying `NotionAPIErrorCode? NotionAPIErrorCode`
and `HttpStatusCode StatusCode`. HTTP 429 throws the derived `NotionApiRateLimitException`, which adds
`TimeSpan? RetryAfter` taken from the `Retry-After` header. `NotionAPIErrorCode` is an extensible
string-enum struct, so an unknown code round-trips instead of throwing; its members include
`InvalidJSON`, `InvalidRequestUrl`, `InvalidRequest`, `InvalidGrant`, `ValidationError`,
`MissingVersion`, `Unauthorized`, `RestrictedResource`, `ObjectNotFound`, `ConflictError`,
`RateLimited`, `InternalServerError`, `BadGateway`, `ServiceUnavailable`,
`DatabaseConnectionUnavailable` and `GatewayTimeout`, each with a matching `...Value` const string.

Retry is opt-in through `ClientOptions.RetryPolicy`. `IRetryPolicy` declares
`bool ShouldRetry(HttpResponseMessage response, HttpMethod method, int attempt)` and
`TimeSpan GetDelay(HttpResponseMessage response, int attempt)`, and
`DefaultRetryPolicy(int maxRetries = 3, TimeSpan? initialDelay = null, TimeSpan? maxDelay = null)`
retries 429 for all methods, honoring `Retry-After`, and 500 and 503 only for idempotent methods, with
exponential backoff plus jitter. Retry is applied inside the rest client, so it works whether the
`HttpClient` is yours or the library's. If your `HttpClient` pipeline already retries, leave
`RetryPolicy` null - nested retries help nobody.

### Serialization and open enums

All JSON goes through `System.Text.Json` with camelCase naming, `JsonIgnoreCondition.WhenWritingNull`,
case-insensitive matching, and an internal `RuntimeTypeConverterFactory` that serializes abstract- or
interface-declared values by their runtime type. Every request model relies on that, so these types must
not be serialized with a bare `JsonSerializerOptions`.

`BlockType`, `Color`, `ObjectType`, `PropertyValueType`, `PropertyType`, `RichTextType`,
`NotionAPIErrorCode` and `VerificationStatus` are open string-enum structs: an unrecognized value from
Notion is preserved verbatim rather than throwing. Each has `...Value` const strings for attribute use,
`static readonly` fields for code, an implicit conversion from string, and value equality. The real C#
enums in the surface - `Direction`, `Timestamp`, `QueryResultType`, `SearchDirection`,
`SearchObjectType`, `FileUploadMode`, `RelationType` and `StatusPropertyValue.StatusColor` - serialize
through `[JsonStringEnumMemberName]`, and the first three each have an `Unknown` member as their zero
value, so leaving them unset means "unknown".

### Polymorphic deserialization

Every "one of N shapes" model - `IBlock`, `PropertyValue`, `IPropertyItemObject`, `RichTextBase`,
`FileObject`, the parent interfaces, `IPageIcon`, `IPageCover`, `DataSourcePropertyConfig`, `Property`,
`IObject` - is annotated with attributes from the `CodeBrix.Json.Extensions.Polymorphism` namespace:
`JsonDiscriminatorAttribute(string propertyName)`,
`JsonKnownTypeAttribute(Type knownType, string discriminatorValue)` and
`JsonFallbackTypeAttribute(Type fallbackType)`, with `FallbackTypeConverter<T>` and
`FallbackTypeConverterFactory` behind them. The rules are enforced at run time: a known or fallback
target must be assignable to the base type, a type may not declare itself as its own target (which is
why the library ships derived `Unknown*` classes), targets must be instantiable or themselves declare a
discriminator, and duplicate discriminator values are rejected.

> [!IMPORTANT]
> The practical consequence: unknown values coming back from Notion do not throw. A block type this
> library does not model deserializes as `UnsupportedBlock`, an unmodeled property type as
> `UnknownPropertyValue`, and so on. Always handle those cases.

### The three page-property families

Confusing these is the single most common mistake with this library.

| Family | What it is | Where you meet it |
| --- | --- | --- |
| `PropertyValue` | A property's **value** on a page | `Page.Properties`, keyed by property name or id - what you read *and* what you write |
| `IPropertyItemObject` | The paginated response for a value too large to inline | `Pages.RetrievePagePropertyItemAsync` |
| `Property` / `DataSourcePropertyConfig` | The **schema** of a column, not a value | Database and data-source definitions |

The concrete `PropertyValue` types are `TitlePropertyValue`, `RichTextPropertyValue`,
`SelectPropertyValue`, `MultiSelectPropertyValue`, `StatusPropertyValue`, `DatePropertyValue`,
`NumberPropertyValue`, `CheckboxPropertyValue`, `PeoplePropertyValue`, `RelationPropertyValue`,
`FilesPropertyValue`, `UrlPropertyValue`, `EmailPropertyValue`, `PhoneNumberPropertyValue`,
`VerificationPropertyValue`, `ButtonPropertyValue`, `PlacePropertyValue`, the read-only
`FormulaPropertyValue`, `RollupPropertyValue`, `CreatedTimePropertyValue`, `CreatedByPropertyValue`,
`LastEditedTimePropertyValue`, `LastEditedByPropertyValue` and `UniqueIdPropertyValue`, and the fallback
`UnknownPropertyValue`. Properties you do not include in an update are left unchanged, and the read-only
ones are computed by Notion and cannot be written.

### Querying a data source

Rows live in a data source, and a database points at one or more of them, so the round trip starts with
the database: `Databases.RetrieveAsync(databaseId)`, then `database.DataSources.First().DataSourceId`.
`QueryDataSourceRequest` carries `DataSourceId`, `FilterProperties`, `Sorts`, `Filter`, `StartCursor`,
`PageSize`, `Archived`, `InTrash` and `ResultType`, and its results are
`IQueryDataSourceResponseObject`, which resolves to a `Page` or a `DataSource` - so
`response.Results.OfType<Page>()` is the normal way to consume it.

`Filter` is the abstract base. `SinglePropertyFilter` adds a `Property`; `CompoundFilter` nests groups
through `CompoundFilter(List<Filter> or = null, List<Filter> and = null)`. Each concrete filter takes the
property name first, then named optional condition arguments: `TitleFilter`, `RichTextFilter`,
`URLFilter`, `EmailFilter` and `PhoneNumberFilter` share the text conditions; `SelectFilter`,
`StatusFilter`, `MultiSelectFilter`, `RelationFilter` and `PeopleFilter` share theirs; `NumberFilter`,
`CheckboxFilter`, `FilesFilter` and `DateFilter` complete the set. `DateFilter`'s comparison arguments
take `RelativeDateValue`, an extensible string-enum struct with implicit conversions from `DateTime`,
`DateTimeOffset` and `string` and the keywords `Today`, `Tomorrow`, `Yesterday`, `OneWeekAgo`,
`OneWeekFromNow`, `OneMonthAgo` and `OneMonthFromNow`; its `pastWeek`, `pastMonth`, `pastYear`,
`nextWeek`, `nextMonth` and `nextYear` arguments are marker objects, selected by passing
`new Dictionary<string, object>()`. `Sort` takes exactly one of `Property` or `Timestamp`, and always
set `Direction` - its default value is `Unknown`.

### The block model's three families

| Family | Purpose | Type shape |
| --- | --- | --- |
| `IBlock` / `Block` | What you read back | `...Block` |
| `IBlockObjectRequest` / `BlockObjectRequest` | What you send when creating or appending | `...BlockRequest` |
| `IUpdateBlock` / `UpdateBlock` | What you send to `Blocks.UpdateAsync` | `...UpdateBlock`, carrying only the fields Notion lets you change |

Nesting marker interfaces constrain what may go where: `INonColumnBlock`, `IColumnChildrenBlock`,
`ITemplateChildrenBlock` and `ISyncedBlockChildren`, each with a `...Request` counterpart. The block set
covers `paragraph`, `heading_1` through `heading_4`, `bulleted_list_item`, `numbered_list_item`,
`to_do`, `toggle`, `code`, `quote`, `callout`, `image`, `video`, `audio`, `file`, `pdf`, `table`,
`table_row`, `column_list`, `column`, `divider`, `breadcrumb`, `table_of_contents`, `bookmark`, `embed`,
`link_preview`, `equation`, `child_page`, `child_database`, `link_to_page`, `synced_block`, `template`,
`tab`, `meeting_notes` and `unsupported`. Heading payload property names really are `Heading_1` through
`Heading_4`, with an underscore. A `column_list` may contain only columns, a column may contain any
`IColumnChildrenBlock`, and `ColumnBlock.Column.Info.WidthRatio` is that column's share of the available
width. `SyncedBlock` with a null `SyncedFrom` is the original; set `SyncedFrom` to create a duplicate
view of another block.

Appending goes through
`BlockAppendChildrenRequest { string BlockId; IEnumerable<IBlockObjectRequest> Children; ContentPosition Position }`,
with children capped at 100 per request, and `ContentPosition` is `StartContentPosition`,
`EndContentPosition` (the default) or `AfterBlockContentPosition`.

### Page and block ordering

Notion keeps a page's children - sub-pages and content blocks alike - in insertion order, and its API
offers no operation to reposition an existing child in place. That is a Notion constraint, not a
limitation of this library, which models the endpoints faithfully. What you *can* do is set
`PagesCreateParameters.Position` to a `PagePosition` subtype (`PageStartPosition`, `PageEndPosition` or
`AfterBlockPagePosition`) or `BlockAppendChildrenRequest.Position` to a `ContentPosition` subtype. The
recipe is therefore to create in the desired order; to fix the order of pages that already exist, trash
them and recreate them in sequence.

### Authoring helpers

For the common "build a page from local content" workflow, prefer these over hand-writing request
objects. `NotionText` has `const int MaxRunLength = 2000` and the members `Run(string content, bool
bold = false, bool italic = false, bool code = false, string linkUrl = null)`, `Split`, `SplitBase`,
`Plain` and `PlainBase` - Notion rejects any rich-text run longer than 2,000 characters, and `Split` and
`SplitBase` break long text at word boundaries into legal runs. `NotionBlocks` supplies the factories
`Paragraph`, `Heading2`, `Heading3`, `Callout`, `Quote`, `Bullet`, `Numbered`, `Toggle`, `Divider`,
`TableRow`, `Table`, `ImageExternal` and `ImageUpload`; its string overloads treat their argument as
literal text, so build emphasis explicitly with `NotionText.Run`, and `Table` takes its width from the
first row. `NotionAuthoringExtensions` extends `INotionClient` with `const int MaxChildrenPerAppend = 100`,
`CreateChildPageAsync`, `AppendChildrenBatchedAsync`, `UploadFileAsync`, `ArchivePageAsync`,
`RetrieveAllChildrenAsync` and `GuessContentType(string path)`. `AppendChildrenBatchedAsync` appends any
number of blocks, splitting into sequential requests of at most 100 so order is preserved, and pausing
`throttleMs` between batches - the default of 350 ms is roughly three requests per second, and 0
disables it.

### The Markdown round trip

Notion ingests and emits its own flavor of Markdown, which is often simpler than building blocks when
you do not need fine interleaving. Create with `PagesCreateParameters.Markdown`, read back with
`Pages.RetrieveAsMarkdownAsync`, and edit with `Pages.UpdateMarkdownAsync` through four
`UpdatePageMarkdownBody` subtypes: `InsertContentMarkdownBody`, `ReplaceContentRangeMarkdownBody`,
`UpdateContentMarkdownBody` and `ReplaceContentMarkdownBody`. An insert's `After` is an ellipsis-format
selection such as `"start text...end text"` and cannot be combined with `Position`. The response type is
`PageMarkdownResponse { string Markdown; bool Truncated; IEnumerable<string> UnknownBlockIds }`.

### Views, comments, emojis and file uploads

A **view** is a saved presentation of a data source - table, board, calendar and the rest - and the API
models its rows through a two-step query: create a query against the view, then page through its
results. `ViewType` is `Table`, `Board`, `List`, `Calendar`, `Timeline`, `Gallery`, `Form`, `Chart`,
`Map` or `Dashboard`, and `ViewConfiguration` is an abstract base with one concrete subclass per view
type plus `UnknownViewConfiguration`, each holding its per-type settings in an `AdditionalData`
dictionary so nothing is lost.

**Comments** are created with `CreateCommentRequest`, whose static factories are
`CreatePageComment(ParentPageInput parent, IEnumerable<RichTextBaseInput> richText)` and
`CreateDiscussionComment(string discussionId, IEnumerable<RichTextBaseInput> richText)`. Set `Parent`
for a new discussion on a page, or `DiscussionId` to reply into an existing one - never both. Updating
replaces the comment's rich text wholesale, and deleting is permanent.

**Custom emojis** list through `IEmojisClient.ListAsync`, and the ids they return are what
`CustomEmojiPageIconRequest` wants when you set a custom emoji as a page icon.

**File uploads** come in two flows. Single-part is `CreateAsync`, then `SendAsync`, then use the
returned id - or `UploadFileAsync`, which does all of it in one call. Multi-part is
`CreateAsync(Mode = MultiPart, NumberOfParts = n)`, then `SendAsync` once per part with a `partNumber`,
then `CompleteAsync`.

### Hard limits

2,000 characters per rich-text run; 100 children per append request; 100 items per page of results.

## Examples

The recommended dependency-injection shape: inject the factory, create and dispose one client per unit
of work.

```csharp
// Startup / registration:
services.AddNotionClientFactory();
// optional: services.AddNotionClientFactory(http => http.Timeout = ...);

// Consuming class — inject INotionClientFactory:
public sealed class MyNotionWorker(INotionClientFactory notionFactory)
{
    public async Task DoWorkAsync(string authToken)
    {
        using var notion = notionFactory.Create(new ClientOptions
        {
            AuthToken = authToken,
        });

        var me = await notion.Users.MeAsync();
        // ... use `notion` for as many calls as this unit of work needs ...
    }   // `notion` is disposed here; the pooled HttpClient is left alone.
}
```

Reading a database's rows. A database holds one or more data sources, so the query goes through the data
source, and the results are filtered to the type you want.

```csharp
using System.Linq;
using CodeBrix.NotionApi;

// A database holds one or more data sources; rows are queried per data source.
var database = await client.Databases.RetrieveAsync("database-id");
var dataSourceId = database.DataSources.First().DataSourceId;

var results = await client.DataSources.QueryAsync(new QueryDataSourceRequest
{
    DataSourceId = dataSourceId,
});

// Results are IQueryDataSourceResponseObject - a Page or a DataSource - so
// filter to the type you want.
foreach (var page in results.Results.OfType<Page>())
{
    Console.WriteLine(page.Id);
}
```

A compound filter with nested groups, two sorts and the pagination loop, collecting every matching page.

```csharp
var filter = new CompoundFilter(and: new List<Filter>
{
    new SelectFilter("Stage", equal: "Review"),
    new CheckboxFilter("Done", equal: false),
    new DateFilter("Due", onOrBefore: DateTime.UtcNow.AddDays(30)),
    new CompoundFilter(or: new List<Filter>
    {
        new MultiSelectFilter("Tags", contains: "finance"),
        new RichTextFilter("Notes", contains: "budget"),
    }),
});

var matches = new List<Page>();
string cursor = null;
do
{
    var response = await notion.DataSources.QueryAsync(
        new QueryDataSourceRequest
        {
            DataSourceId = dataSourceId,
            Filter = filter,
            Sorts = new List<Sort>
            {
                new Sort
                {
                    Property = "Due",
                    Direction = Direction.Ascending,
                },
                new Sort
                {
                    Timestamp = Timestamp.LastEditedTime,
                    Direction = Direction.Descending,
                },
            },
            PageSize = 100,
            StartCursor = cursor,
        });

    matches.AddRange(response.Results.OfType<Page>());
    cursor = response.HasMore ? response.NextCursor : null;
}
while (cursor != null);
```

Writing page properties. The dictionary is keyed by the property *name*, and the same dictionary works
for `UpdatePropertiesAsync` and for `UpdateAsync`.

```csharp
var properties = new Dictionary<string, PropertyValue>
{
    ["Name"]   = new TitlePropertyValue
                 { Title = NotionText.PlainBase("Quarterly report") },
    ["Notes"]  = new RichTextPropertyValue
                 { RichText = NotionText.PlainBase("Draft") },
    ["Status"] = new StatusPropertyValue
                 { Status = new StatusPropertyValue.Data
                   { Name = "In progress" } },
    ["Tags"]   = new MultiSelectPropertyValue
                 { MultiSelect = new List<SelectOption>
                   { new SelectOption { Name = "finance" } } },
    ["Due"]    = new DatePropertyValue
                 { Date = new Date
                   { Start = DateTimeOffset.UtcNow.AddDays(7),
                     IncludeTime = false } },
    ["Points"] = new NumberPropertyValue  { Number = 12.5 },
    ["Done"]   = new CheckboxPropertyValue { Checkbox = false },
};

await notion.Pages.UpdatePropertiesAsync(pageId, properties);
// or: notion.Pages.UpdateAsync(pageId,
//         new PagesUpdateParameters { Properties = properties });
```

Appending mixed block children by hand, which is what the authoring helpers wrap.

```csharp
await notion.Blocks.AppendChildrenAsync(new BlockAppendChildrenRequest
{
    BlockId = pageId,
    Children = new List<IBlockObjectRequest>
    {
        new HeadingTwoBlockRequest
        {
            Heading_2 = new HeadingTwoBlockRequest.Info
                { RichText = NotionText.PlainBase("Findings") },
        },
        new ParagraphBlockRequest
        {
            Paragraph = new ParagraphBlockRequest.Info
                { RichText = NotionText.PlainBase("Body text.") },
        },
        new ToDoBlockRequest
        {
            ToDo = new ToDoBlockRequest.Info
            {
                RichText = NotionText.PlainBase("Ship it"),
                IsChecked = false,
            },
        },
        new CodeBlockRequest
        {
            Code = new CodeBlockRequest.Info
            {
                RichText = NotionText.PlainBase("var x = 1;"),
                Language = "csharp",
            },
        },
        new DividerBlockRequest { Divider = new DividerBlockRequest.Data() },
    },
});
```

And the same job through the authoring helpers: a page, an uploaded image, mixed formatting, and a
batched append that respects the 100-block cap on your behalf.

```csharp
var page = await notion.CreateChildPageAsync(parentPageId, "My Page");

var uploadId = await notion.UploadFileAsync("/path/diagram.png");
var blocks = new List<IBlockObjectRequest>
{
    NotionBlocks.Heading2("Introduction"),
    NotionBlocks.Paragraph("Body text ..."),
    NotionBlocks.Paragraph(new List<RichTextBase>
    {
        NotionText.Run("A "), NotionText.Run("bold", bold: true),
        NotionText.Run(" word."),
    }),
    NotionBlocks.ImageUpload(uploadId, NotionText.PlainBase("A caption")),
    NotionBlocks.Callout("Heads up!", "\U0001F680"),
};
await notion.AppendChildrenBatchedAsync(page.Id, blocks);
```

## Using it in a CodeBrix.Platform application

CodeBrix.NotionApi is not a CodeBrix.Platform add-in: it is a standalone HTTP client with no UI surface,
no native assets and no operating-system restrictions. A CodeBrix.Platform application references it
from its `.Core` library and consumes it exactly as a console application or an ASP.NET Core host would,
with `services.AddNotionClientFactory()` where a service container is available.

Its dependencies arrive automatically and need no version pinning:
[`CodeBrix.Json.Extensions.MitLicenseForever`](https://www.nuget.org/packages/CodeBrix.Json.Extensions.MitLicenseForever)
supplies the polymorphic-JSON attributes and converters, and `Microsoft.Extensions.Logging.Abstractions`,
`Microsoft.Extensions.DependencyInjection.Abstractions` and `Microsoft.Extensions.Http` supply the
logging, container and `HttpClient`-pooling seams.

## Pitfalls

- Do not call `Databases.QueryAsync` - it does not exist. Query `DataSources.QueryAsync` with a
  `DataSourceId` taken from `Database.DataSources`.
- Do not write a rich-text run longer than 2,000 characters; Notion rejects the whole request. Use
  `NotionText.Split` or `PlainBase`.
- Do not append more than 100 children in one request. Use `AppendChildrenBatchedAsync`, or chunk it
  yourself.
- Do not forget to share the target page or database with the integration. Every call then fails with
  `object_not_found`, which reads like a wrong id.
- Do not leave `Direction` or `Timestamp` unset on a `Sort`. Their default value is `Unknown`, not
  `Ascending` or `CreatedTime`.
- Do not mix the request and response families: `RichTextBase` against `RichTextBaseInput`, `IPageIcon`
  against `IPageIconRequest`, `IParentOfPage` against `IParentOfPageRequest`,
  `DataSourcePropertyConfig` against `DataSourcePropertyConfigRequest`, and `Block` against
  `BlockRequest` against `UpdateBlock`. The compiler catches most of these; the parent families are the
  ones people get wrong.
- Do not send a full block to `Blocks.UpdateAsync`. It takes an `IUpdateBlock` carrying only the changed
  payload, not the `...BlockRequest` type. Update blocks take `RichTextBaseInput` for their rich text,
  with `CodeUpdateBlock.Info.RichText` the one exception.
- Do not expect to reorder existing children, and do not expect `MoveAsync` to reposition - it only
  re-parents.
- Do not dispose an `INotionClient` that came from the `AddNotionClient` registration; the container
  owns it. Do dispose the ones from `INotionClientFactory.Create`.
- Do not call `SetHttpClientFactory` twice, or after the factory has created a client. Both throw
  `InvalidOperationException`.
- Do not serialize these models with a default `JsonSerializerOptions`. Abstract- and interface-typed
  members need the library's runtime-type converter to write the runtime type.
- Do not assume an unknown type will throw. It will not: you silently get `UnsupportedBlock`,
  `UnknownPropertyValue`, `UnknownRichText`, `UnknownObject` and their siblings.
- Do not treat `Search` as full-text search. It matches titles only, and only over content shared with
  the integration.
- Do not set both `Parent` and `DiscussionId` on a `CreateCommentRequest`, or both `After` and
  `Position` on an insert.
- Do not hard-code a `NotionVersion`. The default is the version the models are shaped for, and changing
  it changes the request and response shapes.
- Do not read `Archived` on a data source or a file upload; it is `[Obsolete]`, and `InTrash` is the
  member to read. On a page or a database the same code does not even compile - `Page` and `Database`
  have no `Archived` member.
- Do not send `list_start_index` or `list_format` on a numbered list item, and do not set an `Icon` on a
  paragraph that is not a direct child of a `tab` block. Both are validation errors.
- `ListAsync` on views returns partial views - only `Object` and `Id` are populated - so call
  `RetrieveAsync` for `Name`, `Type`, `Sorts` and `Configuration`. Creating a view needs both a
  `DatabaseId` and a `DataSourceId`, deleting refuses to remove a database's last view, and a view query
  is a snapshot with an `ExpiresAt`: re-create it rather than holding a query id for long.
- Notion truncates large property values inside a `Page` response; fetch the full value with
  `Pages.RetrievePagePropertyItemAsync`.

For performance: pool the `HttpClient` - in DI that means `AddNotionClientFactory()` and one client per
unit of work, and outside DI it means reusing one long-lived client, because a client constructed per
request without a factory owns a fresh connection pool and leaks sockets. Batch appends, since one
100-block request is far cheaper than 100 one-block requests. Ask for full pages: `PageSize = 100` is
the maximum everywhere, and leaving it unset makes Notion pick a smaller default and triples your round
trips. Narrow the payload with `QueryDataSourceRequest.FilterProperties`, and push conditions into
`Filter` and `Sorts` rather than fetching everything and filtering in memory.

## Documentation and source

The repository contains no sample applications, demo apps or build tools: it is a single library project
plus its test project, and the tests are the worked example set.

| Document | Where |
| --- | --- |
| Overview (README) | [README.md](https://github.com/ellisnet/CodeBrix.NotionApi/blob/main/README.md) |
| Complete API guide (ships inside the package too) | [AGENT-README.txt](https://github.com/ellisnet/CodeBrix.NotionApi/blob/main/AGENT-README.txt) |
| Map of every document in the repository | [README-INDEX.txt](https://github.com/ellisnet/CodeBrix.NotionApi/blob/main/README-INDEX.txt) |
| Non-package content in the repository | [EXTRAS-README.txt](https://github.com/ellisnet/CodeBrix.NotionApi/blob/main/EXTRAS-README.txt) |
| Tests (worked examples of every API) | [tests/CodeBrix.NotionApi.Tests](https://github.com/ellisnet/CodeBrix.NotionApi/tree/main/tests/CodeBrix.NotionApi.Tests) |

The unit tests run offline against two in-repo fakes: a `FakeServer` harness and `RecordingRestClient`,
an `IRestClient` fake that records calls and returns canned objects, backed by JSON response fixtures
under `tests/CodeBrix.NotionApi.Tests/data`. The integration tests under
`tests/CodeBrix.NotionApi.Tests/Integration` hit the real Notion API and are opt-in: every class skips
unless `NOTION_AUTH_TOKEN`, `NOTION_PARENT_PAGE_ID` and `NOTION_PARENT_DATABASE_ID` are set, with
`AuthenticationClientTests` additionally needing `NOTION_CLIENT_ID`, `NOTION_CLIENT_SECRET` and
`NOTION_OAUTH_CODE`. They create real content in the target workspace and run as one non-parallel
collection, because Notion returns HTTP 409 when several tests write under the same parent at once.

The package carries `AGENT-README.txt` - a complete API reference and usage guide written for AI coding
agents - so an agent can be pointed straight at the file inside the package it is writing code against.

## License

CodeBrix.NotionApi is licensed under the MIT License, and the license is also named in the package ID
(`CodeBrix.NotionApi.MitLicenseForever`). For the provenance and licensing of open source code included
in this library, see
[THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.NotionApi/blob/main/THIRD-PARTY-NOTICES.txt)
in the repository.

---

**Where to go next**

- [CodeBrix.Json.Extensions](CodeBrix.Json.Extensions.md) - the polymorphic-JSON attributes and converters every model here is annotated with
- [CodeBrix.PdfDocuments](CodeBrix.PdfDocuments.md) - turn the Markdown a page hands back into a printable PDF
- [Library catalog](README.md) - every CodeBrix library by topic
- [ellisnet/CodeBrix.NotionApi on GitHub](https://github.com/ellisnet/CodeBrix.NotionApi) - source and tests
