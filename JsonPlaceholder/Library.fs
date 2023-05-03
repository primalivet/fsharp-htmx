module JsonPlaceholder

open System.Net
open System.Net.Http.Json
open System.Threading.Tasks

// ---------------------------------
// Types
// ---------------------------------

type Post =
    { userId: int
      id: int
      title: string
      body: string }

// ---------------------------------
// Requests
// ---------------------------------

let getPostById (id: int64) : Task<Result<Post, string>> =
    task {
        use client = new Http.HttpClient()
        let url = sprintf "https://jsonplaceholder.typicode.com/posts/%d" id

        try
            let! post = client.GetFromJsonAsync<Post>(url)
            return Result.Ok post
        with _ ->
            return Result.Error <| sprintf "Failed to fetch post with id %d" id
    }


let getPostsPerPage (paging: int * int) : Task<Result<Post list, string>> =
    let calcPaging paging =
        match paging with
        | (0, c) -> (0, c)
        | (1, c) -> (0, c)
        | (p, c) -> (p * c, c)

    task {
        let url =
            calcPaging paging
            |> fun (start, limit) ->
                sprintf "https://jsonplaceholder.typicode.com/posts?_start=%d&_limit=%d" start limit

        try
            use client = new Http.HttpClient()
            let! post = client.GetFromJsonAsync<Post list>(url)
            return Result.Ok post
        with _ ->
            return Result.Error <| sprintf "Failed to fetch post for page %d" (fst paging)
    }

// ---------------------------------
// Combinators
// ---------------------------------
